using SaveManager.Domain.Entities;
using SaveManager.Domain.Enums;

namespace SaveManager.Domain.Tests
{
    public class HotKeyGestureTests
    {
        [Fact]
        public void DefaultGesture_IsInvalid()
        {
            Assert.False(HotKeyGesture.None.IsValid);
        }

        [Fact]
        public void GestureWithKeyAndModifier_IsValid()
        {
            var gesture = new HotKeyGesture(HotKeyModifiers.Control, HotKeyKey.S);

            Assert.True(gesture.IsValid);
        }

        [Fact]
        public void SingleKeyWithoutModifiers_IsValid()
        {
            var gesture = new HotKeyGesture(HotKeyModifiers.None, HotKeyKey.S);

            Assert.True(gesture.IsValid);
        }

        [Fact]
        public void GesturesWithSameKeyAndModifiers_AreEqual()
        {
            var a = new HotKeyGesture(HotKeyModifiers.Control | HotKeyModifiers.Shift, HotKeyKey.S);
            var b = new HotKeyGesture(HotKeyModifiers.Shift | HotKeyModifiers.Control, HotKeyKey.S);

            Assert.Equal(a, b);
            Assert.True(a == b);
            Assert.Equal(a.GetHashCode(), b.GetHashCode());
        }

        [Fact]
        public void GesturesDifferingInModifier_AreNotEqual()
        {
            var a = new HotKeyGesture(HotKeyModifiers.Control, HotKeyKey.S);
            var b = new HotKeyGesture(HotKeyModifiers.Control | HotKeyModifiers.Shift, HotKeyKey.S);

            Assert.NotEqual(a, b);
            Assert.True(a != b);
        }

        [Fact]
        public void GesturesDifferingInKey_AreNotEqual()
        {
            var a = new HotKeyGesture(HotKeyModifiers.Control, HotKeyKey.S);
            var b = new HotKeyGesture(HotKeyModifiers.Control, HotKeyKey.D);

            Assert.NotEqual(a, b);
        }
    }
}