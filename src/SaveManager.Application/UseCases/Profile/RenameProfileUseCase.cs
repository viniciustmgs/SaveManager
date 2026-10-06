using SaveManager.Application.Common;
using SaveManager.Domain.Interfaces;

namespace SaveManager.Application.UseCases.Profile
{
    public class RenameProfileUseCase
    {
        private readonly ISaveFileService _saveFileService;
        private readonly IGameRepository _gameRepository;

        public RenameProfileUseCase(ISaveFileService saveFileService, IGameRepository gameRepository)
        {
            _saveFileService = saveFileService;
            _gameRepository = gameRepository;
        }

        public Domain.Entities.Profile Execute(Guid gameId, string currentProfileName, string newProfileName)
        {
            newProfileName = newProfileName.Trim();

            if (!FolderNameValidator.IsValidFolderName(newProfileName, out var error))
                throw new ArgumentException(error);

            var game = _gameRepository.GetById(gameId);

            if (game == null)
                throw new ArgumentException("Game not found");

            var profiles = _saveFileService.ReadProfiles(game);
            var profile = profiles.FirstOrDefault(p => p.Name == currentProfileName);

            if (profile == null)
                throw new ArgumentException("Profile not found");

            if (newProfileName == profile.Name)
                return profile;

            if (profiles.Any(p => p.Name == newProfileName))
                throw new ArgumentException("A profile with this name already exists");

            return _saveFileService.RenameProfile(profile, newProfileName);
        }
    }
}