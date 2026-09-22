using CommunityToolkit.Mvvm.ComponentModel;

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
    }
}