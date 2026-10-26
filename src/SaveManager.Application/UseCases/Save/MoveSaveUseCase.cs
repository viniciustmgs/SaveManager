using SaveManager.Domain.Interfaces;

namespace SaveManager.Application.UseCases.Save
{
    public class MoveSaveUseCase
    {
        private readonly ISaveFileService _saveFileService;

        public MoveSaveUseCase(ISaveFileService saveFileService)
        {
            _saveFileService = saveFileService;
        }

        public Domain.Entities.Save Execute(
            Domain.Entities.Profile profile,
            Domain.Entities.Game game,
            Domain.Entities.Save save,
            string relativeTargetFolderPath)
        {
            return _saveFileService.MoveSave(profile, game, save, relativeTargetFolderPath);
        }
    }
}
