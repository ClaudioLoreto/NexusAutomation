namespace Nexus.Core.Interfaces;

/// <summary>
/// Storyblocks acquisition — implementation deferred until human provides DOM selectors.
/// </summary>
public interface IStoryblocksScraper
{
    Task<bool> EnsureAuthenticatedAsync(CancellationToken cancellationToken = default);
    Task<IReadOnlyList<string>> SearchVerticalVideosAsync(string query, CancellationToken cancellationToken = default);
}
