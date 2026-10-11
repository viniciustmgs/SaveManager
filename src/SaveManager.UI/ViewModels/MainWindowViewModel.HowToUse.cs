using CommunityToolkit.Mvvm.Input;

namespace SaveManager.UI.ViewModels
{
    public partial class MainWindowViewModel
    {
        private bool _isHowToUseOpen;
        public bool IsHowToUseOpen
        {
            get => _isHowToUseOpen;
            set
            {
                if (SetProperty(ref _isHowToUseOpen, value))
                    NotifyOverlayStateChanged();
            }
        }

        [RelayCommand]
        private void OpenHowToUse()
        {
            IsHowToUseOpen = true;
        }

        [RelayCommand]
        private void CloseHowToUse()
        {
            IsHowToUseOpen = false;
        }
    }
}