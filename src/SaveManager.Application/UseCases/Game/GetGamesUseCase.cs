using SaveManager.Domain.Interfaces;

namespace SaveManager.Application.UseCases.Game
{
    public class GetGamesUseCase
    {
        private readonly IGameRepository _gameRepository;

        public GetGamesUseCase(IGameRepository gameRepository)
        {
            _gameRepository = gameRepository;
        }

        public List<Domain.Entities.Game> Execute()
        {
            return _gameRepository.GetAll();
        }
    }
}
