using Avalonia.Controls;
using Avalonia.VisualTree;
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
    public class SaveSortingTests : IDisposable
    {
        private readonly string _root =
            Path.Combine(Path.GetTempPath(), "sm-sort-" + Guid.NewGuid().ToString("N"));

        private string ConfigPath => Path.Combine(_root, "SaveManager", "config.json");
        private string BackupPath => Path.Combine(_root, "backup");
        private string SavesPath => Path.Combine(BackupPath, "p1");

        public SaveSortingTests()
        {
            Directory.CreateDirectory(Path.Combine(_root, "SaveManager"));
            Directory.CreateDirectory(SavesPath);

            File.WriteAllText(Path.Combine(SavesPath, "zeta"), "oldest");
            Thread.Sleep(20);
            File.WriteAllText(Path.Combine(SavesPath, "alpha"), "middle");
            Thread.Sleep(20);
            File.WriteAllText(Path.Combine(SavesPath, "mu"), "newest");

            foreach (var name in new[] { "save_1", "save_3", "save_2", "save_4" })
            {
                File.WriteAllText(Path.Combine(SavesPath, name), name);
                Thread.Sleep(20);
            }

            File.WriteAllText(
                ConfigPath,
                JsonSerializer.Serialize(
                    new AppConfig
                    {
                        Games =
                        [
                            new Game
                            {
                                Id = Guid.Parse("11111111-1111-1111-1111-111111111111"),
                                Name = "g",
                                SavePath = Path.Combine(_root, "game"),
                                BackupFolderPath = BackupPath,
                                SaveType = SaveType.SingleFile
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
        public void LoadsSortedByCreationRatherThanDiskOrder()
        {
            var names = Ui.RunAsync(async () =>
            {
                var (_, vm) = await LoadFirstProfile();

                return Trio(vm);
            });

            Assert.Equal(["zeta", "alpha", "mu"], names);
        }

        [Fact]
        public void ChangingTheSortOptionReordersTheLoadedList()
        {
            var names = Ui.RunAsync(async () =>
            {
                var (_, vm) = await LoadFirstProfile();

                vm.SelectedSortOption = SaveSortOption.AlphabetDescending;

                return Trio(vm);
            });

            Assert.Equal(["zeta", "mu", "alpha"], names);
        }

        [Fact]
        public void ChangingToAscendingOrdersByName()
        {
            var names = Ui.RunAsync(async () =>
            {
                var (_, vm) = await LoadFirstProfile();

                vm.SelectedSortOption = SaveSortOption.AlphabetAscending;

                return Trio(vm);
            });

            Assert.Equal(["alpha", "mu", "zeta"], names);
        }

        [Fact]
        public void SwitchingBackToCreatedRestoresCreationOrder()
        {
            var names = Ui.RunAsync(async () =>
            {
                var (_, vm) = await LoadFirstProfile();

                vm.SelectedSortOption = SaveSortOption.AlphabetAscending;
                vm.SelectedSortOption = SaveSortOption.AlphabetDescending;
                vm.SelectedSortOption = SaveSortOption.Created;

                return Trio(vm);
            });

            Assert.Equal(["zeta", "alpha", "mu"], names);
        }

        [Fact]
        public void TheDropdownItselfDrivesTheReorder()
        {
            var (loaded, option) = Ui.RunAsync(async () =>
            {
                var (window, vm) = await LoadFirstProfile();

                var combo = window
                    .GetVisualDescendants()
                    .OfType<ComboBox>()
                    .First(c => ReferenceEquals(c.ItemsSource, vm.SortOptions));

                combo.SelectedItem = SaveSortOption.AlphabetDescending;

                return (Trio(vm), combo.SelectedItem);
            });

            Assert.Equal(SaveSortOption.AlphabetDescending, option);
            Assert.Equal(["zeta", "mu", "alpha"], loaded);
        }

        [Fact]
        public void CreatedKeepsCreationOrderWhileAlphabeticalReorders()
        {
            var byCreated = Ui.RunAsync(async () =>
            {
                var (_, vm) = await LoadFirstProfile();

                vm.SelectedSortOption = SaveSortOption.Created;

                return vm.Saves.Where(s => s.Name.StartsWith("save_")).Select(s => s.Name).ToList();
            });

            var byAscending = Ui.RunAsync(async () =>
            {
                var (_, vm) = await LoadFirstProfile();

                vm.SelectedSortOption = SaveSortOption.AlphabetAscending;

                return vm.Saves.Where(s => s.Name.StartsWith("save_")).Select(s => s.Name).ToList();
            });

            Assert.Equal(["save_1", "save_3", "save_2", "save_4"], byCreated);
            Assert.Equal(["save_1", "save_2", "save_3", "save_4"], byAscending);
        }

        [Fact]
        public void SwitchingToCreatedAfterZToAReordersRatherThanKeepingTheList()
        {
            var names = Ui.RunAsync(async () =>
            {
                var (_, vm) = await LoadFirstProfile();

                vm.SelectedSortOption = SaveSortOption.AlphabetDescending;
                vm.SelectedSortOption = SaveSortOption.Created;

                return vm.Saves.Where(s => s.Name.StartsWith("save_")).Select(s => s.Name).ToList();
            });

            Assert.Equal(["save_1", "save_3", "save_2", "save_4"], names);
        }

        [Fact]
        public void CreatedIsTheDefaultSoNoInteractionIsNeeded()
        {
            var option = Ui.Get(() => Open().Item2.SelectedSortOption);

            Assert.Equal(SaveSortOption.Created, option);
        }

        private static List<string> Trio(MainWindowViewModel vm) =>
            vm.Saves.Where(s => !s.Name.StartsWith("save_")).Select(s => s.Name).ToList();

        private async Task<(Views.MainWindow, MainWindowViewModel)> LoadFirstProfile()
        {
            var (window, vm) = Open();

            vm.SelectedGame = vm.Games.First();

            await vm.OpenProfilePickerCommand.ExecuteAsync(null);

            vm.PendingSelectedProfile = vm.Profiles.First();

            await vm.ConfirmProfilePickerCommand.ExecuteAsync(null);

            for (var i = 0; i < 100 && vm.Saves.Count == 0; i++)
                await Task.Delay(25);

            return (window, vm);
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
