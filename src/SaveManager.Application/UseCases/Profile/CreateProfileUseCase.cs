using SaveManager.Domain.Interfaces;

namespace SaveManager.Application.UseCases.Profile
{
    public class CreateProfileUseCase
    {
        private readonly ISaveFileService _saveFileService;
        private readonly IGameRepository _gameRepository;

        public CreateProfileUseCase(ISaveFileService saveFileService, IGameRepository gameRepository)
        {
            _saveFileService = saveFileService;
            _gameRepository = gameRepository;
        }

        public void Execute(Guid gameId, string profileName)
        {
            if (string.IsNullOrWhiteSpace(profileName))
                throw new ArgumentException("Profile name can not be empty");

            var game = _gameRepository.GetById(gameId);

            if (game == null)
                throw new ArgumentException("Game not found");

            _saveFileService.CreateProfile(game, profileName);
        }
    }
}
