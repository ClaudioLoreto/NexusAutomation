using Nexus.Core.Interfaces;

namespace Nexus.Scraper.Storyblocks;

/// <summary>
/// Placeholder until human provides Storyblocks DOM selectors (Step 3).
/// </summary>
public sealed class StoryblocksScraperStub : IStoryblocksScraper
{
    public Task<bool> EnsureAuthenticatedAsync(CancellationToken cancellationToken = default) =>
        throw new InvalidOperationException(
            "Storyblocks scraper not configured. Provide CSS selectors and credentials (see CLAUDE.md Step 3).");

    public Task<IReadOnlyList<string>> SearchVerticalVideosAsync(string query, CancellationToken cancellationToken = default) =>
        throw new InvalidOperationException(
            "Storyblocks scraper not configured. Provide CSS selectors and credentials (see CLAUDE.md Step 3).");
}
