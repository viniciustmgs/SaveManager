using Avalonia.Platform.Storage;
using CommunityToolkit.Mvvm.Input;
using SaveManager.Domain.Enums;
using System;
using System.IO;
using System.Threading.Tasks;

namespace SaveManager.UI.ViewModels
{
    public partial class MainWindowViewModel
    {
        private bool _isAddGameDialogOpen;
        public bool IsAddGameDialogOpen
        {
            get => _isAddGameDialogOpen;
            set => SetProperty(ref _isAddGameDialogOpen, value);
        }

        private string _newGameName = string.Empty;
        public string NewGameName
        {
            get => _newGameName;
            set
            {
                if (SetProperty(ref _newGameName, value))
                    OnPropertyChanged(nameof(CanConfirmAddGame));
            }
        }

        private bool _newGameIsSingleFile;
        public bool NewGameIsSingleFile
        {
            get => _newGameIsSingleFile;
            set
            {
                if (SetProperty(ref _newGameIsSingleFile, value))
                {
                    if (value) NewGameIsFolder = false;
                    NewGameSavePath = string.Empty;
                    OnPropertyChanged(nameof(IsNewGameSaveTypeSelected));
                    OnPropertyChanged(nameof(CanConfirmAddGame));
                }
            }
        }

        private bool _newGameIsFolder;
        public bool NewGameIsFolder
        {
            get => _newGameIsFolder;
            set
            {
                if (SetProperty(ref _newGameIsFolder, value))
                {
                    if (value) NewGameIsSingleFile = false;
                    NewGameSavePath = string.Empty;
                    OnPropertyChanged(nameof(IsNewGameSaveTypeSelected));
                    OnPropertyChanged(nameof(CanConfirmAddGame));
                }
            }
        }

        private string _newGameSavePath = string.Empty;
        public string NewGameSavePath
        {
            get => _newGameSavePath;
            set
            {
                if (SetProperty(ref _newGameSavePath, value))
                    OnPropertyChanged(nameof(CanConfirmAddGame));
            }
        }

        private string _newGameBackupPath = string.Empty;
        public string NewGameBackupPath
        {
            get => _newGameBackupPath;
            set
            {
                if (SetProperty(ref _newGameBackupPath, value))
                    OnPropertyChanged(nameof(CanConfirmAddGame));
            }
        }

        private string _addGameErrorMessage = string.Empty;
        public string AddGameErrorMessage
        {
            get => _addGameErrorMessage;
            set
            {
                if (SetProperty(ref _addGameErrorMessage, value))
                    OnPropertyChanged(nameof(HasAddGameError));
            }
        }

        public bool IsNewGameSaveTypeSelected => NewGameIsSingleFile || NewGameIsFolder;
        public bool HasAddGameError => !string.IsNullOrEmpty(AddGameErrorMessage);

        public bool CanConfirmAddGame =>
            !string.IsNullOrWhiteSpace(NewGameName) &&
            IsNewGameSaveTypeSelected &&
            !string.IsNullOrWhiteSpace(NewGameSavePath) &&
            !string.IsNullOrWhiteSpace(NewGameBackupPath);

        private void ResetAddGameForm()
        {
            NewGameName = string.Empty;
            NewGameIsSingleFile = false;
            NewGameIsFolder = false;
            NewGameSavePath = string.Empty;
            NewGameBackupPath = string.Empty;
            AddGameErrorMessage = string.Empty;
        }

        [RelayCommand]
        private void OpenAddGame()
        {
            ResetAddGameForm();
            IsAddGameDialogOpen = true;
        }

        [RelayCommand]
        private void CancelAddGame()
        {
            IsAddGameDialogOpen = false;
            ResetAddGameForm();
        }

        [RelayCommand]
        private async Task ConfirmAddGame()
        {
            if (!CanConfirmAddGame) return;

            try
            {
                _addGame.Execute(
                    NewGameName,
                    NewGameSavePath,
                    NewGameBackupPath,
                    NewGameIsSingleFile ? SaveType.SingleFile : SaveType.Folder
                );

                LoadGames();
                IsAddGameDialogOpen = false;
                ResetAddGameForm();

                await Toast.Show("Game added successfully!", isSuccess: true);
            }
            catch (Exception ex)
            {
                AddGameErrorMessage = ex.Message;
            }
        }

        [RelayCommand]
        private async Task BrowseNewGameSavePath()
        {
            if (_storageProvider == null) return;

            AddGameErrorMessage = string.Empty;

            if (NewGameIsSingleFile)
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
                    AddGameErrorMessage = "Save type is Single File, but a folder was selected. Please select a file.";
                    return;
                }

                NewGameSavePath = path;
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
                    AddGameErrorMessage = "Save type is Folder, but a file was selected. Please select a folder.";
                    return;
                }

                NewGameSavePath = path;
            }
        }

        [RelayCommand]
        private async Task BrowseNewGameBackupPath()
        {
            if (_storageProvider == null) return;

            var folders = await _storageProvider.OpenFolderPickerAsync(new FolderPickerOpenOptions
            {
                Title = "Select Backup Folder",
                AllowMultiple = false
            });

            if (folders.Count == 0) return;

            NewGameBackupPath = folders[0].Path.LocalPath;
        }
    }
}