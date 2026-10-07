using SaveManager.Domain.Entities;
using SaveManager.Domain.Interfaces;

namespace SaveManager.Application.UseCases.Settings
{
    public class SaveSettingsUseCase
    {
        private readonly ISettingsRepository _settingsRepository;

        public SaveSettingsUseCase(ISettingsRepository settingsRepository)
        {
            _settingsRepository = settingsRepository;
        }

        public void Execute(AppSettings settings)
        {
            _settingsRepository.Save(settings);
        }
    }
}