using SaveManager.Domain.Interfaces;

namespace SaveManager.Application.UseCases.Game
{
    public class GetGameByIdUseCase
    {
        private readonly IGameRepository _gameRepository;

        public GetGameByIdUseCase(IGameRepository gameRepository)
        {
            _gameRepository = gameRepository;
        }

        public Domain.Entities.Game? Execute(Guid gameId)
        {
            return _gameRepository.GetById(gameId);
        }
    }
}
