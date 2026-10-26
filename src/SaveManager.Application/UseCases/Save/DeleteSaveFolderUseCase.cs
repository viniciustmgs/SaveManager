using SaveManager.Domain.Interfaces;

namespace SaveManager.Application.UseCases.Save
{
    public class DeleteSaveFolderUseCase
    {
        private readonly ISaveFileService _saveFileService;

        public DeleteSaveFolderUseCase(ISaveFileService saveFileService)
        {
            _saveFileService = saveFileService;
        }

        public void Execute(
            Domain.Entities.Profile profile,
            Domain.Entities.SaveFolder folder)
        {
            _saveFileService.DeleteFolder(profile, folder);
        }
    }
}
