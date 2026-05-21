using Nexus.Core.Enums;

namespace Nexus.Core.DTOs;

public sealed record VideoDto(
    Guid Id,
    NicheType NicheType,
    VideoStatus Status,
    string? Title,
    string? ScriptText,
    string? OutputPath,
    DateTime CreatedAtUtc,
    DateTime? UpdatedAtUtc);
