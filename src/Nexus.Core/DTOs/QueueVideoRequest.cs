using Nexus.Core.Enums;

namespace Nexus.Core.DTOs;

public sealed record QueueVideoRequest(
    NicheType NicheType,
    string? Title,
    string? StoryblocksQuery);
