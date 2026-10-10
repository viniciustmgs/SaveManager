namespace SaveManager.Infrastructure.HotKeys.Windows
{
    public static class KeyboardHookPump
    {
        public enum Outcome
        {
            Dispatch,
            Stop,
            TransientError
        }

        public static Outcome Decide(bool stopRequested, int getMessageResult)
        {
            if (stopRequested)
                return Outcome.Stop;

            if (getMessageResult < 0)
                return Outcome.TransientError;

            return getMessageResult == 0 ? Outcome.Stop : Outcome.Dispatch;
        }
    }
}