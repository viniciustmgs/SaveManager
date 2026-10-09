using SaveManager.Domain.Entities;
using SaveManager.Domain.Enums;
using SaveManager.Domain.Interfaces;
using System.Diagnostics;
using System.Runtime.InteropServices;

namespace SaveManager.Infrastructure.HotKeys.Windows
{
    // windows backend built on a low level keyboard hook
    public sealed class WindowsHookHotKeyService : IGlobalHotKeyService, IHotKeyInputBlocker
    {
        private const int WH_KEYBOARD_LL = 13;
        private const uint WM_QUIT = 0x0012;

        private readonly AutoResetEvent _ready = new(false);
        private readonly HotKeyHookMatcher _matcher = new();
        private readonly Thread _thread;

        private HookProc? _callback;
        private IntPtr _hook;
        private uint _threadId;
        private HotKeyModifiers _held;
        private volatile bool _blocked;
        private volatile bool _stopRequested;
        private bool _disposed;

        public event Action<HotKeyAction>? HotKeyPressed;

        public bool IsSupported => true;

        public string? UnsupportedReason => null;

        public WindowsHookHotKeyService()
        {
            _callback = OnKeyEvent;

            _thread = new Thread(Pump)
            {
                IsBackground = true,
                Name = "SaveManager keyboard hook"
            };
            _thread.Start();

            _ready.WaitOne(TimeSpan.FromSeconds(5));
        }

        public void SetBlocked(bool value) => _blocked = value;

        public IReadOnlyDictionary<HotKeyAction, HotKeyRegistrationResult> Apply(
            IReadOnlyDictionary<HotKeyAction, HotKeyGesture> desired)
        {
            var results = new Dictionary<HotKeyAction, HotKeyRegistrationResult>();

            _matcher.Replace(desired);

            foreach (var (action, gesture) in desired)
            {
                if (!gesture.IsValid)
                {
                    results[action] = HotKeyRegistrationResult.Skipped;
                    continue;
                }

                var validation = HotKeyKeyTranslator.Validate(gesture);

                if (validation.IsFailure)
                {
                    results[action] = validation;
                    continue;
                }

                if (!WindowsKeyTranslator.TryTranslate(gesture, out _))
                {
                    results[action] = HotKeyRegistrationResult.UnsupportedGesture(
                        $"The key for {gesture} is not available on Windows.");
                    continue;
                }

                results[action] = HotKeyRegistrationResult.Registered;
            }

            return results;
        }

        private void Pump()
        {
            _threadId = NativeMethods.GetCurrentThreadId();

            var module = NativeMethods.GetModuleHandle(null);

            _hook = NativeMethods.SetWindowsHookEx(
                WH_KEYBOARD_LL, _callback!, module, 0);

            if (_hook == IntPtr.Zero)
            {
                Debug.WriteLine(
                    $"SaveManager: SetWindowsHookEx failed with error {Marshal.GetLastWin32Error()}.");
            }

            _ready.Set();

            while (!_stopRequested && NativeMethods.GetMessage(out var msg, IntPtr.Zero, 0, 0) > 0)
            {
                NativeMethods.TranslateMessage(ref msg);
                NativeMethods.DispatchMessage(ref msg);
            }

            if (_hook != IntPtr.Zero)
            {
                NativeMethods.UnhookWindowsHookEx(_hook);
                _hook = IntPtr.Zero;
            }
        }

        private IntPtr OnKeyEvent(int code, IntPtr wParam, IntPtr lParam)
        {
            try
            {
                var message = wParam.ToInt32();

                // KBDLLHOOKSTRUCT: vkCode, scanCode, flags, time, dwExtraInfo
                var vk = (uint)Marshal.ReadInt32(lParam, 0);
                var flags = (uint)Marshal.ReadInt32(lParam, 8);

                if (KeyboardHookDecision.ShouldTrackModifier(code))
                    UpdateModifiers(vk, KeyboardHookDecision.IsKeyDown(message) || KeyboardHookDecision.IsKeyUp(message));

                if (KeyboardHookDecision.ShouldDispatch(code, message, flags, _blocked)
                    && _matcher.Match(vk, _held) is { } action)
                {
                    HotKeyPressed?.Invoke(action);
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"SaveManager: keyboard hook callback failed: {ex}");
            }

            // never consume: other applications must still receive the key
            return CallNext(code, wParam, lParam);
        }

        private void UpdateModifiers(uint vk, bool down)
        {
            var modifier = HotKeyHookMatcher.ModifierFor(vk);

            if (modifier is null)
                return;

            if (down)
                _held |= modifier.Value;
            else
                _held &= ~modifier.Value;
        }

        private IntPtr CallNext(int code, IntPtr wParam, IntPtr lParam) =>
            NativeMethods.CallNextHookEx(_hook, code, wParam, lParam);

        public void Dispose()
        {
            if (_disposed)
                return;

            _disposed = true;
            _stopRequested = true;

            if (_thread.IsAlive && _threadId != NativeMethods.GetCurrentThreadId())
                NativeMethods.PostThreadMessage(_threadId, WM_QUIT, IntPtr.Zero, IntPtr.Zero);

            if (_thread.IsAlive)
                _thread.Join(TimeSpan.FromSeconds(2));

            _ready.Dispose();
            HotKeyPressed = null;
        }

        private delegate IntPtr HookProc(int code, IntPtr wParam, IntPtr lParam);

        private static class NativeMethods
        {
            [DllImport("user32.dll", SetLastError = true)]
            internal static extern IntPtr SetWindowsHookEx(
                int idHook, HookProc lpfn, IntPtr hMod, uint dwThreadId);

            [DllImport("user32.dll", SetLastError = true)]
            [return: MarshalAs(UnmanagedType.Bool)]
            internal static extern bool UnhookWindowsHookEx(IntPtr hhk);

            [DllImport("user32.dll")]
            internal static extern IntPtr CallNextHookEx(IntPtr hhk, int nCode, IntPtr wParam, IntPtr lParam);

            [DllImport("user32.dll")]
            internal static extern int GetMessage(out Msg lpMsg, IntPtr hWnd, uint wMsgFilterMin, uint wMsgFilterMax);

            [DllImport("user32.dll")]
            [return: MarshalAs(UnmanagedType.Bool)]
            internal static extern bool TranslateMessage(ref Msg lpMsg);

            [DllImport("user32.dll")]
            internal static extern IntPtr DispatchMessage(ref Msg lpMsg);

            [DllImport("user32.dll", SetLastError = true)]
            [return: MarshalAs(UnmanagedType.Bool)]
            internal static extern bool PostThreadMessage(uint idThread, uint msg, IntPtr wParam, IntPtr lParam);

            [DllImport("kernel32.dll")]
            internal static extern uint GetCurrentThreadId();

            [DllImport("kernel32.dll", CharSet = CharSet.Unicode, SetLastError = true)]
            internal static extern IntPtr GetModuleHandle(string? lpModuleName);

            [StructLayout(LayoutKind.Sequential)]
            internal struct Msg
            {
                public IntPtr hwnd;
                public uint message;
                public IntPtr wParam;
                public IntPtr lParam;
                public uint time;
                public int pt_x;
                public int pt_y;
            }
        }
    }
}