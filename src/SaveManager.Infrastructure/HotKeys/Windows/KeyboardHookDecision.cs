namespace SaveManager.Infrastructure.HotKeys.Windows
{
    public static class KeyboardHookDecision
    {
        private const int HcAction = 0;
        private const uint LlfkhfInjected = 0x00000010;

        private const int WmKeyDown = 0x0100;
        private const int WmKeyUp = 0x0101;
        private const int WmSysKeyDown = 0x0104;
        private const int WmSysKeyUp = 0x0105;

        public static bool ShouldDispatch(int code, int message, uint flags, bool blocked)
        {
            if (code != HcAction)
                return false;

            if (IsInjected(flags))
                return false;

            if (!IsKeyDown(message))
                return false;

            if (blocked)
                return false;

            return true;
        }

        public static bool ShouldTrackModifier(int code) => code == HcAction;

        public static bool IsInjected(uint flags) => (flags & LlfkhfInjected) != 0;

        // alt combinations arrive as WM_SYSKEYDOWN rather than WM_KEYDOWN, so both count as a press
        public static bool IsKeyDown(int message) =>
            message is WmKeyDown or WmSysKeyDown;

        public static bool IsKeyUp(int message) =>
            message is WmKeyUp or WmSysKeyUp;
    }
}