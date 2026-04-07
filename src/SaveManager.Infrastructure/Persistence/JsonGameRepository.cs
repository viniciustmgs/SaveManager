using SaveManager.Domain.Entities;
using SaveManager.Domain.Interfaces;
using SaveManager.Infrastructure.Persistence.Models;
using System.Text.Json;

namespace SaveManager.Infrastructure.Persistence
{
    public class JsonGameRepository : IGameRepository
    {
        private readonly string _configPath = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
            "SaveManager",
            "config.json"
        );

        private AppConfig LoadConfig()
        {
            if (!File.Exists(_configPath))
                return new AppConfig();

            var json = File.ReadAllText(_configPath);
            return JsonSerializer.Deserialize<AppConfig>(json) ?? new AppConfig();
        }

        private void SaveConfig(AppConfig config)
        {
            Directory.CreateDirectory(Path.GetDirectoryName(_configPath)!);

            var json = JsonSerializer.Serialize(config, new JsonSerializerOptions
            {
                WriteIndented = true
            });

            File.WriteAllText(_configPath, json);
        }

        public List<Game> GetAll()
        {
            return LoadConfig().Games;
        }

        public Game? GetById(Guid id)
        {
            return LoadConfig().Games.FirstOrDefault(g => g.Id == id);
        }

        public void Create(Game game)
        {
            var config = LoadConfig();
            config.Games.Add(game);
            SaveConfig(config);
        }

        public void Update(Game game)
        {
            var config = LoadConfig();
            var index = config.Games.FindIndex(g =>  g.Id == game.Id);

            if (index == -1)
                throw new ArgumentException("Game not found");

            config.Games[index] = game;
            SaveConfig(config);
        }

        public void Delete(Guid id)
        {
            var config = LoadConfig();
            var game = config.Games.FirstOrDefault(g => g.Id == id);

            if (game == null)
                throw new ArgumentException("Game not found");

            config.Games.Remove(game);
            SaveConfig(config);
        }
    }
}
