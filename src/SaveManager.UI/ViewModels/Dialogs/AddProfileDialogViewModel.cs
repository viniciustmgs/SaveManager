using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System;

namespace SaveManager.UI.ViewModels.Dialogs
{
    public partial class AddProfileDialogViewModel : ObservableObject
    {
        private string _name = string.Empty;
        public string Name
        {
            get => _name;
            set
            {
                if (SetProperty(ref _name, value))
                    OnPropertyChanged(nameof(CanAddProfile));
            }
        }

        public bool CanAddProfile => !string.IsNullOrWhiteSpace(Name);

        public string? Result { get; private set; }

        public event Action? RequestClose;

        [RelayCommand]
        private void AddProfile()
        {
            if (!CanAddProfile) return;

            Result = Name;
            RequestClose?.Invoke();
        }

        [RelayCommand]
        private void Cancel()
        {
            Result = null;
            RequestClose?.Invoke();
        }
    }
}