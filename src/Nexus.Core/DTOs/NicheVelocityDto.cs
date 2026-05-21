using Nexus.Core.Enums;

namespace Nexus.Core.DTOs;

/// <summary>
/// View velocity snapshot used for niche auto-switching.
/// </summary>
public sealed record NicheVelocityDto(
    NicheType NicheType,
    double ViewsPerHour,
    DateTime MeasuredAtUtc);
