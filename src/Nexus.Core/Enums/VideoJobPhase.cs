namespace Nexus.Core.Enums;

/// <summary>
/// Engine pipeline phases for a single short. The legacy
/// YouTubeAutomation.Data project never had a formal state column —
/// state was inferred from nullable path fields. Nexus makes the state
/// explicit so:
///   - the dashboard can render progress without filesystem I/O,
///   - failed jobs can be resumed exactly where they stopped, and
///   - per-phase cost and timing can be tracked in <c>VideoJob</c>.
/// </summary>
public enum VideoJobPhase
{
    /// <summary>Row created, no work started.</summary>
    Pending = 0,

    /// <summary>OpenAI script generated and persisted.</summary>
    ScriptDone = 1,

    /// <summary>OpenAI / ElevenLabs voiceover rendered to disk.</summary>
    AudioDone = 2,

    /// <summary>Storyblocks clips downloaded and queued for assembly.</summary>
    MediaDone = 3,

    /// <summary>FFmpeg finished — final MP4 is on disk.</summary>
    RenderDone = 4,

    /// <summary>Terminal failure — see linked <c>RenderError</c> rows.</summary>
    Failed = -1
}
