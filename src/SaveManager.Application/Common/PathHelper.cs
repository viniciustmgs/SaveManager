namespace SaveManager.Application.Common
{
    public static class PathHelper
    {
        // windows and macOS resolve paths ignoring case
        private static readonly StringComparison PathComparison =
            OperatingSystem.IsWindows() || OperatingSystem.IsMacOS()
                ? StringComparison.OrdinalIgnoreCase
                : StringComparison.Ordinal;

        public static bool IsSameOrSubdirectory(string candidate, string basePath)
        {
            var normalizedCandidate = NormalizePath(candidate);
            var normalizedBase = NormalizePath(basePath);

            if (string.Equals(normalizedCandidate, normalizedBase, PathComparison))
                return true;

            var baseWithSeparator = normalizedBase.EndsWith(Path.DirectorySeparatorChar)
                ? normalizedBase
                : normalizedBase + Path.DirectorySeparatorChar;

            return normalizedCandidate.StartsWith(baseWithSeparator, PathComparison);
        }

        private static string NormalizePath(string path)
        {
            var fullPath = Path.GetFullPath(path);
            return fullPath.TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar);
        }
    }
}