using SaveManager.Domain.Entities;
using SaveManager.Domain.Enums;

namespace SaveManager.Infrastructure.HotKeys.Windows
{
    public sealed class HotKeyHookMatcher
    {
        private readonly Dictionary<uint, Binding> _byKeyCode = new();

        private readonly record struct Binding(HotKeyModifiers Modifiers, HotKeyAction Action);

        public void Replace(IReadOnlyDictionary<HotKeyAction, HotKeyGesture> desired)
        {
            _byKeyCode.Clear();

            foreach (var (action, gesture) in desired)
            {
                if (!gesture.IsValid)
                    continue;

                if (!WindowsKeyTranslator.TryTranslate(gesture, out var keyCode))
                    continue;

                _byKeyCode[keyCode] = new Binding(gesture.Modifiers, action);
            }
        }

        public int Count => _byKeyCode.Count;

        public HotKeyAction? Match(uint virtualKeyCode, HotKeyModifiers modifiers)
        {
            if (!_byKeyCode.TryGetValue(virtualKeyCode, out var binding))
                return null;

            return binding.Modifiers == modifiers ? binding.Action : null;
        }

        public static HotKeyModifiers? ModifierFor(uint virtualKeyCode) =>
            WindowsVirtualKeys.ToModifier(virtualKeyCode);
    }
}