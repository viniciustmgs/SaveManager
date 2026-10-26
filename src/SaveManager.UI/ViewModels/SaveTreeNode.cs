using CommunityToolkit.Mvvm.ComponentModel;
using SaveManager.Domain.Entities;
using System.Collections.ObjectModel;

namespace SaveManager.UI.ViewModels
{
    public sealed class SaveTreeNode : ObservableObject
    {
        private bool _isExpanded;

        private SaveTreeNode()
        {
        }

        public static SaveTreeNode ForFolder(SaveFolder folder, bool isExpanded) =>
            new()
            {
                IsFolder = true,
                Name = folder.Name,
                Folder = folder,
                RelativePath = folder.RelativePath,
                _isExpanded = isExpanded
            };

        public static SaveTreeNode ForSave(Save save) =>
            new()
            {
                IsFolder = false,
                Name = save.Name,
                Save = save,
                RelativePath = save.RelativeFolderPath
            };

        public bool IsFolder { get; private init; }

        public string Name { get; private init; } = string.Empty;

        public Save? Save { get; private init; }

        public SaveFolder? Folder { get; private init; }

        public string RelativePath { get; private init; } = string.Empty;

        public ObservableCollection<SaveTreeNode> Children { get; } = [];

        public bool IsExpanded
        {
            get => _isExpanded;
            set => SetProperty(ref _isExpanded, value);
        }
    }
}
