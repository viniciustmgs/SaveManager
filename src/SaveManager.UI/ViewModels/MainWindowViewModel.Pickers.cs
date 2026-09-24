using CommunityToolkit.Mvvm.Input;
using SaveManager.Domain.Entities;
using System;
using System.Collections.ObjectModel;
using System.Threading.Tasks;

namespace SaveManager.UI.ViewModels
{
    public partial class MainWindowViewModel
    {
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
        private async Task OpenProfilePicker()
        {
            if (!IsGameSelected)
                return;

            PendingSelectedProfile = SelectedProfile;
            IsProfilePickerOpen = true;

            await RefreshProfiles();
        }

        [RelayCommand]
        private void CancelProfilePicker()
        {
            IsProfilePickerOpen = false;
            PendingSelectedProfile = null;
        }

        [RelayCommand]
        private async Task ConfirmProfilePicker()
        {
            if (PendingSelectedProfile == null)
                return;

            SelectedProfile = PendingSelectedProfile;
            IsProfilePickerOpen = false;
            PendingSelectedProfile = null;

            await RefreshSaves();
        }

        private async Task RefreshProfiles()
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
            }
            catch (Exception ex)
            {
                if (requestVersion != _selectedGameLoadVersion)
                    return;

                await Toast.Show($"Failed to load profiles: {ex.Message}", isSuccess: false);
            }
        }

        private async Task RefreshSaves()
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
            }
            catch (Exception ex)
            {
                if (requestVersion != _selectedProfileLoadVersion)
                    return;

                await Toast.Show($"Failed to load saves: {ex.Message}", isSuccess: false);
            }
        }
    }
}