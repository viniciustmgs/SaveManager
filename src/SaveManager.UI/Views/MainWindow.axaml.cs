using Avalonia.Controls;
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
    }
}