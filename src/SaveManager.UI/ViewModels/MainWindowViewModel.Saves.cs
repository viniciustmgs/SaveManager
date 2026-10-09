using CommunityToolkit.Mvvm.Input;
using SaveManager.Domain.Entities;
using System;
using System.Threading.Tasks;

namespace SaveManager.UI.ViewModels
{
    public partial class MainWindowViewModel
    {
        [RelayCommand]
        private async Task CreateSave()
        {
            if (SelectedGame == null || SelectedProfile == null) return;

            try
            {
                var save = _createSave.Execute(SelectedProfile, SelectedGame);
                _allSaves.Add(save);
                RebuildSaves();
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

        private Save? _deleteSaveTarget;

        private bool _isDeleteSaveConfirmOpen;
        public bool IsDeleteSaveConfirmOpen
        {
            get => _isDeleteSaveConfirmOpen;
            set
            {
                if (SetProperty(ref _isDeleteSaveConfirmOpen, value))
                    NotifyOverlayStateChanged();
            }
        }

        public string DeleteSaveConfirmMessage =>
            _deleteSaveTarget is null
                ? string.Empty
                : $"{_deleteSaveTarget.Name} is going to be deleted, do you want to proceed?";

        [RelayCommand]
        private void DeleteSelectedSave()
        {
            if (SelectedSave is null)
                return;

            OpenDeleteSaveConfirm(SelectedSave);
        }

        public void RequestDeleteSave(Save save) => OpenDeleteSaveConfirm(save);

        private void OpenDeleteSaveConfirm(Save save)
        {
            _deleteSaveTarget = save;

            OnPropertyChanged(nameof(DeleteSaveConfirmMessage));
            IsDeleteSaveConfirmOpen = true;
        }

        [RelayCommand]
        private void CancelDeleteSave()
        {
            IsDeleteSaveConfirmOpen = false;
            _deleteSaveTarget = null;
        }

        [RelayCommand]
        private async Task ConfirmDeleteSave()
        {
            if (_deleteSaveTarget is null)
                return;

            var doomed = _deleteSaveTarget;

            IsDeleteSaveConfirmOpen = false;
            _deleteSaveTarget = null;

            try
            {
                _deleteSave.Execute(doomed);
                _allSaves.Remove(doomed);
                RebuildSaves();

                SelectedSave = null;

                await Toast.Show("Save deleted successfully!", isSuccess: true);
            }
            catch (Exception ex)
            {
                await Toast.Show($"Failed to delete save: {ex.Message}", isSuccess: false);
            }
        }
    }
}