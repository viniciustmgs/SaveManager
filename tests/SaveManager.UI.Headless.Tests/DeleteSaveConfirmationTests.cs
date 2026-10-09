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
    public class DeleteSaveConfirmationTests : IDisposable
    {
        private readonly string _root =
            Path.Combine(Path.GetTempPath(), "sm-del-" + Guid.NewGuid().ToString("N"));

        private string ConfigPath => Path.Combine(_root, "SaveManager", "config.json");
        private string BackupPath => Path.Combine(_root, "backup");
        private string SavesPath => Path.Combine(BackupPath, "p1");

        public DeleteSaveConfirmationTests()
        {
            Directory.CreateDirectory(Path.Combine(_root, "SaveManager"));
            Directory.CreateDirectory(SavesPath);

            foreach (var name in new[] { "alpha", "bravo", "charlie" })
                File.WriteAllText(Path.Combine(SavesPath, name), name);

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
        public void TheButtonAsksFirstAndDeletesNothingYet()
        {
            var (open, remaining) = Ui.RunAsync(async () =>
            {
                var (_, vm) = await Load();

                vm.SelectedSave = vm.Saves.First();
                vm.DeleteSelectedSaveCommand.Execute(null);

                return (vm.IsDeleteSaveConfirmOpen, vm.Saves.Count);
            });

            Assert.True(open);
            Assert.Equal(3, remaining);
            Assert.True(File.Exists(Path.Combine(SavesPath, "alpha")));
        }

        [Fact]
        public void ThePromptNamesTheSaveBeingDeleted()
        {
            var message = Ui.RunAsync(async () =>
            {
                var (_, vm) = await Load();

                vm.SelectedSave = vm.Saves.First(s => s.Name == "bravo");
                vm.DeleteSelectedSaveCommand.Execute(null);

                return vm.DeleteSaveConfirmMessage;
            });

            Assert.Equal("bravo is going to be deleted, do you want to proceed?", message);
        }

        [Fact]
        public void CancellingKeepsTheSaveAndClosesThePrompt()
        {
            var (open, remaining) = Ui.RunAsync(async () =>
            {
                var (_, vm) = await Load();

                vm.DeleteSelectedSaveCommand.Execute(null);
                vm.CancelDeleteSaveCommand.Execute(null);

                return (vm.IsDeleteSaveConfirmOpen, vm.Saves.Count);
            });

            Assert.False(open);
            Assert.Equal(3, remaining);
            Assert.True(File.Exists(Path.Combine(SavesPath, "alpha")));
        }

        [Fact]
        public void ConfirmingDeletesTheSaveFromDiskAndFromTheList()
        {
            var (open, remaining, gone) = Ui.RunAsync(async () =>
            {
                var (_, vm) = await Load();

                vm.SelectedSave = vm.Saves.First(s => s.Name == "bravo");
                vm.DeleteSelectedSaveCommand.Execute(null);

                await vm.ConfirmDeleteSaveCommand.ExecuteAsync(null);

                return (vm.IsDeleteSaveConfirmOpen, vm.Saves.Count, File.Exists(Path.Combine(SavesPath, "bravo")));
            });

            Assert.False(open);
            Assert.Equal(2, remaining);
            Assert.False(gone);
        }

        [Fact]
        public void ConfirmingWithNothingTargetedDoesNothing()
        {
            var count = Ui.RunAsync(async () =>
            {
                var (_, vm) = await Load();

                await vm.ConfirmDeleteSaveCommand.ExecuteAsync(null);

                return vm.Saves.Count;
            });

            Assert.Equal(3, count);
        }

        [Fact]
        public void TheContextMenuEntryAsksAboutThatRowRatherThanTheSelection()
        {
            var message = Ui.RunAsync(async () =>
            {
                var (_, vm) = await Load();

                vm.SelectedSave = vm.Saves.First(s => s.Name == "alpha");

                vm.RequestDeleteSave(vm.Saves.First(s => s.Name == "charlie"));

                return vm.DeleteSaveConfirmMessage;
            });

            Assert.Contains("charlie", message);
            Assert.DoesNotContain("alpha", message);
        }

        [Fact]
        public void ConfirmingDeletesTheTargetedRowNotWhicheverWasSelected()
        {
            var (alphaKept, bravoGone) = Ui.RunAsync(async () =>
            {
                var (_, vm) = await Load();

                vm.SelectedSave = vm.Saves.First(s => s.Name == "alpha");

                vm.RequestDeleteSave(vm.Saves.First(s => s.Name == "bravo"));
                await vm.ConfirmDeleteSaveCommand.ExecuteAsync(null);

                return (
                    File.Exists(Path.Combine(SavesPath, "alpha")),
                    File.Exists(Path.Combine(SavesPath, "bravo")));
            });

            Assert.True(alphaKept);
            Assert.False(bravoGone);
        }

        [Fact]
        public void ThePromptCountsAsAnOverlaySoHotkeysStayQuiet()
        {
            var blocked = Ui.RunAsync(async () =>
            {
                var (_, vm) = await Load();

                vm.SelectedSave = vm.Saves.First();
                vm.DeleteSelectedSaveCommand.Execute(null);

                return vm.IsAnyOverlayOpen;
            });

            Assert.True(blocked);
        }

        [Fact]
        public void ThePromptCarriesTheSaveNameAndBothButtons()
        {
            var (message, cancel, confirm) = Ui.RunAsync(async () =>
            {
                var (window, vm) = await Load();

                vm.SelectedSave = vm.Saves.First(s => s.Name == "bravo");
                vm.DeleteSelectedSaveCommand.Execute(null);

                var buttons = window.GetVisualDescendants().OfType<Button>().ToList();

                return (
                    Find(window, "DeleteSavePromptText")?.Text,
                    buttons.Any(b => b.Command == vm.CancelDeleteSaveCommand),
                    buttons.Any(b => b.Command == vm.ConfirmDeleteSaveCommand));
            });

            Assert.Equal("bravo is going to be deleted, do you want to proceed?", message);
            Assert.True(cancel);
            Assert.True(confirm);
        }

        [Fact]
        public void EveryRowCarriesTheContextMenuNotJustTheNameText()
        {
            var (rows, rowMenus, textMenus) = Ui.RunAsync(async () =>
            {
                var (window, vm) = await Load();

                window.UpdateLayout();

                var items = window.GetVisualDescendants().OfType<ListBoxItem>().ToList();

                return (
                    items.Count,
                    items.Count(i => i.ContextMenu is not null),
                    items.SelectMany(i => i.GetVisualDescendants())
                        .OfType<TextBlock>()
                        .Count(t => t.ContextMenu is not null));
            });

            Assert.Equal(3, rows);
            Assert.Equal(3, rowMenus);
            Assert.Equal(0, textMenus);
        }

        [Fact]
        public void TheRowIsWiderThanItsNameSoTheWholeRowIsClickable()
        {
            var (row, text) = Ui.RunAsync(async () =>
            {
                var (window, vm) = await Load();

                window.UpdateLayout();

                var item = window.GetVisualDescendants().OfType<ListBoxItem>().First();

                var label = item.GetVisualDescendants().OfType<TextBlock>().First();

                return (item.Bounds.Width, label.Bounds.Width);
            });

            Assert.True(
                row > text,
                $"the row ({row}) should be wider than the name ({text}), otherwise the name fills it");
        }

        private static TextBlock? Find(Window window, string name) =>
            window.GetVisualDescendants()
                .OfType<TextBlock>()
                .FirstOrDefault(t => t.Name == name);

        private async Task<(Window, MainWindowViewModel)> Load()
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

        private (Window, MainWindowViewModel) Open()
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