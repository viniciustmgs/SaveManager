namespace SaveManager.Application.UseCases.Save
{
    public class FilterSavesUseCase
    {
        public List<Domain.Entities.Save> Execute(
            IEnumerable<Domain.Entities.Save> saves, string? term)
        {
            if (string.IsNullOrWhiteSpace(term))
                return saves.ToList();

            var needle = term.Trim();

            return saves
                .Where(save => save.Name.Contains(needle, StringComparison.OrdinalIgnoreCase))
                .ToList();
        }
    }
}
