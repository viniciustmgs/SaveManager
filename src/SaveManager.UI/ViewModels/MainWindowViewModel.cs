using Avalonia.Platform.Storage;
using CommunityToolkit.Mvvm.ComponentModel;
using SaveManager.Application.UseCases.Game;
using SaveManager.Application.UseCases.Profile;
using SaveManager.Application.UseCases.Save;
using SaveManager.Application.UseCases.Settings;
using SaveManager.Domain.Entities;
using SaveManager.Domain.Enums;
using SaveManager.UI.DI;
using SaveManager.UI.HotKeys;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;

namespace SaveManager.UI.ViewModels
{
    public partial class MainWindowViewModel : ObservableObject
    {
        private readonly GetGamesUseCase _getGames;
        private readonly GetProfilesUseCase _getProfiles;
        private readonly GetSavesUseCase _getSaves;
        private readonly GetSaveFoldersUseCase _getSaveFolders;
        private readonly CreateSaveFolderUseCase _createSaveFolder;
        private readonly RenameSaveFolderUseCase _renameSaveFolder;
        private readonly DeleteSaveFolderUseCase _deleteSaveFolder;
        private readonly MoveSaveUseCase _moveSave;
        private readonly SortSavesUseCase _sortSaves;
        private readonly FilterSavesUseCase _filterSaves;
        private readonly CreateSaveUseCase _createSave;
        private readonly LoadSaveUseCase _loadSave;
        private readonly ReplaceSaveUseCase _replaceSave;
        private readonly DeleteSaveUseCase _deleteSave;
        private readonly AddGameUseCase _addGame;
        private readonly UpdateGameUseCase _updateGame;
        private readonly RemoveGameUseCase _removeGame;
        private readonly CreateProfileUseCase _createProfile;
        private readonly RemoveProfileUseCase _removeProfile;
        private readonly RenameProfileUseCase _renameProfile;
        private readonly RenameSaveUseCase _renameSave;
        private readonly GetSettingsUseCase _getSettings;
        private readonly SaveSettingsUseCase _saveSettings;

        private IStorageProvider? _storageProvider;
        private int _selectedGameLoadVersion;
        private int _selectedProfileLoadVersion;

        private HotKeyCoordinator? _hotKeys;

        public ToastViewModel Toast { get; } = new();

        private ObservableCollection<Game> _games = [];
        public ObservableCollection<Game> Games
        {
            get => _games;
            set => SetProperty(ref _games, value);
        }

        private ObservableCollection<Profile> _profiles = [];
        public ObservableCollection<Profile> Profiles
        {
            get => _profiles;
            set => SetProperty(ref _profiles, value);
        }

        private readonly List<Save> _allSaves = [];
        private readonly List<SaveFolder> _allFolders = [];
        private readonly HashSet<string> _expandedFolders = new(PathComparer);

        private static readonly StringComparer PathComparer =
            OperatingSystem.IsWindows() || OperatingSystem.IsMacOS()
                ? StringComparer.OrdinalIgnoreCase
                : StringComparer.Ordinal;

        private ObservableCollection<Save> _saves = [];
        public ObservableCollection<Save> Saves
        {
            get => _saves;
            set => SetProperty(ref _saves, value);
        }

        private ObservableCollection<SaveTreeNode> _saveTree = [];
        public ObservableCollection<SaveTreeNode> SaveTree
        {
            get => _saveTree;
            set => SetProperty(ref _saveTree, value);
        }

        private SaveSortOption _selectedSortOption = SaveSortOption.Created;
        public SaveSortOption SelectedSortOption
        {
            get => _selectedSortOption;
            set
            {
                if (SetProperty(ref _selectedSortOption, value))
                    RebuildSaves();
            }
        }

        public IReadOnlyList<SaveSortOption> SortOptions { get; } =
        [
            SaveSortOption.Created,
            SaveSortOption.AlphabetAscending,
            SaveSortOption.AlphabetDescending
        ];

        private string _saveSearchText = string.Empty;
        public string SaveSearchText
        {
            get => _saveSearchText;
            set
            {
                if (SetProperty(ref _saveSearchText, value))
                    RebuildSaves();
            }
        }

        public bool IsSaveSearchActive => !string.IsNullOrWhiteSpace(SaveSearchText);

        private void RebuildSaves()
        {
            CaptureExpansion(SaveTree, _expandedFolders);

            var selectedSavePath = SelectedSave?.SavePath;
            var selectedFolderPath = SelectedNode?.IsFolder == true
                ? SelectedNode.RelativePath
                : null;

            var term = SaveSearchText?.Trim() ?? string.Empty;
            var searchActive = term.Length > 0;

            bool NameMatches(string name) =>
                !searchActive || name.Contains(term, StringComparison.OrdinalIgnoreCase);

            bool PathMatches(string relativePath) =>
                !searchActive || Segments(relativePath).Any(NameMatches);

            var visibleSaves = _allSaves
                .Where(save => NameMatches(save.Name) || PathMatches(save.RelativeFolderPath))
                .ToList();

            var foldersWithVisibleSaves = new HashSet<string>(PathComparer);
            foreach (var save in visibleSaves)
                AddAncestors(save.RelativeFolderPath, foldersWithVisibleSaves);

            var visibleFolders = _allFolders
                .Where(folder => PathMatches(folder.RelativePath)
                                 || foldersWithVisibleSaves.Contains(folder.RelativePath))
                .ToList();

            var savesByFolder = visibleSaves
                .GroupBy(save => save.RelativeFolderPath, PathComparer)
                .ToDictionary(group => group.Key, group => group.ToList(), PathComparer);

            var nodesByPath = new Dictionary<string, SaveTreeNode>(PathComparer);

            foreach (var folder in visibleFolders.OrderBy(folder => folder.RelativePath, PathComparer))
            {
                nodesByPath[folder.RelativePath] = SaveTreeNode.ForFolder(
                    folder, _expandedFolders.Contains(folder.RelativePath));
            }

            var roots = new List<SaveTreeNode>();

            foreach (var folder in visibleFolders.OrderBy(folder => folder.RelativePath, PathComparer))
            {
                var node = nodesByPath[folder.RelativePath];
                var parentRelative = ParentOf(folder.RelativePath);

                if (parentRelative.Length == 0)
                    roots.Add(node);
                else if (nodesByPath.TryGetValue(parentRelative, out var parentNode))
                    parentNode.Children.Add(node);
            }

            foreach (var (relative, saves) in savesByFolder)
            {
                IList<SaveTreeNode>? target = relative.Length == 0
                    ? roots
                    : nodesByPath.TryGetValue(relative, out var folderNode)
                        ? folderNode.Children
                        : null;

                if (target is null)
                    continue;

                foreach (var save in _sortSaves.Execute(saves, SelectedSortOption))
                    target.Add(SaveTreeNode.ForSave(save));
            }

            var rootNodes = new ObservableCollection<SaveTreeNode>(roots);
            SortLevel(rootNodes);

            foreach (var node in nodesByPath.Values)
                SortLevel(node.Children);

            var flat = new List<Save>();
            Flatten(rootNodes, flat);

            Saves = new ObservableCollection<Save>(flat);
            SaveTree = rootNodes;

            if (selectedFolderPath is not null
                && nodesByPath.TryGetValue(selectedFolderPath, out var restoredFolder))
            {
                SelectedNode = restoredFolder;
            }
            else if (selectedSavePath is not null
                     && FindSaveNode(rootNodes, selectedSavePath) is { } restoredSave)
            {
                SelectedNode = restoredSave;
            }
            else
            {
                SelectedNode = null;
                SelectedSave = null;
            }
        }

        private void SetContents(IEnumerable<Save> saves, IEnumerable<SaveFolder> folders)
        {
            _allSaves.Clear();
            _allSaves.AddRange(saves);

            _allFolders.Clear();
            _allFolders.AddRange(folders);

            RebuildSaves();
        }

        private static void CaptureExpansion(
            IEnumerable<SaveTreeNode> nodes, HashSet<string> into)
        {
            foreach (var node in nodes)
            {
                if (node.IsFolder)
                {
                    if (node.IsExpanded)
                        into.Add(node.RelativePath);
                    else
                        into.Remove(node.RelativePath);
                }

                CaptureExpansion(node.Children, into);
            }
        }

        private static void AddAncestors(string relativePath, HashSet<string> into)
        {
            var current = relativePath;

            while (!string.IsNullOrEmpty(current))
            {
                into.Add(current);
                current = ParentOf(current);
            }
        }

        private static IEnumerable<string> Segments(string relativePath) =>
            string.IsNullOrEmpty(relativePath)
                ? Array.Empty<string>()
                : relativePath.Split(
                    [Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar],
                    StringSplitOptions.RemoveEmptyEntries);

        private static string ParentOf(string relativePath) =>
            string.IsNullOrEmpty(relativePath)
                ? string.Empty
                : Path.GetDirectoryName(relativePath) ?? string.Empty;

        private static void SortLevel(ObservableCollection<SaveTreeNode> nodes)
        {
            var folders = nodes
                .Where(node => node.IsFolder)
                .OrderBy(node => node.Name, StringComparer.OrdinalIgnoreCase)
                .ToList();

            var saves = nodes.Where(node => !node.IsFolder).ToList();

            nodes.Clear();

            foreach (var folder in folders)
                nodes.Add(folder);

            foreach (var save in saves)
                nodes.Add(save);
        }

        private static void Flatten(IEnumerable<SaveTreeNode> nodes, List<Save> into)
        {
            foreach (var node in nodes)
            {
                if (node.Save is { } save)
                    into.Add(save);

                Flatten(node.Children, into);
            }
        }

        private static SaveTreeNode? FindSaveNode(
            IEnumerable<SaveTreeNode> nodes, string savePath)
        {
            foreach (var node in nodes)
            {
                if (node.Save is { } save
                    && string.Equals(save.SavePath, savePath, PathComparison))
                {
                    return node;
                }

                if (FindSaveNode(node.Children, savePath) is { } found)
                    return found;
            }

            return null;
        }

        private static readonly StringComparison PathComparison =
            OperatingSystem.IsWindows() || OperatingSystem.IsMacOS()
                ? StringComparison.OrdinalIgnoreCase
                : StringComparison.Ordinal;

        private Game? _selectedGame;
        public Game? SelectedGame
        {
            get => _selectedGame;
            set
            {
                if (SetProperty(ref _selectedGame, value))
                {
                    OnPropertyChanged(nameof(IsGameSelected));
                    OnPropertyChanged(nameof(CanManageSaves));

                    Profiles = [];
                    _expandedFolders.Clear();
                    SetContents([], []);
                    SelectedProfile = null;
                    SelectedNode = null;

                    _ = RefreshProfiles();
                }
            }
        }

        private Profile? _selectedProfile;
        public Profile? SelectedProfile
        {
            get => _selectedProfile;
            set
            {
                if (SetProperty(ref _selectedProfile, value))
                {
                    OnPropertyChanged(nameof(IsProfileSelected));
                    OnPropertyChanged(nameof(CanManageSaves));

                    _expandedFolders.Clear();
                    SetContents([], []);
                    SelectedNode = null;

                    _ = RefreshSaves();
                }
            }
        }

        private SaveTreeNode? _selectedNode;
        public SaveTreeNode? SelectedNode
        {
            get => _selectedNode;
            set
            {
                if (!SetProperty(ref _selectedNode, value))
                    return;

                if (!ReferenceEquals(_selectedSave, value?.Save))
                {
                    _selectedSave = value?.Save;
                    OnPropertyChanged(nameof(SelectedSave));
                    OnPropertyChanged(nameof(IsSaveSelected));
                }

                OnPropertyChanged(nameof(SelectedFolder));
                OnPropertyChanged(nameof(IsFolderSelected));
            }
        }

        public SaveFolder? SelectedFolder => SelectedNode?.Folder;

        private Save? _selectedSave;
        public Save? SelectedSave
        {
            get => _selectedSave;
            set
            {
                if (!SetProperty(ref _selectedSave, value))
                    return;

                OnPropertyChanged(nameof(IsSaveSelected));

                var node = value is null ? null : FindSaveNode(SaveTree, value.SavePath);

                if (ReferenceEquals(_selectedNode, node))
                    return;

                _selectedNode = node;
                OnPropertyChanged(nameof(SelectedNode));
                OnPropertyChanged(nameof(SelectedFolder));
                OnPropertyChanged(nameof(IsFolderSelected));
            }
        }

        public bool IsGameSelected => SelectedGame != null;
        public bool IsProfileSelected => SelectedProfile != null;
        public bool CanManageSaves => SelectedGame != null && SelectedProfile != null;
        public bool IsSaveSelected => SelectedSave != null;
        public bool IsFolderSelected => SelectedNode?.IsFolder == true;

        public MainWindowViewModel()
        {
            _getGames = AppServiceProvider.GetService<GetGamesUseCase>();
            _getProfiles = AppServiceProvider.GetService<GetProfilesUseCase>();
            _getSaves = AppServiceProvider.GetService<GetSavesUseCase>();
            _getSaveFolders = AppServiceProvider.GetService<GetSaveFoldersUseCase>();
            _createSaveFolder = AppServiceProvider.GetService<CreateSaveFolderUseCase>();
            _renameSaveFolder = AppServiceProvider.GetService<RenameSaveFolderUseCase>();
            _deleteSaveFolder = AppServiceProvider.GetService<DeleteSaveFolderUseCase>();
            _moveSave = AppServiceProvider.GetService<MoveSaveUseCase>();
            _sortSaves = AppServiceProvider.GetService<SortSavesUseCase>();
            _filterSaves = AppServiceProvider.GetService<FilterSavesUseCase>();
            _createSave = AppServiceProvider.GetService<CreateSaveUseCase>();
            _loadSave = AppServiceProvider.GetService<LoadSaveUseCase>();
            _replaceSave = AppServiceProvider.GetService<ReplaceSaveUseCase>();
            _deleteSave = AppServiceProvider.GetService<DeleteSaveUseCase>();
            _addGame = AppServiceProvider.GetService<AddGameUseCase>();
            _updateGame = AppServiceProvider.GetService<UpdateGameUseCase>();
            _removeGame = AppServiceProvider.GetService<RemoveGameUseCase>();
            _createProfile = AppServiceProvider.GetService<CreateProfileUseCase>();
            _removeProfile = AppServiceProvider.GetService<RemoveProfileUseCase>();
            _renameProfile = AppServiceProvider.GetService<RenameProfileUseCase>();
            _renameSave = AppServiceProvider.GetService<RenameSaveUseCase>();
            _getSettings = AppServiceProvider.GetService<GetSettingsUseCase>();
            _saveSettings = AppServiceProvider.GetService<SaveSettingsUseCase>();

            LoadGames();
            LoadSettings();
        }

        public void SetStorageProvider(IStorageProvider storageProvider)
        {
            _storageProvider = storageProvider;
        }

        public void LoadGames()
        {
            Games = new ObservableCollection<Game>(_getGames.Execute());
            Profiles = [];
            SetContents([], []);
            SelectedGame = null;
            SelectedProfile = null;
            SelectedNode = null;

            _selectedGameLoadVersion++;
            _selectedProfileLoadVersion++;
        }
    }
}