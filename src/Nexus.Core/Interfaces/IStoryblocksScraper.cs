using Nexus.Core.DTOs;

namespace Nexus.Core.Interfaces;

public interface IStoryblocksScraper
{
    /// <summary>
    /// Ensures session is valid via cookies.json or manual Google OAuth (headed).
    /// </summary>
    Task<bool> EnsureAuthenticatedAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Searches Storyblocks with Footage / Vertical / 4K filters and downloads MP4 clips.
    /// </summary>
    Task<IReadOnlyList<ScrapedMediaResult>> SearchAndDownloadAsync(
        string query,
        int maxDownloads = 5,
        CancellationToken cancellationToken = default);
}
