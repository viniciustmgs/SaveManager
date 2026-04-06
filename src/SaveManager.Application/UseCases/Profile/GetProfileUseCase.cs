using SaveManager.Domain.Interfaces;

namespace SaveManager.Application.UseCases.Profile
{
    public class GetProfileUseCase
    {
        private readonly ISaveFileService _saveFileService;
        private readonly IGameRepository _gameRepository;

        public GetProfileUseCase(ISaveFileService saveFileService, IGameRepository gameRepository)
        {
            _saveFileService = saveFileService;
            _gameRepository = gameRepository;
        }

        public List<Domain.Entities.Profile> Execute(Guid gameId)
        {
            var game = _gameRepository.GetById(gameId);

            if (game == null)
                throw new ArgumentException("Game not found");

            return _saveFileService.ReadProfiles(game);
        }
    }
}
