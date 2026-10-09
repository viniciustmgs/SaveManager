using SaveManager.Domain.Entities;
using SaveManager.Domain.Interfaces;

namespace SaveManager.Infrastructure.Persistence
{
    public class JsonGameRepository : IGameRepository
    {
        private readonly AppConfigStore _configStore;

        public JsonGameRepository(AppConfigStore configStore)
        {
            _configStore = configStore;
        }

        public List<Game> GetAll()
        {
            return _configStore.Load().Games;
        }

        public Game? GetById(Guid id)
        {
            return _configStore.Load().Games.FirstOrDefault(g => g.Id == id);
        }

        public void Create(Game game)
        {
            var config = _configStore.Load();
            config.Games.Add(game);
            _configStore.Save(config);
        }

        public void Update(Game game)
        {
            var config = _configStore.Load();
            var index = config.Games.FindIndex(g => g.Id == game.Id);

            if (index == -1)
                throw new ArgumentException("Game not found");

            config.Games[index] = game;
            _configStore.Save(config);
        }

        public void Delete(Guid id)
        {
            var config = _configStore.Load();
            var game = config.Games.FirstOrDefault(g => g.Id == id);

            if (game == null)
                throw new ArgumentException("Game not found");

            config.Games.Remove(game);
            _configStore.Save(config);
        }
    }
}