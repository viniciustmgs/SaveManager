namespace SaveManager.Infrastructure.HotKeys.Windows
{
    // the Win32 virtual key codes the hook has to reason about
    internal static class WindowsVirtualKeys
    {
        internal const uint LShift = 0xA0;
        internal const uint RShift = 0xA1;
        internal const uint LCtrl = 0xA2;
        internal const uint RCtrl = 0xA3;
        internal const uint LAlt = 0xA4;
        internal const uint RAlt = 0xA5;

        internal const uint LWin = 0x5B;
        internal const uint RWin = 0x5C;

        internal static Domain.Enums.HotKeyModifiers? ToModifier(uint vk) => vk switch
        {
            LShift or RShift => Domain.Enums.HotKeyModifiers.Shift,
            LCtrl or RCtrl => Domain.Enums.HotKeyModifiers.Control,
            LAlt or RAlt => Domain.Enums.HotKeyModifiers.Alt,
            LWin or RWin => Domain.Enums.HotKeyModifiers.Meta,
            _ => null
        };
    }
}