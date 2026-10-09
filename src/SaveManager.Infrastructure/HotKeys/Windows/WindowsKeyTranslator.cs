using SaveManager.Domain.Entities;
using SaveManager.Domain.Enums;

namespace SaveManager.Infrastructure.HotKeys.Windows
{
    // translates a gesture into a Win32 virtual key code
    public sealed class WindowsKeyTranslator : HotKeyKeyTranslator
    {
        private const uint VK_BACK = 0x08;
        private const uint VK_TAB = 0x09;
        private const uint VK_RETURN = 0x0D;
        private const uint VK_ESCAPE = 0x1B;
        private const uint VK_SPACE = 0x20;
        private const uint VK_PRIOR = 0x21;
        private const uint VK_NEXT = 0x22;
        private const uint VK_END = 0x23;
        private const uint VK_HOME = 0x24;
        private const uint VK_LEFT = 0x25;
        private const uint VK_UP = 0x26;
        private const uint VK_RIGHT = 0x27;
        private const uint VK_DOWN = 0x28;
        private const uint VK_INSERT = 0x2D;
        private const uint VK_DELETE = 0x2E;
        private const uint VK_0 = 0x30;
        private const uint VK_1 = 0x31;
        private const uint VK_NUMPAD0 = 0x60;

        public static readonly WindowsKeyTranslator Instance = new();

        private static readonly Dictionary<HotKeyKey, uint> VkByKey = BuildTable();

        private static Dictionary<HotKeyKey, uint> BuildTable()
        {
            var map = new Dictionary<HotKeyKey, uint>();

            for (var i = 0; i < 26; i++)
                map[(HotKeyKey)((int)HotKeyKey.A + i)] = (uint)(0x41 + i);

            for (var i = 0; i <= 9; i++)
            {
                // top row runs 1..0, so index 0 maps to VK_1 and index 9 to VK_0
                map[HotKeyKey.D1 + i] = (uint)(VK_1 + (i == 9 ? -1 : i));
                map[HotKeyKey.NumPad1 + i] = (uint)(VK_NUMPAD0 + (i == 9 ? -1 : i));
            }

            for (var i = 0; i < 12; i++)
                map[HotKeyKey.F1 + i] = (uint)(0x70 + i);

            foreach (var (key, vk) in new (HotKeyKey, uint)[]
            {
                (HotKeyKey.Space, VK_SPACE),
                (HotKeyKey.Enter, VK_RETURN),
                (HotKeyKey.Tab, VK_TAB),
                (HotKeyKey.Escape, VK_ESCAPE),
                (HotKeyKey.Backspace, VK_BACK),
                (HotKeyKey.Delete, VK_DELETE),
                (HotKeyKey.Insert, VK_INSERT),
                (HotKeyKey.Home, VK_HOME),
                (HotKeyKey.End, VK_END),
                (HotKeyKey.PageUp, VK_PRIOR),
                (HotKeyKey.PageDown, VK_NEXT),
                (HotKeyKey.Up, VK_UP),
                (HotKeyKey.Down, VK_DOWN),
                (HotKeyKey.Left, VK_LEFT),
                (HotKeyKey.Right, VK_RIGHT)
            })
            {
                map[key] = vk;
            }

            return map;
        }

        public override uint Translate(HotKeyGesture gesture)
        {
            return VkByKey.TryGetValue(gesture.Key, out var vk) ? vk : 0;
        }

        public static bool TryTranslate(HotKeyGesture gesture, out uint vk)
            => VkByKey.TryGetValue(gesture.Key, out vk);

        public static uint TranslateModifiers(HotKeyModifiers modifiers)
        {
            uint result = 0;

            if (modifiers.HasFlag(HotKeyModifiers.Alt)) result |= 0x0001;
            if (modifiers.HasFlag(HotKeyModifiers.Control)) result |= 0x0002;
            if (modifiers.HasFlag(HotKeyModifiers.Shift)) result |= 0x0004;
            if (modifiers.HasFlag(HotKeyModifiers.Meta)) result |= 0x0008;

            return result;
        }

        public const uint ModNoRepeat = 0x4000;
    }
}