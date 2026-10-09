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
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace SaveManager.UI.ViewModels
{
    public partial class MainWindowViewModel : ObservableObject
    {
        private readonly GetGamesUseCase _getGames;
        private readonly GetProfilesUseCase _getProfiles;
        private readonly GetSavesUseCase _getSaves;
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

        private ObservableCollection<Save> _saves = [];
        public ObservableCollection<Save> Saves
        {
            get => _saves;
            set => SetProperty(ref _saves, value);
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
            var selected = SelectedSave;

            var visible = _sortSaves.Execute(
                _filterSaves.Execute(_allSaves, SaveSearchText),
                SelectedSortOption);

            Saves.Clear();

            foreach (var save in visible)
                Saves.Add(save);

            if (selected is not null && visible.Contains(selected))
                SelectedSave = selected;
            else if (selected is not null)
                SelectedSave = null;
        }

        private void SetAllSaves(IEnumerable<Save> saves)
        {
            _allSaves.Clear();
            _allSaves.AddRange(saves);

            RebuildSaves();
        }

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
                    OnPropertyChanged(nameof(SelectedGameDisplayText));

                    Profiles = [];
                    SetAllSaves([]);
                    SelectedProfile = null;
                    SelectedSave = null;
                }
            }
        }

        public string SelectedGameDisplayText =>
            SelectedGame?.Name ?? "Select game";

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
                    OnPropertyChanged(nameof(SelectedProfileDisplayText));

                    SetAllSaves([]);
                    SelectedSave = null;
                }
            }
        }

        public string SelectedProfileDisplayText =>
            SelectedProfile?.Name ?? "Select profile";

        private Save? _selectedSave;
        public Save? SelectedSave
        {
            get => _selectedSave;
            set
            {
                if (SetProperty(ref _selectedSave, value))
                    OnPropertyChanged(nameof(IsSaveSelected));
            }
        }

        public bool IsGameSelected => SelectedGame != null;
        public bool IsProfileSelected => SelectedProfile != null;
        public bool CanManageSaves => SelectedGame != null && SelectedProfile != null;
        public bool IsSaveSelected => SelectedSave != null;

        public MainWindowViewModel()
        {
            _getGames = AppServiceProvider.GetService<GetGamesUseCase>();
            _getProfiles = AppServiceProvider.GetService<GetProfilesUseCase>();
            _getSaves = AppServiceProvider.GetService<GetSavesUseCase>();
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
            SetAllSaves([]);
            SelectedGame = null;
            SelectedProfile = null;
            SelectedSave = null;
            PendingSelectedGame = null;
            PendingSelectedProfile = null;

            _selectedGameLoadVersion++;
            _selectedProfileLoadVersion++;
        }
    }
}