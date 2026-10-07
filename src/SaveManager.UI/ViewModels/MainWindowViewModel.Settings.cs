using CommunityToolkit.Mvvm.Input;
using SaveManager.Domain.Entities;
using System;
using System.Threading.Tasks;

namespace SaveManager.UI.ViewModels
{
    public partial class MainWindowViewModel
    {
        // the hotkey properties below are a draft and only replace it on save
        // snapshot of the persisted settings. Never handed out directly: callers get the
        // bound draft properties instead, so edits stay local until saved.
        private AppSettings _settings = new();

        private bool _isSettingsDialogOpen;
        public bool IsSettingsDialogOpen
        {
            get => _isSettingsDialogOpen;
            set => SetProperty(ref _isSettingsDialogOpen, value);
        }

        private bool _isSettingsDiscardPromptOpen;
        public bool IsSettingsDiscardPromptOpen
        {
            get => _isSettingsDiscardPromptOpen;
            set => SetProperty(ref _isSettingsDiscardPromptOpen, value);
        }

        private bool _globalHotkeysEnabled;
        public bool GlobalHotkeysEnabled
        {
            get => _globalHotkeysEnabled;
            set
            {
                if (SetProperty(ref _globalHotkeysEnabled, value))
                    OnPropertyChanged(nameof(HasUnsavedChanges));
            }
        }

        private string _createSaveHotkey = string.Empty;
        public string CreateSaveHotkey
        {
            get => _createSaveHotkey;
            set
            {
                if (!SetProperty(ref _createSaveHotkey, value))
                    return;

                OnPropertyChanged(nameof(HasUnsavedChanges));

                if (string.IsNullOrEmpty(value))
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
                if (!SetProperty(ref _loadSaveHotkey, value))
                    return;

                OnPropertyChanged(nameof(HasUnsavedChanges));

                if (string.IsNullOrEmpty(value))
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
                if (!SetProperty(ref _nextSaveHotkey, value))
                    return;

                OnPropertyChanged(nameof(HasUnsavedChanges));

                if (string.IsNullOrEmpty(value))
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
                if (!SetProperty(ref _previousSaveHotkey, value))
                    return;

                OnPropertyChanged(nameof(HasUnsavedChanges));

                if (string.IsNullOrEmpty(value))
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
                if (!SetProperty(ref _toggleGlobalHotkeysHotkey, value))
                    return;

                OnPropertyChanged(nameof(HasUnsavedChanges));

                if (string.IsNullOrEmpty(value))
                    return;

                if (CreateSaveHotkey == value) CreateSaveHotkey = string.Empty;
                if (LoadSaveHotkey == value) LoadSaveHotkey = string.Empty;
                if (NextSaveHotkey == value) NextSaveHotkey = string.Empty;
                if (PreviousSaveHotkey == value) PreviousSaveHotkey = string.Empty;
            }
        }

        public bool HasUnsavedChanges =>
            GlobalHotkeysEnabled != _settings.GlobalHotkeysEnabled
            || CreateSaveHotkey != _settings.CreateSave
            || LoadSaveHotkey != _settings.LoadSave
            || NextSaveHotkey != _settings.NextSave
            || PreviousSaveHotkey != _settings.PreviousSave
            || ToggleGlobalHotkeysHotkey != _settings.ToggleGlobalHotkeys;

        public void LoadSettings()
        {
            _settings = _getSettings.Execute();
        }

        private void LoadDraftFromSettings()
        {
            GlobalHotkeysEnabled = _settings.GlobalHotkeysEnabled;
            CreateSaveHotkey = _settings.CreateSave;
            LoadSaveHotkey = _settings.LoadSave;
            NextSaveHotkey = _settings.NextSave;
            PreviousSaveHotkey = _settings.PreviousSave;
            ToggleGlobalHotkeysHotkey = _settings.ToggleGlobalHotkeys;
        }

        [RelayCommand]
        private void OpenSettings()
        {
            LoadSettings();
            LoadDraftFromSettings();

            IsSettingsDiscardPromptOpen = false;
            IsSettingsDialogOpen = true;
        }

        [RelayCommand]
        private async Task SaveSettings()
        {
            var settings = new AppSettings
            {
                GlobalHotkeysEnabled = GlobalHotkeysEnabled,
                CreateSave = CreateSaveHotkey,
                LoadSave = LoadSaveHotkey,
                NextSave = NextSaveHotkey,
                PreviousSave = PreviousSaveHotkey,
                ToggleGlobalHotkeys = ToggleGlobalHotkeysHotkey
            };

            try
            {
                _saveSettings.Execute(settings);
                _settings = settings;

                await Toast.Show("Settings saved", isSuccess: true);
            }
            catch (Exception ex)
            {
                await Toast.Show($"Failed to save settings: {ex.Message}", isSuccess: false);
            }
        }

        [RelayCommand]
        private void CloseSettings()
        {
            if (HasUnsavedChanges)
            {
                IsSettingsDiscardPromptOpen = true;
                return;
            }

            HideSettingsOverlay();
        }

        [RelayCommand]
        private void DiscardSettingsChanges()
        {
            LoadDraftFromSettings();

            HideSettingsOverlay();
        }

        [RelayCommand]
        private void CancelDiscardSettings()
        {
            IsSettingsDiscardPromptOpen = false;
        }

        private void HideSettingsOverlay()
        {
            IsSettingsDialogOpen = false;
            IsSettingsDiscardPromptOpen = false;
        }
    }
}