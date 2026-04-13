using Avalonia.Controls;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using SaveManager.Application.UseCases.Game;
using SaveManager.Application.UseCases.Profile;
using SaveManager.Application.UseCases.Save;
using SaveManager.Domain.Entities;
using SaveManager.UI.DI;
using SaveManager.UI.ViewModels.Dialogs;
using SaveManager.UI.Views.Dialogs;
using System;
using System.Collections.ObjectModel;
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

        public ToastViewModel Toast { get; } = new();

        public ObservableCollection<Game> Games { get; } = [];
        public ObservableCollection<Profile> Profiles { get; } = [];
        public ObservableCollection<Save> Saves { get; } = [];

        public bool IsGameSelected => SelectedGame != null;
        public bool IsProfileSelected => SelectedProfile != null;
        public bool CanManageSaves => SelectedGame != null && SelectedProfile != null;
        public bool IsSaveSelected => SelectedSave != null;

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

                    var saves = _getSaves.Execute(value, SelectedGame);
                    foreach (var save in saves)
                        Saves.Add(save);
                }
            }
        }

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

        public void LoadGames()
        {
            Games.Clear();
            SelectedGame = null;

            var games = _getGames.Execute();
            foreach (var game in games)
                Games.Add(game);
        }

        private async Task OpenDialog(Window dialog)
        {
            var mainWindow = (App.Current?.ApplicationLifetime as
                Avalonia.Controls.ApplicationLifetimes.IClassicDesktopStyleApplicationLifetime)
                ?.MainWindow;

            if (mainWindow == null) return;

            await dialog.ShowDialog(mainWindow);
        }

        [RelayCommand]
        private async Task OpenAddGame()
        {
            var dialog = new AddGameDialog();
            await OpenDialog(dialog);

            var result = (dialog.DataContext as AddGameDialogViewModel)?.Result;
            if (result == null) return;

            _addGame.Execute(result.Value.Name, result.Value.SavePath, result.Value.BackupPath, result.Value.SaveType);
            LoadGames();
        }

        [RelayCommand]
        private async Task OpenAddProfile()
        {
            if (SelectedGame == null) return;

            var dialog = new AddProfileDialog();
            await OpenDialog(dialog);

            var result = (dialog.DataContext as AddProfileDialogViewModel)?.Result;
            if (result == null) return;

            _createProfile.Execute(SelectedGame.Id, result);

            var profiles = _getProfiles.Execute(SelectedGame);
            Profiles.Clear();
            foreach (var profile in profiles)
                Profiles.Add(profile);
        }

        [RelayCommand]
        private void OpenSettings()
        {
            // dialog será implementado depois
        }
    }
}