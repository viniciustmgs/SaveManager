using Avalonia.Controls;
using Avalonia.Interactivity;
using SaveManager.Domain.Enums;
using SaveManager.UI.ViewModels.Dialogs;

namespace SaveManager.UI.Views.Dialogs
{
    public partial class AddGameDialog : Window
    {
        public AddGameDialogResult? Result { get; private set; }

        public AddGameDialog()
        {
            InitializeComponent();

            var viewModel = new AddGameDialogViewModel();
            viewModel.StorageProvider = StorageProvider;

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

        private void OnCancelClick(object? sender, RoutedEventArgs e)
        {
            Result = null;
            Close();
        }

        private void OnAddGameClick(object? sender, RoutedEventArgs e)
        {
            if (DataContext is not AddGameDialogViewModel vm || !vm.CanAddGame)
                return;

            Result = new AddGameDialogResult
            {
                GameName = vm.Name,
                SavePath = vm.SavePath,
                BackupPath = vm.BackupPath,
                SaveType = vm.IsSingleFile ? SaveType.SingleFile : SaveType.Folder
            };

            Close();
        }
    }
}