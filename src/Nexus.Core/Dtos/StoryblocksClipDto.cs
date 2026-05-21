namespace Nexus.Core.Dtos;

public sealed record StoryblocksClipDto(
    string StoryblocksId,
    string Title,
    IReadOnlyList<string> Tags,
    string PreviewUrl,
    string LocalFilePath,
    int WidthPx,
    int HeightPx,
    TimeSpan Duration);
