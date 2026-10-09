using SaveManager.Domain.Entities;
using SaveManager.Domain.Enums;
using SaveManager.Infrastructure.HotKeys;
using SaveManager.Infrastructure.HotKeys.Windows;

namespace SaveManager.Infrastructure.Tests
{
    public class HotKeyTranslatorTests
    {
        [Fact]
        public void Validate_RejectsGestureWithNoKey()
        {
            var result = HotKeyKeyTranslator.Validate(HotKeyGesture.None);

            Assert.True(result.IsFailure);
            Assert.Equal(HotKeyRegistrationStatus.UnsupportedGesture, result.Status);
        }

        [Fact]
        public void Validate_AcceptsSingleKeyWithoutModifiers()
        {
            var gesture = new HotKeyGesture(HotKeyModifiers.None, HotKeyKey.S);

            Assert.True(HotKeyKeyTranslator.Validate(gesture).IsRegistered);
        }

        [Fact]
        public void Validate_AcceptsGestureWithOneModifier()
        {
            var gesture = new HotKeyGesture(HotKeyModifiers.Meta, HotKeyKey.S);

            Assert.True(HotKeyKeyTranslator.Validate(gesture).IsRegistered);
        }

        [Fact]
        public void Windows_TranslatesLetterToAsciiVirtualKey()
        {
            var gesture = new HotKeyGesture(HotKeyModifiers.Control, HotKeyKey.A);

            Assert.True(WindowsKeyTranslator.TryTranslate(gesture, out var vk));
            Assert.Equal(0x41u, vk);
        }

        [Fact]
        public void Windows_TranslatesTopRowDigitsInOneToZeroOrder()
        {
            // VK_1..VK_9 then VK_0, so D0 must not come out as VK_0+9
            Assert.True(WindowsKeyTranslator.TryTranslate(
                new HotKeyGesture(HotKeyModifiers.Control, HotKeyKey.D1), out var one));
            Assert.True(WindowsKeyTranslator.TryTranslate(
                new HotKeyGesture(HotKeyModifiers.Control, HotKeyKey.D0), out var zero));

            Assert.Equal(0x31u, one);
            Assert.Equal(0x30u, zero);
        }

        [Fact]
        public void Windows_MapsKnownModifierFlags()
        {
            var mods = WindowsKeyTranslator.TranslateModifiers(
                HotKeyModifiers.Control | HotKeyModifiers.Shift);

            Assert.Equal(0x0002u, mods & 0x0002); // MOD_CONTROL
            Assert.Equal(0x0004u, mods & 0x0004); // MOD_SHIFT
        }
    }
}
