using Avalonia.Platform.Storage;
using CommunityToolkit.Mvvm.Input;
using SaveManager.Application.Common;
using SaveManager.Domain.Entities;
using SaveManager.Domain.Enums;
using System;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace SaveManager.UI.ViewModels
{
    public partial class MainWindowViewModel
    {
        // --- games list overlay ---

        private bool _isEditGamesDialogOpen;
        public bool IsEditGamesDialogOpen
        {
            get => _isEditGamesDialogOpen;
            set
            {
                if (SetProperty(ref _isEditGamesDialogOpen, value))
                    NotifyOverlayStateChanged();
            }
        }

        private Game? _editGamesSelectedGame;
        public Game? EditGamesSelectedGame
        {
            get => _editGamesSelectedGame;
            set
            {
                if (SetProperty(ref _editGamesSelectedGame, value))
                    OnPropertyChanged(nameof(CanEditOrDeleteSelectedGame));
            }
        }

        public bool CanEditOrDeleteSelectedGame => EditGamesSelectedGame != null;

        [RelayCommand]
        private void OpenEditGames()
        {
            EditGamesSelectedGame = null;
            IsEditGamesDialogOpen = true;
        }

        [RelayCommand]
        private void CloseEditGames()
        {
            IsEditGamesDialogOpen = false;
            EditGamesSelectedGame = null;
        }

        private void RefreshGamesList()
        {
            Games = new ObservableCollection<Game>(_getGames.Execute());
        }

        // --- edit game form (nested overlay) ---

        private Game? _editGameTarget;

        private bool _isEditGameFormOpen;
        public bool IsEditGameFormOpen
        {
            get => _isEditGameFormOpen;
            set
            {
                if (SetProperty(ref _isEditGameFormOpen, value))
                    NotifyOverlayStateChanged();
            }
        }

        private string _editGameName = string.Empty;
        public string EditGameName
        {
            get => _editGameName;
            set
            {
                if (SetProperty(ref _editGameName, value))
                    OnPropertyChanged(nameof(CanConfirmEditGame));
            }
        }

        private string _editGameSavePath = string.Empty;
        public string EditGameSavePath
        {
            get => _editGameSavePath;
            set
            {
                if (SetProperty(ref _editGameSavePath, value))
                    OnPropertyChanged(nameof(CanConfirmEditGame));
            }
        }

        private string _editGameBackupPath = string.Empty;
        public string EditGameBackupPath
        {
            get => _editGameBackupPath;
            set
            {
                if (SetProperty(ref _editGameBackupPath, value))
                    OnPropertyChanged(nameof(CanConfirmEditGame));
            }
        }

        private string _editGameErrorMessage = string.Empty;
        public string EditGameErrorMessage
        {
            get => _editGameErrorMessage;
            set
            {
                if (SetProperty(ref _editGameErrorMessage, value))
                    OnPropertyChanged(nameof(HasEditGameError));
            }
        }

        public bool HasEditGameError => !string.IsNullOrEmpty(EditGameErrorMessage);

        public bool CanConfirmEditGame =>
            !string.IsNullOrWhiteSpace(EditGameName) &&
            !string.IsNullOrWhiteSpace(EditGameSavePath) &&
            !string.IsNullOrWhiteSpace(EditGameBackupPath);

        [RelayCommand]
        private void EditSelectedGame()
        {
            if (EditGamesSelectedGame == null)
                return;

            _editGameTarget = EditGamesSelectedGame;
            EditGameName = _editGameTarget.Name;
            EditGameSavePath = _editGameTarget.SavePath;
            EditGameBackupPath = _editGameTarget.BackupFolderPath;
            EditGameErrorMessage = string.Empty;
            IsEditGameFormOpen = true;
        }

        [RelayCommand]
        private void CancelEditGame()
        {
            IsEditGameFormOpen = false;
            _editGameTarget = null;
            EditGameErrorMessage = string.Empty;
        }

        [RelayCommand]
        private async Task ConfirmEditGame()
        {
            if (!CanConfirmEditGame || _editGameTarget == null)
                return;

            try
            {
                var targetId = _editGameTarget.Id;

                _updateGame.Execute(
                    targetId,
                    EditGameName,
                    EditGameSavePath,
                    EditGameBackupPath,
                    _editGameTarget.SaveType
                );

                var wasSelectedGame = SelectedGame?.Id == targetId;

                RefreshGamesList();

                if (wasSelectedGame)
                    SelectedGame = Games.FirstOrDefault(g => g.Id == targetId);

                EditGamesSelectedGame = Games.FirstOrDefault(g => g.Id == targetId);

                IsEditGameFormOpen = false;
                _editGameTarget = null;

                await Toast.Show("Game updated successfully!", isSuccess: true);
            }
            catch (Exception ex)
            {
                EditGameErrorMessage = ex.Message;
            }
        }

        [RelayCommand]
        private async Task BrowseEditGameSavePath()
        {
            if (_storageProvider == null || _editGameTarget == null)
                return;

            EditGameErrorMessage = string.Empty;

            if (_editGameTarget.SaveType == SaveType.SingleFile)
            {
                var files = await _storageProvider.OpenFilePickerAsync(new FilePickerOpenOptions
                {
                    Title = "Select Save File",
                    AllowMultiple = false
                });

                if (files.Count == 0) return;

                var path = files[0].Path.LocalPath;

                if (Directory.Exists(path))
                {
                    EditGameErrorMessage = "Save type is Single File, but a folder was selected. Please select a file.";
                    return;
                }

                EditGameSavePath = path;
            }
            else
            {
                var folders = await _storageProvider.OpenFolderPickerAsync(new FolderPickerOpenOptions
                {
                    Title = "Select Save Folder",
                    AllowMultiple = false
                });

                if (folders.Count == 0) return;

                var path = folders[0].Path.LocalPath;

                if (File.Exists(path))
                {
                    EditGameErrorMessage = "Save type is Folder, but a file was selected. Please select a folder.";
                    return;
                }

                if (!string.IsNullOrEmpty(EditGameBackupPath) && PathHelper.IsSameOrSubdirectory(EditGameBackupPath, path))
                {
                    EditGameErrorMessage = "The save folder cannot be the backup folder";
                    return;
                }

                EditGameSavePath = path;
            }
        }

        [RelayCommand]
        private async Task BrowseEditGameBackupPath()
        {
            if (_storageProvider == null)
                return;

            var folders = await _storageProvider.OpenFolderPickerAsync(new FolderPickerOpenOptions
            {
                Title = "Select Backup Folder",
                AllowMultiple = false
            });

            if (folders.Count == 0) return;

            var path = folders[0].Path.LocalPath;

            if (_editGameTarget!.SaveType == SaveType.Folder &&
                !string.IsNullOrEmpty(EditGameSavePath) &&
                PathHelper.IsSameOrSubdirectory(path, EditGameSavePath))
            {
                EditGameErrorMessage = "The save folder cannot be the backup folder";
                return;
            }

            EditGameBackupPath = folders[0].Path.LocalPath;
        }

        // --- delete game confirmation (nested overlay) ---

        private Game? _deleteGameTarget;

        private bool _isDeleteGameConfirmOpen;
        public bool IsDeleteGameConfirmOpen
        {
            get => _isDeleteGameConfirmOpen;
            set
            {
                if (SetProperty(ref _isDeleteGameConfirmOpen, value))
                    NotifyOverlayStateChanged();
            }
        }

        [RelayCommand]
        private void DeleteSelectedGame()
        {
            if (EditGamesSelectedGame == null)
                return;

            _deleteGameTarget = EditGamesSelectedGame;
            IsDeleteGameConfirmOpen = true;
        }

        [RelayCommand]
        private void CancelDeleteGame()
        {
            IsDeleteGameConfirmOpen = false;
            _deleteGameTarget = null;
        }

        [RelayCommand]
        private async Task ConfirmDeleteGame()
        {
            if (_deleteGameTarget == null)
                return;

            try
            {
                var targetId = _deleteGameTarget.Id;
                _removeGame.Execute(targetId);

                var wasSelectedGame = SelectedGame?.Id == targetId;

                RefreshGamesList();

                if (wasSelectedGame)
                    SelectedGame = null;

                EditGamesSelectedGame = null;
                IsDeleteGameConfirmOpen = false;
                _deleteGameTarget = null;

                await Toast.Show("Game deleted successfully!", isSuccess: true);
            }
            catch (Exception ex)
            {
                IsDeleteGameConfirmOpen = false;
                _deleteGameTarget = null;

                await Toast.Show($"Failed to delete game: {ex.Message}", isSuccess: false);
            }
        }
    }
}