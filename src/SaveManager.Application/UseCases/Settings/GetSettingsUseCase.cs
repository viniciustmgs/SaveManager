using SaveManager.Domain.Entities;
using SaveManager.Domain.Interfaces;

namespace SaveManager.Application.UseCases.Settings
{
    public class GetSettingsUseCase
    {
        private readonly ISettingsRepository _settingsRepository;

        public GetSettingsUseCase(ISettingsRepository settingsRepository)
        {
            _settingsRepository = settingsRepository;
        }

        public AppSettings Execute()
        {
            return _settingsRepository.Get();
        }
    }
}