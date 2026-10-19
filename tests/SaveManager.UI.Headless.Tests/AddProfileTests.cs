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
    public class AddProfileTests : IDisposable
    {
        private readonly string _root =
            Path.Combine(Path.GetTempPath(), "sm-add-profile-" + Guid.NewGuid().ToString("N"));

        private string ConfigPath => Path.Combine(_root, "SaveManager", "config.json");
        private string BackupPath => Path.Combine(_root, "backup");

        public AddProfileTests()
        {
            Directory.CreateDirectory(Path.Combine(_root, "SaveManager"));
            Directory.CreateDirectory(BackupPath);

            File.WriteAllText(
                ConfigPath,
                JsonSerializer.Serialize(
                    new AppConfig
                    {
                        Games =
                        [
                            new Game
                            {
                                Id = Guid.Parse("22222222-2222-2222-2222-222222222222"),
                                Name = "g",
                                SavePath = Path.Combine(_root, "game"),
                                BackupFolderPath = BackupPath,
                                SaveType = SaveType.Folder
                            }
                        ],
                        Settings = new AppSettings
                        {
                            GlobalHotkeysEnabled = false
                        }
                    }));
        }

        public void Dispose()
        {
            try
            {
                Directory.Delete(_root, recursive: true);
            }
            catch (IOException)
            {
            }
        }

        [Fact]
        public void AddingTheFirstProfileAddsItToTheList()
        {
            var names = Ui.RunAsync(async () =>
            {
                var (_, vm) = Open();

                vm.SelectedGame = vm.Games.First();
                await WaitForProfiles(vm);

                vm.OpenAddProfileCommand.Execute(null);
                vm.NewProfileName = "slot-1";
                await vm.ConfirmAddProfileCommand.ExecuteAsync(null);

                return vm.Profiles.Select(p => p.Name).ToArray();
            });

            Assert.Equal(["slot-1"], names);
        }

        [Fact]
        public void AddingAnotherProfileKeepsTheExistingOnes()
        {
            Directory.CreateDirectory(Path.Combine(BackupPath, "slot-1"));

            var names = Ui.RunAsync(async () =>
            {
                var (_, vm) = Open();

                vm.SelectedGame = vm.Games.First();
                await WaitForProfiles(vm);

                vm.OpenAddProfileCommand.Execute(null);
                vm.NewProfileName = "slot-2";
                await vm.ConfirmAddProfileCommand.ExecuteAsync(null);

                return vm.Profiles.Select(p => p.Name).OrderBy(n => n).ToArray();
            });

            Assert.Equal(["slot-1", "slot-2"], names);
        }

        private static async Task WaitForProfiles(MainWindowViewModel vm)
        {
            for (var i = 0; i < 100 && vm.Profiles.Count == 0; i++)
                await Task.Delay(25);
        }

        private (Views.MainWindow, MainWindowViewModel) Open()
        {
            var services = new ServiceCollection();

            services.AddSingleton<IHotKeyWindowHost, NoWindowHost>();
            services.AddSingleton<IGlobalHotKeyService>(new NoopHotKeyService());
            services.AddSingleton(new AppConfigStore(ConfigPath));

            services.AddInfrastructure();
            services.AddApplication();

            UI.DI.AppServiceProvider.Build(services);

            var window = new Views.MainWindow();
            window.Show();
            window.Activate();

            return (window, (MainWindowViewModel)window.DataContext!);
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
