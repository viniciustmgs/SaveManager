using SaveManager.Domain.Entities;
using SaveManager.Domain.Enums;
using SaveManager.Infrastructure.HotKeys.Windows;

namespace SaveManager.Infrastructure.Tests
{
    // the only test that touches real Win32
    public class WindowsHookLifecycleTests
    {
        [Fact]
        public void HookInstallsAcceptsBindingsAndUnhooksCleanly()
        {
            if (!OperatingSystem.IsWindows())
                return;

            using var service = new WindowsHookHotKeyService();

            var desired = new Dictionary<HotKeyAction, HotKeyGesture>
            {
                [HotKeyAction.CreateSave] = new HotKeyGesture(HotKeyModifiers.Control, HotKeyKey.S),
                [HotKeyAction.NextSave] = new HotKeyGesture(HotKeyModifiers.None, HotKeyKey.F7)
            };

            var results = service.Apply(desired);

            Assert.True(results[HotKeyAction.CreateSave].IsRegistered);
            Assert.True(results[HotKeyAction.NextSave].IsRegistered);
        }

        [Fact]
        public void BlankBindings_AreSkippedRatherThanRegistered()
        {
            if (!OperatingSystem.IsWindows())
                return;

            using var service = new WindowsHookHotKeyService();

            var desired = new Dictionary<HotKeyAction, HotKeyGesture>
            {
                [HotKeyAction.CreateSave] = HotKeyGesture.None
            };

            var results = service.Apply(desired);

            Assert.False(results[HotKeyAction.CreateSave].IsRegistered);
        }
    }
}
