using SaveManager.Domain.Entities;
using SaveManager.Domain.Enums;
using SaveManager.Infrastructure.HotKeys;
using SaveManager.Infrastructure.HotKeys.Windows;

namespace SaveManager.Infrastructure.Tests
{
    // the matcher decides if an observed keystroke fires an action
    public class HotKeyHookMatcherTests
    {
        private const uint VkS = 0x53;
        private const uint VkF7 = 0x76;

        private static HotKeyHookMatcher Matcher(params (HotKeyAction Action, HotKeyGesture Gesture)[] bindings)
        {
            var desired = new Dictionary<HotKeyAction, HotKeyGesture>();

            foreach (var (action, gesture) in bindings)
                desired[action] = gesture;

            var matcher = new HotKeyHookMatcher();
            matcher.Replace(desired);

            return matcher;
        }

        [Fact]
        public void ExactCombination_Fires()
        {
            var matcher = Matcher((HotKeyAction.LoadSave,
                new HotKeyGesture(HotKeyModifiers.Control, HotKeyKey.S)));

            Assert.Equal(HotKeyAction.LoadSave, matcher.Match(VkS, HotKeyModifiers.Control));
        }

        [Fact]
        public void BareKey_Fires()
        {
            var matcher = Matcher((HotKeyAction.CreateSave,
                new HotKeyGesture(HotKeyModifiers.None, HotKeyKey.S)));

            Assert.Equal(HotKeyAction.CreateSave, matcher.Match(VkS, HotKeyModifiers.None));
        }

        [Fact]
        public void ExtraModifier_DoesNotFire()
        {
            var matcher = Matcher((HotKeyAction.LoadSave,
                new HotKeyGesture(HotKeyModifiers.Control, HotKeyKey.S)));

            Assert.Null(matcher.Match(VkS, HotKeyModifiers.Control | HotKeyModifiers.Shift));
        }

        [Fact]
        public void MissingModifier_DoesNotFire()
        {
            var matcher = Matcher((HotKeyAction.LoadSave,
                new HotKeyGesture(HotKeyModifiers.Control, HotKeyKey.S)));

            Assert.Null(matcher.Match(VkS, HotKeyModifiers.None));
        }

        [Fact]
        public void UnboundKey_DoesNotFire()
        {
            var matcher = Matcher((HotKeyAction.LoadSave,
                new HotKeyGesture(HotKeyModifiers.Control, HotKeyKey.S)));

            Assert.Null(matcher.Match(VkF7, HotKeyModifiers.Control));
        }

        [Fact]
        public void BlankGesture_IsIgnored()
        {
            var matcher = Matcher((HotKeyAction.LoadSave, HotKeyGesture.None));

            Assert.Equal(0, matcher.Count);
            Assert.Null(matcher.Match(VkS, HotKeyModifiers.None));
        }

        [Fact]
        public void TwoBindings_ResolveIndependently()
        {
            var matcher = Matcher(
                (HotKeyAction.LoadSave, new HotKeyGesture(HotKeyModifiers.Control, HotKeyKey.S)),
                (HotKeyAction.NextSave, new HotKeyGesture(HotKeyModifiers.Alt, HotKeyKey.F7)));

            Assert.Equal(2, matcher.Count);
            Assert.Equal(HotKeyAction.LoadSave, matcher.Match(VkS, HotKeyModifiers.Control));
            Assert.Equal(HotKeyAction.NextSave, matcher.Match(VkF7, HotKeyModifiers.Alt));
        }

        [Fact]
        public void Replace_DropsThePreviousSet()
        {
            var matcher = Matcher((HotKeyAction.LoadSave,
                new HotKeyGesture(HotKeyModifiers.Control, HotKeyKey.S)));

            matcher.Replace(new Dictionary<HotKeyAction, HotKeyGesture>
            {
                [HotKeyAction.NextSave] = new HotKeyGesture(HotKeyModifiers.Alt, HotKeyKey.F7)
            });

            Assert.Null(matcher.Match(VkS, HotKeyModifiers.Control));
            Assert.Equal(HotKeyAction.NextSave, matcher.Match(VkF7, HotKeyModifiers.Alt));
        }

        [Theory]
        [InlineData(0xA0, HotKeyModifiers.Shift)]
        [InlineData(0xA1, HotKeyModifiers.Shift)]
        [InlineData(0xA2, HotKeyModifiers.Control)]
        [InlineData(0xA3, HotKeyModifiers.Control)]
        [InlineData(0xA4, HotKeyModifiers.Alt)]
        [InlineData(0xA5, HotKeyModifiers.Alt)]
        [InlineData(0x5B, HotKeyModifiers.Meta)]
        [InlineData(0x5C, HotKeyModifiers.Meta)]
        public void ModifierKeys_AreRecognisedOnEitherSide(uint vk, HotKeyModifiers expected)
        {
            Assert.Equal(expected, HotKeyHookMatcher.ModifierFor(vk));
        }

        [Fact]
        public void OrdinaryKey_IsNotAModifier()
        {
            Assert.Null(HotKeyHookMatcher.ModifierFor(VkS));
        }

        [Fact]
        public void Validate_AcceptsSingleKeyAndCombination()
        {
            Assert.True(HotKeyKeyTranslator.Validate(
                new HotKeyGesture(HotKeyModifiers.None, HotKeyKey.S)).IsRegistered);

            Assert.True(HotKeyKeyTranslator.Validate(
                new HotKeyGesture(HotKeyModifiers.Control, HotKeyKey.S)).IsRegistered);
        }

        [Fact]
        public void Modifiers_AreMappedWithoutTheRegistrationOnlyFlag()
        {
            var mods = WindowsKeyTranslator.TranslateModifiers(HotKeyModifiers.Control);

            Assert.Equal(0x0002u, mods);
            Assert.Equal(0u, mods & WindowsKeyTranslator.ModNoRepeat);
        }
    }
}