using SaveManager.Infrastructure.Persistence.Models;
using System.Text.Json;

namespace SaveManager.Infrastructure.Persistence
{
    // single owner of config.json
    public class AppConfigStore
    {
        private static readonly JsonSerializerOptions SerializerOptions = new()
        {
            WriteIndented = true
        };

        private readonly string _configPath;

        public AppConfigStore() : this(DefaultConfigPath())
        {
        }

        public AppConfigStore(string configPath)
        {
            _configPath = configPath;
        }

        public static string DefaultConfigPath() => Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
            "SaveManager",
            "config.json"
        );

        public AppConfig Load()
        {
            if (!File.Exists(_configPath))
                return new AppConfig();

            var json = File.ReadAllText(_configPath);
            return JsonSerializer.Deserialize<AppConfig>(json) ?? new AppConfig();
        }

        public void Save(AppConfig config)
        {
            Directory.CreateDirectory(Path.GetDirectoryName(_configPath)!);

            var json = JsonSerializer.Serialize(config, SerializerOptions);
            File.WriteAllText(_configPath, json);
        }
    }
}