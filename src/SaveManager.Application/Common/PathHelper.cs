namespace SaveManager.Application.Common
{
    public static class PathHelper
    {
        /// <summary>
        /// Returns true if "candidate" is the same folder as "basePath", or a folder nested anywhere inside it.
        /// </summary>
        public static bool IsSameOrSubdirectory(string candidate, string basePath)
        {
            var normalizedCandidate = NormalizePath(candidate);
            var normalizedBase = NormalizePath(basePath);

            if (string.Equals(normalizedCandidate, normalizedBase, StringComparison.OrdinalIgnoreCase))
                return true;

            var baseWithSeparator = normalizedBase.EndsWith(Path.DirectorySeparatorChar)
                ? normalizedBase
                : normalizedBase + Path.DirectorySeparatorChar;

            return normalizedCandidate.StartsWith(baseWithSeparator, StringComparison.OrdinalIgnoreCase);
        }

        private static string NormalizePath(string path)
        {
            var fullPath = Path.GetFullPath(path);
            return fullPath.TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar);
        }
    }
}