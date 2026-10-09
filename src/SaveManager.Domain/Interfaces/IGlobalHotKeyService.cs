using SaveManager.Domain.Entities;
using SaveManager.Domain.Enums;

namespace SaveManager.Domain.Interfaces
{
    public interface IGlobalHotKeyService : IDisposable
    {
        event Action<HotKeyAction>? HotKeyPressed;

        bool IsSupported { get; }

        string? UnsupportedReason { get; }

        IReadOnlyDictionary<HotKeyAction, HotKeyRegistrationResult> Apply(
            IReadOnlyDictionary<HotKeyAction, HotKeyGesture> desired);
    }
}