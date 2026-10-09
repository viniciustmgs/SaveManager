using Avalonia.Input;
using SaveManager.Domain.Entities;
using SaveManager.Domain.Enums;
using SaveManager.UI.HotKeys;

namespace SaveManager.UI.Tests
{
    public class HotKeyGestureConverterTests
    {
        [Theory]
        [InlineData("Ctrl+Shift+S")]
        [InlineData("Ctrl+Alt+Delete")]
        [InlineData("Meta+F1")]
        [InlineData("Alt+NumPad7")]
        [InlineData("Ctrl+Shift+Alt+Meta+Z")]
        public void TryParse_AcceptsStoredGestureFormat(string value)
        {
            Assert.True(HotKeyGestureConverter.TryParse(value, out var gesture));
            Assert.True(gesture.IsValid);
        }

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        [InlineData("   ")]
        [InlineData("not a gesture")]
        [InlineData("Ctrl+OemComma")]
        public void TryParse_RejectsUnusableValues(string? value)
        {
            Assert.False(HotKeyGestureConverter.TryParse(value, out _));
        }

        [Theory]
        [InlineData("NumPad7")]
        [InlineData("F5")]
        [InlineData("A")]
        public void TryParse_AcceptsSingleKeyWithoutModifiers(string value)
        {
            Assert.True(HotKeyGestureConverter.TryParse(value, out var gesture));
            Assert.Equal(HotKeyModifiers.None, gesture.Modifiers);
        }

        [Theory]
        [InlineData("Ctrl+Shift+S")]
        [InlineData("Ctrl+Alt+Delete")]
        [InlineData("Meta+F1")]
        [InlineData("Alt+NumPad7")]
        [InlineData("Ctrl+Shift+Alt+Meta+Z")]
        public void RoundTrip_PreservesStoredFormat(string value)
        {
            Assert.True(HotKeyGestureConverter.TryParse(value, out var gesture));

            var formatted = HotKeyGestureConverter.Format(gesture);

            Assert.True(HotKeyGestureConverter.TryParse(formatted, out var reparsed));
            Assert.Equal(gesture, reparsed);
        }

        [Fact]
        public void Format_RendersSingleKeyWithoutModifiers()
        {
            var gesture = new HotKeyGesture(HotKeyModifiers.None, HotKeyKey.S);

            Assert.Equal("S", HotKeyGestureConverter.Format(gesture));
        }

        [Fact]
        public void Format_ReturnsEmptyForInvalidGesture()
        {
            Assert.Equal(string.Empty, HotKeyGestureConverter.Format(HotKeyGesture.None));
        }

        [Fact]
        public void TryConvert_MapsModifiersToDomainFlags()
        {
            var keyGesture = new KeyGesture(Key.S, KeyModifiers.Control | KeyModifiers.Meta);

            Assert.True(HotKeyGestureConverter.TryConvert(keyGesture, out var gesture));
            Assert.Equal(HotKeyModifiers.Control | HotKeyModifiers.Meta, gesture.Modifiers);
            Assert.Equal(HotKeyKey.S, gesture.Key);
        }

        [Fact]
        public void TryConvert_AcceptsModifierlessKeyGesture()
        {
            var keyGesture = new KeyGesture(Key.F5, KeyModifiers.None);

            Assert.True(HotKeyGestureConverter.TryConvert(keyGesture, out var gesture));
            Assert.Equal(HotKeyKey.F5, gesture.Key);
            Assert.Equal(HotKeyModifiers.None, gesture.Modifiers);
        }

        [Theory]
        [InlineData(Key.Enter)]
        [InlineData(Key.Back)]
        [InlineData(Key.PageUp)]
        public void AliasKeys_MapToTheSamePortableKey(Key key)
        {
            var keyGesture = new KeyGesture(key, KeyModifiers.Control);

            Assert.True(HotKeyGestureConverter.TryConvert(keyGesture, out var gesture));
            Assert.True(gesture.IsValid);
        }

        [Fact]
        public void Modifiers_RoundTripThroughDomain()
        {
            var original = KeyModifiers.Alt | KeyModifiers.Control |
                KeyModifiers.Shift | KeyModifiers.Meta;

            var mapped = HotKeyGestureConverter.MapModifiers(original);

            Assert.Equal(original, HotKeyGestureConverter.UnmapModifiers(mapped));
        }
    }
}