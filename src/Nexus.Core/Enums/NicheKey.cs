namespace Nexus.Core.Enums;

/// <summary>
/// Stable identifier for the three seeded niches. The string column in the
/// database stores the enum name verbatim so that human operators can read it.
/// </summary>
public enum NicheKey
{
    Finance = 0,
    TechAI = 1,
    Legal = 2
}
