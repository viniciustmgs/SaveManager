using SaveManager.Application.UseCases.Save;

namespace SaveManager.Application.Tests
{
    public class FilterSavesUseCaseTests
    {
        private static Domain.Entities.Save Named(string name) => new() { Name = name };

        private static List<Domain.Entities.Save> Sample() =>
        [
            Named("hollow_knight"),
            Named("Hollow Knight"),
            Named("Dark Souls"),
            Named("ELDEN RING"),
            Named("no_match")
        ];

        [Fact]
        public void AnEmptyTermKeepsEverything()
        {
            var result = new FilterSavesUseCase().Execute(Sample(), "");

            Assert.Equal(5, result.Count);
        }

        [Fact]
        public void AWhitespaceOnlyTermKeepsEverything()
        {
            var result = new FilterSavesUseCase().Execute(Sample(), "   ");

            Assert.Equal(5, result.Count);
        }

        [Fact]
        public void ANullTermKeepsEverything()
        {
            Assert.Equal(5, new FilterSavesUseCase().Execute(Sample(), null).Count);
        }

        [Fact]
        public void MatchingIgnoresCase()
        {
            var result = new FilterSavesUseCase().Execute(Sample(), "HOLLOW");

            Assert.Equal(["hollow_knight", "Hollow Knight"], result.Select(s => s.Name));
        }

        [Fact]
        public void MatchingIsCaseInsensitiveInBothDirections()
        {
            Assert.Equal(2, new FilterSavesUseCase().Execute(Sample(), "hollow").Count);
            Assert.Equal(2, new FilterSavesUseCase().Execute(Sample(), "HOLLOW").Count);
            Assert.Equal(2, new FilterSavesUseCase().Execute(Sample(), "HoLlOw").Count);
        }

        [Fact]
        public void MatchingIsASubstringNotAPrefix()
        {
            var result = new FilterSavesUseCase().Execute(Sample(), "sou");

            Assert.Equal(["Dark Souls"], result.Select(s => s.Name));
        }

        [Fact]
        public void SurroundingWhitespaceInTheTermIsIgnored()
        {
            var result = new FilterSavesUseCase().Execute(Sample(), "  elden  ");

            Assert.Equal(["ELDEN RING"], result.Select(s => s.Name));
        }

        [Fact]
        public void NoMatchesYieldsAnEmptyList()
        {
            Assert.Empty(new FilterSavesUseCase().Execute(Sample(), "zzzz"));
        }

        [Fact]
        public void FilteringDoesNotMutateTheInputCollection()
        {
            var input = Sample();
            var before = input.Select(s => s.Name).ToList();

            new FilterSavesUseCase().Execute(input, "hollow");

            Assert.Equal(before, input.Select(s => s.Name));
        }

        [Fact]
        public void FilteringPreservesTheInputOrder()
        {
            var result = new FilterSavesUseCase().Execute(Sample(), "hollow");

            Assert.Equal(["hollow_knight", "Hollow Knight"], result.Select(s => s.Name));
        }
    }
}
