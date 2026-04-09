using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using SaveManager.Application.UseCases.Game;
using SaveManager.Application.UseCases.Profile;
using SaveManager.Application.UseCases.Save;
using SaveManager.Domain.Entities;
using SaveManager.UI.DI;
using System;
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

        public ObservableCollection<Game> Games { get; } = [];
        public ObservableCollection<Profile> Profiles { get; } = [];
        public ObservableCollection<Save> Saves { get; } = [];

        public bool IsGameSelected => SelectedGame != null;
        public bool IsProfileSelected => SelectedProfile != null;
        public bool CanManageSaves => SelectedGame != null && SelectedProfile != null;

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
                    Profiles.Clear();
                    Saves.Clear();
                    SelectedProfile = null;
                    SelectedSave = null;

                    if (value == null) return;

                    var profiles = _getProfiles.Execute(value);
                    foreach (var profile in profiles)
                        Profiles.Add(profile);
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
                    Saves.Clear();
                    SelectedSave = null;

                    if (value == null || SelectedGame == null) return;

                    var saves = _getSaves.Execute(value);
                    foreach (var save in saves)
                        Saves.Add(save);
                }
            }
        }

        private Save? _selectedSave;
        public Save? SelectedSave
        {
            get => _selectedSave;
            set => SetProperty(ref _selectedSave, value);
        }

        public MainWindowViewModel()
        {
            _getGames = AppServiceProvider.GetService<GetGamesUseCase>();
            _getProfiles = AppServiceProvider.GetService<GetProfilesUseCase>();
            _getSaves = AppServiceProvider.GetService<GetSavesUseCase>();
            _createSave = AppServiceProvider.GetService<CreateSaveUseCase>();
            _loadSave = AppServiceProvider.GetService<LoadSaveUseCase>();
            _replaceSave = AppServiceProvider.GetService<ReplaceSaveUseCase>();
            _deleteSave = AppServiceProvider.GetService<DeleteSaveUseCase>();

            LoadGames();
        }

        [RelayCommand]
        private void CreateSave()
        {
            if (SelectedGame == null || SelectedProfile == null) return;

            var save = _createSave.Execute(SelectedProfile, SelectedGame);
            Saves.Add(save);
        }

        [RelayCommand]
        private void LoadSave()
        {
            if (SelectedGame == null || SelectedSave == null) return;

            _loadSave.Execute(SelectedGame, SelectedSave);
        }

        [RelayCommand]
        private void ReplaceSave()
        {
            if (SelectedGame == null || SelectedSave == null) return;

            _replaceSave.Execute(SelectedGame, SelectedSave);
        }

        [RelayCommand]
        private void DeleteSave()
        {
            if (SelectedSave == null) return;

            _deleteSave.Execute(SelectedSave);
            Saves.Remove(SelectedSave);
            SelectedSave = null;
        }

        public void LoadGames()
        {
            Games.Clear();
            SelectedGame = null;

            var games = _getGames.Execute();
            foreach (var game in games)
                Games.Add(game);
        }

        [RelayCommand]
        private void OpenAddGame()
        {
            // dialog será implementado depois
        }

        [RelayCommand]
        private void OpenAddProfile()
        {
            // dialog será implementado depois
        }

        [RelayCommand]
        private void OpenSettings()
        {
            // dialog será implementado depois
        }
    }
}