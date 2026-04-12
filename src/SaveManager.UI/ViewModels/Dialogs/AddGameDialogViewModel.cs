using Avalonia.Platform.Storage;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using SaveManager.Domain.Enums;
using System;
using System.IO;
using System.Threading.Tasks;

namespace SaveManager.UI.ViewModels.Dialogs
{
    public partial class AddGameDialogViewModel : ObservableObject
    {
        public IStorageProvider? StorageProvider { get; set; }

        private string _name = string.Empty;
        public string Name
        {
            get => _name;
            set
            {
                if (SetProperty(ref _name, value))
                    OnPropertyChanged(nameof(CanAddGame));
            }
        }

        private bool _isSingleFile;
        public bool IsSingleFile
        {
            get => _isSingleFile;
            set
            {
                if (SetProperty(ref _isSingleFile, value))
                {
                    if (value) IsFolder = false;
                    SavePath = string.Empty;
                    OnPropertyChanged(nameof(IsSaveTypeSelected));
                    OnPropertyChanged(nameof(CanAddGame));
                }
            }
        }

        private bool _isFolder;
        public bool IsFolder
        {
            get => _isFolder;
            set
            {
                if (SetProperty(ref _isFolder, value))
                {
                    if (value) IsSingleFile = false;
                    SavePath = string.Empty;
                    OnPropertyChanged(nameof(IsSaveTypeSelected));
                    OnPropertyChanged(nameof(CanAddGame));
                }
            }
        }

        private string _savePath = string.Empty;
        public string SavePath
        {
            get => _savePath;
            set
            {
                if (SetProperty(ref _savePath, value))
                    OnPropertyChanged(nameof(CanAddGame));
            }
        }

        private string _backupPath = string.Empty;
        public string BackupPath
        {
            get => _backupPath;
            set
            {
                if (SetProperty(ref _backupPath, value))
                    OnPropertyChanged(nameof(CanAddGame));
            }
        }

        private string _errorMessage = string.Empty;
        public string ErrorMessage
        {
            get => _errorMessage;
            set
            {
                if (SetProperty(ref _errorMessage, value))
                    OnPropertyChanged(nameof(HasError));
            }
        }

        public bool IsSaveTypeSelected => IsSingleFile || IsFolder;
        public bool HasError => !string.IsNullOrEmpty(ErrorMessage);
        public bool CanAddGame =>
            !string.IsNullOrWhiteSpace(Name) &&
            IsSaveTypeSelected &&
            !string.IsNullOrWhiteSpace(SavePath) &&
            !string.IsNullOrWhiteSpace(BackupPath);

        // result for MainWindowViewModel to use
        public (string Name, string SavePath, string BackupPath, SaveType SaveType)? Result { get; private set; }

        public event Action? RequestClose;

        [RelayCommand]
        private async Task BrowseSavePath()
        {
            if (StorageProvider == null) return;

            ErrorMessage = string.Empty;

            if (IsSingleFile)
            {
                var files = await StorageProvider.OpenFilePickerAsync(new FilePickerOpenOptions
                {
                    Title = "Select Save File",
                    AllowMultiple = false
                });

                if (files.Count == 0) return;

                var path = files[0].Path.LocalPath;

                if (Directory.Exists(path))
                {
                    ErrorMessage = "Save type is Single File, but a folder was selected. Please select a file.";
                    return;
                }

                SavePath = path;
            }
            else
            {
                var folders = await StorageProvider.OpenFolderPickerAsync(new FolderPickerOpenOptions
                {
                    Title = "Select Save Folder",
                    AllowMultiple = false
                });

                if (folders.Count == 0) return;

                var path = folders[0].Path.LocalPath;

                if (File.Exists(path))
                {
                    ErrorMessage = "Save type is Folder, but a file was selected. Please select a folder.";
                    return;
                }

                SavePath = path;
            }
        }

        [RelayCommand]
        private async Task BrowseBackupPath()
        {
            if (StorageProvider == null) return;

            var folders = await StorageProvider.OpenFolderPickerAsync(new FolderPickerOpenOptions
            {
                Title = "Select Backup Folder",
                AllowMultiple = false
            });

            if (folders.Count == 0) return;

            BackupPath = folders[0].Path.LocalPath;
        }

        [RelayCommand]
        private void AddGame()
        {
            if (!CanAddGame) return;

            Result = (
                Name,
                SavePath,
                BackupPath,
                IsSingleFile ? SaveType.SingleFile : SaveType.Folder
            );

            RequestClose?.Invoke();
        }

        [RelayCommand]
        private void Cancel()
        {
            Result = null;
            RequestClose?.Invoke();
        }
    }
}