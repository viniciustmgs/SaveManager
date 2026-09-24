using CommunityToolkit.Mvvm.Input;
using System;
using System.Threading.Tasks;

namespace SaveManager.UI.ViewModels
{
    public partial class MainWindowViewModel
    {
        private bool _isAddProfileDialogOpen;
        public bool IsAddProfileDialogOpen
        {
            get => _isAddProfileDialogOpen;
            set => SetProperty(ref _isAddProfileDialogOpen, value);
        }

        private string _newProfileName = string.Empty;
        public string NewProfileName
        {
            get => _newProfileName;
            set
            {
                if (SetProperty(ref _newProfileName, value))
                    OnPropertyChanged(nameof(CanConfirmAddProfile));
            }
        }

        private string _addProfileErrorMessage = string.Empty;
        public string AddProfileErrorMessage
        {
            get => _addProfileErrorMessage;
            set
            {
                if (SetProperty(ref _addProfileErrorMessage, value))
                    OnPropertyChanged(nameof(HasAddProfileError));
            }
        }

        public bool HasAddProfileError => !string.IsNullOrEmpty(AddProfileErrorMessage);

        public bool CanConfirmAddProfile => !string.IsNullOrWhiteSpace(NewProfileName);

        private void ResetAddProfileForm()
        {
            NewProfileName = string.Empty;
            AddProfileErrorMessage = string.Empty;
        }

        [RelayCommand]
        private void OpenAddProfile()
        {
            if (!IsGameSelected)
                return;

            ResetAddProfileForm();
            IsAddProfileDialogOpen = true;
        }

        [RelayCommand]
        private void CancelAddProfile()
        {
            IsAddProfileDialogOpen = false;
            ResetAddProfileForm();
        }

        [RelayCommand]
        private async Task ConfirmAddProfile()
        {
            if (!CanConfirmAddProfile || SelectedGame == null)
                return;

            try
            {
                _createProfile.Execute(SelectedGame.Id, NewProfileName);

                IsAddProfileDialogOpen = false;
                ResetAddProfileForm();

                await Toast.Show("Profile added successfully!", isSuccess: true);
            }
            catch (Exception ex)
            {
                AddProfileErrorMessage = ex.Message;
            }
        }
    }
}