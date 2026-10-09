namespace SaveManager.Domain.Enums
{
    // platform neutral key identity for a hotkey
    public enum HotKeyKey
    {
        None,

        A, B, C, D, E, F, G, H, I, J, K, L, M,
        N, O, P, Q, R, S, T, U, V, W, X, Y, Z,

        // top row digit keys, 1 through 0
        D1, D2, D3, D4, D5, D6, D7, D8, D9, D0,

        NumPad1, NumPad2, NumPad3, NumPad4, NumPad5,
        NumPad6, NumPad7, NumPad8, NumPad9, NumPad0,

        F1, F2, F3, F4, F5, F6,
        F7, F8, F9, F10, F11, F12,

        Space, Enter, Tab, Escape, Backspace,
        Delete, Insert, Home, End, PageUp, PageDown,
        Up, Down, Left, Right
    }
}