namespace SaveManager.Application.Common
{
    public static class FolderNameValidator
    {
        // reserved on Windows regardless of extension
        private static readonly string[] ReservedWindowsNames =
        [
            "CON", "PRN", "AUX", "NUL",
            "COM1", "COM2", "COM3", "COM4", "COM5", "COM6", "COM7", "COM8", "COM9",
            "LPT1", "LPT2", "LPT3", "LPT4", "LPT5", "LPT6", "LPT7", "LPT8", "LPT9"
        ];

        public static bool IsValidFolderName(string name, out string? errorMessage)
        {
            if (string.IsNullOrWhiteSpace(name))
            {
                errorMessage = "Name can not be empty";
                return false;
            }

            if (name.IndexOfAny(Path.GetInvalidFileNameChars()) >= 0)
            {
                errorMessage = "Name contains characters that are not allowed";
                return false;
            }

            // windows silently trims trailing dots/spaces from folder names, which
            // can make the folder that gets created not match what was typed.
            if (name.EndsWith('.') || name.EndsWith(' '))
            {
                errorMessage = "Name can not end with a space or a period";
                return false;
            }

            var nameWithoutExtension = name.Contains('.')
                ? name[..name.IndexOf('.')]
                : name;

            if (ReservedWindowsNames.Contains(nameWithoutExtension, StringComparer.OrdinalIgnoreCase))
            {
                errorMessage = $"\"{name}\" is a reserved name and can not be used";
                return false;
            }

            errorMessage = null;
            return true;
        }
    }
}