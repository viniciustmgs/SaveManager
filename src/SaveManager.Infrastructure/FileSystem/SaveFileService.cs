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
            if (!Directory.Exists(game.BackupFolderPath))
                return [];

            return Directory
                .GetDirectories(game.BackupFolderPath)
                .Select(path => new Profile
                {
                    Name = Path.GetFileName(path),
                    FolderPath = path
                })
                .ToList();
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

        public List<Save> ReadSaves(Profile profile)
        {
            if (!Directory.Exists(profile.FolderPath))
                return [];

            return Directory
                .GetDirectories(profile.FolderPath)
                .Select(path => new Save
                {
                    Name = Path.GetFileName(path),
                    FolderPath = path,
                    CreatedAt = Directory.GetCreationTime(path)
                })
                .ToList();
        }

        public Save CreateSave(Profile profile, Game game)
        {
            var saveName = SaveNameGenerator.Generate(profile, game);
            var savePath = Path.Combine(profile.FolderPath, saveName);

            Directory.CreateDirectory(savePath);

            if (game.SaveType == SaveType.SingleFile)
            {
                var file = Directory.GetFiles(game.SaveFolderPath).FirstOrDefault();

                if (file == null)
                    throw new ArgumentException("No save file found");

                File.Copy(file, Path.Combine(savePath, Path.GetFileName(file)), overwrite: true);
            }
            else
            {
                CopyDirectory(game.SaveFolderPath, savePath);
            }

            return new Save
            {
                Name = saveName,
                FolderPath = savePath,
                CreatedAt = Directory.GetCreationTime(savePath)
            };
        }

        public void LoadSave(Game game, Save save)
        {
            if (!Directory.Exists(save.FolderPath))
                throw new ArgumentException("Save não encontrado.");

            string? originalFileName = null;

            if (game.SaveType == SaveType.SingleFile)
            {
                var originalFile = Directory.GetFiles(game.SaveFolderPath).FirstOrDefault();
                originalFileName = originalFile != null ? Path.GetFileName(originalFile) : null;
            }

            ClearDirectory(game.SaveFolderPath);

            if (game.SaveType == SaveType.SingleFile)
            {
                var file = Directory.GetFiles(save.FolderPath).FirstOrDefault();

                if (file == null)
                    throw new ArgumentException("Nenhum arquivo encontrado no save.");

                var destFileName = originalFileName ?? Path.GetFileName(file);
                File.Copy(file, Path.Combine(game.SaveFolderPath, destFileName), overwrite: true);
            }
            else
            {
                CopyDirectory(save.FolderPath, game.SaveFolderPath);
            }
        }

        public void ReplaceSave(Game game, Save save)
        {
            if (!Directory.Exists(save.FolderPath))
                throw new ArgumentException("Save not found");

            ClearDirectory(save.FolderPath);

            if (game.SaveType == SaveType.SingleFile)
            {
                var file = Directory.GetFiles(game.SaveFolderPath).FirstOrDefault();

                if (file == null)
                    throw new ArgumentException("No save found");

                File.Copy(file, Path.Combine(save.FolderPath, Path.GetFileName(file)), overwrite: true);
            }
            else
            {
                CopyDirectory(game.SaveFolderPath, save.FolderPath);
            }
        }

        public void DeleteSave(Save save)
        {
            if (!Directory.Exists(save.FolderPath))
                throw new ArgumentException("Save not found");

            Directory.Delete(save.FolderPath, recursive: true);
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
