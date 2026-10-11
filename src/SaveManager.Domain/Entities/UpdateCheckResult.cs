using SaveManager.Domain.Enums;

namespace SaveManager.Domain.Entities
{
    public sealed record UpdateCheckResult(
        UpdateCheckStatus Status,
        string CurrentVersion,
        string LatestVersion,
        string ReleasesUrl)
    {
        public static UpdateCheckResult UpToDate(string current, string latest, string url) =>
            new(UpdateCheckStatus.UpToDate, current, latest, url);

        public static UpdateCheckResult UpdateAvailable(string current, string latest, string url) =>
            new(UpdateCheckStatus.UpdateAvailable, current, latest, url);

        public static UpdateCheckResult NoReleases(string current, string url) =>
            new(UpdateCheckStatus.NoReleases, current, string.Empty, url);

        public static UpdateCheckResult Failed(string current, string url) =>
            new(UpdateCheckStatus.Failed, current, string.Empty, url);
    }
}