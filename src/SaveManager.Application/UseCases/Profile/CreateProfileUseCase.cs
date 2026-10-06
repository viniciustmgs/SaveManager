using SaveManager.Application.Common;
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
            profileName = profileName.Trim();

            if (!FolderNameValidator.IsValidFolderName(profileName, out var error))
                throw new ArgumentException(error);

            var game = _gameRepository.GetById(gameId);

            if (game == null)
                throw new ArgumentException("Game not found");

            _saveFileService.CreateProfile(game, profileName);
        }
    }
}