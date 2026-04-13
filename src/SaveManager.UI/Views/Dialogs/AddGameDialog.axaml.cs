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

            viewModel.PropertyChanged += (sender, e) =>
            {
                if (e.PropertyName == nameof(AddGameDialogViewModel.SavePath))
                    ScrollToEnd(SavePathBox);

                if (e.PropertyName == nameof(AddGameDialogViewModel.BackupPath))
                    ScrollToEnd(BackupPathBox);
            };
        }

        private void ScrollToEnd(TextBox? textBox)
        {
            if (textBox == null) return;
            textBox.CaretIndex = textBox.Text?.Length ?? 0;
        }
    }
}