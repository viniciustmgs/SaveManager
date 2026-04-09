using SaveManager.Domain.Enums;
using SaveManager.Domain.Interfaces;

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

            //TODO: IMPLEMENTAR UMAS VALIDAÇÕES MELHORES

            if (game == null)
                throw new ArgumentException("Game not found");

            if (string.IsNullOrEmpty(name))
                throw new ArgumentException("The name of the game must not be empty.");

            if (!Directory.Exists(saveFolderPath))
                throw new ArgumentException("The save file doesn't exist");

            if (!Directory.Exists(backupFolderPath))
                throw new ArgumentException("The backup folder doesn't exist");

            game.Name = name;
            game.SaveFolderPath = saveFolderPath;
            game.BackupFolderPath = backupFolderPath;
            game.SaveType = saveType;

            _gameRepository.Update(game);
        }
    }
}
