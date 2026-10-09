using CommunityToolkit.Mvvm.Input;
using SaveManager.Domain.Entities;
using SaveManager.Domain.Enums;
using SaveManager.UI.HotKeys;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SaveManager.UI.ViewModels
{
    public partial class MainWindowViewModel
    {
        // the hotkey properties below are a draft and only replace it on save
        private AppSettings _settings = new();

        private bool _isSettingsDialogOpen;
        public bool IsSettingsDialogOpen
        {
            get => _isSettingsDialogOpen;
            set
            {
                if (SetProperty(ref _isSettingsDialogOpen, value))
                    NotifyOverlayStateChanged();
            }
        }

        private bool _isSettingsDiscardPromptOpen;
        public bool IsSettingsDiscardPromptOpen
        {
            get => _isSettingsDiscardPromptOpen;
            set
            {
                if (SetProperty(ref _isSettingsDiscardPromptOpen, value))
                    NotifyOverlayStateChanged();
            }
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
                if (SetProperty(ref _createSaveHotkey, value))
                    OnHotkeyDraftChanged(HotKeyAction.CreateSave, value);
            }
        }

        private string _loadSaveHotkey = string.Empty;
        public string LoadSaveHotkey
        {
            get => _loadSaveHotkey;
            set
            {
                if (SetProperty(ref _loadSaveHotkey, value))
                    OnHotkeyDraftChanged(HotKeyAction.LoadSave, value);
            }
        }

        private string _nextSaveHotkey = string.Empty;
        public string NextSaveHotkey
        {
            get => _nextSaveHotkey;
            set
            {
                if (SetProperty(ref _nextSaveHotkey, value))
                    OnHotkeyDraftChanged(HotKeyAction.NextSave, value);
            }
        }

        private string _previousSaveHotkey = string.Empty;
        public string PreviousSaveHotkey
        {
            get => _previousSaveHotkey;
            set
            {
                if (SetProperty(ref _previousSaveHotkey, value))
                    OnHotkeyDraftChanged(HotKeyAction.PreviousSave, value);
            }
        }

        private string _toggleGlobalHotkeysHotkey = string.Empty;
        public string ToggleGlobalHotkeysHotkey
        {
            get => _toggleGlobalHotkeysHotkey;
            set
            {
                if (SetProperty(ref _toggleGlobalHotkeysHotkey, value))
                    OnHotkeyDraftChanged(HotKeyAction.ToggleGlobalHotkeys, value);
            }
        }

        private static readonly HotKeyAction[] AllHotKeyActions =
        {
            HotKeyAction.CreateSave,
            HotKeyAction.LoadSave,
            HotKeyAction.NextSave,
            HotKeyAction.PreviousSave,
            HotKeyAction.ToggleGlobalHotkeys
        };

        private string GetHotkeyDraft(HotKeyAction action) => action switch
        {
            HotKeyAction.CreateSave => CreateSaveHotkey,
            HotKeyAction.LoadSave => LoadSaveHotkey,
            HotKeyAction.NextSave => NextSaveHotkey,
            HotKeyAction.PreviousSave => PreviousSaveHotkey,
            HotKeyAction.ToggleGlobalHotkeys => ToggleGlobalHotkeysHotkey,
            _ => string.Empty
        };

        private void OnHotkeyDraftChanged(HotKeyAction action, string value)
        {
            OnPropertyChanged(nameof(HasUnsavedChanges));

            if (string.IsNullOrEmpty(value))
                return;

            foreach (var other in AllHotKeyActions)
            {
                if (other == action)
                    continue;

                if (GetHotkeyDraft(other) != value)
                    continue;

                ClearHotkeyDraft(other);
            }
        }

        private void ClearHotkeyDraft(HotKeyAction action)
        {
            switch (action)
            {
                case HotKeyAction.CreateSave: CreateSaveHotkey = string.Empty; break;
                case HotKeyAction.LoadSave: LoadSaveHotkey = string.Empty; break;
                case HotKeyAction.NextSave: NextSaveHotkey = string.Empty; break;
                case HotKeyAction.PreviousSave: PreviousSaveHotkey = string.Empty; break;
                case HotKeyAction.ToggleGlobalHotkeys: ToggleGlobalHotkeysHotkey = string.Empty; break;
            }
        }

        private AppSettings BuildSettingsFromDraft() => new()
        {
            GlobalHotkeysEnabled = GlobalHotkeysEnabled,
            CreateSave = CreateSaveHotkey,
            LoadSave = LoadSaveHotkey,
            NextSave = NextSaveHotkey,
            PreviousSave = PreviousSaveHotkey,
            ToggleGlobalHotkeys = ToggleGlobalHotkeysHotkey
        };

        private static string ReadSettingsHotkey(AppSettings settings, HotKeyAction action) => action switch
        {
            HotKeyAction.CreateSave => settings.CreateSave,
            HotKeyAction.LoadSave => settings.LoadSave,
            HotKeyAction.NextSave => settings.NextSave,
            HotKeyAction.PreviousSave => settings.PreviousSave,
            HotKeyAction.ToggleGlobalHotkeys => settings.ToggleGlobalHotkeys,
            _ => string.Empty
        };

        private void WriteSettingsHotkey(AppSettings settings, HotKeyAction action, string value)
        {
            switch (action)
            {
                case HotKeyAction.CreateSave: settings.CreateSave = value; break;
                case HotKeyAction.LoadSave: settings.LoadSave = value; break;
                case HotKeyAction.NextSave: settings.NextSave = value; break;
                case HotKeyAction.PreviousSave: settings.PreviousSave = value; break;
                case HotKeyAction.ToggleGlobalHotkeys: settings.ToggleGlobalHotkeys = value; break;
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
            _settings = Normalize(_getSettings.Execute());
        }

        private static AppSettings Normalize(AppSettings stored) => new()
        {
            GlobalHotkeysEnabled = stored.GlobalHotkeysEnabled,
            CreateSave = NormalizeHotkey(stored.CreateSave),
            LoadSave = NormalizeHotkey(stored.LoadSave),
            NextSave = NormalizeHotkey(stored.NextSave),
            PreviousSave = NormalizeHotkey(stored.PreviousSave),
            ToggleGlobalHotkeys = NormalizeHotkey(stored.ToggleGlobalHotkeys)
        };

        private void LoadDraftFromSettings()
        {
            GlobalHotkeysEnabled = _settings.GlobalHotkeysEnabled;
            CreateSaveHotkey = _settings.CreateSave;
            LoadSaveHotkey = _settings.LoadSave;
            NextSaveHotkey = _settings.NextSave;
            PreviousSaveHotkey = _settings.PreviousSave;
            ToggleGlobalHotkeysHotkey = _settings.ToggleGlobalHotkeys;
        }

        private static string NormalizeHotkey(string value) =>
            HotKeyGestureConverter.TryParse(value, out var gesture)
                ? HotKeyGestureConverter.Format(gesture)
                : string.Empty;

        [RelayCommand]
        private void OpenSettings()
        {
            LoadSettings();
            LoadDraftFromSettings();

            _hotKeys?.Suspend();

            ApplyHotKeyInputState();

            IsSettingsDiscardPromptOpen = false;
            IsSettingsDialogOpen = true;
        }

        [RelayCommand]
        private async Task SaveSettings()
        {
            var candidate = BuildSettingsFromDraft();

            IReadOnlyDictionary<HotKeyAction, HotKeyRegistrationResult> results;

            try
            {
                results = _hotKeys?.Apply(candidate)
                    ?? new Dictionary<HotKeyAction, HotKeyRegistrationResult>();
            }
            catch (Exception ex)
            {
                await Toast.Show($"Failed to register hotkeys: {ex.Message}", isSuccess: false);
                return;
            }

            var failures = RollbackFailedHotkeys(candidate, results);

            try
            {
                _saveSettings.Execute(candidate);
                _settings = candidate;
                LoadDraftFromSettings();

                _hotKeys?.Suspend();

                if (failures.Count == 0)
                    await Toast.Show("Settings saved", isSuccess: true);
                else
                    await Toast.Show(DescribeFailures(failures), isSuccess: false);
            }
            catch (Exception ex)
            {
                await Toast.Show($"Failed to save settings: {ex.Message}", isSuccess: false);
            }
        }

        private List<HotKeyAction> RollbackFailedHotkeys(
            AppSettings candidate,
            IReadOnlyDictionary<HotKeyAction, HotKeyRegistrationResult> results)
        {
            var failures = new List<HotKeyAction>();

            foreach (var (action, result) in results)
            {
                if (!result.IsFailure)
                    continue;

                failures.Add(action);
                WriteSettingsHotkey(candidate, action, ReadSettingsHotkey(_settings, action));
            }

            if (failures.Count > 0)
            {
                _hotKeys?.Apply(candidate);
                LoadDraftFromSettings();
            }

            return failures;
        }

        private static string DescribeFailures(List<HotKeyAction> failures)
        {
            var actions = string.Join(", ", failures.Select(DescribeAction));

            return failures.Count == 1
                ? $"The hotkey for {actions} was not registered and was left unchanged."
                : $"The hotkeys for {actions} were not registered and were left unchanged.";
        }

        private static string DescribeAction(HotKeyAction action) => action switch
        {
            HotKeyAction.CreateSave => "Create Save",
            HotKeyAction.LoadSave => "Load Save",
            HotKeyAction.NextSave => "Next Save",
            HotKeyAction.PreviousSave => "Previous Save",
            HotKeyAction.ToggleGlobalHotkeys => "Toggle Global Hotkeys",
            _ => action.ToString()
        };

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

            _hotKeys?.Resume();
        }
    }
}