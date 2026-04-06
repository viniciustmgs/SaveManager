using SaveManager.Domain.Entities;

namespace SaveManager.Domain.Interfaces
{
    public interface IGameRepository
    {
        List<Game> GetAll();
        Game? GetById(Guid gameId);
        void Create(Game game);
        void Update(Game game);
        void Delete(Guid gameId);
    }
}
