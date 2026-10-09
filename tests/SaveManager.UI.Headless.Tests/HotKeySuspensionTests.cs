using SaveManager.Domain.Entities;
using SaveManager.Domain.Enums;
using SaveManager.Domain.Interfaces;
using SaveManager.UI.HotKeys;

namespace SaveManager.UI.Headless.Tests
{
    public class HotKeySuspensionTests
    {
        private static AppSettings SettingsWith(string create, string load) => new()
        {
            GlobalHotkeysEnabled = true,
            CreateSave = create,
            LoadSave = load,
            NextSave = string.Empty,
            PreviousSave = string.Empty,
            ToggleGlobalHotkeys = string.Empty
        };

        [Fact]
        public void Apply_RegistersTheBindings()
        {
            var (bound, last) = Ui.Get(() =>
            {
                var service = new RecordingService();

                using var coordinator = new HotKeyCoordinator(service);

                coordinator.Apply(SettingsWith("F7", "Ctrl+S"));

                return (service.IsEverythingBound, service.LastDesired);
            });

            Assert.True(bound);
            Assert.Equal(HotKeyKey.F7, last[HotKeyAction.CreateSave].Key);
        }

        [Fact]
        public void Suspend_ReleasesEveryBinding()
        {
            var boundWhileSuspended = Ui.Get(() =>
            {
                var service = new RecordingService();

                using var coordinator = new HotKeyCoordinator(service);

                coordinator.Apply(SettingsWith("F7", "Ctrl+S"));
                coordinator.Suspend();

                return service.IsEverythingBound;
            });

            Assert.False(boundWhileSuspended);
        }

        [Fact]
        public void Resume_TakesTheBindingsBack()
        {
            var boundAfterResume = Ui.Get(() =>
            {
                var service = new RecordingService();

                using var coordinator = new HotKeyCoordinator(service);

                coordinator.Apply(SettingsWith("F7", "Ctrl+S"));
                coordinator.Suspend();
                coordinator.Resume();

                return service.IsEverythingBound;
            });

            Assert.True(boundAfterResume);
        }

        [Fact]
        public void Suspend_IsIdempotent()
        {
            var calls = Ui.Get(() =>
            {
                var service = new RecordingService();

                using var coordinator = new HotKeyCoordinator(service);

                coordinator.Apply(SettingsWith("F7", string.Empty));
                coordinator.Suspend();
                coordinator.Suspend();

                return service.ApplyCount;
            });

            Assert.Equal(2, calls);
        }

        [Fact]
        public void Resume_DoesNothingWhenNotSuspended()
        {
            var calls = Ui.Get(() =>
            {
                var service = new RecordingService();

                using var coordinator = new HotKeyCoordinator(service);

                coordinator.Apply(SettingsWith("F7", string.Empty));
                coordinator.Resume();

                return service.ApplyCount;
            });

            Assert.Equal(1, calls);
        }

        [Fact]
        public void Suspend_PreservesWhatResumeRestores()
        {
            var last = Ui.Get(() =>
            {
                var service = new RecordingService();

                using var coordinator = new HotKeyCoordinator(service);

                coordinator.Apply(SettingsWith("Ctrl+Shift+S", "Alt+F4"));
                coordinator.Suspend();
                coordinator.Resume();

                return service.LastDesired;
            });

            Assert.Equal(
                HotKeyModifiers.Control | HotKeyModifiers.Shift,
                last[HotKeyAction.CreateSave].Modifiers);

            Assert.Equal(HotKeyModifiers.Alt, last[HotKeyAction.LoadSave].Modifiers);
        }

        private sealed class RecordingService : IGlobalHotKeyService
        {
            public event Action<HotKeyAction>? HotKeyPressed
            {
                add { }
                remove { }
            }

            public bool IsSupported => true;
            public string? UnsupportedReason => null;

            public int ApplyCount { get; private set; }

            public Dictionary<HotKeyAction, HotKeyGesture> LastDesired { get; private set; } = new();

            public bool IsEverythingBound =>
                LastDesired.Count > 0 && LastDesired.Values.All(g => g.IsValid);

            public IReadOnlyDictionary<HotKeyAction, HotKeyRegistrationResult> Apply(
                IReadOnlyDictionary<HotKeyAction, HotKeyGesture> desired)
            {
                ApplyCount++;

                LastDesired = new Dictionary<HotKeyAction, HotKeyGesture>(desired);

                var results = new Dictionary<HotKeyAction, HotKeyRegistrationResult>();

                foreach (var (action, gesture) in desired)
                {
                    results[action] = gesture.IsValid
                        ? HotKeyRegistrationResult.Registered
                        : HotKeyRegistrationResult.Skipped;
                }

                return results;
            }

            public void Dispose()
            {
            }
        }
    }
}