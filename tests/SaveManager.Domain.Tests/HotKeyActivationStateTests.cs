using SaveManager.Domain.Entities;
using SaveManager.Domain.Enums;

namespace SaveManager.Domain.Tests
{
    public class HotKeyActivationStateTests
    {
        private static readonly HotKeyAction[] SaveActions =
        {
            HotKeyAction.CreateSave,
            HotKeyAction.LoadSave,
            HotKeyAction.NextSave,
            HotKeyAction.PreviousSave
        };

        [Fact]
        public void Defaults_ToDisabledMasterSwitchAndUnmutedOthers()
        {
            var state = new HotKeyActivationState();

            Assert.False(state.GlobalEnabled);
            Assert.True(state.OthersEnabled);
        }

        [Fact]
        public void MasterSwitchOff_BlocksEverythingIncludingToggle()
        {
            var state = new HotKeyActivationState();

            state.SetGlobalEnabled(false);

            foreach (var action in SaveActions)
                Assert.False(state.CanDispatch(action));

            Assert.False(state.CanDispatch(HotKeyAction.ToggleGlobalHotkeys));
        }

        [Fact]
        public void MasterSwitchOn_DispatchesSaveActions()
        {
            var state = new HotKeyActivationState();

            state.SetGlobalEnabled(true);

            foreach (var action in SaveActions)
                Assert.True(state.CanDispatch(action));
        }

        [Fact]
        public void Toggle_BlocksSaveActionsButNotItself()
        {
            var state = new HotKeyActivationState();
            state.SetGlobalEnabled(true);

            state.Toggle();

            Assert.False(state.OthersEnabled);

            foreach (var action in SaveActions)
                Assert.False(state.CanDispatch(action));

            Assert.True(state.CanDispatch(HotKeyAction.ToggleGlobalHotkeys));
        }

        [Fact]
        public void Toggle_TwiceUnmutesSaveActions()
        {
            var state = new HotKeyActivationState();
            state.SetGlobalEnabled(true);

            state.Toggle();
            state.Toggle();

            Assert.True(state.OthersEnabled);
            Assert.True(state.CanDispatch(HotKeyAction.CreateSave));
        }

        [Fact]
        public void Toggle_IsIgnoredWhileMasterSwitchIsOff()
        {
            var state = new HotKeyActivationState();

            state.Toggle();

            Assert.True(state.OthersEnabled);
        }

        [Fact]
        public void DisablingMasterSwitch_ResetsMuteSoReEnablingWorks()
        {
            var state = new HotKeyActivationState();
            state.SetGlobalEnabled(true);
            state.Toggle();

            Assert.False(state.OthersEnabled);

            state.SetGlobalEnabled(false);

            Assert.True(state.OthersEnabled);

            state.SetGlobalEnabled(true);

            Assert.True(state.CanDispatch(HotKeyAction.CreateSave));
        }

        [Fact]
        public void ClearingToggleBinding_ResetsMute()
        {
            var state = new HotKeyActivationState();
            state.SetGlobalEnabled(true);
            state.Toggle();

            Assert.False(state.OthersEnabled);

            state.OnToggleBindingCleared();

            Assert.True(state.OthersEnabled);
            Assert.True(state.CanDispatch(HotKeyAction.CreateSave));
        }

        [Fact]
        public void OthersEnabledChanged_FiresOnToggle()
        {
            var state = new HotKeyActivationState();
            state.SetGlobalEnabled(true);

            var observed = new List<bool>();
            state.OthersEnabledChanged += observed.Add;

            state.Toggle();

            Assert.Equal(new[] { false }, observed);
        }

        [Fact]
        public void OthersEnabledChanged_FiresWhenMasterSwitchOffResetsMute()
        {
            var state = new HotKeyActivationState();
            state.SetGlobalEnabled(true);
            state.Toggle();

            var observed = new List<bool>();
            state.OthersEnabledChanged += observed.Add;

            state.SetGlobalEnabled(false);

            Assert.Equal(new[] { true }, observed);
        }

        [Fact]
        public void OthersEnabledChanged_DoesNotFireWhenAlreadyUnmuted()
        {
            var state = new HotKeyActivationState();
            state.SetGlobalEnabled(true);

            var observed = new List<bool>();
            state.OthersEnabledChanged += observed.Add;

            state.OnToggleBindingCleared();
            state.SetGlobalEnabled(false);

            Assert.Empty(observed);
        }

        [Fact]
        public void Toggle_FiresOthersEnabledChangedOnce()
        {
            var state = new HotKeyActivationState();
            state.SetGlobalEnabled(true);

            var observed = new List<bool>();
            state.OthersEnabledChanged += observed.Add;

            state.Toggle();
            state.Toggle();

            Assert.Equal(new[] { false, true }, observed);
        }
    }
}