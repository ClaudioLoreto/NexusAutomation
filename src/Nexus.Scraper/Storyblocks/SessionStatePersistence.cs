namespace Nexus.Scraper.Storyblocks;

/// <summary>
/// Tiny semantic anchor for the Playwright StorageState file (cookies +
/// localStorage + sessionStorage) used to restore an authenticated
/// Storyblocks session across runs.
///
/// <para>
/// This used to expose a SaveAsync helper, but the snapshot logic was
/// pulled inline into <c>StoryblocksScraper.WaitForManualGoogleLoginAsync</c>
/// where it lives at the single, obvious "auth just completed" boundary.
/// What remains here is a presence probe that <c>CreateSessionAsync</c>
/// uses to decide between "load existing state at context creation" and
/// "open headed Chromium for manual Google OAuth".
/// </para>
///
/// <para>
/// Loading is handled NATIVELY by Playwright via
/// <see cref="Microsoft.Playwright.BrowserNewContextOptions.StorageStatePath"/>
/// — there is no separate read step. The path travels straight from disk
/// into the browser context, restoring cookies and localStorage / session
/// storage simultaneously before the first network request fires.
/// </para>
/// </summary>
internal static class SessionStatePersistence
{
    public static bool Exists(string statePath) => File.Exists(statePath);
}
