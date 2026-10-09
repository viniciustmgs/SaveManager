using SaveManager.Domain.Entities;
using SaveManager.Domain.Enums;
using SaveManager.Domain.Interfaces;
using SaveManager.Infrastructure.HotKeys;
using SaveManager.Infrastructure.HotKeys.Windows;
using System;
using System.Collections.Generic;
using System.Linq;
using Xunit;

namespace SaveManager.Infrastructure.Tests
{
    /// Windows gets the hook backend, everything else gets one that registers nothing
    public class GlobalHotKeyServiceFactoryTests
    {
        [Fact]
        public void TheBackendMatchesTheCurrentOperatingSystem()
        {
            using var service = GlobalHotKeyServiceFactory.Create(new NoWindowHost());

            if (OperatingSystem.IsWindows())
                Assert.IsType<WindowsHookHotKeyService>(service);
            else
                Assert.IsType<UnsupportedGlobalHotKeyService>(service);
        }

        [Fact]
        public void AnUnsupportedBackendReportsItselfAsUnsupportedWithAReason()
        {
            var service = new UnsupportedGlobalHotKeyService("only Windows");

            Assert.False(service.IsSupported);
            Assert.Equal("only Windows", service.UnsupportedReason);
        }

        [Fact]
        public void AnUnsupportedBackendFailsEveryRealBindingAndRegistersNothing()
        {
            var service = new UnsupportedGlobalHotKeyService("only Windows");

            var results = service.Apply(new Dictionary<HotKeyAction, HotKeyGesture>
            {
                [HotKeyAction.CreateSave] = new HotKeyGesture(HotKeyModifiers.Control, HotKeyKey.S),
                [HotKeyAction.LoadSave] = new HotKeyGesture(HotKeyModifiers.None, HotKeyKey.F7)
            });

            Assert.Equal(2, results.Count);
            Assert.All(results.Values, r => Assert.True(r.IsFailure));
            Assert.All(results.Values, r => Assert.Equal("only Windows", r.Message));
            Assert.DoesNotContain(results.Values, r => r.IsRegistered);
        }

        [Fact]
        public void AnUnsupportedBackendSkipsBlankBindingsRatherThanCallingThemFailures()
        {
            var service = new UnsupportedGlobalHotKeyService("only Windows");

            var results = service.Apply(new Dictionary<HotKeyAction, HotKeyGesture>
            {
                [HotKeyAction.CreateSave] = HotKeyGesture.None
            });

            Assert.Equal(HotKeyRegistrationStatus.Skipped, results[HotKeyAction.CreateSave].Status);
        }

        [Fact]
        public void AnUnsupportedBackendNeverRaisesAPress()
        {
            var service = new UnsupportedGlobalHotKeyService("only Windows");

            var raised = false;
            service.HotKeyPressed += _ => raised = true;

            service.Apply(new Dictionary<HotKeyAction, HotKeyGesture>
            {
                [HotKeyAction.CreateSave] = new HotKeyGesture(HotKeyModifiers.Control, HotKeyKey.S)
            });

            Assert.False(raised);
        }

        [Fact]
        public void AConfigCarriedOverFromWindowsCannotBreakANonWindowsBuild()
        {
            // the scenario: bindings exist in config.json, the app is started on a
            // platform with no backend. Applying them must not throw and must not
            // claim that anything was registered
            var service = new UnsupportedGlobalHotKeyService("only Windows");

            var desired = new Dictionary<HotKeyAction, HotKeyGesture>
            {
                [HotKeyAction.CreateSave] = new HotKeyGesture(HotKeyModifiers.Control, HotKeyKey.S),
                [HotKeyAction.LoadSave] = new HotKeyGesture(HotKeyModifiers.None, HotKeyKey.F7),
                [HotKeyAction.NextSave] = HotKeyGesture.None,
                [HotKeyAction.PreviousSave] = HotKeyGesture.None,
                [HotKeyAction.ToggleGlobalHotkeys] = new HotKeyGesture(HotKeyModifiers.Control | HotKeyModifiers.Alt, HotKeyKey.M)
            };

            var results = service.Apply(desired);

            Assert.DoesNotContain(results.Values, r => r.IsRegistered);
            Assert.Equal(3, results.Values.Count(r => r.Status == HotKeyRegistrationStatus.Failed));
            Assert.Equal(2, results.Values.Count(r => r.Status == HotKeyRegistrationStatus.Skipped));
        }

        private sealed class NoWindowHost : IHotKeyWindowHost
        {
            public bool IsAvailable => false;
            public IntPtr Handle => IntPtr.Zero;

            public IDisposable AddMessageHook(Func<IntPtr, uint, IntPtr, IntPtr, bool> handler) =>
                throw new NotSupportedException();
        }
    }
}