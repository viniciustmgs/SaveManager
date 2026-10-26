using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Platform;
using Avalonia.VisualTree;
using SaveManager.Domain.Entities;
using SaveManager.UI.ViewModels;
using System;

namespace SaveManager.UI.Views
{
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();

            SavesTree.AddHandler(
                PointerPressedEvent,
                OnTreePointerPressed,
                RoutingStrategies.Bubble,
                handledEventsToo: true);

            SavesTree.AddHandler(
                PointerMovedEvent,
                OnTreePointerMoved,
                RoutingStrategies.Bubble,
                handledEventsToo: true);

            SavesTree.AddHandler(
                DragDrop.DragOverEvent,
                OnTreeDragOver,
                RoutingStrategies.Bubble,
                handledEventsToo: true);

            SavesTree.AddHandler(
                DragDrop.DropEvent,
                OnTreeDrop,
                RoutingStrategies.Bubble,
                handledEventsToo: true);

            SavesTree.AddHandler(
                DragDrop.DragLeaveEvent,
                OnTreeDragLeave,
                RoutingStrategies.Bubble,
                handledEventsToo: true);

            Icon = LoadWindowIcon();

            if (DataContext is MainWindowViewModel vm)
                vm.SetStorageProvider(StorageProvider);
        }

        private static WindowIcon LoadWindowIcon()
        {
            using var stream = AssetLoader.Open(new Uri("avares://SaveManager/Assets/appicon.ico"));

            return new WindowIcon(stream);
        }

        protected override void OnKeyDown(KeyEventArgs e)
        {
            base.OnKeyDown(e);

            if (e.KeyModifiers == KeyModifiers.Control && e.Key == Key.F)
            {
                FocusSearchBox();
                e.Handled = true;
                return;
            }

            if (e.Key == Key.Delete)
            {
                if (TryRequestDeleteSave(out var handled))
                    e.Handled = handled;
            }
        }

        private bool TryRequestDeleteSave(out bool handled)
        {
            handled = false;

            if (DataContext is not MainWindowViewModel vm)
                return false;

            if (vm.IsAnyOverlayOpen || vm.IsSearchBoxFocused)
                return false;

            if (vm.SelectedNode is { IsFolder: true, Folder: { } folder })
            {
                vm.RequestDeleteFolder(folder);
                handled = true;

                return true;
            }

            if (vm.SelectedSave is null)
                return false;

            vm.DeleteSelectedSaveCommand.Execute(null);
            handled = true;

            return true;
        }

        private void FocusSearchBox()
        {
            SearchBox.Focus();

            SearchBox.SelectAll();
        }

        private void OnSearchBoxKeyDown(object? sender, KeyEventArgs e)
        {
            if (e.Key != Key.Escape)
                return;

            if (DataContext is MainWindowViewModel vm)
                vm.SaveSearchText = string.Empty;

            SavesTree.Focus();
            e.Handled = true;
        }

        private void OnSearchBoxGotFocus(object? sender, RoutedEventArgs e)
        {
            if (DataContext is MainWindowViewModel vm)
                vm.IsSearchBoxFocused = true;
        }

        private void OnSearchBoxLostFocus(object? sender, RoutedEventArgs e)
        {
            if (DataContext is MainWindowViewModel vm)
                vm.IsSearchBoxFocused = false;
        }

        private void OnNewFolderHereClick(object? sender, RoutedEventArgs e)
        {
            if (DataContext is not MainWindowViewModel vm)
                return;

            if (sender is MenuItem { DataContext: SaveTreeNode node })
                vm.SelectedNode = node;

            vm.OpenCreateFolderCommand.Execute(null);
        }

        private void OnNewFolderInEmptyAreaClick(object? sender, RoutedEventArgs e)
        {
            if (DataContext is not MainWindowViewModel vm)
                return;

            vm.SelectedNode = null;
            vm.OpenCreateFolderCommand.Execute(null);
        }

        private void OnRenameNodeClick(object? sender, RoutedEventArgs e)
        {
            if (DataContext is not MainWindowViewModel vm
                || sender is not MenuItem { DataContext: SaveTreeNode node })
            {
                return;
            }

            if (node.Folder is { } folder)
                vm.OpenRenameFolderCommand.Execute(folder);
            else if (node.Save is { } save)
                vm.OpenRenameSaveCommand.Execute(save);
        }

        private void OnDeleteNodeClick(object? sender, RoutedEventArgs e)
        {
            if (DataContext is not MainWindowViewModel vm
                || sender is not MenuItem { DataContext: SaveTreeNode node })
            {
                return;
            }

            if (node.Folder is { } folder)
                vm.RequestDeleteFolder(folder);
            else if (node.Save is { } save)
                vm.RequestDeleteSave(save);
        }

        private static readonly DataFormat<Save> DragSaveFormat =
            DataFormat.CreateInProcessFormat<Save>("SaveManager.Save");

        private Point _dragOrigin;
        private SaveTreeNode? _dragCandidate;
        private PointerPressedEventArgs? _dragPress;
        private bool _isInternalDrag;

        private void OnTreePointerPressed(object? sender, PointerPressedEventArgs e)
        {
            ResetDrag();

            if (!e.GetCurrentPoint(SavesTree).Properties.IsLeftButtonPressed)
                return;

            if (FindNode(e.Source as Visual) is not { IsFolder: false, Save: not null } node)
                return;

            _dragOrigin = e.GetPosition(SavesTree);
            _dragCandidate = node;
            _dragPress = e;
        }

        private async void OnTreePointerMoved(object? sender, PointerEventArgs e)
        {
            if (_dragCandidate is not { Save: { } save } || _dragPress is not { } press)
                return;

            if (!e.GetCurrentPoint(SavesTree).Properties.IsLeftButtonPressed)
            {
                ResetDrag();
                return;
            }

            var position = e.GetPosition(SavesTree);

            if (Math.Abs(position.X - _dragOrigin.X) < 4
                && Math.Abs(position.Y - _dragOrigin.Y) < 4)
            {
                return;
            }

            ResetDrag();

            var transfer = new DataTransfer();
            transfer.Add(DataTransferItem.Create(DragSaveFormat, save));

            _isInternalDrag = true;
            ShowDragGhost(save, e.GetPosition(DragGhostLayer));

            try
            {
                await DragDrop.DoDragDropAsync(press, transfer, DragDropEffects.Move);
            }
            finally
            {
                _isInternalDrag = false;
                HideDragGhost();
            }
        }

        private void OnTreeDragOver(object? sender, DragEventArgs e)
        {
            e.DragEffects = e.DataTransfer.Contains(DragSaveFormat)
                ? DragDropEffects.Move
                : DragDropEffects.None;

            e.Handled = true;

            if (_isInternalDrag)
            {
                DragGhost.IsVisible = true;
                MoveDragGhost(e.GetPosition(DragGhostLayer));
            }
        }

        private void OnTreeDragLeave(object? sender, DragEventArgs e)
        {
            if (!_isInternalDrag)
                return;

            var position = e.GetPosition(SavesTree);

            if (position.X < 0
                || position.Y < 0
                || position.X >= SavesTree.Bounds.Width
                || position.Y >= SavesTree.Bounds.Height)
            {
                HideDragGhost();
            }
        }

        private async void OnTreeDrop(object? sender, DragEventArgs e)
        {
            if (e.DataTransfer.TryGetValue(DragSaveFormat) is not { } save)
                return;

            e.Handled = true;

            var target = TargetFolderFor(e);

            if (DataContext is MainWindowViewModel vm)
                await vm.MoveSave(save, target);
        }

        private void ResetDrag()
        {
            _dragCandidate = null;
            _dragPress = null;
        }

        private void ShowDragGhost(Save save, Point position)
        {
            DragGhostText.Text = save.Name;
            DragGhost.IsVisible = true;
            MoveDragGhost(position);
        }

        private void MoveDragGhost(Point position)
        {
            Canvas.SetLeft(DragGhost, position.X + 14);
            Canvas.SetTop(DragGhost, position.Y + 18);
        }

        private void HideDragGhost()
        {
            DragGhost.IsVisible = false;
        }

        private string TargetFolderFor(DragEventArgs e)
        {
            var node = FindNode(e.Source as Visual);

            return node switch
            {
                { IsFolder: true } => node.RelativePath,
                { Save: not null } => node.Save.RelativeFolderPath,
                _ => string.Empty
            };
        }

        private static SaveTreeNode? FindNode(Visual? source)
        {
            var current = source;

            while (current is not null)
            {
                if (current is TreeViewItem { DataContext: SaveTreeNode node })
                    return node;

                current = current.GetVisualParent();
            }

            return null;
        }
    }
}
