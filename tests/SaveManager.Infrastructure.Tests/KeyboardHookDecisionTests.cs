using SaveManager.Infrastructure.HotKeys.Windows;

namespace SaveManager.Infrastructure.Tests
{
    // the filters a low level hook applies before matching
    public class KeyboardHookDecisionTests
    {
        private const int WmKeyDown = 0x0100;
        private const int WmKeyUp = 0x0101;
        private const int WmSysKeyDown = 0x0104;
        private const uint NoFlags = 0;
        private const uint Injected = 0x10;

        [Fact]
        public void NormalKeyPress_Dispatches()
        {
            Assert.True(KeyboardHookDecision.ShouldDispatch(0, WmKeyDown, NoFlags, blocked: false));
        }

        [Theory]
        [InlineData(-1)]
        [InlineData(-2)]
        public void NegativeCode_DoesNotDispatch(int code)
        {
            Assert.False(KeyboardHookDecision.ShouldDispatch(code, WmKeyDown, NoFlags, blocked: false));
        }

        [Fact]
        public void KeyRelease_DoesNotDispatch()
        {
            Assert.False(KeyboardHookDecision.ShouldDispatch(0, WmKeyUp, NoFlags, blocked: false));
        }

        [Fact]
        public void AltCombination_Dispatches()
        {
            // Alt combinations arrive as WM_SYSKEYDOWN, not WM_KEYDOWN
            Assert.True(KeyboardHookDecision.ShouldDispatch(0, WmSysKeyDown, NoFlags, blocked: false));
        }

        [Fact]
        public void InjectedInput_DoesNotDispatch()
        {
            Assert.False(KeyboardHookDecision.ShouldDispatch(0, WmKeyDown, Injected, blocked: false));
        }

        [Fact]
        public void BlockedInput_DoesNotDispatch()
        {
            Assert.False(KeyboardHookDecision.ShouldDispatch(0, WmKeyDown, NoFlags, blocked: true));
        }

        [Fact]
        public void ModifierStateIsTrackedForNormalCodes()
        {
            Assert.True(KeyboardHookDecision.ShouldTrackModifier(0));
            Assert.False(KeyboardHookDecision.ShouldTrackModifier(-1));
        }

        [Theory]
        [InlineData(0x0100, true, false)]
        [InlineData(0x0101, false, true)]
        [InlineData(0x0104, true, false)]
        [InlineData(0x0105, false, true)]
        public void PressAndRelease_AreDistinguished(int message, bool down, bool up)
        {
            Assert.Equal(down, KeyboardHookDecision.IsKeyDown(message));
            Assert.Equal(up, KeyboardHookDecision.IsKeyUp(message));
        }

        [Fact]
        public void UnknownMessage_IsNeitherPressNorRelease()
        {
            Assert.False(KeyboardHookDecision.IsKeyDown(0x9999));
            Assert.False(KeyboardHookDecision.IsKeyUp(0x9999));
        }

        [Fact]
        public void InjectedFlag_IsDetected()
        {
            Assert.True(KeyboardHookDecision.IsInjected(0x10));
            Assert.True(KeyboardHookDecision.IsInjected(0x11));
            Assert.False(KeyboardHookDecision.IsInjected(0));
            Assert.False(KeyboardHookDecision.IsInjected(0x01));
        }
    }
}
