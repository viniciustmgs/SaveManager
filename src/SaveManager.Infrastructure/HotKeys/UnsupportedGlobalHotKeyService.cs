using SaveManager.Domain.Entities;
using SaveManager.Domain.Enums;
using SaveManager.Domain.Interfaces;

namespace SaveManager.Infrastructure.HotKeys
{
    public sealed class UnsupportedGlobalHotKeyService : IGlobalHotKeyService
    {
        private readonly string _reason;

        public event Action<HotKeyAction>? HotKeyPressed
        {
            add { }
            remove { }
        }

        public bool IsSupported => false;

        public string? UnsupportedReason => _reason;

        public UnsupportedGlobalHotKeyService(string reason)
        {
            _reason = reason;
        }

        public IReadOnlyDictionary<HotKeyAction, HotKeyRegistrationResult> Apply(
            IReadOnlyDictionary<HotKeyAction, HotKeyGesture> desired)
        {
            var results = new Dictionary<HotKeyAction, HotKeyRegistrationResult>();

            foreach (var (action, gesture) in desired)
            {
                results[action] = gesture.IsValid
                    ? HotKeyRegistrationResult.Failed(_reason)
                    : HotKeyRegistrationResult.Skipped;
            }

            return results;
        }

        public void Dispose()
        {
        }
    }
}