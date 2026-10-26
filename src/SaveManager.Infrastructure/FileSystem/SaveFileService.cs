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

        public Profile RenameProfile(Profile profile, string newName)
        {
            var parentFolder = Path.GetDirectoryName(profile.FolderPath)
                ?? throw new ArgumentException("Invalid profile path");

            var newPath = Path.Combine(parentFolder, newName);

            Directory.Move(profile.FolderPath, newPath);

            return new Profile
            {
                Name = newName,
                FolderPath = newPath
            };
        }

        public const string FolderMarkerFileName = ".savefolder";

        public List<Save> ReadSaves(Profile profile, Game game)
        {
            var result = new List<Save>();

            if (!Directory.Exists(profile.FolderPath))
                return result;

            Walk(profile.FolderPath, string.Empty);

            return result;

            void Walk(string directory, string relative)
            {
                if (game.SaveType == SaveType.SingleFile)
                {
                    foreach (var file in Directory.GetFiles(directory))
                    {
                        if (IsFolderMarker(file))
                            continue;

                        result.Add(new Save
                        {
                            Name = Path.GetFileName(file),
                            SavePath = file,
                            CreatedAt = File.GetCreationTime(file),
                            RelativeFolderPath = relative
                        });
                    }

                    foreach (var sub in Directory.GetDirectories(directory))
                        Walk(sub, ChildRelative(relative, Path.GetFileName(sub)));

                    return;
                }

                foreach (var sub in Directory.GetDirectories(directory))
                {
                    var name = Path.GetFileName(sub);

                    if (IsOrganizationalFolder(sub, game))
                    {
                        Walk(sub, ChildRelative(relative, name));
                    }
                    else
                    {
                        result.Add(new Save
                        {
                            Name = name,
                            SavePath = sub,
                            CreatedAt = Directory.GetCreationTime(sub),
                            RelativeFolderPath = relative
                        });
                    }
                }
            }
        }

        public List<SaveFolder> ReadFolders(Profile profile, Game game)
        {
            var result = new List<SaveFolder>();

            if (!Directory.Exists(profile.FolderPath))
                return result;

            Walk(profile.FolderPath, string.Empty);

            return result;

            void Walk(string directory, string relative)
            {
                foreach (var sub in Directory.GetDirectories(directory))
                {
                    if (!IsOrganizationalFolder(sub, game))
                        continue;

                    var name = Path.GetFileName(sub);
                    var childRelative = ChildRelative(relative, name);

                    result.Add(new SaveFolder
                    {
                        Name = name,
                        FolderPath = sub,
                        RelativePath = childRelative
                    });

                    Walk(sub, childRelative);
                }
            }
        }

        public Save CreateSave(Profile profile, Game game, string relativeFolderPath)
        {
            var targetFolder = ResolveFolder(profile, relativeFolderPath);

            if (!Directory.Exists(targetFolder))
                throw new ArgumentException("Folder not found");

            var saveName = SaveNameGenerator.Generate(targetFolder, game);

            if (game.SaveType == SaveType.SingleFile)
            {
                if (!File.Exists(game.SavePath))
                    throw new ArgumentException("No save file found");

                var savePath = Path.Combine(targetFolder, saveName);
                File.Copy(game.SavePath, savePath, overwrite: true);
                
                // some weird shit was happening with the file timestamp on windows, so I have to set it manually for safety
                File.SetCreationTime(savePath, DateTime.Now);

                return new Save
                {
                    Name = saveName,
                    SavePath = savePath,
                    CreatedAt = File.GetCreationTime(savePath),
                    RelativeFolderPath = relativeFolderPath
                };
            }
            else
            {
                var savePath = Path.Combine(targetFolder, saveName);
                CopyDirectory(game.SavePath, savePath);

                // same as above
                Directory.SetCreationTime(savePath, DateTime.Now);

                return new Save
                {
                    Name = saveName,
                    SavePath = savePath,
                    CreatedAt = Directory.GetCreationTime(savePath),
                    RelativeFolderPath = relativeFolderPath
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

        public Save RenameSave(Game game, Save save, string newName)
        {
            if (string.IsNullOrWhiteSpace(newName))
                throw new ArgumentException("Save name can not be empty");

            var parentFolder = Path.GetDirectoryName(save.SavePath)
                ?? throw new ArgumentException("Invalid save path");

            var newPath = Path.Combine(parentFolder, newName);

            if (string.Equals(newPath, save.SavePath, StringComparison.Ordinal))
                return save;

            if (game.SaveType == SaveType.SingleFile)
            {
                if (!File.Exists(save.SavePath))
                    throw new ArgumentException("Save file not found");

                if (File.Exists(newPath))
                    throw new ArgumentException("A save with this name already exists");

                File.Move(save.SavePath, newPath);

                return new Save
                {
                    Name = newName,
                    SavePath = newPath,
                    CreatedAt = save.CreatedAt,
                    RelativeFolderPath = save.RelativeFolderPath
                };
            }
            else
            {
                if (!Directory.Exists(save.SavePath))
                    throw new ArgumentException("Save folder not found");

                if (Directory.Exists(newPath))
                    throw new ArgumentException("A save with this name already exists");

                Directory.Move(save.SavePath, newPath);

                return new Save
                {
                    Name = newName,
                    SavePath = newPath,
                    CreatedAt = save.CreatedAt,
                    RelativeFolderPath = save.RelativeFolderPath
                };
            }
        }

        public Save MoveSave(Profile profile, Game game, Save save, string relativeTargetFolderPath)
        {
            if (string.Equals(save.RelativeFolderPath, relativeTargetFolderPath, PathComparison))
                return save;

            var targetFolder = ResolveFolder(profile, relativeTargetFolderPath);

            if (!Directory.Exists(targetFolder))
                throw new ArgumentException("Folder not found");

            var newPath = Path.Combine(targetFolder, save.Name);

            if (File.Exists(newPath) || Directory.Exists(newPath))
                throw new ArgumentException("A save with this name already exists in that folder");

            if (game.SaveType == SaveType.SingleFile)
            {
                if (!File.Exists(save.SavePath))
                    throw new ArgumentException("Save file not found");

                File.Move(save.SavePath, newPath);
            }
            else
            {
                if (!Directory.Exists(save.SavePath))
                    throw new ArgumentException("Save folder not found");

                Directory.Move(save.SavePath, newPath);
            }

            return new Save
            {
                Name = save.Name,
                SavePath = newPath,
                CreatedAt = save.CreatedAt,
                RelativeFolderPath = relativeTargetFolderPath
            };
        }

        public SaveFolder CreateFolder(Profile profile, Game game, string relativeParentPath, string name)
        {
            ValidateFolderSegment(name);

            var parent = ResolveFolder(profile, relativeParentPath);

            if (!Directory.Exists(parent))
                throw new ArgumentException("Parent folder not found");

            var path = Path.Combine(parent, name);

            if (Directory.Exists(path) || File.Exists(path))
                throw new ArgumentException("A folder or save with this name already exists");

            Directory.CreateDirectory(path);
            File.WriteAllText(Path.Combine(path, FolderMarkerFileName), string.Empty);

            return new SaveFolder
            {
                Name = name,
                FolderPath = path,
                RelativePath = ChildRelative(relativeParentPath, name)
            };
        }

        public SaveFolder RenameFolder(Profile profile, Game game, SaveFolder folder, string newName)
        {
            ValidateFolderSegment(newName);

            var parent = Path.GetDirectoryName(folder.FolderPath)
                ?? throw new ArgumentException("Invalid folder path");

            var newPath = Path.Combine(parent, newName);

            if (string.Equals(newPath, folder.FolderPath, StringComparison.Ordinal))
                return folder;

            if (Directory.Exists(newPath) || File.Exists(newPath))
                throw new ArgumentException("A folder or save with this name already exists");

            Directory.Move(folder.FolderPath, newPath);

            var relativeParent = Path.GetDirectoryName(folder.RelativePath)
                ?? string.Empty;

            return new SaveFolder
            {
                Name = newName,
                FolderPath = newPath,
                RelativePath = ChildRelative(relativeParent, newName)
            };
        }

        public void DeleteFolder(Profile profile, SaveFolder folder)
        {
            if (!Directory.Exists(folder.FolderPath))
                throw new ArgumentException("Folder not found");

            if (IsSameOrInside(folder.FolderPath, profile.FolderPath))
                throw new ArgumentException("Can not delete the profile folder");

            if (!IsInside(folder.FolderPath, profile.FolderPath))
                throw new ArgumentException("Folder is outside the profile");

            Directory.Delete(folder.FolderPath, recursive: true);
        }

        private static bool IsFolderMarker(string path) =>
            string.Equals(
                Path.GetFileName(path),
                FolderMarkerFileName,
                StringComparison.OrdinalIgnoreCase);

        private static bool IsOrganizationalFolder(string path, Game game) =>
            game.SaveType == SaveType.SingleFile
            || File.Exists(Path.Combine(path, FolderMarkerFileName));

        private static string ChildRelative(string parentRelative, string name) =>
            string.IsNullOrEmpty(parentRelative)
                ? name
                : Path.Combine(parentRelative, name);

        private static string ResolveFolder(Profile profile, string relativeFolderPath) =>
            string.IsNullOrEmpty(relativeFolderPath)
                ? profile.FolderPath
                : Path.Combine(profile.FolderPath, relativeFolderPath);

        private static bool IsInside(string path, string parent) =>
            Path.GetFullPath(path)
                .StartsWith(
                    Path.GetFullPath(parent).TrimEnd(Path.DirectorySeparatorChar) + Path.DirectorySeparatorChar,
                    PathComparison);

        private static bool IsSameOrInside(string path, string parent) =>
            string.Equals(
                Path.GetFullPath(path).TrimEnd(Path.DirectorySeparatorChar),
                Path.GetFullPath(parent).TrimEnd(Path.DirectorySeparatorChar),
                PathComparison);

        private static void ValidateFolderSegment(string name)
        {
            if (string.IsNullOrWhiteSpace(name))
                throw new ArgumentException("Name can not be empty");

            if (name is "." or "..")
                throw new ArgumentException("Name is not allowed");

            if (name.IndexOfAny(Path.GetInvalidFileNameChars()) >= 0)
                throw new ArgumentException("Name contains characters that are not allowed");
        }

        private static readonly StringComparison PathComparison =
            OperatingSystem.IsWindows() || OperatingSystem.IsMacOS()
                ? StringComparison.OrdinalIgnoreCase
                : StringComparison.Ordinal;

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