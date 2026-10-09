using System;

namespace SaveManager.Domain.Enums
{
    // platform neutral modifier flags
    [Flags]
    public enum HotKeyModifiers
    {
        None = 0,
        Alt = 1 << 0,
        Control = 1 << 1,
        Shift = 1 << 2,
        Meta = 1 << 3
    }
}