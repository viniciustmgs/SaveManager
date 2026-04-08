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

        public Domain.Entities.Save Execute(Domain.Entities.Profile profile, Domain.Entities.Game game)
        {
            return _saveFileService.CreateSave(profile, game);
        }
    }
}
