using Avalonia.Controls;
using SaveManager.UI.ViewModels.Dialogs;

namespace SaveManager.UI.Views.Dialogs
{
    public partial class AddProfileDialog : Window
    {
        public AddProfileDialog()
        {
            InitializeComponent();

            var viewModel = new AddProfileDialogViewModel();
            viewModel.RequestClose += Close;

            DataContext = viewModel;
        }
    }
}