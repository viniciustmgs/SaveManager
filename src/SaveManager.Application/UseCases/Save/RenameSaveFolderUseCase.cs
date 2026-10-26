using SaveManager.Application.Common;
using SaveManager.Domain.Interfaces;

namespace SaveManager.Application.UseCases.Save
{
    public class RenameSaveFolderUseCase
    {
        private readonly ISaveFileService _saveFileService;

        public RenameSaveFolderUseCase(ISaveFileService saveFileService)
        {
            _saveFileService = saveFileService;
        }

        public Domain.Entities.SaveFolder Execute(
            Domain.Entities.Profile profile,
            Domain.Entities.Game game,
            Domain.Entities.SaveFolder folder,
            string newName)
        {
            newName = newName.Trim();

            if (!FolderNameValidator.IsValidFolderName(newName, out var error))
                throw new ArgumentException(error);

            return _saveFileService.RenameFolder(profile, game, folder, newName);
        }
    }
}
