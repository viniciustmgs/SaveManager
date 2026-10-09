using Avalonia.Input;
using SaveManager.Domain.Entities;
using SaveManager.Domain.Enums;
using System;
using System.Collections.Generic;

namespace SaveManager.UI.HotKeys
{
    // bridges avalonia's input vocabulary and the platform neutral one
    public static class HotKeyGestureConverter
    {
        private static readonly Dictionary<Key, HotKeyKey> Aliases = new()
        {
            [Key.Back] = HotKeyKey.Backspace
        };

        private static readonly Dictionary<Key, HotKeyKey> ForwardMap = BuildForwardMap();
        private static readonly Dictionary<HotKeyKey, Key> ReverseMap = BuildReverseMap();

        public static bool TryParse(string? value, out HotKeyGesture gesture)
        {
            gesture = HotKeyGesture.None;

            if (string.IsNullOrWhiteSpace(value))
                return false;

            KeyGesture keyGesture;

            try
            {
                keyGesture = KeyGesture.Parse(value);
            }
            catch (Exception)
            {
                return false;
            }

            return TryConvert(keyGesture, out gesture);
        }

        public static bool TryConvert(KeyGesture keyGesture, out HotKeyGesture gesture)
        {
            gesture = HotKeyGesture.None;

            if (!TryMapKey(keyGesture.Key, out var key))
                return false;

            gesture = new HotKeyGesture(MapModifiers(keyGesture.KeyModifiers), key);
            return true;
        }

        public static string Format(HotKeyGesture gesture)
        {
            if (!gesture.IsValid)
                return string.Empty;

            var key = UnmapKey(gesture.Key);

            if (key == Key.None)
                return string.Empty;

            return new KeyGesture(key, UnmapModifiers(gesture.Modifiers)).ToString();
        }

        public static HotKeyModifiers MapModifiers(KeyModifiers modifiers)
        {
            var result = HotKeyModifiers.None;

            if (modifiers.HasFlag(KeyModifiers.Alt)) result |= HotKeyModifiers.Alt;
            if (modifiers.HasFlag(KeyModifiers.Control)) result |= HotKeyModifiers.Control;
            if (modifiers.HasFlag(KeyModifiers.Shift)) result |= HotKeyModifiers.Shift;
            if (modifiers.HasFlag(KeyModifiers.Meta)) result |= HotKeyModifiers.Meta;

            return result;
        }

        public static KeyModifiers UnmapModifiers(HotKeyModifiers modifiers)
        {
            var result = KeyModifiers.None;

            if (modifiers.HasFlag(HotKeyModifiers.Alt)) result |= KeyModifiers.Alt;
            if (modifiers.HasFlag(HotKeyModifiers.Control)) result |= KeyModifiers.Control;
            if (modifiers.HasFlag(HotKeyModifiers.Shift)) result |= KeyModifiers.Shift;
            if (modifiers.HasFlag(HotKeyModifiers.Meta)) result |= KeyModifiers.Meta;

            return result;
        }

        private static bool TryMapKey(Key key, out HotKeyKey mapped)
        {
            mapped = HotKeyKey.None;

            if (key == Key.None)
                return false;

            if (Aliases.TryGetValue(key, out mapped))
                return true;

            return ForwardMap.TryGetValue(key, out mapped);
        }

        private static Key UnmapKey(HotKeyKey key) =>
            ReverseMap.TryGetValue(key, out var mapped) ? mapped : Key.None;

        private static Dictionary<Key, HotKeyKey> BuildForwardMap()
        {
            var map = new Dictionary<Key, HotKeyKey>();

            foreach (var name in Enum.GetNames<HotKeyKey>())
            {
                if (name == nameof(HotKeyKey.None))
                    continue;

                if (Enum.TryParse<Key>(name, out var key) && key != Key.None)
                    map[key] = Enum.Parse<HotKeyKey>(name);
            }

            return map;
        }

        private static Dictionary<HotKeyKey, Key> BuildReverseMap()
        {
            var map = new Dictionary<HotKeyKey, Key>();

            foreach (var (key, portable) in ForwardMap)
            {
                if (!map.ContainsKey(portable))
                    map[portable] = key;
            }

            foreach (var (key, portable) in Aliases)
                map[portable] = key;

            return map;
        }
    }
}