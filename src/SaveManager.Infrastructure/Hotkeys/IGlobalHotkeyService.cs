namespace SaveManager.Infrastructure.Hotkeys
{
    public interface IGlobalHotkeyService
    {
        void RegisterHotkey(string action, string hotkey, Action callback);
        void UnregisterHotkey(string action);
        void UnregisterAll();
    }
}
