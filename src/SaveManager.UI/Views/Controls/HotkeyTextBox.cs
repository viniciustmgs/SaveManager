using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Media;
using SaveManager.UI.HotKeys;
using System;

namespace SaveManager.UI.Views.Controls
{
    public class HotkeyTextBox : TextBox
    {
        public const string DefaultPlaceholder = "Press a Key";
        public const string UnsupportedPlaceholder = "Key not supported";

        public static readonly SolidColorBrush DefaultPlaceholderForeground =
            new(Color.FromRgb(0x88, 0x88, 0x88));

        public static readonly SolidColorBrush ErrorPlaceholderForeground =
            new(Color.FromRgb(0xF4, 0x43, 0x36));

        protected override Type StyleKeyOverride => typeof(TextBox);

        public HotkeyTextBox()
        {
            IsReadOnly = true;

            CaretBrush = Brushes.Transparent;

            TextAlignment = TextAlignment.Center;
            PlaceholderForeground = DefaultPlaceholderForeground;
            PlaceholderText = string.Empty;
        }

        protected override void OnGotFocus(FocusChangedEventArgs e)
        {
            base.OnGotFocus(e);

            Text = string.Empty;
            PlaceholderForeground = DefaultPlaceholderForeground;
            PlaceholderText = DefaultPlaceholder;
        }

        protected override void OnLostFocus(FocusChangedEventArgs e)
        {
            base.OnLostFocus(e);

            PlaceholderText = string.Empty;
        }

        protected override void OnKeyDown(KeyEventArgs e)
        {
            base.OnKeyDown(e);

            if (IsModifier(e.Key))
            {
                e.Handled = true;
                return;
            }

            var gesture = new KeyGesture(e.Key, e.KeyModifiers);

            if (!HotKeyGestureConverter.TryConvert(gesture, out _))
            {
                Text = string.Empty;
                PlaceholderForeground = ErrorPlaceholderForeground;
                PlaceholderText = UnsupportedPlaceholder;

                e.Handled = true;
                return;
            }

            Text = gesture.ToString();
            e.Handled = true;

            Blur();
        }

        private void Blur() => TopLevel.GetTopLevel(this)?.FocusManager?.Focus(null);

        private static bool IsModifier(Key key) => key
            is Key.LeftCtrl or Key.RightCtrl
            or Key.LeftShift or Key.RightShift
            or Key.LeftAlt or Key.RightAlt
            or Key.LWin or Key.RWin
            or Key.None;
    }
}