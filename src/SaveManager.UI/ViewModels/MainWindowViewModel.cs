using Avalonia.Platform.Storage;
using CommunityToolkit.Mvvm.ComponentModel;
using SaveManager.Application.UseCases.Game;
using SaveManager.Application.UseCases.Profile;
using SaveManager.Application.UseCases.Save;
using SaveManager.Domain.Entities;
using SaveManager.UI.DI;
using System.Collections.ObjectModel;

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
    }
}