using SaveManager.Domain.Interfaces;

namespace SaveManager.Application.UseCases.Save
{
    public class GetSaveFoldersUseCase
    {
        private readonly ISaveFileService _saveFileService;

        public GetSaveFoldersUseCase(ISaveFileService saveFileService)
        {
            _saveFileService = saveFileService;
        }

        public List<Domain.Entities.SaveFolder> Execute(
            Domain.Entities.Profile profile,
            Domain.Entities.Game game)
        {
            return _saveFileService.ReadFolders(profile, game);
        }
    }
}
