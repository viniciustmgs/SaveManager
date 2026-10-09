using Avalonia.Data.Converters;
using SaveManager.Domain.Enums;
using System;
using System.Globalization;

namespace SaveManager.UI.ViewModels
{
    public sealed class SaveSortOptionToTextConverter : IValueConverter
    {
        public static SaveSortOptionToTextConverter Instance { get; } = new();

        public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            return value is SaveSortOption option
                ? option switch
                {
                    SaveSortOption.Created => "Created",
                    SaveSortOption.AlphabetAscending => "Alphabet A-Z",
                    SaveSortOption.AlphabetDescending => "Alphabet Z-A",
                    _ => option.ToString()
                }
                : string.Empty;
        }

        public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            throw new NotSupportedException();
        }
    }
}
