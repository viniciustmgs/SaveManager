using SaveManager.Infrastructure.HotKeys.Windows;

namespace SaveManager.Infrastructure.Tests
{
    public class KeyboardHookPumpTests
    {
        [Theory]
        [InlineData(0x0100)]
        [InlineData(0x0001)]
        public void Message_Dispatches(int result)
        {
            Assert.Equal(
                KeyboardHookPump.Outcome.Dispatch,
                KeyboardHookPump.Decide(stopRequested: false, result));
        }

        [Fact]
        public void Quit_Stops()
        {
            Assert.Equal(
                KeyboardHookPump.Outcome.Stop,
                KeyboardHookPump.Decide(stopRequested: false, 0));
        }

        [Fact]
        public void ErrorResult_IsTransientAndDoesNotStop()
        {
            Assert.Equal(
                KeyboardHookPump.Outcome.TransientError,
                KeyboardHookPump.Decide(stopRequested: false, -1));
        }

        [Theory]
        [InlineData(0x0100)]
        [InlineData(0)]
        [InlineData(-1)]
        public void StopRequested_WinsOverEveryResult(int result)
        {
            Assert.Equal(
                KeyboardHookPump.Outcome.Stop,
                KeyboardHookPump.Decide(stopRequested: true, result));
        }
    }
}