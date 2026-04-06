using SaveManager.Domain.Interfaces;

namespace SaveManager.Application.UseCases.Save
{
    public class DeleteSaveUseCase
    {
        private readonly ISaveFileService _saveFileService;

        public DeleteSaveUseCase(ISaveFileService saveFileService)
        {
            _saveFileService = saveFileService;
        }

        public void Execute(Domain.Entities.Game game, Domain.Entities.Save save)
        {
            _saveFileService.DeleteSave(game, save);
        }
    }
}
