namespace Nexus.Core.Enums;

/// <summary>
/// Resilient state machine for a Shorts production job.
/// </summary>
public enum VideoStatus
{
    Pending = 0,
    TrendAnalyzed = 1,
    Scripting = 2,
    MediaDownloaded = 3,
    Rendering = 4,
    Completed = 5,
    ErrorRequiresHuman = 6
}
