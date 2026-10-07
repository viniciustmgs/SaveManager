using SaveManager.Domain.Entities;

namespace SaveManager.Domain.Interfaces
{
    public interface ISettingsRepository
    {
        AppSettings Get();
        void Save(AppSettings settings);
    }
}