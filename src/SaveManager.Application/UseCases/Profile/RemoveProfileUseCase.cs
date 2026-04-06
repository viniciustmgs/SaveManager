using SaveManager.Domain.Interfaces;

namespace SaveManager.Application.UseCases.Profile
{
    public class RemoveProfileUseCase
    {
        private readonly ISaveFileService _saveFileService;
        private readonly IGameRepository _gameRepository;

        public RemoveProfileUseCase(ISaveFileService saveFileService, IGameRepository gameRepository)
        {
            _saveFileService = saveFileService;
            _gameRepository = gameRepository;
        }

        public void Execute(Guid gameId, string profileName)
        {
            var game = _gameRepository.GetById(gameId);

            if (game == null)
                throw new ArgumentException("Game not found");

            var profiles = _saveFileService.ReadProfiles(game);
            var profile = profiles.FirstOrDefault(p => p.Name == profileName);

            if (profile == null)
                throw new ArgumentException("Profile not found");

            _saveFileService.DeleteProfile(profile);
        }
    }
}
