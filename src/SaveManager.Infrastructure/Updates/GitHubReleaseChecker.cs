using SaveManager.Domain.Entities;
using SaveManager.Domain.Interfaces;
using System.Net;
using System.Text.Json;

namespace SaveManager.Infrastructure.Updates
{
    public sealed class GitHubReleaseChecker : IReleaseChecker
    {
        public const string DefaultEndpoint =
            "https://api.github.com/repos/viniciustmgs/SaveManager/releases/latest";

        public const string DefaultReleasesUrl =
            "https://github.com/viniciustmgs/SaveManager/releases";

        private readonly HttpClient _http;
        private readonly string _endpoint;

        public GitHubReleaseChecker(HttpClient http, string? endpoint = null, string? releasesUrl = null)
        {
            _http = http;
            _endpoint = endpoint ?? DefaultEndpoint;
            ReleasesUrl = releasesUrl ?? DefaultReleasesUrl;
        }

        public string ReleasesUrl { get; }

        public async Task<UpdateCheckResult> CheckAsync(
            string currentVersion,
            CancellationToken cancellationToken)
        {
            try
            {
                using var request = new HttpRequestMessage(HttpMethod.Get, _endpoint);

                request.Headers.Add("User-Agent", "SaveManager");
                request.Headers.Add("Accept", "application/vnd.github+json");

                using var response = await _http.SendAsync(request, cancellationToken);

                if (response.StatusCode == HttpStatusCode.NotFound)
                    return UpdateCheckResult.NoReleases(currentVersion, ReleasesUrl);

                if (!response.IsSuccessStatusCode)
                    return UpdateCheckResult.Failed(currentVersion, ReleasesUrl);

                var json = await response.Content.ReadAsStringAsync(cancellationToken);

                var tag = ReadTag(json);

                if (!AppVersion.TryParseReleaseTag(tag, out var latest) ||
                    !AppVersion.TryParse(currentVersion, out var current))
                {
                    return UpdateCheckResult.Failed(currentVersion, ReleasesUrl);
                }

                return latest.CompareTo(current) > 0
                    ? UpdateCheckResult.UpdateAvailable(currentVersion, tag!, ReleasesUrl)
                    : UpdateCheckResult.UpToDate(currentVersion, tag!, ReleasesUrl);
            }
            catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
            {
                throw;
            }
            catch (Exception)
            {
                return UpdateCheckResult.Failed(currentVersion, ReleasesUrl);
            }
        }

        private static string? ReadTag(string json)
        {
            try
            {
                using var document = JsonDocument.Parse(json);

                return document.RootElement.TryGetProperty("tag_name", out var tag)
                       && tag.ValueKind == JsonValueKind.String
                    ? tag.GetString()
                    : null;
            }
            catch (JsonException)
            {
                return null;
            }
        }
    }
}