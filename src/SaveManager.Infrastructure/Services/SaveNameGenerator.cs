using SaveManager.Domain.Entities;
using SaveManager.Domain.Enums;

namespace SaveManager.Infrastructure.Services
{
    public static class SaveNameGenerator
    {
        public static string Generate(Profile profile, Game game)
        {
            string baseName;

            if (game.SaveType == SaveType.SingleFile)
            {
                var file = Directory.GetFiles(game.SaveFolderPath).FirstOrDefault();

                if (file == null)
                    throw new ArgumentException("No save file found");

                baseName = Path.GetFileName(file);
            }
            else
            {
                baseName = Path.GetFileName(game.SaveFolderPath.TrimEnd(Path.DirectorySeparatorChar));
            }

            var existingSaves = Directory.GetDirectories(profile.FolderPath);

            var hasBase = existingSaves.Any(s => Path.GetFileName(s) == baseName);

            if (!hasBase)
                return baseName;

            var index = 0;
            while (existingSaves.Any(s => Path.GetFileName(s) == $"{baseName}_{index}"))
                index++;

            return $"{baseName}_{index}";
        }
    }
}
