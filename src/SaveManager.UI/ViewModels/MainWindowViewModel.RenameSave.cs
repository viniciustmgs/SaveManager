using CommunityToolkit.Mvvm.Input;
using SaveManager.Domain.Entities;
using System;
using System.Threading.Tasks;

namespace SaveManager.UI.ViewModels
{
    public partial class MainWindowViewModel
    {
        private Save? _renameSaveTarget;

        private bool _isRenameSaveDialogOpen;
        public bool IsRenameSaveDialogOpen
        {
            get => _isRenameSaveDialogOpen;
            set => SetProperty(ref _isRenameSaveDialogOpen, value);
        }

        private string _renameSaveName = string.Empty;
        public string RenameSaveName
        {
            get => _renameSaveName;
            set
            {
                if (SetProperty(ref _renameSaveName, value))
                    OnPropertyChanged(nameof(CanConfirmRenameSave));
            }
        }

        private string _renameSaveErrorMessage = string.Empty;
        public string RenameSaveErrorMessage
        {
            get => _renameSaveErrorMessage;
            set
            {
                if (SetProperty(ref _renameSaveErrorMessage, value))
                    OnPropertyChanged(nameof(HasRenameSaveError));
            }
        }

        public bool HasRenameSaveError => !string.IsNullOrEmpty(RenameSaveErrorMessage);

        public bool CanConfirmRenameSave => !string.IsNullOrWhiteSpace(RenameSaveName);

        [RelayCommand]
        private void OpenRenameSave(Save save)
        {
            _renameSaveTarget = save;
            RenameSaveName = save.Name;
            RenameSaveErrorMessage = string.Empty;
            IsRenameSaveDialogOpen = true;
        }

        [RelayCommand]
        private void CancelRenameSave()
        {
            IsRenameSaveDialogOpen = false;
            _renameSaveTarget = null;
            RenameSaveName = string.Empty;
            RenameSaveErrorMessage = string.Empty;
        }

        [RelayCommand]
        private async Task ConfirmRenameSave()
        {
            if (!CanConfirmRenameSave || _renameSaveTarget == null || SelectedGame == null)
                return;

            try
            {
                var target = _renameSaveTarget;
                var updatedSave = _renameSave.Execute(SelectedGame, target, RenameSaveName.Trim());

                var index = Saves.IndexOf(target);
                if (index >= 0)
                    Saves[index] = updatedSave;

                if (SelectedSave == target)
                    SelectedSave = updatedSave;

                IsRenameSaveDialogOpen = false;
                _renameSaveTarget = null;
                RenameSaveName = string.Empty;

                await Toast.Show("Save renamed successfully!", isSuccess: true);
            }
            catch (Exception ex)
            {
                RenameSaveErrorMessage = ex.Message;
            }
        }
    }
}