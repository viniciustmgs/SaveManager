using SaveManager.Domain.Entities;
using SaveManager.Domain.Enums;

namespace SaveManager.Infrastructure.Services
{
    public static class SaveNameGenerator
    {
        private static readonly StringComparison NameComparison =
            OperatingSystem.IsWindows() || OperatingSystem.IsMacOS()
                ? StringComparison.OrdinalIgnoreCase
                : StringComparison.Ordinal;

        private static bool Contains(List<string?> names, string candidate) =>
            names.Any(name => string.Equals(name, candidate, NameComparison));

        public static string Generate(Profile profile, Game game)
        {
            if (game.SaveType == SaveType.SingleFile)
            {
                var fileName = Path.GetFileNameWithoutExtension(game.SavePath);
                var extension = Path.GetExtension(game.SavePath);

                var existingFiles = Directory.GetFiles(profile.FolderPath)
                    .Select(Path.GetFileName)
                    .ToList();

                var baseName = $"{fileName}{extension}";
                if (!Contains(existingFiles, baseName))
                    return baseName;

                var index = 0;
                while (Contains(existingFiles, $"{fileName}_{index}{extension}"))
                    index++;

                return $"{fileName}_{index}{extension}";
            }
            else
            {
                var baseName = Path.GetFileName(game.SavePath.TrimEnd(Path.DirectorySeparatorChar));

                var existingSaves = Directory.GetDirectories(profile.FolderPath)
                    .Select(Path.GetFileName)
                    .ToList();

                if (!Contains(existingSaves, baseName))
                    return baseName;

                var index = 0;
                while (Contains(existingSaves, $"{baseName}_{index}"))
                    index++;

                return $"{baseName}_{index}";
            }
        }
    }
}