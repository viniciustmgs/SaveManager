using SaveManager.Domain.Enums;
using SaveManager.UI.ViewModels;
using System.Globalization;

namespace SaveManager.UI.Tests
{
    public class SaveSortOptionToTextConverterTests
    {
        private static string Convert(object? value) =>
            (string)SaveSortOptionToTextConverter.Instance.Convert(
                value, typeof(string), null, CultureInfo.InvariantCulture);

        [Theory]
        [InlineData(SaveSortOption.Created, "Created")]
        [InlineData(SaveSortOption.AlphabetAscending, "Alphabet A-Z")]
        [InlineData(SaveSortOption.AlphabetDescending, "Alphabet Z-A")]
        public void ConvertsEachOptionToTheWordingTheDropdownShows(SaveSortOption option, string expected)
        {
            Assert.Equal(expected, Convert(option));
        }

        [Fact]
        public void DoesNotFallBackToTheRawEnumName()
        {
            Assert.DoesNotContain("AlphabetAscending", Convert(SaveSortOption.AlphabetAscending));
        }

        [Fact]
        public void ReturnsEmptyForANullValue()
        {
            Assert.Equal(string.Empty, Convert(null));
        }
    }
}
