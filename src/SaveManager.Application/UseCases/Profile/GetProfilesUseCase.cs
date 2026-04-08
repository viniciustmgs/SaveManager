using SaveManager.Domain.Interfaces;

namespace SaveManager.Application.UseCases.Profile
{
    public class GetProfilesUseCase
    {
        private readonly ISaveFileService _saveFileService;

        public GetProfilesUseCase(ISaveFileService saveFileService, IGameRepository gameRepository)
        {
            _saveFileService = saveFileService;
        }

        public List<Domain.Entities.Profile> Execute(Domain.Entities.Game game)
        {
            return _saveFileService.ReadProfiles(game);
        }
    }
}
