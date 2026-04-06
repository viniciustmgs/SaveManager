using SaveManager.Domain.Interfaces;

namespace SaveManager.Application.UseCases.Save
{
    public class ReplaceSaveUseCase
    {
        private readonly ISaveFileService _saveFileService;

        public ReplaceSaveUseCase(ISaveFileService saveFileService)
        {
            _saveFileService = saveFileService;
        }

        public void Execute(Domain.Entities.Game game, Domain.Entities.Save currentSave, Domain.Entities.Save newSave)
        {
            _saveFileService.ReplaceSave(game, currentSave, newSave);
        }
    }
}
