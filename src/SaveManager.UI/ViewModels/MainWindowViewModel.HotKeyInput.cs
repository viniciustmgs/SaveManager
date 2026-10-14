namespace SaveManager.UI.ViewModels
{
    public partial class MainWindowViewModel
    {
        public bool IsAnyOverlayOpen =>
            IsAddGameDialogOpen
            || IsEditGamesDialogOpen
            || IsEditGameFormOpen
            || IsDeleteGameConfirmOpen
            || IsAddProfileDialogOpen
            || IsEditProfilesDialogOpen
            || IsEditProfileFormOpen
            || IsDeleteProfileConfirmOpen
            || IsRenameSaveDialogOpen
            || IsDeleteSaveConfirmOpen
            || IsSettingsDialogOpen
            || IsSettingsDiscardPromptOpen
            || IsHowToUseOpen
            || IsCheckUpdatesOpen;

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