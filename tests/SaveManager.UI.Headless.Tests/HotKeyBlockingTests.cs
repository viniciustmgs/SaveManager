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
    public class HotKeyBlockingTests : IDisposable
    {
        private readonly string _dir =
            Path.Combine(Path.GetTempPath(), "sm-blocking-" + Guid.NewGuid().ToString("N"));

        public HotKeyBlockingTests()
        {
            Directory.CreateDirectory(Path.Combine(_dir, "SaveManager"));
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

        private MainWindowViewModel BuildViewModel()
        {
            var configPath = Path.Combine(_dir, "SaveManager", "config.json");

            File.WriteAllText(configPath, """
            { "Games": [], "Settings": { "GlobalHotkeysEnabled": true } }
            """);

            var services = new ServiceCollection();

            services.AddSingleton<IHotKeyWindowHost, NoWindowHost>();
            services.AddSingleton(new AppConfigStore(configPath));
            services.AddInfrastructure();
            services.AddApplication();

            UI.DI.AppServiceProvider.Build(services);

            var vm = new MainWindowViewModel();
            vm.InitializeHotKeys(new HotKeyCoordinator(new NoopService()));

            return vm;
        }

        [Fact]
        public void WithNothingOpen_HotkeysAreNotBlocked()
        {
            var blocked = Ui.Get(() => BuildViewModel().IsHotKeyInputBlocked);

            Assert.False(blocked);
        }

        [Fact]
        public void SearchBoxFocus_BlocksHotkeys()
        {
            var blocked = Ui.Get(() =>
            {
                var vm = BuildViewModel();
                vm.IsSearchBoxFocused = true;

                return vm.IsHotKeyInputBlocked;
            });

            Assert.True(blocked);
        }

        [Fact]
        public void LosingSearchBoxFocus_UnblocksHotkeys()
        {
            var blocked = Ui.Get(() =>
            {
                var vm = BuildViewModel();
                vm.IsSearchBoxFocused = true;
                vm.IsSearchBoxFocused = false;

                return vm.IsHotKeyInputBlocked;
            });

            Assert.False(blocked);
        }

        public static TheoryData<string> AllOverlayFlags => new()
        {
            nameof(MainWindowViewModel.IsGamePickerOpen),
            nameof(MainWindowViewModel.IsAddGameDialogOpen),
            nameof(MainWindowViewModel.IsEditGamesDialogOpen),
            nameof(MainWindowViewModel.IsEditGameFormOpen),
            nameof(MainWindowViewModel.IsDeleteGameConfirmOpen),
            nameof(MainWindowViewModel.IsProfilePickerOpen),
            nameof(MainWindowViewModel.IsAddProfileDialogOpen),
            nameof(MainWindowViewModel.IsEditProfilesDialogOpen),
            nameof(MainWindowViewModel.IsEditProfileFormOpen),
            nameof(MainWindowViewModel.IsDeleteProfileConfirmOpen),
            nameof(MainWindowViewModel.IsRenameSaveDialogOpen),
            nameof(MainWindowViewModel.IsSettingsDialogOpen),
            nameof(MainWindowViewModel.IsSettingsDiscardPromptOpen)
        };

        [Theory]
        [MemberData(nameof(AllOverlayFlags))]
        public void EveryOverlay_BlocksHotkeys(string flag)
        {
            var blocked = Ui.Get(() =>
            {
                var vm = BuildViewModel();

                typeof(MainWindowViewModel)
                    .GetProperty(flag)!
                    .SetValue(vm, true);

                return vm.IsHotKeyInputBlocked;
            });

            Assert.True(blocked, $"{flag} did not block hotkeys");
        }

        [Theory]
        [MemberData(nameof(AllOverlayFlags))]
        public void ClosingEveryOverlay_UnblocksHotkeys(string flag)
        {
            var blocked = Ui.Get(() =>
            {
                var vm = BuildViewModel();

                var property = typeof(MainWindowViewModel).GetProperty(flag)!;
                property.SetValue(vm, true);
                property.SetValue(vm, false);

                return vm.IsHotKeyInputBlocked;
            });

            Assert.False(blocked, $"{flag} left hotkeys blocked after closing");
        }

        [Fact]
        public void OverlayState_ReachesTheCoordinator()
        {
            var blockedByCoordinator = Ui.Get(() =>
            {
                var service = new NoopService();

                var configPath = Path.Combine(_dir, "SaveManager", "config.json");

                File.WriteAllText(configPath,
                    """{ "Games": [], "Settings": { "GlobalHotkeysEnabled": true } }""");

                var services = new ServiceCollection();
                services.AddSingleton<IHotKeyWindowHost, NoWindowHost>();
                services.AddSingleton(new AppConfigStore(configPath));
                services.AddInfrastructure();
                services.AddApplication();
                UI.DI.AppServiceProvider.Build(services);

                var vm = new MainWindowViewModel();
                var coordinator = new HotKeyCoordinator(service);

                vm.InitializeHotKeys(coordinator);

                var initial = service.Blocked;

                vm.IsGamePickerOpen = true;
                var whileOpen = service.Blocked;

                vm.IsGamePickerOpen = false;
                var afterClose = service.Blocked;

                coordinator.Dispose();

                return (initial, whileOpen, afterClose);
            });

            Assert.False(blockedByCoordinator.initial);
            Assert.True(blockedByCoordinator.whileOpen);
            Assert.False(blockedByCoordinator.afterClose);
        }

        private class NoopService : IGlobalHotKeyService, IHotKeyInputBlocker
        {
            public event Action<HotKeyAction>? HotKeyPressed
            {
                add { }
                remove { }
            }

            public bool IsSupported => true;
            public string? UnsupportedReason => null;

            public virtual void SetBlocked(bool value)
            {
                Blocked = value;
            }

            public bool Blocked { get; private set; }

            public IReadOnlyDictionary<HotKeyAction, HotKeyRegistrationResult> Apply(
                IReadOnlyDictionary<HotKeyAction, HotKeyGesture> desired)
            {
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
