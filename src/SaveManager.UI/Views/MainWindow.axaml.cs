using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using SaveManager.Domain.Entities;
using SaveManager.UI.ViewModels;

namespace SaveManager.UI.Views
{
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();

            if (DataContext is MainWindowViewModel vm)
                vm.SetStorageProvider(StorageProvider);
        }

        protected override void OnKeyDown(KeyEventArgs e)
        {
            base.OnKeyDown(e);

            if (e.KeyModifiers == KeyModifiers.Control && e.Key == Key.F)
            {
                FocusSearchBox();
                e.Handled = true;
            }
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
    }
}
