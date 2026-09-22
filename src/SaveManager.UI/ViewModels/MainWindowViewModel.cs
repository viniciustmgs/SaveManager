using Avalonia.Platform.Storage;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using SaveManager.Application.UseCases.Game;
using SaveManager.Application.UseCases.Profile;
using SaveManager.Application.UseCases.Save;
using SaveManager.Domain.Entities;
using SaveManager.Domain.Enums;
using SaveManager.UI.DI;
using System;
using System.Collections.ObjectModel;
using System.IO;
using System.Threading.Tasks;

namespace SaveManager.UI.ViewModels
{
    public partial class MainWindowViewModel : ObservableObject
    {
        private readonly GetGamesUseCase _getGames;
        private readonly GetProfilesUseCase _getProfiles;
        private readonly GetSavesUseCase _getSaves;
        private readonly CreateSaveUseCase _createSave;
        private readonly LoadSaveUseCase _loadSave;
        private readonly ReplaceSaveUseCase _replaceSave;
        private readonly DeleteSaveUseCase _deleteSave;
        private readonly AddGameUseCase _addGame;
        private readonly CreateProfileUseCase _createProfile;

        private IStorageProvider? _storageProvider;
        private int _selectedGameLoadVersion;
        private int _selectedProfileLoadVersion;

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

        private ObservableCollection<Save> _saves = [];
        public ObservableCollection<Save> Saves
        {
            get => _saves;
            set => SetProperty(ref _saves, value);
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
                    Saves = [];
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

                    Saves = [];
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

        private bool _isAddGameDialogOpen;
        public bool IsAddGameDialogOpen
        {
            get => _isAddGameDialogOpen;
            set => SetProperty(ref _isAddGameDialogOpen, value);
        }

        private bool _isGamePickerOpen;
        public bool IsGamePickerOpen
        {
            get => _isGamePickerOpen;
            set => SetProperty(ref _isGamePickerOpen, value);
        }

        private Game? _pendingSelectedGame;
        public Game? PendingSelectedGame
        {
            get => _pendingSelectedGame;
            set
            {
                if (SetProperty(ref _pendingSelectedGame, value))
                    OnPropertyChanged(nameof(CanConfirmGamePicker));
            }
        }

        public bool CanConfirmGamePicker => PendingSelectedGame != null;

        private bool _isProfilePickerOpen;
        public bool IsProfilePickerOpen
        {
            get => _isProfilePickerOpen;
            set => SetProperty(ref _isProfilePickerOpen, value);
        }

        private Profile? _pendingSelectedProfile;
        public Profile? PendingSelectedProfile
        {
            get => _pendingSelectedProfile;
            set
            {
                if (SetProperty(ref _pendingSelectedProfile, value))
                    OnPropertyChanged(nameof(CanConfirmProfilePicker));
            }
        }

        public bool CanConfirmProfilePicker => PendingSelectedProfile != null;

        private string _newGameName = string.Empty;
        public string NewGameName
        {
            get => _newGameName;
            set
            {
                if (SetProperty(ref _newGameName, value))
                    OnPropertyChanged(nameof(CanConfirmAddGame));
            }
        }

        private bool _newGameIsSingleFile;
        public bool NewGameIsSingleFile
        {
            get => _newGameIsSingleFile;
            set
            {
                if (SetProperty(ref _newGameIsSingleFile, value))
                {
                    if (value) NewGameIsFolder = false;
                    NewGameSavePath = string.Empty;
                    OnPropertyChanged(nameof(IsNewGameSaveTypeSelected));
                    OnPropertyChanged(nameof(CanConfirmAddGame));
                }
            }
        }

        private bool _newGameIsFolder;
        public bool NewGameIsFolder
        {
            get => _newGameIsFolder;
            set
            {
                if (SetProperty(ref _newGameIsFolder, value))
                {
                    if (value) NewGameIsSingleFile = false;
                    NewGameSavePath = string.Empty;
                    OnPropertyChanged(nameof(IsNewGameSaveTypeSelected));
                    OnPropertyChanged(nameof(CanConfirmAddGame));
                }
            }
        }

        private string _newGameSavePath = string.Empty;
        public string NewGameSavePath
        {
            get => _newGameSavePath;
            set
            {
                if (SetProperty(ref _newGameSavePath, value))
                    OnPropertyChanged(nameof(CanConfirmAddGame));
            }
        }

        private string _newGameBackupPath = string.Empty;
        public string NewGameBackupPath
        {
            get => _newGameBackupPath;
            set
            {
                if (SetProperty(ref _newGameBackupPath, value))
                    OnPropertyChanged(nameof(CanConfirmAddGame));
            }
        }

        private string _addGameErrorMessage = string.Empty;
        public string AddGameErrorMessage
        {
            get => _addGameErrorMessage;
            set
            {
                if (SetProperty(ref _addGameErrorMessage, value))
                    OnPropertyChanged(nameof(HasAddGameError));
            }
        }

        public bool IsNewGameSaveTypeSelected => NewGameIsSingleFile || NewGameIsFolder;
        public bool HasAddGameError => !string.IsNullOrEmpty(AddGameErrorMessage);

        public bool CanConfirmAddGame =>
            !string.IsNullOrWhiteSpace(NewGameName) &&
            IsNewGameSaveTypeSelected &&
            !string.IsNullOrWhiteSpace(NewGameSavePath) &&
            !string.IsNullOrWhiteSpace(NewGameBackupPath);

        public MainWindowViewModel()
        {
            _getGames = AppServiceProvider.GetService<GetGamesUseCase>();
            _getProfiles = AppServiceProvider.GetService<GetProfilesUseCase>();
            _getSaves = AppServiceProvider.GetService<GetSavesUseCase>();
            _createSave = AppServiceProvider.GetService<CreateSaveUseCase>();
            _loadSave = AppServiceProvider.GetService<LoadSaveUseCase>();
            _replaceSave = AppServiceProvider.GetService<ReplaceSaveUseCase>();
            _deleteSave = AppServiceProvider.GetService<DeleteSaveUseCase>();
            _addGame = AppServiceProvider.GetService<AddGameUseCase>();
            _createProfile = AppServiceProvider.GetService<CreateProfileUseCase>();

            LoadGames();
        }

        public void SetStorageProvider(IStorageProvider storageProvider)
        {
            _storageProvider = storageProvider;
        }

        public void LoadGames()
        {
            Games = new ObservableCollection<Game>(_getGames.Execute());
            Profiles = [];
            Saves = [];
            SelectedGame = null;
            SelectedProfile = null;
            SelectedSave = null;
            PendingSelectedGame = null;
            PendingSelectedProfile = null;

            _selectedGameLoadVersion++;
            _selectedProfileLoadVersion++;
        }

        [RelayCommand]
        private void OpenGamePicker()
        {
            PendingSelectedGame = SelectedGame;
            IsGamePickerOpen = true;
        }

        [RelayCommand]
        private void CancelGamePicker()
        {
            IsGamePickerOpen = false;
            PendingSelectedGame = null;
        }

        [RelayCommand]
        private void ConfirmGamePicker()
        {
            if (PendingSelectedGame == null)
                return;

            SelectedGame = PendingSelectedGame;
            IsGamePickerOpen = false;
            PendingSelectedGame = null;
        }

        [RelayCommand]
        private void OpenProfilePicker()
        {
            if (!IsGameSelected)
                return;

            PendingSelectedProfile = SelectedProfile;
            IsProfilePickerOpen = true;
        }

        [RelayCommand]
        private void CancelProfilePicker()
        {
            IsProfilePickerOpen = false;
            PendingSelectedProfile = null;
        }

        [RelayCommand]
        private void ConfirmProfilePicker()
        {
            if (PendingSelectedProfile == null)
                return;

            SelectedProfile = PendingSelectedProfile;
            IsProfilePickerOpen = false;
            PendingSelectedProfile = null;
        }

        [RelayCommand]
        private async Task LoadProfiles()
        {
            if (SelectedGame == null)
                return;

            var game = SelectedGame;
            var requestVersion = ++_selectedGameLoadVersion;

            try
            {
                var profiles = await Task.Run(() => _getProfiles.Execute(game));

                if (requestVersion != _selectedGameLoadVersion)
                    return;

                if (SelectedGame?.Id != game.Id)
                    return;

                Profiles = new ObservableCollection<Profile>(profiles);
                SelectedProfile = null;
                Saves = [];
                SelectedSave = null;
            }
            catch (Exception ex)
            {
                if (requestVersion != _selectedGameLoadVersion)
                    return;

                await Toast.Show($"Failed to load profiles: {ex.Message}", isSuccess: false);
            }
        }

        [RelayCommand]
        private async Task LoadSaves()
        {
            if (SelectedGame == null || SelectedProfile == null)
                return;

            var game = SelectedGame;
            var profile = SelectedProfile;
            var requestVersion = ++_selectedProfileLoadVersion;

            try
            {
                var saves = await Task.Run(() => _getSaves.Execute(profile, game));

                if (requestVersion != _selectedProfileLoadVersion)
                    return;

                if (SelectedGame?.Id != game.Id)
                    return;

                if (SelectedProfile?.FolderPath != profile.FolderPath)
                    return;

                Saves = new ObservableCollection<Save>(saves);
                SelectedSave = null;
            }
            catch (Exception ex)
            {
                if (requestVersion != _selectedProfileLoadVersion)
                    return;

                await Toast.Show($"Failed to load saves: {ex.Message}", isSuccess: false);
            }
        }

        [RelayCommand]
        private async Task CreateSave()
        {
            if (SelectedGame == null || SelectedProfile == null) return;

            try
            {
                var save = _createSave.Execute(SelectedProfile, SelectedGame);
                Saves.Add(save);
                await Toast.Show("Save created successfully!", isSuccess: true);
            }
            catch (Exception ex)
            {
                await Toast.Show($"Failed to create save: {ex.Message}", isSuccess: false);
            }
        }

        [RelayCommand]
        private async Task LoadSave()
        {
            if (SelectedGame == null || SelectedSave == null) return;

            try
            {
                _loadSave.Execute(SelectedGame, SelectedSave);
                await Toast.Show("Save loaded successfully!", isSuccess: true);
            }
            catch (Exception ex)
            {
                await Toast.Show($"Failed to load save: {ex.Message}", isSuccess: false);
            }
        }

        [RelayCommand]
        private async Task ReplaceSave()
        {
            if (SelectedGame == null || SelectedSave == null) return;

            try
            {
                _replaceSave.Execute(SelectedGame, SelectedSave);
                await Toast.Show("Save replaced successfully!", isSuccess: true);
            }
            catch (Exception ex)
            {
                await Toast.Show($"Failed to replace save: {ex.Message}", isSuccess: false);
            }
        }

        [RelayCommand]
        private void DeleteSave()
        {
            if (SelectedSave == null) return;

            _deleteSave.Execute(SelectedSave);
            Saves.Remove(SelectedSave);
            SelectedSave = null;
        }

        private void ResetAddGameForm()
        {
            NewGameName = string.Empty;
            NewGameIsSingleFile = false;
            NewGameIsFolder = false;
            NewGameSavePath = string.Empty;
            NewGameBackupPath = string.Empty;
            AddGameErrorMessage = string.Empty;
        }

        [RelayCommand]
        private void OpenAddGame()
        {
            ResetAddGameForm();
            IsAddGameDialogOpen = true;
        }

        [RelayCommand]
        private void CancelAddGame()
        {
            IsAddGameDialogOpen = false;
            ResetAddGameForm();
        }

        [RelayCommand]
        private async Task ConfirmAddGame()
        {
            if (!CanConfirmAddGame) return;

            try
            {
                _addGame.Execute(
                    NewGameName,
                    NewGameSavePath,
                    NewGameBackupPath,
                    NewGameIsSingleFile ? SaveType.SingleFile : SaveType.Folder
                );

                LoadGames();
                IsAddGameDialogOpen = false;
                ResetAddGameForm();

                await Toast.Show("Game added successfully!", isSuccess: true);
            }
            catch (Exception ex)
            {
                AddGameErrorMessage = ex.Message;
            }
        }

        [RelayCommand]
        private async Task BrowseNewGameSavePath()
        {
            if (_storageProvider == null) return;

            AddGameErrorMessage = string.Empty;

            if (NewGameIsSingleFile)
            {
                var files = await _storageProvider.OpenFilePickerAsync(new FilePickerOpenOptions
                {
                    Title = "Select Save File",
                    AllowMultiple = false
                });

                if (files.Count == 0) return;

                var path = files[0].Path.LocalPath;

                if (Directory.Exists(path))
                {
                    AddGameErrorMessage = "Save type is Single File, but a folder was selected. Please select a file.";
                    return;
                }

                NewGameSavePath = path;
            }
            else
            {
                var folders = await _storageProvider.OpenFolderPickerAsync(new FolderPickerOpenOptions
                {
                    Title = "Select Save Folder",
                    AllowMultiple = false
                });

                if (folders.Count == 0) return;

                var path = folders[0].Path.LocalPath;

                if (File.Exists(path))
                {
                    AddGameErrorMessage = "Save type is Folder, but a file was selected. Please select a folder.";
                    return;
                }

                NewGameSavePath = path;
            }
        }

        [RelayCommand]
        private async Task BrowseNewGameBackupPath()
        {
            if (_storageProvider == null) return;

            var folders = await _storageProvider.OpenFolderPickerAsync(new FolderPickerOpenOptions
            {
                Title = "Select Backup Folder",
                AllowMultiple = false
            });

            if (folders.Count == 0) return;

            NewGameBackupPath = folders[0].Path.LocalPath;
        }

        [RelayCommand]
        private void OpenAddProfile()
        {
            // vamos ajustar depois
        }

        [RelayCommand]
        private void OpenSettings()
        {
            // dialog será implementado depois
        }
    }
}