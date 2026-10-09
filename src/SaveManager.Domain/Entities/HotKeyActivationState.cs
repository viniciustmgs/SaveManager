using SaveManager.Domain.Enums;

namespace SaveManager.Domain.Entities
{
    public sealed class HotKeyActivationState
    {
        public const HotKeyAction ToggleAction = HotKeyAction.ToggleGlobalHotkeys;

        public event Action<bool>? OthersEnabledChanged;

        public bool GlobalEnabled { get; private set; }

        public bool OthersEnabled { get; private set; } = true;

        public void SetGlobalEnabled(bool value)
        {
            GlobalEnabled = value;

            if (!value)
                SetOthersEnabled(true);
        }

        public void OnToggleBindingCleared()
        {
            SetOthersEnabled(true);
        }

        private void SetOthersEnabled(bool value)
        {
            if (OthersEnabled == value)
                return;

            OthersEnabled = value;
            OthersEnabledChanged?.Invoke(value);
        }

        public void Toggle()
        {
            if (!GlobalEnabled)
                return;

            SetOthersEnabled(!OthersEnabled);
        }

        public bool CanDispatch(HotKeyAction action)
        {
            if (!GlobalEnabled)
                return false;

            if (action == ToggleAction)
                return true;

            return OthersEnabled;
        }
    }
}