using SaveManager.Domain.Entities;
using SaveManager.Domain.Enums;

namespace SaveManager.Infrastructure.HotKeys
{
    public abstract class HotKeyKeyTranslator
    {
        protected static IEnumerable<HotKeyKey> AllKeys =>
            Enum.GetValues<HotKeyKey>().Where(k => k != HotKeyKey.None);

        protected static HotKeyModifiers KnownModifiers =>
            HotKeyModifiers.Alt | HotKeyModifiers.Control |
            HotKeyModifiers.Shift | HotKeyModifiers.Meta;

        public static HotKeyRegistrationResult Validate(HotKeyGesture gesture)
        {
            if (!gesture.IsValid)
                return HotKeyRegistrationResult.UnsupportedGesture(
                    "A hotkey needs a key.");

            if ((gesture.Modifiers & ~KnownModifiers) != 0)
                return HotKeyRegistrationResult.UnsupportedGesture(
                    "This hotkey uses a modifier that is not supported.");

            return HotKeyRegistrationResult.Registered;
        }

        public abstract uint Translate(HotKeyGesture gesture);
    }
}