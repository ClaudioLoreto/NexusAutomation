using Nexus.Core.DTOs;
using Nexus.Core.Enums;

namespace Nexus.Core.Interfaces;

public interface IMediaScraper
{
    Task<MediaDownloadResult> DownloadVerticalVideoAsync(NicheType niche, string[] searchKeywords, CancellationToken ct = default);
    Task<bool> ValidateSessionAsync(CancellationToken ct = default);
    Task InitializeAsync(CancellationToken ct = default);
}
