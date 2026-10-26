using CommunityToolkit.Mvvm.Input;
using SaveManager.Domain.Enums;
using SaveManager.UI.HotKeys;
using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace SaveManager.UI.ViewModels
{
    public partial class MainWindowViewModel
    {
        public void InitializeHotKeys(HotKeyCoordinator coordinator)
        {
            _hotKeys = coordinator;

            _hotKeys.HotKeyPressed += OnGlobalHotKeyPressed;
            _hotKeys.OthersEnabledChanged += OnOthersEnabledChanged;

            ApplyHotKeyInputState();

            _hotKeys.Apply(_settings);

            OnPropertyChanged(nameof(HasHotKeySupportIssue));
            OnPropertyChanged(nameof(HotKeyUnsupportedReason));
        }

        private void OnOthersEnabledChanged(bool enabled)
        {
            _ = Toast.Show(
                enabled ? "Save hotkeys are active" : "Save hotkeys are disabled",
                isSuccess: true,
                isWarning: !enabled);
        }

        public bool HasHotKeySupportIssue => _hotKeys is not null && !_hotKeys.IsSupported;

        public string? HotKeyUnsupportedReason => _hotKeys?.UnsupportedReason;

        public bool CanConfigureHotkeys => OperatingSystem.IsWindows();

        private void OnGlobalHotKeyPressed(HotKeyAction action)
        {
            Debug.WriteLine($"SaveManager: global hotkey fired: {action}");

            switch (action)
            {
                case HotKeyAction.CreateSave:
                    CreateSaveCommand.Execute(null);
                    break;

                case HotKeyAction.LoadSave:
                    LoadSaveCommand.Execute(null);
                    break;

                case HotKeyAction.NextSave:
                    NextSaveCommand.Execute(null);
                    break;

                case HotKeyAction.PreviousSave:
                    PreviousSaveCommand.Execute(null);
                    break;

                case HotKeyAction.ToggleGlobalHotkeys:
                    break;
            }
        }

        [RelayCommand]
        private void NextSave()
        {
            if (!CanMoveSaveSelection)
                return;

            var nodes = VisibleNodes();
            var index = IndexOfSelection(nodes);

            if (index < 0)
            {
                SelectedNode = nodes[0];
                return;
            }

            if (index + 1 < nodes.Count)
                SelectedNode = nodes[index + 1];
        }

        [RelayCommand]
        private void PreviousSave()
        {
            if (!CanMoveSaveSelection)
                return;

            var nodes = VisibleNodes();
            var index = IndexOfSelection(nodes);

            if (index <= 0)
            {
                SelectedNode = nodes[0];
                return;
            }

            SelectedNode = nodes[index - 1];
        }

        private bool CanMoveSaveSelection =>
            SelectedGame != null && SelectedProfile != null && SaveTree.Count > 0;

        private List<SaveTreeNode> VisibleNodes()
        {
            var nodes = new List<SaveTreeNode>();

            AddVisibleNodes(SaveTree, nodes);

            return nodes;
        }

        private static void AddVisibleNodes(
            IEnumerable<SaveTreeNode> source, List<SaveTreeNode> into)
        {
            foreach (var node in source)
            {
                into.Add(node);

                if (node.IsFolder && node.IsExpanded)
                    AddVisibleNodes(node.Children, into);
            }
        }

        private int IndexOfSelection(List<SaveTreeNode> nodes)
        {
            if (SelectedNode is not { } selected)
                return -1;

            var index = nodes.IndexOf(selected);

            if (index >= 0)
                return index;

            var folderPath = selected.IsFolder
                ? ParentOf(selected.RelativePath)
                : selected.Save?.RelativeFolderPath ?? string.Empty;

            while (folderPath.Length > 0)
            {
                var ancestor = nodes.Find(node =>
                    node.IsFolder && string.Equals(node.RelativePath, folderPath, PathComparison));

                if (ancestor is not null)
                    return nodes.IndexOf(ancestor);

                folderPath = ParentOf(folderPath);
            }

            return -1;
        }
    }
}