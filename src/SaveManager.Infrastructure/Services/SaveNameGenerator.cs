using SaveManager.Domain.Entities;
using SaveManager.Domain.Enums;

namespace SaveManager.Infrastructure.Services
{
    public static class SaveNameGenerator
    {
        public static string Generate(Profile profile, Game game)
        {
            if (game.SaveType == SaveType.SingleFile)
            {
                var fileName = Path.GetFileNameWithoutExtension(game.SavePath);
                var extension = Path.GetExtension(game.SavePath);

                var existingFiles = Directory.GetFiles(profile.FolderPath)
                    .Select(Path.GetFileName)
                    .ToList();

                // tries the original name first
                var baseName = $"{fileName}{extension}";
                if (!existingFiles.Contains(baseName))
                    return baseName;

                // if exists, add index
                var index = 0;
                while (existingFiles.Contains($"{fileName}_{index}{extension}"))
                    index++;

                return $"{fileName}_{index}{extension}";
            }
            else
            {
                var baseName = Path.GetFileName(game.SavePath.TrimEnd(Path.DirectorySeparatorChar));

                var existingSaves = Directory.GetDirectories(profile.FolderPath)
                    .Select(Path.GetFileName)
                    .ToList();

                if (!existingSaves.Contains(baseName))
                    return baseName;

                var index = 0;
                while (existingSaves.Contains($"{baseName}_{index}"))
                    index++;

                return $"{baseName}_{index}";
            }
        }
    }
}