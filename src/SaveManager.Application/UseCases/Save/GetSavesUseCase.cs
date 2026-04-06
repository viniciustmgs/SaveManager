using SaveManager.Domain.Interfaces;

namespace SaveManager.Application.UseCases.Save
{
    public class GetSavesUseCase
    {
        private readonly ISaveFileService _saveFileService;

        public GetSavesUseCase(ISaveFileService saveFileService)
        {
            _saveFileService = saveFileService;
        }

        public List<Domain.Entities.Save> Execute(Domain.Entities.Profile profile)
        {
            return _saveFileService.ReadSaves(profile);
        }
    }
}
