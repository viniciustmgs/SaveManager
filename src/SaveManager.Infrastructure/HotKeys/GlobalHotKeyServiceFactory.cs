using SaveManager.Domain.Interfaces;
using SaveManager.Infrastructure.HotKeys.Windows;

namespace SaveManager.Infrastructure.HotKeys
{
    // picks the backend that matches the current OS
    public static class GlobalHotKeyServiceFactory
    {
        public static IGlobalHotKeyService Create(IHotKeyWindowHost windowHost)
        {
            _ = windowHost;

            if (OperatingSystem.IsWindows())
                return new WindowsHookHotKeyService();

            // no backend outside windows yet
            return new UnsupportedGlobalHotKeyService(
                "Global hotkeys are only implemented for Windows.");
        }
    }
}
