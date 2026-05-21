namespace Nexus.Core.Dtos;

public sealed record ScriptResponseDto(
    Guid VideoId,
    string Title,
    string Hook,
    string BodySsml,
    string CallToAction,
    IReadOnlyList<string> Hashtags);
