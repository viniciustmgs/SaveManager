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
