using SaveManager.Domain.Enums;
using System;

namespace SaveManager.Domain.Entities
{
    // a key combination in platform neutral form
    public readonly struct HotKeyGesture : IEquatable<HotKeyGesture>
    {
        public static readonly HotKeyGesture None = new(HotKeyModifiers.None, HotKeyKey.None);

        public HotKeyGesture(HotKeyModifiers modifiers, HotKeyKey key)
        {
            Modifiers = modifiers;
            Key = key;
        }

        public HotKeyModifiers Modifiers { get; }
        public HotKeyKey Key { get; }

        public bool IsValid => Key != HotKeyKey.None;

        public bool Equals(HotKeyGesture other) => Modifiers == other.Modifiers && Key == other.Key;
        public override bool Equals(object? obj) => obj is HotKeyGesture other && Equals(other);
        public override int GetHashCode() => HashCode.Combine((int)Modifiers, (int)Key);
        public override string ToString() => $"{Modifiers}+{Key}";

        public static bool operator ==(HotKeyGesture left, HotKeyGesture right) => left.Equals(right);
        public static bool operator !=(HotKeyGesture left, HotKeyGesture right) => !left.Equals(right);
    }
}