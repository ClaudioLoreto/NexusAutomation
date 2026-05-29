using Nexus.Core.DTOs;

namespace Nexus.Core.Interfaces;

public interface IStoryblocksScraper
{
    /// <summary>
    /// Ensures the session is valid: loads <c>data/state.json</c> (Playwright
    /// StorageState: cookies + localStorage + sessionStorage) if present,
    /// otherwise launches a HEADED Chromium for manual Google OAuth and
    /// persists the resulting state on success.
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
