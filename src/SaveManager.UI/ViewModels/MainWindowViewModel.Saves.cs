using CommunityToolkit.Mvvm.Input;
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
    }
}