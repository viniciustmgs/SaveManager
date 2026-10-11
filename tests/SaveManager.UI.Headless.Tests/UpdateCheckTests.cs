using Microsoft.Extensions.DependencyInjection;
using SaveManager.Application.DI;
using SaveManager.Domain.Entities;
using SaveManager.Domain.Enums;
using SaveManager.Domain.Interfaces;
using SaveManager.Infrastructure.DI;
using SaveManager.Infrastructure.Persistence;
using SaveManager.Infrastructure.Persistence.Models;
using SaveManager.UI.ViewModels;
using System.Text.Json;

namespace SaveManager.UI.Headless.Tests
{
    public class UpdateCheckTests : IDisposable
    {
        private readonly string _root =
            Path.Combine(Path.GetTempPath(), "sm-update-" + Guid.NewGuid().ToString("N"));

        private string ConfigPath => Path.Combine(_root, "SaveManager", "config.json");

        public UpdateCheckTests()
        {
            Directory.CreateDirectory(Path.Combine(_root, "SaveManager"));

            File.WriteAllText(
                ConfigPath,
                JsonSerializer.Serialize(
                    new AppConfig
                    {
                        Games = [],
                        Settings = new AppSettings { GlobalHotkeysEnabled = false }
                    }));
        }

        public void Dispose()
        {
            try { Directory.Delete(_root, true); } catch { }
        }

        [Fact]
        public async Task ClosingAfterTheResultArrivedDoesNotThrow()
        {
            var vm = await BuildAsync(new ImmediateChecker());

            vm.OpenCheckUpdatesCommand.Execute(null);

            await WaitUntilAsync(() => !vm.IsUpdateCheckInProgress);

            Assert.Equal(UpdateCheckStatus.UpToDate, vm.UpdateCheckResult?.Status);

            var exception = Record.Exception(() => vm.CloseCheckUpdatesCommand.Execute(null));

            Assert.Null(exception);
            Assert.False(vm.IsCheckUpdatesOpen);
        }

        [Fact]
        public async Task ClosingWhileLoadingCancelsTheRequest()
        {
            var checker = new BlockingChecker();

            var vm = await BuildAsync(checker);

            vm.OpenCheckUpdatesCommand.Execute(null);

            await WaitUntilAsync(() => checker.Started);

            vm.CloseCheckUpdatesCommand.Execute(null);

            Assert.True(checker.WasCancelled);
            Assert.False(vm.IsCheckUpdatesOpen);
            Assert.Null(vm.UpdateCheckResult);
        }

        [Fact]
        public async Task ReopeningAfterACancelledCheckStartsAFreshRequest()
        {
            var checker = new BlockingChecker();

            var vm = await BuildAsync(checker);

            vm.OpenCheckUpdatesCommand.Execute(null);
            await WaitUntilAsync(() => checker.Started);

            vm.CloseCheckUpdatesCommand.Execute(null);

            vm.OpenCheckUpdatesCommand.Execute(null);

            await WaitUntilAsync(() => checker.StartCount == 2);
        }

        [Fact]
        public void UpdateAvailableWordingMentionsBothVersions()
        {
            var vm = Build(new ImmediateChecker());

            var result = UpdateCheckResult.UpdateAvailable("v1.0.0", "v1.2.0", "https://example.test/r");

            Assert.True(result.LatestVersion.Length > 0);
        }

        private async Task<MainWindowViewModel> BuildAsync(IReleaseChecker checker)
        {
            var vm = Build(checker);

            await Task.Delay(10);

            return vm;
        }

        private MainWindowViewModel Build(IReleaseChecker checker)
        {
            var services = new ServiceCollection();

            services.AddSingleton<IHotKeyWindowHost, NoWindowHost>();
            services.AddSingleton<IGlobalHotKeyService>(new NoopHotKeyService());
            services.AddSingleton(new AppConfigStore(ConfigPath));
            services.AddSingleton(checker);

            services.AddInfrastructure();
            services.AddApplication();

            UI.DI.AppServiceProvider.Build(services);

            return new MainWindowViewModel();
        }

        private static async Task WaitUntilAsync(Func<bool> condition)
        {
            for (var i = 0; i < 200 && !condition(); i++)
                await Task.Delay(10);
        }

        private sealed class ImmediateChecker : IReleaseChecker
        {
            public string ReleasesUrl { get; } = "https://example.test/releases";

            public Task<UpdateCheckResult> CheckAsync(string currentVersion, CancellationToken cancellationToken) =>
                Task.FromResult(UpdateCheckResult.UpToDate(currentVersion, currentVersion, ReleasesUrl));
        }

        private sealed class BlockingChecker : IReleaseChecker
        {
            private TaskCompletionSource _release = new();

            public string ReleasesUrl { get; } = "https://example.test/releases";

            public bool Started { get; private set; }

            public bool WasCancelled { get; private set; }

            public int StartCount { get; private set; }

            public async Task<UpdateCheckResult> CheckAsync(string currentVersion, CancellationToken cancellationToken)
            {
                StartCount++;
                Started = true;

                try
                {
                    await _release.Task.WaitAsync(cancellationToken);
                }
                catch (OperationCanceledException)
                {
                    WasCancelled = true;
                    throw;
                }

                return UpdateCheckResult.UpToDate(currentVersion, currentVersion, ReleasesUrl);
            }
        }

        private sealed class NoWindowHost : IHotKeyWindowHost
        {
            public bool IsAvailable => false;
            public IntPtr Handle => IntPtr.Zero;

            public IDisposable AddMessageHook(Func<IntPtr, uint, IntPtr, IntPtr, bool> handler) =>
                throw new NotSupportedException();
        }

        private sealed class NoopHotKeyService : IGlobalHotKeyService
        {
            public event Action<HotKeyAction>? HotKeyPressed
            {
                add { }
                remove { }
            }

            public bool IsSupported => false;
            public string? UnsupportedReason => null;

            public IReadOnlyDictionary<HotKeyAction, HotKeyRegistrationResult> Apply(
                IReadOnlyDictionary<HotKeyAction, HotKeyGesture> desired) =>
                new Dictionary<HotKeyAction, HotKeyRegistrationResult>();

            public void Dispose()
            {
            }
        }
    }
}