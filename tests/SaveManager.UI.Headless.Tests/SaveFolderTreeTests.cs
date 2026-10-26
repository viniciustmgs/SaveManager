using Avalonia;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.VisualTree;
using Microsoft.Extensions.DependencyInjection;
using SaveManager.Application.DI;
using SaveManager.Domain.Entities;
using SaveManager.Domain.Enums;
using SaveManager.Domain.Interfaces;
using SaveManager.Infrastructure.DI;
using SaveManager.Infrastructure.FileSystem;
using SaveManager.Infrastructure.Persistence;
using SaveManager.Infrastructure.Persistence.Models;
using SaveManager.UI.ViewModels;
using System.Text.Json;

namespace SaveManager.UI.Headless.Tests
{
    public class SaveFolderTreeTests : IDisposable
    {
        private const string Marker = SaveFileService.FolderMarkerFileName;

        private readonly string _root =
            Path.Combine(Path.GetTempPath(), "sm-tree-" + Guid.NewGuid().ToString("N"));

        private string ConfigPath => Path.Combine(_root, "SaveManager", "config.json");
        private string BackupPath => Path.Combine(_root, "backup");
        private string ProfilePath => Path.Combine(BackupPath, "p1");
        private string GameSource => Path.Combine(_root, "game-source");

        private Views.MainWindow? _window;

        public SaveFolderTreeTests()
        {
            Directory.CreateDirectory(Path.Combine(_root, "SaveManager"));
            Directory.CreateDirectory(ProfilePath);

            Directory.CreateDirectory(GameSource);
            File.WriteAllText(Path.Combine(GameSource, "slot.sav"), "payload");

            File.WriteAllText(
                ConfigPath,
                JsonSerializer.Serialize(
                    new AppConfig
                    {
                        Games =
                        [
                            new Game
                            {
                                Id = Guid.Parse("33333333-3333-3333-3333-333333333333"),
                                Name = "g",
                                SavePath = GameSource,
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

        private void MakeFolder(string relative)
        {
            var path = Path.Combine(ProfilePath, relative);
            Directory.CreateDirectory(path);
            File.WriteAllText(Path.Combine(path, Marker), string.Empty);
        }

        private void MakeSave(string relative)
        {
            var path = Path.Combine(ProfilePath, relative);
            Directory.CreateDirectory(path);
            File.WriteAllText(Path.Combine(path, "slot.sav"), "payload");
        }

        [Fact]
        public void FoldersComeBeforeSavesAndSavesNestUnderThem()
        {
            MakeFolder("zzz-folder");
            MakeSave(Path.Combine("zzz-folder", "inner"));
            MakeSave("aaa-save");

            var (tree, flat) = Ui.RunAsync(async () =>
            {
                var vm = await Load();

                return (
                    vm.SaveTree.Select(n => n.Name).ToArray(),
                    vm.Saves.Select(s => s.Name).ToArray());
            });

            Assert.Equal(["zzz-folder", "aaa-save"], tree);
            Assert.Equal(["inner", "aaa-save"], flat);
        }

        [Fact]
        public void AFolderNodeCarriesItsSavesAsChildren()
        {
            MakeFolder("group");
            MakeSave(Path.Combine("group", "inner"));

            var names = Ui.RunAsync(async () =>
            {
                var vm = await Load();

                return vm.SaveTree.Single().Children.Select(n => n.Name).ToArray();
            });

            Assert.Equal(["inner"], names);
        }

        [Fact]
        public void SettingTheSelectedSaveDirectlyAlsoSelectsItsNode()
        {
            MakeFolder("group");
            MakeSave(Path.Combine("group", "inner"));

            var (save, node) = Ui.RunAsync(async () =>
            {
                var vm = await Load();

                vm.SelectedSave = vm.Saves.Single();

                return (vm.SelectedSave?.Name, vm.SelectedNode?.Name);
            });

            Assert.Equal("inner", save);
            Assert.Equal("inner", node);
        }

        [Fact]
        public void AFolderRowIsRenderedAndItsContextMenuTargetsTheNode()
        {
            MakeFolder("group");
            MakeSave(Path.Combine("group", "inner"));

            var (rowName, menuTarget) = Ui.RunAsync(async () =>
            {
                var vm = await Load();

                _window!.UpdateLayout();

                var item = _window.GetVisualDescendants().OfType<TreeViewItem>().First();
                var menu = item.ContextMenu!;

                menu.Open(item);

                var entry = menu.Items.OfType<MenuItem>().First();
                var target = (entry.DataContext as SaveTreeNode)?.Name;

                menu.Close();

                return ((item.DataContext as SaveTreeNode)?.Name, target);
            });

            Assert.Equal("group", rowName);
            Assert.Equal("group", menuTarget);
        }

        [Fact]
        public void ExpandingARowUpdatesTheNode()
        {
            MakeFolder("group");
            MakeSave(Path.Combine("group", "inner"));

            var nodeExpanded = Ui.RunAsync(async () =>
            {
                await Load();

                _window!.UpdateLayout();

                var item = _window.GetVisualDescendants().OfType<TreeViewItem>().First();

                item.SetCurrentValue(TreeViewItem.IsExpandedProperty, true);
                _window.UpdateLayout();

                return ((SaveTreeNode)item.DataContext!).IsExpanded;
            });

            Assert.True(nodeExpanded);
        }

        [Fact]
        public void ExpansionStateSurvivesARebuild()
        {
            MakeFolder("group");
            MakeSave(Path.Combine("group", "inner"));

            var (afterExpand, afterCollapse) = Ui.RunAsync(async () =>
            {
                var vm = await Load();

                vm.SaveTree.Single().IsExpanded = true;
                vm.SelectedSortOption = SaveSortOption.AlphabetAscending;
                var expanded = vm.SaveTree.Single().IsExpanded;

                vm.SaveTree.Single().IsExpanded = false;
                vm.SelectedSortOption = SaveSortOption.AlphabetDescending;
                var collapsed = vm.SaveTree.Single().IsExpanded;

                return (expanded, collapsed);
            });

            Assert.True(afterExpand);
            Assert.False(afterCollapse);
        }

        [Fact]
        public void CreatingASaveWithAFolderSelectedPutsItInside()
        {
            MakeFolder("group");

            var relative = Ui.RunAsync(async () =>
            {
                var vm = await Load();

                vm.SelectedNode = vm.SaveTree.Single();

                await vm.CreateSaveCommand.ExecuteAsync(null);

                return vm.Saves.Single().RelativeFolderPath;
            });

            Assert.Equal("group", relative);
            Assert.True(Directory.Exists(Path.Combine(ProfilePath, "group", "game-source")));
        }

        [Fact]
        public void CreatingASaveWithASaveSelectedPutsItInTheSameFolder()
        {
            MakeFolder("group");
            MakeSave(Path.Combine("group", "inner"));

            var (folder, name) = Ui.RunAsync(async () =>
            {
                var vm = await Load();

                vm.SelectedNode = vm.SaveTree.Single().Children.Single();

                await vm.CreateSaveCommand.ExecuteAsync(null);

                var created = vm.Saves.Single(s => s.Name != "inner");

                return (created.RelativeFolderPath, created.Name);
            });

            Assert.Equal("group", folder);
            Assert.Equal("game-source", name);
        }

        [Fact]
        public void CreatingASaveWithNothingSelectedGoesToTheRoot()
        {
            MakeFolder("group");

            var relative = Ui.RunAsync(async () =>
            {
                var vm = await Load();

                vm.SelectedNode = null;

                await vm.CreateSaveCommand.ExecuteAsync(null);

                return vm.Saves.Single().RelativeFolderPath;
            });

            Assert.Equal(string.Empty, relative);
        }

        [Fact]
        public void SearchHidesFoldersWithoutMatches()
        {
            MakeFolder("keep");
            MakeSave(Path.Combine("keep", "alpha"));
            MakeFolder("other");
            MakeSave(Path.Combine("other", "beta"));

            var names = Ui.RunAsync(async () =>
            {
                var vm = await Load();

                vm.SaveSearchText = "alpha";

                return vm.SaveTree.Select(n => n.Name).ToArray();
            });

            Assert.Equal(["keep"], names);
        }

        [Fact]
        public void AFolderWhoseNameMatchesShowsAllOfItsContents()
        {
            MakeFolder("keep");
            MakeSave(Path.Combine("keep", "alpha"));
            MakeFolder("other");
            MakeSave(Path.Combine("other", "beta"));

            var (roots, children) = Ui.RunAsync(async () =>
            {
                var vm = await Load();

                vm.SaveSearchText = "keep";

                return (
                    vm.SaveTree.Select(n => n.Name).ToArray(),
                    vm.SaveTree.Single().Children.Select(n => n.Name).ToArray());
            });

            Assert.Equal(["keep"], roots);
            Assert.Equal(["alpha"], children);
        }

        [Fact]
        public void ANestedFolderStaysVisibleWhenSomethingDeepInsideMatches()
        {
            MakeFolder("outer");
            MakeFolder(Path.Combine("outer", "inner"));
            MakeSave(Path.Combine("outer", "inner", "treasure"));

            var (roots, inner) = Ui.RunAsync(async () =>
            {
                var vm = await Load();

                vm.SaveSearchText = "treasure";

                return (
                    vm.SaveTree.Select(n => n.Name).ToArray(),
                    vm.SaveTree.Single().Children.Select(n => n.Name).ToArray());
            });

            Assert.Equal(["outer"], roots);
            Assert.Equal(["inner"], inner);
        }

        [Fact]
        public void DeletingAFolderRemovesItAndTheSavesInside()
        {
            MakeFolder("group");
            MakeSave(Path.Combine("group", "inner"));

            var (roots, saves, onDisk) = Ui.RunAsync(async () =>
            {
                var vm = await Load();

                vm.RequestDeleteFolder(vm.SaveTree.Single().Folder!);

                await vm.ConfirmDeleteFolderCommand.ExecuteAsync(null);

                return (
                    vm.SaveTree.Count,
                    vm.Saves.Count,
                    Directory.Exists(Path.Combine(ProfilePath, "group")));
            });

            Assert.Equal(0, roots);
            Assert.Equal(0, saves);
            Assert.False(onDisk);
        }

        [Fact]
        public void MovingASaveIntoAFolderMovesItInTheTree()
        {
            MakeFolder("group");
            MakeSave("loose");

            var (roots, child) = Ui.RunAsync(async () =>
            {
                var vm = await Load();

                var folder = vm.SaveTree.Single(n => n.IsFolder).Folder!;
                var save = vm.Saves.Single();

                await vm.MoveSave(save, folder.RelativePath);

                return (
                    vm.SaveTree.Select(n => n.Name).ToArray(),
                    vm.SaveTree.Single().Children.Select(n => n.Name).ToArray());
            });

            Assert.Equal(["group"], roots);
            Assert.Equal(["loose"], child);
        }

        [Fact]
        public void RenamingAFolderKeepsItsSaves()
        {
            MakeFolder("old");
            MakeSave(Path.Combine("old", "inner"));

            var (roots, child) = Ui.RunAsync(async () =>
            {
                var vm = await Load();

                vm.OpenRenameFolderCommand.Execute(vm.SaveTree.Single().Folder!);
                vm.RenameFolderName = "new";

                await vm.ConfirmRenameFolderCommand.ExecuteAsync(null);

                return (
                    vm.SaveTree.Select(n => n.Name).ToArray(),
                    vm.SaveTree.Single().Children.Select(n => n.Name).ToArray());
            });

            Assert.Equal(["new"], roots);
            Assert.Equal(["inner"], child);
        }

        [Fact]
        public void ANewFolderStartsExpanded()
        {
            var (expanded, path) = Ui.RunAsync(async () =>
            {
                var vm = await Load();

                vm.OpenCreateFolderCommand.Execute(null);
                vm.NewFolderName = "fresh";

                await vm.ConfirmCreateFolderCommand.ExecuteAsync(null);

                var node = vm.SaveTree.Single();

                return (node.IsExpanded, node.RelativePath);
            });

            Assert.True(expanded);
            Assert.Equal("fresh", path);
        }

        [Fact]
        public void CreatingADuplicateFolderShowsAnErrorAndKeepsTheDialogOpen()
        {
            MakeFolder("taken");

            var (open, error) = Ui.RunAsync(async () =>
            {
                var vm = await Load();

                vm.OpenCreateFolderCommand.Execute(null);
                vm.NewFolderName = "taken";

                await vm.ConfirmCreateFolderCommand.ExecuteAsync(null);

                return (vm.IsCreateFolderDialogOpen, vm.CreateFolderErrorMessage);
            });

            Assert.True(open);
            Assert.Contains("already exists", error);
        }

        [Fact]
        public void TheEmptyAreaMenuCreatesAFolderAtTheRoot()
        {
            MakeFolder("group");
            MakeSave(Path.Combine("group", "inner"));

            var (open, selected, roots) = Ui.RunAsync(async () =>
            {
                var vm = await Load();

                vm.SelectedNode = vm.SaveTree.First(node => node.IsFolder);

                _window!.UpdateLayout();

                var tree = _window.GetVisualDescendants().OfType<TreeView>().First();
                var item = tree.ContextMenu!.Items.OfType<MenuItem>().First();

                item.RaiseEvent(new RoutedEventArgs(MenuItem.ClickEvent));

                var open = vm.IsCreateFolderDialogOpen;
                var selected = vm.SelectedNode;

                vm.NewFolderName = "fresh";

                await vm.ConfirmCreateFolderCommand.ExecuteAsync(null);

                return (open, selected, vm.SaveTree.Select(node => node.Name).ToArray());
            });

            Assert.True(open);
            Assert.Null(selected);
            Assert.Equal(["fresh", "group"], roots);
        }

        [Fact]
        public void OnlyFoldersShowAnIconAndIndentationIsReduced()
        {
            MakeFolder("group");
            MakeSave(Path.Combine("group", "inner"));
            MakeSave("aaa-save");

            var (folderIcon, saveIcon, innerIndent, rootSaveX) = Ui.RunAsync(async () =>
            {
                var vm = await Load();

                vm.SaveTree.First(node => node.IsFolder).IsExpanded = true;
                _window!.UpdateLayout();

                var items = _window.GetVisualDescendants().OfType<TreeViewItem>().ToList();

                TreeViewItem Item(string name) =>
                    items.First(item => ((SaveTreeNode)item.DataContext!).Name == name);

                bool HasVisibleIcon(TreeViewItem item) =>
                    item.GetVisualDescendants().Any(visual =>
                        visual.GetType().Name == "MaterialIcon" && visual.IsVisible);

                double Indent(TreeViewItem item) =>
                    item.GetVisualDescendants()
                        .OfType<Grid>()
                        .First(grid => grid.Name == "PART_Header")
                        .Margin.Left;

                double RootSaveX(TreeViewItem item) =>
                    item.GetVisualDescendants()
                        .OfType<TextBlock>()
                        .First(text => text.Text == "aaa-save")
                        .TranslatePoint(new Point(0, 0), item)!
                        .Value.X;

                return (HasVisibleIcon(Item("group")),
                        HasVisibleIcon(Item("inner")),
                        Indent(Item("inner")),
                        RootSaveX(Item("aaa-save")));
            });

            Assert.True(folderIcon);
            Assert.False(saveIcon);
            Assert.Equal(10, innerIndent);
            Assert.True(rootSaveX < 20, $"root save started at {rootSaveX}px");
        }

        [Fact]
        public void GlobalHotkeysSelectFoldersAndSkipCollapsedContents()
        {
            MakeFolder("group");
            MakeSave(Path.Combine("group", "inner"));
            MakeSave("aaa-save");

            var names = Ui.RunAsync(async () =>
            {
                var vm = await Load();
                var seen = new List<string>();

                for (var i = 0; i < 4; i++)
                {
                    vm.NextSaveCommand.Execute(null);
                    seen.Add(vm.SelectedNode?.Name ?? "<none>");
                }

                return seen.ToArray();
            });

            Assert.Equal(["group", "aaa-save", "aaa-save", "aaa-save"], names);
        }

        [Fact]
        public void GlobalHotkeysEnterAnExpandedFolder()
        {
            MakeFolder("group");
            MakeSave(Path.Combine("group", "inner"));
            MakeSave("aaa-save");

            var names = Ui.RunAsync(async () =>
            {
                var vm = await Load();

                vm.SaveTree.First(node => node.IsFolder).IsExpanded = true;

                var seen = new List<string>();

                for (var i = 0; i < 3; i++)
                {
                    vm.NextSaveCommand.Execute(null);
                    seen.Add(vm.SelectedNode?.Name ?? "<none>");
                }

                return seen.ToArray();
            });

            Assert.Equal(["group", "inner", "aaa-save"], names);
        }

        [Fact]
        public void GlobalHotkeysResumeFromTheCollapsedAncestor()
        {
            MakeFolder("group");
            MakeSave(Path.Combine("group", "inner"));
            MakeSave("aaa-save");

            var (next, previous) = Ui.RunAsync(async () =>
            {
                var vm = await Load();

                var folder = vm.SaveTree.First(node => node.IsFolder);
                folder.IsExpanded = true;

                vm.SelectedSave = vm.Saves.First(save => save.Name == "inner");
                folder.IsExpanded = false;

                vm.NextSaveCommand.Execute(null);
                var next = vm.SelectedNode?.Name;

                vm.SelectedSave = vm.Saves.First(save => save.Name == "inner");
                vm.PreviousSaveCommand.Execute(null);
                var previous = vm.SelectedNode?.Name;

                return (next, previous);
            });

            Assert.Equal("aaa-save", next);
            Assert.Equal("group", previous);
        }

        private async Task<MainWindowViewModel> Load()
        {
            var vm = Open();

            vm.SelectedGame = vm.Games.First();

            await WaitUntil(() => vm.Profiles.Count > 0);

            vm.SelectedProfile = vm.Profiles.First();

            await WaitUntil(() => vm.SaveTree.Count > 0 || vm.Profiles.Count == 0);

            await Task.Delay(100);

            return vm;
        }

        private static async Task WaitUntil(Func<bool> condition)
        {
            for (var i = 0; i < 100 && !condition(); i++)
                await Task.Delay(25);
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

            _window = window;

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
