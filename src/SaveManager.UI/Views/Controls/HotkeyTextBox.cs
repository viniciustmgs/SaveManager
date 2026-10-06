using System;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Media;

namespace SaveManager.UI.Views.Controls
{
    /// A read-only TextBox that captures a single key combination and displays it
    /// as a human readable gesture instead of the typed character.
    /// the stored value stays parseable by <see cref="KeyGesture.Parse(string)"/>.
    public class HotkeyTextBox : TextBox
    {
        public const string DefaultPlaceholder = "Press the Key";

        public static readonly SolidColorBrush DefaultPlaceholderForeground =
            new(Color.FromRgb(0x88, 0x88, 0x88));

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

            Text = new KeyGesture(e.Key, e.KeyModifiers).ToString();
            e.Handled = true;

            TopLevel.GetTopLevel(this)?.FocusManager?.Focus(null);
        }

        private static bool IsModifier(Key key) => key
            is Key.LeftCtrl or Key.RightCtrl
            or Key.LeftShift or Key.RightShift
            or Key.LeftAlt or Key.RightAlt
            or Key.LWin or Key.RWin
            or Key.None;
    }
}