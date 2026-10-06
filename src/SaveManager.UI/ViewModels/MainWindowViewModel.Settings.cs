using CommunityToolkit.Mvvm.Input;

namespace SaveManager.UI.ViewModels
{
    public partial class MainWindowViewModel
    {
        private bool _isSettingsDialogOpen;
        public bool IsSettingsDialogOpen
        {
            get => _isSettingsDialogOpen;
            set => SetProperty(ref _isSettingsDialogOpen, value);
        }

        private bool _globalHotkeysEnabled;
        public bool GlobalHotkeysEnabled
        {
            get => _globalHotkeysEnabled;
            set => SetProperty(ref _globalHotkeysEnabled, value);
        }

        private string _createSaveHotkey = string.Empty;
        public string CreateSaveHotkey
        {
            get => _createSaveHotkey;
            set
            {
                if (!SetProperty(ref _createSaveHotkey, value) || string.IsNullOrEmpty(value))
                    return;

                if (LoadSaveHotkey == value) LoadSaveHotkey = string.Empty;
                if (NextSaveHotkey == value) NextSaveHotkey = string.Empty;
                if (PreviousSaveHotkey == value) PreviousSaveHotkey = string.Empty;
                if (ToggleGlobalHotkeysHotkey == value) ToggleGlobalHotkeysHotkey = string.Empty;
            }
        }

        private string _loadSaveHotkey = string.Empty;
        public string LoadSaveHotkey
        {
            get => _loadSaveHotkey;
            set
            {
                if (!SetProperty(ref _loadSaveHotkey, value) || string.IsNullOrEmpty(value))
                    return;

                if (CreateSaveHotkey == value) CreateSaveHotkey = string.Empty;
                if (NextSaveHotkey == value) NextSaveHotkey = string.Empty;
                if (PreviousSaveHotkey == value) PreviousSaveHotkey = string.Empty;
                if (ToggleGlobalHotkeysHotkey == value) ToggleGlobalHotkeysHotkey = string.Empty;
            }
        }

        private string _nextSaveHotkey = string.Empty;
        public string NextSaveHotkey
        {
            get => _nextSaveHotkey;
            set
            {
                if (!SetProperty(ref _nextSaveHotkey, value) || string.IsNullOrEmpty(value))
                    return;

                if (CreateSaveHotkey == value) CreateSaveHotkey = string.Empty;
                if (LoadSaveHotkey == value) LoadSaveHotkey = string.Empty;
                if (PreviousSaveHotkey == value) PreviousSaveHotkey = string.Empty;
                if (ToggleGlobalHotkeysHotkey == value) ToggleGlobalHotkeysHotkey = string.Empty;
            }
        }

        private string _previousSaveHotkey = string.Empty;
        public string PreviousSaveHotkey
        {
            get => _previousSaveHotkey;
            set
            {
                if (!SetProperty(ref _previousSaveHotkey, value) || string.IsNullOrEmpty(value))
                    return;

                if (CreateSaveHotkey == value) CreateSaveHotkey = string.Empty;
                if (LoadSaveHotkey == value) LoadSaveHotkey = string.Empty;
                if (NextSaveHotkey == value) NextSaveHotkey = string.Empty;
                if (ToggleGlobalHotkeysHotkey == value) ToggleGlobalHotkeysHotkey = string.Empty;
            }
        }

        private string _toggleGlobalHotkeysHotkey = string.Empty;
        public string ToggleGlobalHotkeysHotkey
        {
            get => _toggleGlobalHotkeysHotkey;
            set
            {
                if (!SetProperty(ref _toggleGlobalHotkeysHotkey, value) || string.IsNullOrEmpty(value))
                    return;

                if (CreateSaveHotkey == value) CreateSaveHotkey = string.Empty;
                if (LoadSaveHotkey == value) LoadSaveHotkey = string.Empty;
                if (NextSaveHotkey == value) NextSaveHotkey = string.Empty;
                if (PreviousSaveHotkey == value) PreviousSaveHotkey = string.Empty;
            }
        }

        [RelayCommand]
        private void OpenSettings()
        {
            ResetSettingsDraft();
            IsSettingsDialogOpen = true;
        }

        // Draft state only: nothing here is persisted yet, so opening the overlay starts blank.
        // When persistence lands this becomes the load from config.json, and a SaveSettings
        // command writes these properties back.
        private void ResetSettingsDraft()
        {
            GlobalHotkeysEnabled = false;
            CreateSaveHotkey = string.Empty;
            LoadSaveHotkey = string.Empty;
            NextSaveHotkey = string.Empty;
            PreviousSaveHotkey = string.Empty;
            ToggleGlobalHotkeysHotkey = string.Empty;
        }

        [RelayCommand]
        private void CloseSettings()
        {
            IsSettingsDialogOpen = false;
        }
    }
}