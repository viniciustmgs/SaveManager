using SaveManager.Domain.Enums;

namespace SaveManager.Application.UseCases.Save
{
    public class SortSavesUseCase
    {
        public List<Domain.Entities.Save> Execute(
            IEnumerable<Domain.Entities.Save> saves, SaveSortOption option)
        {
            return option switch
            {
                // the filesystem hands these back in creation order, but only incidentally,
                // so order by the timestamp explicitly to make it a real guarantee
                SaveSortOption.Created =>
                    saves.OrderBy(save => save.CreatedAt).ToList(),

                SaveSortOption.AlphabetAscending =>
                    saves.OrderBy(save => save.Name, StringComparer.OrdinalIgnoreCase).ToList(),

                SaveSortOption.AlphabetDescending =>
                    saves.OrderByDescending(save => save.Name, StringComparer.OrdinalIgnoreCase).ToList(),

                _ => saves.OrderBy(save => save.CreatedAt).ToList()
            };
        }
    }
}
