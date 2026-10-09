using Avalonia.Controls;
using Microsoft.Extensions.DependencyInjection;
using SaveManager.Application.DI;
using SaveManager.Domain.Entities;
using SaveManager.Domain.Enums;
using SaveManager.Domain.Interfaces;
using SaveManager.Infrastructure.DI;
using SaveManager.Infrastructure.Persistence;
using SaveManager.UI.HotKeys;
using SaveManager.UI.ViewModels;

namespace SaveManager.UI.Headless.Tests
{
    public class SettingsLifecycleTests : IDisposable
    {
        private readonly string _dir =
            Path.Combine(Path.GetTempPath(), "sm-lifecycle-" + Guid.NewGuid().ToString("N"));

        private string ConfigPath => Path.Combine(_dir, "SaveManager", "config.json");

        public SettingsLifecycleTests()
        {
            Directory.CreateDirectory(Path.Combine(_dir, "SaveManager"));

            File.WriteAllText(ConfigPath, """
            {
              "Games": [],
              "Settings": {
                "GlobalHotkeysEnabled": true,
                "CreateSave": "F7",
                "LoadSave": "Ctrl+S",
                "NextSave": "",
                "PreviousSave": "",
                "ToggleGlobalHotkeys": ""
              }
            }
            """);
        }

        public void Dispose()
        {
            try
            {
                Directory.Delete(_dir, recursive: true);
            }
            catch (IOException)
            {
            }
        }

        [Fact]
        public void OpeningSettingsReleasesTheBindings()
        {
            var boundWhileOpen = Ui.Get(() =>
            {
                var (_, vm, service) = Open();

                vm.OpenSettingsCommand.Execute(null);

                return service.IsEverythingBound;
            });

            Assert.False(boundWhileOpen);
        }

        [Fact]
        public void ClosingSettingsTakesTheBindingsBack()
        {
            var boundAfterClose = Ui.Get(() =>
            {
                var (_, vm, service) = Open();

                vm.OpenSettingsCommand.Execute(null);
                vm.CloseSettingsCommand.Execute(null);

                return service.IsEverythingBound;
            });

            Assert.True(boundAfterClose);
        }

        [Fact]
        public void SavingWhileTheOverlayStaysOpenKeepsThemReleased()
        {
            var boundAfterSave = Ui.RunAsync(async () =>
            {
                var (_, vm, service) = Open();

                vm.OpenSettingsCommand.Execute(null);
                vm.CreateSaveHotkey = "Alt+F4";

                await vm.SaveSettingsCommand.ExecuteAsync(null);

                return service.IsEverythingBound;
            });

            Assert.False(boundAfterSave);
        }

        private (Window, MainWindowViewModel, RecordingService) Open()
        {
            var services = new ServiceCollection();

            var service = new RecordingService();

            services.AddSingleton<IHotKeyWindowHost, NoWindowHost>();
            services.AddSingleton<IGlobalHotKeyService>(service);
            services.AddSingleton(new AppConfigStore(ConfigPath));

            services.AddInfrastructure();
            services.AddApplication();

            UI.DI.AppServiceProvider.Build(services);

            var window = new Views.MainWindow();
            window.Show();
            window.Activate();

            var vm = (MainWindowViewModel)window.DataContext!;

            // stand in for App.OnMainWindowOpened
            vm.InitializeHotKeys(new HotKeyCoordinator(service));

            return (window, vm, service);
        }

        private sealed class RecordingService : IGlobalHotKeyService
        {
            public event Action<HotKeyAction>? HotKeyPressed
            {
                add { }
                remove { }
            }

            public bool IsSupported => true;
            public string? UnsupportedReason => null;

            public Dictionary<HotKeyAction, HotKeyGesture> LastDesired { get; private set; } = new();

            public bool IsEverythingBound =>
                LastDesired.Count > 0
                && LastDesired.Values.All(g => g.IsValid)
                && LastDesired.ContainsKey(HotKeyAction.CreateSave)
                && LastDesired[HotKeyAction.CreateSave].IsValid;

            public IReadOnlyDictionary<HotKeyAction, HotKeyRegistrationResult> Apply(
                IReadOnlyDictionary<HotKeyAction, HotKeyGesture> desired)
            {
                LastDesired = new Dictionary<HotKeyAction, HotKeyGesture>(desired);

                var results = new Dictionary<HotKeyAction, HotKeyRegistrationResult>();

                foreach (var (action, gesture) in desired)
                {
                    results[action] = gesture.IsValid
                        ? HotKeyRegistrationResult.Registered
                        : HotKeyRegistrationResult.Skipped;
                }

                return results;
            }

            public void Dispose()
            {
            }
        }

        private sealed class NoWindowHost : IHotKeyWindowHost
        {
            public bool IsAvailable => false;
            public IntPtr Handle => IntPtr.Zero;

            public IDisposable AddMessageHook(Func<IntPtr, uint, IntPtr, IntPtr, bool> handler) =>
                new Nothing();
        }

        private sealed class Nothing : IDisposable
        {
            public void Dispose()
            {
            }
        }
    }
}
