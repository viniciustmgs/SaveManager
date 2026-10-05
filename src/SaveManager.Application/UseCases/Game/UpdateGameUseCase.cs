using SaveManager.Domain.Enums;
using SaveManager.Domain.Interfaces;
using SaveManager.Application.Common;

namespace SaveManager.Application.UseCases.Game
{
    public class UpdateGameUseCase
    {
        private readonly IGameRepository _gameRepository;

        public UpdateGameUseCase(IGameRepository gameRepository)
        {
            _gameRepository = gameRepository;
        }

        public void Execute(Guid gameId, string name, string saveFolderPath, string backupFolderPath, SaveType saveType)
        {
            var game = _gameRepository.GetById(gameId);

            if (game == null)
                throw new ArgumentException("Game not found");

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

                if (PathHelper.IsSameOrSubdirectory(backupFolderPath, saveFolderPath))
                    throw new ArgumentException("The save folder cannot be the backup folder");
            }

            if (!Directory.Exists(backupFolderPath))
                throw new ArgumentException("The backup folder doesn't exist");

            game.Name = name;
            game.SavePath = saveFolderPath;
            game.BackupFolderPath = backupFolderPath;
            game.SaveType = saveType;

            _gameRepository.Update(game);
        }
    }
}