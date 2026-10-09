using CommunityToolkit.Mvvm.Input;
using SaveManager.Domain.Entities;
using SaveManager.Domain.Enums;
using SaveManager.UI.HotKeys;
using System;
using System.Diagnostics;

namespace SaveManager.UI.ViewModels
{
    public partial class MainWindowViewModel
    {
        public void InitializeHotKeys(HotKeyCoordinator coordinator)
        {
            _hotKeys = coordinator;

            _hotKeys.HotKeyPressed += OnGlobalHotKeyPressed;
            _hotKeys.OthersEnabledChanged += OnOthersEnabledChanged;

            ApplyHotKeyInputState();

            _hotKeys.Apply(_settings);

            OnPropertyChanged(nameof(HasHotKeySupportIssue));
            OnPropertyChanged(nameof(HotKeyUnsupportedReason));
        }

        private void OnOthersEnabledChanged(bool enabled)
        {
            _ = Toast.Show(
                enabled ? "Save hotkeys are active" : "Save hotkeys are disabled",
                isSuccess: true,
                isWarning: !enabled);
        }

        public bool HasHotKeySupportIssue => _hotKeys is not null && !_hotKeys.IsSupported;

        public string? HotKeyUnsupportedReason => _hotKeys?.UnsupportedReason;

        public bool CanConfigureHotkeys => OperatingSystem.IsWindows();

        private void OnGlobalHotKeyPressed(HotKeyAction action)
        {
            Debug.WriteLine($"SaveManager: global hotkey fired: {action}");

            switch (action)
            {
                case HotKeyAction.CreateSave:
                    CreateSaveCommand.Execute(null);
                    break;

                case HotKeyAction.LoadSave:
                    LoadSaveCommand.Execute(null);
                    break;

                case HotKeyAction.NextSave:
                    NextSaveCommand.Execute(null);
                    break;

                case HotKeyAction.PreviousSave:
                    PreviousSaveCommand.Execute(null);
                    break;

                case HotKeyAction.ToggleGlobalHotkeys:
                    break;
            }
        }

        [RelayCommand]
        private void NextSave()
        {
            if (!CanMoveSaveSelection)
                return;

            var index = SelectedSave is null ? -1 : Saves.IndexOf(SelectedSave);

            if (index < 0)
            {
                SelectedSave = Saves[0];
                return;
            }

            if (index + 1 >= Saves.Count)
                return;

            SelectedSave = Saves[index + 1];
        }

        [RelayCommand]
        private void PreviousSave()
        {
            if (!CanMoveSaveSelection)
                return;

            var index = SelectedSave is null ? -1 : Saves.IndexOf(SelectedSave);

            if (index <= 0)
            {
                SelectedSave = Saves[0];
                return;
            }

            SelectedSave = Saves[index - 1];
        }

        private bool CanMoveSaveSelection =>
            SelectedGame != null && SelectedProfile != null && Saves.Count > 0;
    }
}