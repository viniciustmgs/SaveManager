namespace SaveManager.Domain.Interfaces
{
    public interface IHotKeyWindowHost
    {
        bool IsAvailable { get; }

        IntPtr Handle { get; }

        IDisposable AddMessageHook(Func<IntPtr, uint, IntPtr, IntPtr, bool> handler);
    }
}