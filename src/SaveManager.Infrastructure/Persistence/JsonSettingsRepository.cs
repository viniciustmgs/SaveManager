using SaveManager.Domain.Entities;
using SaveManager.Domain.Interfaces;

namespace SaveManager.Infrastructure.Persistence
{
    public class JsonSettingsRepository : ISettingsRepository
    {
        private readonly AppConfigStore _configStore;

        public JsonSettingsRepository(AppConfigStore configStore)
        {
            _configStore = configStore;
        }

        public AppSettings Get()
        {
            return _configStore.Load().Settings;
        }

        public void Save(AppSettings settings)
        {
            var config = _configStore.Load();
            config.Settings = settings;
            _configStore.Save(config);
        }
    }
}