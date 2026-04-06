using SaveManager.Domain.Entities;

namespace SaveManager.Domain.Interfaces
{
    public interface ISaveFileService
    {
        List<Profile> ReadProfiles(Game game);
        Profile CreateProfile(Game game, string profileName);
        void DeleteProfile(Profile profile);
        List<Save> ReadSaves(Profile profile);
        Save CreateSave(Profile profile, string saveName);
        void RestoreSave (Game game, Save save);
        void DeleteSave(Game game, Save save);
        void ReplaceSave(Game game, Save currentSave, Save newSave);
    }
}
