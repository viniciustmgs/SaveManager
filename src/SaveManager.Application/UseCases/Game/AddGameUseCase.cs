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

        public void Execute(string name, string saveFolderPath, string backupFolderPath, bool isSingleFile)
        {
            //TODO: IMPLEMENTAR UMAS VALIDAÇÕES MELHORES

            if (string.IsNullOrEmpty(name))
                throw new ArgumentException("The name of the game must not be empty.");

            if (!Directory.Exists(saveFolderPath))
                throw new ArgumentException("The save file doesn't exist");

            if (!Directory.Exists(backupFolderPath))
                throw new ArgumentException("The backup folder doesn't exist");

            var game = new Domain.Entities.Game
            {
                Id = Guid.NewGuid(),
                Name = name,
                SaveFolderPath = saveFolderPath,
                BackupFolderPath = backupFolderPath,
                IsSingleFile = isSingleFile
            };

            _gameRepository.Create(game);
        }
    }
}
