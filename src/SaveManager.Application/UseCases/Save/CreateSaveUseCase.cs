using SaveManager.Domain.Interfaces;

namespace SaveManager.Application.UseCases.Save
{
    public class CreateSaveUseCase
    {
        private readonly ISaveFileService _saveFileService;

        public CreateSaveUseCase(ISaveFileService saveFileService)
        {
            _saveFileService = saveFileService;
        }

        public void Execute(Domain.Entities.Profile profile, string saveName)
        {
            if (string.IsNullOrWhiteSpace(saveName))
                throw new ArgumentException("O nome do save não pode ser vazio.");

            _saveFileService.CreateSave(profile, saveName);
        }
    }
}
