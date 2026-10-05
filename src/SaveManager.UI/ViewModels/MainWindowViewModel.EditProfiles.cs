using CommunityToolkit.Mvvm.Input;
using SaveManager.Domain.Entities;
using System.Threading.Tasks;

namespace SaveManager.UI.ViewModels
{
    public partial class MainWindowViewModel
    {
        private bool _isEditProfilesDialogOpen;
        public bool IsEditProfilesDialogOpen
        {
            get => _isEditProfilesDialogOpen;
            set => SetProperty(ref _isEditProfilesDialogOpen, value);
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

        [RelayCommand]
        private void EditSelectedProfile()
        {
            // implementar depois
        }

        [RelayCommand]
        private void DeleteSelectedProfile()
        {
            // implementar depois
        }
    }
}