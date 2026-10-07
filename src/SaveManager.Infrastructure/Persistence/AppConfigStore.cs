using SaveManager.Infrastructure.Persistence.Models;
using System.Text.Json;

namespace SaveManager.Infrastructure.Persistence
{
    /// single owner of config.json. Every writer goes through load -> mutate -> save so
    /// that saving one section never rolls back another section's changes.
    public class AppConfigStore
    {
        private static readonly string ConfigPath = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
            "SaveManager",
            "config.json"
        );

        private static readonly JsonSerializerOptions SerializerOptions = new()
        {
            WriteIndented = true
        };

        public AppConfig Load()
        {
            if (!File.Exists(ConfigPath))
                return new AppConfig();

            var json = File.ReadAllText(ConfigPath);
            return JsonSerializer.Deserialize<AppConfig>(json) ?? new AppConfig();
        }

        public void Save(AppConfig config)
        {
            Directory.CreateDirectory(Path.GetDirectoryName(ConfigPath)!);

            var json = JsonSerializer.Serialize(config, SerializerOptions);
            File.WriteAllText(ConfigPath, json);
        }
    }
}