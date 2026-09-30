using SaveManager.Domain.Interfaces;

namespace SaveManager.Application.UseCases.Save
{
    public class RenameSaveUseCase
    {
        private readonly ISaveFileService _saveFileService;

        public RenameSaveUseCase(ISaveFileService saveFileService)
        {
            _saveFileService = saveFileService;
        }

        public Domain.Entities.Save Execute(Domain.Entities.Game game, Domain.Entities.Save save, string newName)
        {
            return _saveFileService.RenameSave(game, save, newName);
        }
    }
}