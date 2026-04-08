using SaveManager.Domain.Interfaces;

namespace SaveManager.Application.UseCases.Save
{
    public class LoadSaveUseCase
    {
        private readonly ISaveFileService _saveFileService;

        public LoadSaveUseCase(ISaveFileService saveFileService)
        {
            _saveFileService = saveFileService;
        }

        public void Execute(Domain.Entities.Game game, Domain.Entities.Save save)
        {
            _saveFileService.LoadSave(game, save);
        }
    }
}
