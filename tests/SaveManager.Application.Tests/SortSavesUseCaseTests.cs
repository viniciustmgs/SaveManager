using SaveManager.Application.UseCases.Save;
using SaveManager.Domain.Entities;
using SaveManager.Domain.Enums;

namespace SaveManager.Application.Tests
{
    public class SortSavesUseCaseTests
    {
        private static Save Make(string name, int minute) => new()
        {
            Name = name,
            SavePath = $"/tmp/{name}",
            CreatedAt = new DateTime(2026, 1, 1, 0, minute, 0, DateTimeKind.Local)
        };

        private static List<Save> Unsorted() =>
        [
            Make("Charlie", 30),
            Make("alpha", 10),
            Make("Bravo", 20)
        ];

        [Fact]
        public void Created_OrdersOldestFirst()
        {
            var result = new SortSavesUseCase().Execute(Unsorted(), SaveSortOption.Created);

            Assert.Equal(["alpha", "Bravo", "Charlie"], result.Select(s => s.Name));
        }

        [Fact]
        public void AlphabetAscending_OrdersByName()
        {
            var result = new SortSavesUseCase().Execute(Unsorted(), SaveSortOption.AlphabetAscending);

            Assert.Equal(["alpha", "Bravo", "Charlie"], result.Select(s => s.Name));
        }

        [Fact]
        public void AlphabetDescending_ReversesTheAscendingOrder()
        {
            var result = new SortSavesUseCase().Execute(Unsorted(), SaveSortOption.AlphabetDescending);

            Assert.Equal(["Charlie", "Bravo", "alpha"], result.Select(s => s.Name));
        }

        [Fact]
        public void AlphabetAscending_IgnoresCaseSoMixedCaseNamesSortTogether()
        {
            // Ordinal would put every capital letter before every lower case one
            var result = new SortSavesUseCase().Execute(Unsorted(), SaveSortOption.AlphabetAscending);

            Assert.Equal("alpha", result[0].Name);
        }

        [Fact]
        public void Created_IsTheDefaultSortSoAnUnknownOptionStillBehavesLikeIt()
        {
            var result = new SortSavesUseCase().Execute(Unsorted(), (SaveSortOption)999);

            Assert.Equal(["alpha", "Bravo", "Charlie"], result.Select(s => s.Name));
        }

        [Fact]
        public void Execute_DoesNotMutateTheInputCollection()
        {
            var input = Unsorted();
            var before = input.Select(s => s.Name).ToList();

            new SortSavesUseCase().Execute(input, SaveSortOption.AlphabetDescending);

            Assert.Equal(before, input.Select(s => s.Name));
        }

        [Fact]
        public void Execute_HandlesAnEmptyList()
        {
            Assert.Empty(new SortSavesUseCase().Execute([], SaveSortOption.Created));
        }

        [Fact]
        public void Execute_HandlesASingleItem()
        {
            var result = new SortSavesUseCase().Execute([Make("solo", 5)], SaveSortOption.AlphabetDescending);

            Assert.Equal("solo", Assert.Single(result).Name);
        }
    }
}
