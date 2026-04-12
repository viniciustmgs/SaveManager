using Avalonia.Controls;
using SaveManager.UI.ViewModels.Dialogs;

namespace SaveManager.UI.Views.Dialogs
{
    public partial class AddGameDialog : Window
    {
        public AddGameDialog()
        {
            InitializeComponent();

            var viewModel = new AddGameDialogViewModel();
            viewModel.StorageProvider = StorageProvider;
            viewModel.RequestClose += Close;

            DataContext = viewModel;
        }
    }
}