using SaveManager.Domain.Interfaces;

namespace SaveManager.Application.UseCases.Save
{
    public class RestoreSaveUseCase
    {
        private readonly ISaveFileService _saveFileService;

        public RestoreSaveUseCase(ISaveFileService saveFileService)
        {
            _saveFileService = saveFileService;
        }

        public void Execute(Domain.Entities.Game game, Domain.Entities.Save save)
        {
            _saveFileService.RestoreSave(game, save);
        }
    }
}
