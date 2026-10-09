namespace SaveManager.UI.ViewModels
{
    public partial class MainWindowViewModel
    {
        public bool IsAnyOverlayOpen =>
            IsGamePickerOpen
            || IsAddGameDialogOpen
            || IsEditGamesDialogOpen
            || IsEditGameFormOpen
            || IsDeleteGameConfirmOpen
            || IsProfilePickerOpen
            || IsAddProfileDialogOpen
            || IsEditProfilesDialogOpen
            || IsEditProfileFormOpen
            || IsDeleteProfileConfirmOpen
            || IsRenameSaveDialogOpen
            || IsSettingsDialogOpen
            || IsSettingsDiscardPromptOpen;

        private bool _isSearchBoxFocused;
        public bool IsSearchBoxFocused
        {
            get => _isSearchBoxFocused;
            set
            {
                if (SetProperty(ref _isSearchBoxFocused, value))
                    NotifyHotKeyInputStateChanged();
            }
        }

        public bool IsHotKeyInputBlocked => IsAnyOverlayOpen || IsSearchBoxFocused;

        private void NotifyOverlayStateChanged()
        {
            OnPropertyChanged(nameof(IsAnyOverlayOpen));
            NotifyHotKeyInputStateChanged();
        }

        private void NotifyHotKeyInputStateChanged()
        {
            OnPropertyChanged(nameof(IsHotKeyInputBlocked));
            _hotKeys?.SetBlocked(IsHotKeyInputBlocked);
        }

        public void ApplyHotKeyInputState() => NotifyHotKeyInputStateChanged();
    }
}