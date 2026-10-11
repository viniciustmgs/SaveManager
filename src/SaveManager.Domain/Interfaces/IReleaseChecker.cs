using SaveManager.Domain.Entities;

namespace SaveManager.Domain.Interfaces
{
    public interface IReleaseChecker
    {
        string ReleasesUrl { get; }

        Task<UpdateCheckResult> CheckAsync(string currentVersion, CancellationToken cancellationToken);
    }
}