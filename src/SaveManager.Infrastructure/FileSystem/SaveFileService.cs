using SaveManager.Domain.Entities;
using SaveManager.Domain.Enums;
using SaveManager.Domain.Interfaces;
using SaveManager.Infrastructure.Services;

namespace SaveManager.Infrastructure.FileSystem
{
    public class SaveFileService : ISaveFileService
    {
        public List<Profile> ReadProfiles(Game game)
        {
            System.Diagnostics.Debug.WriteLine($"ReadProfiles START: {game.Name}");
            System.Diagnostics.Debug.WriteLine($"BackupFolderPath: {game.BackupFolderPath}");

            if (!Directory.Exists(game.BackupFolderPath))
            {
                System.Diagnostics.Debug.WriteLine("Backup folder does not exist.");
                return [];
            }

            System.Diagnostics.Debug.WriteLine("Before GetDirectories");
            var directories = Directory.GetDirectories(game.BackupFolderPath);
            System.Diagnostics.Debug.WriteLine($"After GetDirectories - count: {directories.Length}");

            var result = new List<Profile>();

            foreach (var path in directories)
            {
                System.Diagnostics.Debug.WriteLine($"Profile dir: {path}");

                result.Add(new Profile
                {
                    Name = Path.GetFileName(path),
                    FolderPath = path
                });
            }

            System.Diagnostics.Debug.WriteLine("ReadProfiles END");
            return result;
        }

        public Profile CreateProfile(Game game, string profileName)
        {
            var profilePath = Path.Combine(game.BackupFolderPath, profileName);

            if (Directory.Exists(profilePath))
                throw new ArgumentException("A profile with this name already exists");

            Directory.CreateDirectory(profilePath);

            return new Profile
            {
                Name = profileName,
                FolderPath = profilePath
            };
        }

        public void DeleteProfile(Profile profile)
        {
            if (!Directory.Exists(profile.FolderPath))
                throw new ArgumentException("Profile not found");

            Directory.Delete(profile.FolderPath, recursive: true);
        }

        public List<Save> ReadSaves(Profile profile, Game game)
        {
            if (!Directory.Exists(profile.FolderPath))
                return [];

            if (game.SaveType == SaveType.SingleFile)
            {
                return Directory
                    .GetFiles(profile.FolderPath)
                    .Select(path => new Save
                    {
                        Name = Path.GetFileName(path),
                        SavePath = path,
                        CreatedAt = File.GetCreationTime(path)
                    })
                    .ToList();
            }
            else
            {
                return Directory
                    .GetDirectories(profile.FolderPath)
                    .Select(path => new Save
                    {
                        Name = Path.GetFileName(path),
                        SavePath = path,
                        CreatedAt = Directory.GetCreationTime(path)
                    })
                    .ToList();
            }
        }

        public Save CreateSave(Profile profile, Game game)
        {
            var saveName = SaveNameGenerator.Generate(profile, game);

            if (game.SaveType == SaveType.SingleFile)
            {
                if (!File.Exists(game.SavePath))
                    throw new ArgumentException("No save file found");

                var savePath = Path.Combine(profile.FolderPath, saveName);
                File.Copy(game.SavePath, savePath, overwrite: true);

                return new Save
                {
                    Name = saveName,
                    SavePath = savePath,
                    CreatedAt = File.GetCreationTime(savePath)
                };
            }
            else
            {
                var savePath = Path.Combine(profile.FolderPath, saveName);
                CopyDirectory(game.SavePath, savePath);

                return new Save
                {
                    Name = saveName,
                    SavePath = savePath,
                    CreatedAt = Directory.GetCreationTime(savePath)
                };
            }
        }

        public void LoadSave(Game game, Save save)
        {
            if (game.SaveType == SaveType.SingleFile)
            {
                if (!File.Exists(save.SavePath))
                    throw new ArgumentException("Save file not found");

                File.Copy(save.SavePath, game.SavePath, overwrite: true);
            }
            else
            {
                if (!Directory.Exists(save.SavePath))
                    throw new ArgumentException("Save folder not found");

                ClearDirectory(game.SavePath);
                CopyDirectory(save.SavePath, game.SavePath);
            }
        }

        public void ReplaceSave(Game game, Save save)
        {
            if (game.SaveType == SaveType.SingleFile)
            {
                if (!File.Exists(game.SavePath))
                    throw new ArgumentException("No save file found");

                File.Copy(game.SavePath, save.SavePath, overwrite: true);
            }
            else
            {
                if (!Directory.Exists(save.SavePath))
                    throw new ArgumentException("Save folder not found");

                ClearDirectory(save.SavePath);
                CopyDirectory(game.SavePath, save.SavePath);
            }
        }

        public void DeleteSave(Save save)
        {
            if (File.Exists(save.SavePath))
            {
                File.Delete(save.SavePath);
            }
            else if (Directory.Exists(save.SavePath))
            {
                Directory.Delete(save.SavePath, recursive: true);
            }
            else
            {
                throw new ArgumentException("Save not found");
            }
        }

        private void CopyDirectory(string sourcePath, string destinationPath)
        {
            Directory.CreateDirectory(destinationPath);

            foreach (var file in Directory.GetFiles(sourcePath))
            {
                var destFile = Path.Combine(destinationPath, Path.GetFileName(file));
                File.Copy(file, destFile, overwrite: true);
            }

            foreach (var directory in Directory.GetDirectories(sourcePath))
            {
                var destDir = Path.Combine(destinationPath, Path.GetFileName(directory));
                CopyDirectory(directory, destDir);
            }
        }

        private void ClearDirectory(string path)
        {
            foreach (var file in Directory.GetFiles(path))
                File.Delete(file);

            foreach (var directory in Directory.GetDirectories(path))
                Directory.Delete(directory, recursive: true);
        }
    }
}