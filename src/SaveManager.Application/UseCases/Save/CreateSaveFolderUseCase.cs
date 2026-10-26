using SaveManager.Application.Common;
using SaveManager.Domain.Interfaces;

namespace SaveManager.Application.UseCases.Save
{
    public class CreateSaveFolderUseCase
    {
        private readonly ISaveFileService _saveFileService;

        public CreateSaveFolderUseCase(ISaveFileService saveFileService)
        {
            _saveFileService = saveFileService;
        }

        public Domain.Entities.SaveFolder Execute(
            Domain.Entities.Profile profile,
            Domain.Entities.Game game,
            string relativeParentPath,
            string name)
        {
            name = name.Trim();

            if (!FolderNameValidator.IsValidFolderName(name, out var error))
                throw new ArgumentException(error);

            return _saveFileService.CreateFolder(profile, game, relativeParentPath, name);
        }
    }
}
