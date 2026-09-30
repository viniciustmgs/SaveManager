using Avalonia.Controls;
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

        private void OnRenameSaveClick(object? sender, RoutedEventArgs e)
        {
            if (sender is MenuItem { DataContext: Save save } && DataContext is MainWindowViewModel vm)
                vm.OpenRenameSaveCommand.Execute(save);
        }
    }
}