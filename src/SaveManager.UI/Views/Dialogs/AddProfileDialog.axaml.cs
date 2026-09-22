using Avalonia.Controls;
using Avalonia.Interactivity;
using SaveManager.UI.ViewModels.Dialogs;

namespace SaveManager.UI.Views.Dialogs
{
    public partial class AddProfileDialog : Window
    {
        public AddProfileDialog()
        {
            InitializeComponent();
            DataContext = new AddProfileDialogViewModel();
        }

        private void OnCancelClick(object? sender, RoutedEventArgs e)
        {
            Close(null);
        }

        private void OnAddProfileClick(object? sender, RoutedEventArgs e)
        {
            if (DataContext is not AddProfileDialogViewModel vm || !vm.CanAddProfile)
                return;

            Close(vm.Name);
        }
    }
}