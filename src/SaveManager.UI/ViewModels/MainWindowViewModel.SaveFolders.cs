using CommunityToolkit.Mvvm.Input;
using SaveManager.Domain.Entities;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace SaveManager.UI.ViewModels
{
    public partial class MainWindowViewModel
    {
        private bool _isCreateFolderDialogOpen;
        public bool IsCreateFolderDialogOpen
        {
            get => _isCreateFolderDialogOpen;
            set
            {
                if (SetProperty(ref _isCreateFolderDialogOpen, value))
                    NotifyOverlayStateChanged();
            }
        }

        private string _newFolderName = string.Empty;
        public string NewFolderName
        {
            get => _newFolderName;
            set
            {
                if (SetProperty(ref _newFolderName, value))
                    OnPropertyChanged(nameof(CanConfirmNewFolder));
            }
        }

        private string _createFolderErrorMessage = string.Empty;
        public string CreateFolderErrorMessage
        {
            get => _createFolderErrorMessage;
            set
            {
                if (SetProperty(ref _createFolderErrorMessage, value))
                    OnPropertyChanged(nameof(HasCreateFolderError));
            }
        }

        public bool HasCreateFolderError => !string.IsNullOrEmpty(CreateFolderErrorMessage);

        public bool CanConfirmNewFolder => !string.IsNullOrWhiteSpace(NewFolderName);

        [RelayCommand]
        private void OpenCreateFolder()
        {
            if (!CanManageSaves)
                return;

            NewFolderName = string.Empty;
            CreateFolderErrorMessage = string.Empty;
            IsCreateFolderDialogOpen = true;
        }

        [RelayCommand]
        private void CancelCreateFolder()
        {
            IsCreateFolderDialogOpen = false;
            NewFolderName = string.Empty;
            CreateFolderErrorMessage = string.Empty;
        }

        [RelayCommand]
        private async Task ConfirmCreateFolder()
        {
            if (!CanConfirmNewFolder || SelectedGame == null || SelectedProfile == null)
                return;

            try
            {
                var parent = TargetFolderForNewContent();

                var folder = _createSaveFolder.Execute(
                    SelectedProfile, SelectedGame, parent, NewFolderName);

                _expandedFolders.Add(folder.RelativePath);

                await RefreshSaves();

                SelectedNode = FindFolderNode(SaveTree, folder.RelativePath);

                IsCreateFolderDialogOpen = false;
                NewFolderName = string.Empty;
                CreateFolderErrorMessage = string.Empty;

                await Toast.Show("Folder created successfully!", isSuccess: true);
            }
            catch (Exception ex)
            {
                CreateFolderErrorMessage = ex.Message;
            }
        }

        private SaveFolder? _renameFolderTarget;

        private bool _isRenameFolderDialogOpen;
        public bool IsRenameFolderDialogOpen
        {
            get => _isRenameFolderDialogOpen;
            set
            {
                if (SetProperty(ref _isRenameFolderDialogOpen, value))
                    NotifyOverlayStateChanged();
            }
        }

        private string _renameFolderName = string.Empty;
        public string RenameFolderName
        {
            get => _renameFolderName;
            set
            {
                if (SetProperty(ref _renameFolderName, value))
                    OnPropertyChanged(nameof(CanConfirmRenameFolder));
            }
        }

        private string _renameFolderErrorMessage = string.Empty;
        public string RenameFolderErrorMessage
        {
            get => _renameFolderErrorMessage;
            set
            {
                if (SetProperty(ref _renameFolderErrorMessage, value))
                    OnPropertyChanged(nameof(HasRenameFolderError));
            }
        }

        public bool HasRenameFolderError => !string.IsNullOrEmpty(RenameFolderErrorMessage);

        public bool CanConfirmRenameFolder => !string.IsNullOrWhiteSpace(RenameFolderName);

        [RelayCommand]
        private void OpenRenameFolder(SaveFolder folder)
        {
            _renameFolderTarget = folder;
            RenameFolderName = folder.Name;
            RenameFolderErrorMessage = string.Empty;
            IsRenameFolderDialogOpen = true;
        }

        [RelayCommand]
        private void CancelRenameFolder()
        {
            IsRenameFolderDialogOpen = false;
            _renameFolderTarget = null;
            RenameFolderName = string.Empty;
            RenameFolderErrorMessage = string.Empty;
        }

        [RelayCommand]
        private async Task ConfirmRenameFolder()
        {
            if (!CanConfirmRenameFolder || _renameFolderTarget == null
                || SelectedGame == null || SelectedProfile == null)
            {
                return;
            }

            try
            {
                var target = _renameFolderTarget;
                var oldPath = target.RelativePath;

                var renamed = _renameSaveFolder.Execute(
                    SelectedProfile, SelectedGame, target, RenameFolderName);

                RekeyExpansion(oldPath, renamed.RelativePath);

                await RefreshSaves();

                SelectedNode = FindFolderNode(SaveTree, renamed.RelativePath);

                IsRenameFolderDialogOpen = false;
                _renameFolderTarget = null;
                RenameFolderName = string.Empty;
                RenameFolderErrorMessage = string.Empty;

                await Toast.Show("Folder renamed successfully!", isSuccess: true);
            }
            catch (Exception ex)
            {
                RenameFolderErrorMessage = ex.Message;
            }
        }

        private SaveFolder? _deleteFolderTarget;

        private bool _isDeleteFolderConfirmOpen;
        public bool IsDeleteFolderConfirmOpen
        {
            get => _isDeleteFolderConfirmOpen;
            set
            {
                if (SetProperty(ref _isDeleteFolderConfirmOpen, value))
                    NotifyOverlayStateChanged();
            }
        }

        public string DeleteFolderConfirmMessage
        {
            get
            {
                if (_deleteFolderTarget is null)
                    return string.Empty;

                var count = CountSavesInside(_deleteFolderTarget.RelativePath);
                var name = _deleteFolderTarget.Name;

                return count > 0
                    ? $"\"{name}\" and the {count} save(s) inside it are going to be deleted, do you want to proceed?"
                    : $"\"{name}\" is going to be deleted, do you want to proceed?";
            }
        }

        public void RequestDeleteFolder(SaveFolder folder)
        {
            _deleteFolderTarget = folder;

            OnPropertyChanged(nameof(DeleteFolderConfirmMessage));
            IsDeleteFolderConfirmOpen = true;
        }

        [RelayCommand]
        private void CancelDeleteFolder()
        {
            IsDeleteFolderConfirmOpen = false;
            _deleteFolderTarget = null;
        }

        [RelayCommand]
        private async Task ConfirmDeleteFolder()
        {
            if (_deleteFolderTarget is null || SelectedProfile == null)
                return;

            var doomed = _deleteFolderTarget;

            IsDeleteFolderConfirmOpen = false;
            _deleteFolderTarget = null;

            try
            {
                _deleteSaveFolder.Execute(SelectedProfile, doomed);

                ClearExpansion(doomed.RelativePath);

                await RefreshSaves();

                SelectedNode = null;

                await Toast.Show("Folder deleted successfully!", isSuccess: true);
            }
            catch (Exception ex)
            {
                await Toast.Show($"Failed to delete folder: {ex.Message}", isSuccess: false);
            }
        }

        private int CountSavesInside(string relativeFolderPath) =>
            _allSaves.Count(save => IsInFolder(save.RelativeFolderPath, relativeFolderPath));

        private static bool IsInFolder(string relativePath, string relativeFolderPath) =>
            string.Equals(relativePath, relativeFolderPath, PathComparison)
            || relativePath.StartsWith(
                relativeFolderPath + Path.DirectorySeparatorChar, PathComparison);

        private void RekeyExpansion(string oldPrefix, string newPrefix)
        {
            var affected = _expandedFolders
                .Where(path => IsInFolder(path, oldPrefix))
                .ToList();

            foreach (var path in affected)
            {
                _expandedFolders.Remove(path);
                _expandedFolders.Add(newPrefix + path[oldPrefix.Length..]);
            }
        }

        private void ClearExpansion(string prefix)
        {
            foreach (var path in _expandedFolders.Where(path => IsInFolder(path, prefix)).ToList())
                _expandedFolders.Remove(path);
        }

        private static SaveTreeNode? FindFolderNode(
            IEnumerable<SaveTreeNode> nodes, string relativePath)
        {
            foreach (var node in nodes)
            {
                if (node.IsFolder && string.Equals(node.RelativePath, relativePath, PathComparison))
                    return node;

                if (FindFolderNode(node.Children, relativePath) is { } found)
                    return found;
            }

            return null;
        }
    }
}
