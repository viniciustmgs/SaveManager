using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Platform;
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

            Icon = LoadWindowIcon();

            if (DataContext is MainWindowViewModel vm)
                vm.SetStorageProvider(StorageProvider);
        }

        private static WindowIcon LoadWindowIcon()
        {
            using var stream = AssetLoader.Open(new Uri("avares://SaveManager.UI/Assets/appicon.ico"));

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

            SavesList.Focus();
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

        private void OnRenameSaveClick(object? sender, RoutedEventArgs e)
        {
            if (sender is MenuItem { DataContext: Save save } && DataContext is MainWindowViewModel vm)
                vm.OpenRenameSaveCommand.Execute(save);
        }

        private void OnDeleteSaveClick(object? sender, RoutedEventArgs e)
        {
            if (sender is MenuItem { DataContext: Save save } && DataContext is MainWindowViewModel vm)
                vm.RequestDeleteSave(save);
        }
    }
}
