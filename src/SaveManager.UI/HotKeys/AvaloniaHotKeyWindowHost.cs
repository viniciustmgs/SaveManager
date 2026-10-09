using Avalonia.Controls;
using SaveManager.Domain.Interfaces;
using System;

namespace SaveManager.UI.HotKeys
{
    public sealed class AvaloniaHotKeyWindowHost : IHotKeyWindowHost
    {
        private readonly Func<Window?> _windowAccessor;

        public AvaloniaHotKeyWindowHost(Func<Window?> windowAccessor)
        {
            _windowAccessor = windowAccessor;
        }

        public bool IsAvailable => Handle != IntPtr.Zero;

        public IntPtr Handle
        {
            get
            {
                var window = _windowAccessor();

                return window?.TryGetPlatformHandle()?.Handle ?? IntPtr.Zero;
            }
        }

        public IDisposable AddMessageHook(Func<IntPtr, uint, IntPtr, IntPtr, bool> handler)
        {
            var window = _windowAccessor()
                ?? throw new InvalidOperationException("The main window is not available yet.");

            if (window.TryGetPlatformHandle() is null)
                throw new InvalidOperationException(
                    "The main window has no native handle yet. Attach the hotkey host after Opened.");

            IntPtr Hook(IntPtr hWnd, uint msg, IntPtr wParam, IntPtr lParam, ref bool handled)
            {
                if (handler(hWnd, msg, wParam, lParam))
                {
                    handled = true;
                }

                return IntPtr.Zero;
            }

            var callback = new Win32Properties.CustomWndProcHookCallback(Hook);

            Win32Properties.AddWndProcHookCallback(window, callback);

            return new Subscription(window, callback);
        }

        private sealed class Subscription : IDisposable
        {
            private Window? _window;
            private Win32Properties.CustomWndProcHookCallback? _callback;

            public Subscription(Window window, Win32Properties.CustomWndProcHookCallback callback)
            {
                _window = window;
                _callback = callback;
            }

            public void Dispose()
            {
                if (_window is null || _callback is null)
                    return;

                Win32Properties.RemoveWndProcHookCallback(_window, _callback);

                _window = null;
                _callback = null;
            }
        }
    }
}