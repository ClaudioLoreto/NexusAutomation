using Nexus.Core.Dtos;

namespace Nexus.Core.Interfaces;

public interface IStoryblocksScraper
{
    Task EnsureLoggedInAsync(CancellationToken ct = default);

    Task<IReadOnlyList<StoryblocksClipDto>> SearchVerticalClipsAsync(
        string query,
        int maxResults,
        CancellationToken ct = default);

    Task<StoryblocksClipDto> DownloadAsync(
        StoryblocksClipDto clip,
        string targetDirectory,
        CancellationToken ct = default);
}
