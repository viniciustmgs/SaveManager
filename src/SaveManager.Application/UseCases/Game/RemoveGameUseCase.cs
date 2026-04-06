using SaveManager.Domain.Interfaces;

namespace SaveManager.Application.UseCases.Game
{
    public class RemoveGameUseCase
    {
        private readonly IGameRepository _gameRepository;

        public RemoveGameUseCase(IGameRepository gameRepository)
        {
            _gameRepository = gameRepository;
        }

        public void Execute(Guid gameId)
        {
            var game = _gameRepository.GetById(gameId);

            if (game == null)
                throw new ArgumentException("Game not found");

            _gameRepository.Delete(gameId);
        }
    }
}
