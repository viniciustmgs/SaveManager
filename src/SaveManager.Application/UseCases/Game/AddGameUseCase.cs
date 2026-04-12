using SaveManager.Domain.Enums;
using SaveManager.Domain.Interfaces;

namespace SaveManager.Application.UseCases.Game
{
    public class AddGameUseCase
    {
        private readonly IGameRepository _gameRepository;

        public AddGameUseCase(IGameRepository gameRepository)
        {
            _gameRepository = gameRepository;
        }

        public void Execute(string name, string saveFolderPath, string backupFolderPath, SaveType saveType)
        {
            if (string.IsNullOrEmpty(name))
                throw new ArgumentException("The name of the game must not be empty.");

            if (saveType == SaveType.SingleFile)
            {
                if (!File.Exists(saveFolderPath))
                    throw new ArgumentException("The save file doesn't exist");
            }
            else
            {
                if (!Directory.Exists(saveFolderPath))
                    throw new ArgumentException("The save folder doesn't exist");
            }

            if (!Directory.Exists(backupFolderPath))
                throw new ArgumentException("The backup folder doesn't exist");

            var game = new Domain.Entities.Game
            {
                Id = Guid.NewGuid(),
                Name = name,
                SaveFolderPath = saveFolderPath,
                BackupFolderPath = backupFolderPath,
                SaveType = saveType
            };

            _gameRepository.Create(game);
        }
    }
}