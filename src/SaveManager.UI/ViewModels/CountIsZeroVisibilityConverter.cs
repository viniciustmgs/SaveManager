using Avalonia.Data.Converters;
using System;
using System.Globalization;

namespace SaveManager.UI.ViewModels
{
    public sealed class CountIsZeroVisibilityConverter : IValueConverter
    {
        public static CountIsZeroVisibilityConverter Instance { get; } = new();

        public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture) =>
            value is int count && count == 0;

        public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture) =>
            throw new NotSupportedException();
    }
}
