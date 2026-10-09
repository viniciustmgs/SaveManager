using CommunityToolkit.Mvvm.Input;
using SaveManager.Domain.Entities;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace SaveManager.UI.ViewModels
{
    public partial class MainWindowViewModel
    {
        // --- profiles list overlay ---

        private bool _isEditProfilesDialogOpen;
        public bool IsEditProfilesDialogOpen
        {
            get => _isEditProfilesDialogOpen;
            set
            {
                if (SetProperty(ref _isEditProfilesDialogOpen, value))
                    NotifyOverlayStateChanged();
            }
        }

        private Profile? _editProfilesSelectedProfile;
        public Profile? EditProfilesSelectedProfile
        {
            get => _editProfilesSelectedProfile;
            set
            {
                if (SetProperty(ref _editProfilesSelectedProfile, value))
                    OnPropertyChanged(nameof(CanEditOrDeleteSelectedProfile));
            }
        }

        public bool CanEditOrDeleteSelectedProfile => EditProfilesSelectedProfile != null;

        [RelayCommand]
        private async Task OpenEditProfiles()
        {
            if (!IsGameSelected)
                return;

            EditProfilesSelectedProfile = null;
            IsEditProfilesDialogOpen = true;

            await RefreshProfiles();
        }

        [RelayCommand]
        private void CloseEditProfiles()
        {
            IsEditProfilesDialogOpen = false;
            EditProfilesSelectedProfile = null;
        }

        // --- edit profile form (nested overlay) ---

        private Profile? _editProfileTarget;

        private bool _isEditProfileFormOpen;
        public bool IsEditProfileFormOpen
        {
            get => _isEditProfileFormOpen;
            set
            {
                if (SetProperty(ref _isEditProfileFormOpen, value))
                    NotifyOverlayStateChanged();
            }
        }

        private string _editProfileName = string.Empty;
        public string EditProfileName
        {
            get => _editProfileName;
            set
            {
                if (SetProperty(ref _editProfileName, value))
                    OnPropertyChanged(nameof(CanConfirmEditProfile));
            }
        }

        private string _editProfileErrorMessage = string.Empty;
        public string EditProfileErrorMessage
        {
            get => _editProfileErrorMessage;
            set
            {
                if (SetProperty(ref _editProfileErrorMessage, value))
                    OnPropertyChanged(nameof(HasEditProfileError));
            }
        }

        public bool HasEditProfileError => !string.IsNullOrEmpty(EditProfileErrorMessage);

        public bool CanConfirmEditProfile => !string.IsNullOrWhiteSpace(EditProfileName);

        [RelayCommand]
        private void EditSelectedProfile()
        {
            if (EditProfilesSelectedProfile == null)
                return;

            _editProfileTarget = EditProfilesSelectedProfile;
            EditProfileName = _editProfileTarget.Name;
            EditProfileErrorMessage = string.Empty;
            IsEditProfileFormOpen = true;
        }

        [RelayCommand]
        private void CancelEditProfile()
        {
            IsEditProfileFormOpen = false;
            _editProfileTarget = null;
            EditProfileErrorMessage = string.Empty;
        }

        [RelayCommand]
        private async Task ConfirmEditProfile()
        {
            if (!CanConfirmEditProfile || _editProfileTarget == null || SelectedGame == null)
                return;

            try
            {
                var oldName = _editProfileTarget.Name;
                var wasSelectedProfile = SelectedProfile?.Name == oldName;

                var updatedProfile = _renameProfile.Execute(SelectedGame.Id, oldName, EditProfileName);

                await RefreshProfiles();

                EditProfilesSelectedProfile = Profiles.FirstOrDefault(p => p.Name == updatedProfile.Name);

                if (wasSelectedProfile)
                    SelectedProfile = EditProfilesSelectedProfile;

                IsEditProfileFormOpen = false;
                _editProfileTarget = null;

                await Toast.Show("Profile updated successfully!", isSuccess: true);
            }
            catch (Exception ex)
            {
                EditProfileErrorMessage = ex.Message;
            }
        }

        // --- delete profile confirmation (nested overlay) ---

        private Profile? _deleteProfileTarget;

        private bool _isDeleteProfileConfirmOpen;
        public bool IsDeleteProfileConfirmOpen
        {
            get => _isDeleteProfileConfirmOpen;
            set
            {
                if (SetProperty(ref _isDeleteProfileConfirmOpen, value))
                    NotifyOverlayStateChanged();
            }
        }

        [RelayCommand]
        private void DeleteSelectedProfile()
        {
            if (EditProfilesSelectedProfile == null)
                return;

            _deleteProfileTarget = EditProfilesSelectedProfile;
            IsDeleteProfileConfirmOpen = true;
        }

        [RelayCommand]
        private void CancelDeleteProfile()
        {
            IsDeleteProfileConfirmOpen = false;
            _deleteProfileTarget = null;
        }

        [RelayCommand]
        private async Task ConfirmDeleteProfile()
        {
            if (_deleteProfileTarget == null || SelectedGame == null)
                return;

            try
            {
                var targetName = _deleteProfileTarget.Name;
                var wasSelectedProfile = SelectedProfile?.Name == targetName;

                _removeProfile.Execute(SelectedGame.Id, targetName);

                await RefreshProfiles();

                if (wasSelectedProfile)
                    SelectedProfile = null;

                EditProfilesSelectedProfile = null;
                IsDeleteProfileConfirmOpen = false;
                _deleteProfileTarget = null;

                await Toast.Show("Profile deleted successfully!", isSuccess: true);
            }
            catch (Exception ex)
            {
                IsDeleteProfileConfirmOpen = false;
                _deleteProfileTarget = null;

                await Toast.Show($"Failed to delete profile: {ex.Message}", isSuccess: false);
            }
        }
    }
}