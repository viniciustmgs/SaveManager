namespace SaveManager.Infrastructure.Updates
{
    public readonly record struct AppVersion(int Major, int Minor, int Patch) : IComparable<AppVersion>
    {
        public static bool TryParse(string? value, out AppVersion version)
        {
            version = default;

            if (string.IsNullOrWhiteSpace(value))
                return false;

            var text = value.Trim();

            if (text.StartsWith('v') || text.StartsWith('V'))
                text = text[1..];

            return TryParseCore(text, out version);
        }

        public static bool TryParseReleaseTag(string? tag, out AppVersion version)
        {
            version = default;

            if (string.IsNullOrWhiteSpace(tag))
                return false;

            var text = tag.Trim();

            if (text.StartsWith('v') || text.StartsWith('V'))
                text = text[1..];

            var plus = text.IndexOf('+');

            if (plus >= 0)
                text = text[..plus];

            if (text.Contains('-'))
                return false;

            return TryParseCore(text, out version);
        }

        private static bool TryParseCore(string text, out AppVersion version)
        {
            version = default;

            var plus = text.IndexOf('+');

            if (plus >= 0)
                text = text[..plus];

            var dash = text.IndexOf('-');

            if (dash >= 0)
                text = text[..dash];

            var parts = text.Split('.');

            if (parts.Length is 0 or > 3)
                return false;

            var numbers = new int[3];

            for (var i = 0; i < parts.Length; i++)
            {
                if (!int.TryParse(parts[i], out numbers[i]) || numbers[i] < 0)
                    return false;
            }

            version = new AppVersion(numbers[0], numbers[1], numbers[2]);
            return true;
        }

        public int CompareTo(AppVersion other)
        {
            var result = Major.CompareTo(other.Major);

            if (result != 0)
                return result;

            result = Minor.CompareTo(other.Minor);

            return result != 0 ? result : Patch.CompareTo(other.Patch);
        }
    }
}