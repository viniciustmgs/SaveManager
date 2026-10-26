using SaveManager.Domain.Entities;

namespace SaveManager.Domain.Interfaces
{
    public interface ISaveFileService
    {
        List<Profile> ReadProfiles(Game game);
        Profile CreateProfile(Game game, string profileName);
        void DeleteProfile(Profile profile);
        List<Save> ReadSaves(Profile profile, Game game);
        List<SaveFolder> ReadFolders(Profile profile, Game game);
        Save CreateSave(Profile profile, Game game, string relativeFolderPath);
        void LoadSave (Game game, Save save);
        void DeleteSave(Save save);
        void ReplaceSave(Game game, Save save);
        Save RenameSave(Game game, Save save, string newName);
        Save MoveSave(Profile profile, Game game, Save save, string relativeTargetFolderPath);
        SaveFolder CreateFolder(Profile profile, Game game, string relativeParentPath, string name);
        SaveFolder RenameFolder(Profile profile, Game game, SaveFolder folder, string newName);
        void DeleteFolder(Profile profile, SaveFolder folder);
        Profile RenameProfile(Profile profile, string newName);
    }
}
