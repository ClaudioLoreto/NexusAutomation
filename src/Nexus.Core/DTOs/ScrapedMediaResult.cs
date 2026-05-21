namespace Nexus.Core.DTOs;

public sealed record ScrapedMediaResult(
    string FilePath,
    string? Title,
    IReadOnlyList<string> Tags);
