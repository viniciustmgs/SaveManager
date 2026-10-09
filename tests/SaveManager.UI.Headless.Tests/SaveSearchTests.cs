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
    public class SaveSearchTests : IDisposable
    {
        private readonly string _root =
            Path.Combine(Path.GetTempPath(), "sm-search-" + Guid.NewGuid().ToString("N"));

        private string ConfigPath => Path.Combine(_root, "SaveManager", "config.json");
        private string BackupPath => Path.Combine(_root, "backup");
        private string SavesPath => Path.Combine(BackupPath, "p1");

        public SaveSearchTests()
        {
            Directory.CreateDirectory(Path.Combine(_root, "SaveManager"));
            Directory.CreateDirectory(SavesPath);

            foreach (var name in new[] { "hollow_knight", "Dark Souls", "HOLLOW KNIGHT", "elden ring" })
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
                        Settings = new AppSettings { GlobalHotkeysEnabled = false }
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
        public void EverythingIsVisibleBeforeAnythingIsTyped()
        {
            var names = Ui.RunAsync(async () => (await Load()).Saves.Select(s => s.Name).ToList());

            Assert.Equal(4, names.Count);
        }

        [Fact]
        public void TypingNarrowsTheListToMatches()
        {
            var names = Ui.RunAsync(async () =>
            {
                var vm = await Load();
                vm.SaveSearchText = "dark";
                return vm.Saves.Select(s => s.Name).ToList();
            });

            Assert.Equal(["Dark Souls"], names);
        }

        [Fact]
        public void MatchingIgnoresCase()
        {
            var lower = Ui.RunAsync<int>(async () =>
            {
                var vm = await Load();
                vm.SaveSearchText = "hollow";
                return vm.Saves.Select(s => s.Name).Count();
            });

            var upper = Ui.RunAsync<int>(async () =>
            {
                var vm = await Load();
                vm.SaveSearchText = "HOLLOW";
                return vm.Saves.Select(s => s.Name).Count();
            });

            var mixed = Ui.RunAsync<int>(async () =>
            {
                var vm = await Load();
                vm.SaveSearchText = "HoLlOw";
                return vm.Saves.Select(s => s.Name).Count();
            });

            Assert.Equal(2, lower);
            Assert.Equal(2, upper);
            Assert.Equal(2, mixed);
        }

        [Fact]
        public void ClearingTheBoxBringsEverythingBack()
        {
            var names = Ui.RunAsync(async () =>
            {
                var vm = await Load();

                vm.SaveSearchText = "dark";
                var narrowed = vm.Saves.Count;

                vm.SaveSearchText = string.Empty;

                return new[] { narrowed, vm.Saves.Count };
            });

            Assert.Equal([1, 4], names);
        }

        [Fact]
        public void WhitespaceOnlyIsTreatedAsNoFilter()
        {
            var count = Ui.RunAsync<int>(async () =>
            {
                var vm = await Load();
                vm.SaveSearchText = "   ";
                return vm.Saves.Count;
            });

            Assert.Equal(4, count);
        }

        [Fact]
        public void SearchingAppliesOnTopOfTheChosenSort()
        {
            var names = Ui.RunAsync(async () =>
            {
                var vm = await Load();

                vm.SelectedSortOption = SaveSortOption.AlphabetDescending;
                vm.SaveSearchText = "l";

                return vm.Saves.Select(s => s.Name).ToList();
            });

            Assert.Equal(4, names.Count);
            Assert.Equal("hollow_knight", names[0]);
            Assert.Equal("Dark Souls", names[^1]);
        }

        [Fact]
        public void RenamingWhileSearchingKeepsAMatchingSaveVisible()
        {
            var names = Ui.RunAsync(async () =>
            {
                var vm = await Load();

                vm.SaveSearchText = "hollow";

                var target = vm.Saves.First(s => s.Name == "hollow_knight");

                vm.OpenRenameSaveCommand.Execute(target);
                vm.RenameSaveName = "hollow_hollow";
                await vm.ConfirmRenameSaveCommand.ExecuteAsync(null);

                return vm.Saves.Select(s => s.Name).ToList();
            });

            Assert.Equal(["HOLLOW KNIGHT", "hollow_hollow"], names.OrderBy(n => n).ToList());
        }

        [Fact]
        public void RenamingSoItNoLongerMatchesDropsItOutOfTheList()
        {
            var names = Ui.RunAsync(async () =>
            {
                var vm = await Load();

                vm.SaveSearchText = "dark";

                vm.OpenRenameSaveCommand.Execute(vm.Saves.First());
                vm.RenameSaveName = "zzz renamed";
                await vm.ConfirmRenameSaveCommand.ExecuteAsync(null);

                return vm.Saves.Select(s => s.Name).ToList();
            });

            Assert.Empty(names);
        }

        [Fact]
        public void TheSelectionIsDroppedWhenTheSearchHidesIt()
        {
            var isStale = Ui.RunAsync<bool>(async () =>
            {
                var vm = await Load();

                vm.SelectedSave = vm.Saves.First(s => s.Name == "Dark Souls");

                vm.SaveSearchText = "zzzzz";

                return ReferenceEquals(vm.SelectedSave, null);
            });

            Assert.True(isStale);
        }

        [Fact]
        public void TheSearchFlagTracksWhetherTheBoxHasContent()
        {
            var flags = Ui.RunAsync(async () =>
            {
                var vm = await Load();

                vm.SaveSearchText = "";
                var idle = vm.IsSaveSearchActive;

                vm.SaveSearchText = "x";

                return new[] { idle, vm.IsSaveSearchActive };
            });

            Assert.Equal([false, true], flags);
        }

        private async Task<MainWindowViewModel> Load()
        {
            var vm = Open();

            vm.SelectedGame = vm.Games.First();

            await vm.OpenProfilePickerCommand.ExecuteAsync(null);
            vm.PendingSelectedProfile = vm.Profiles.First();
            await vm.ConfirmProfilePickerCommand.ExecuteAsync(null);

            for (var i = 0; i < 100 && vm.Saves.Count == 0; i++)
                await Task.Delay(25);

            return vm;
        }

        private MainWindowViewModel Open()
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

            return (MainWindowViewModel)window.DataContext!;
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
