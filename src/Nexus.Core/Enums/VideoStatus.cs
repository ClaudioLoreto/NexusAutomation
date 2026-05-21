namespace Nexus.Core.Enums;

/// <summary>
/// Canonical state machine for a Nexus video.
/// Transitions are sequential, except <see cref="Error_Requires_Human"/>
/// which may be reached from any prior state when a worker fails
/// irrecoverably and a human must intervene.
/// </summary>
public enum VideoStatus
{
    Pending = 0,
    TrendAnalyzed = 1,
    Scripting = 2,
    MediaDownloaded = 3,
    Rendering = 4,
    Completed = 5,
    Error_Requires_Human = 99
}
