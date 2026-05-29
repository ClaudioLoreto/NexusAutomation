using Nexus.Core.Enums;

namespace Nexus.Data.Entities;

/// <summary>
/// One end-to-end shorts-generation job: niche, prompt → script → audio →
/// clips → render. The <see cref="Phase"/> column is the explicit state
/// machine the legacy YouTubeAutomation pipeline lacked (it inferred
/// state from nullable path columns on its <c>Media</c> entity).
///
/// <para>
/// Cost-tracking columns are first-class so the dashboard can show
/// per-job and per-niche spend without aggregating logs. Errors live in
/// <see cref="RenderError"/> rows, NOT in a single nvarchar(max) string.
/// </para>
/// </summary>
public class VideoJob
{
    public Guid Id { get; set; } = Guid.NewGuid();

    public int NicheId { get; set; }
    public Niche Niche { get; set; } = null!;

    /// <summary>Topic / prompt the LLM was asked to script around.</summary>
    public string Topic { get; set; } = string.Empty;

    /// <summary>Storyblocks search query (may diverge from <see cref="Topic"/>).</summary>
    public string? StoryblocksQuery { get; set; }

    public VideoJobPhase Phase { get; set; } = VideoJobPhase.Pending;

    public string? Title { get; set; }
    public string? ScriptBody { get; set; }
    public string? Description { get; set; }

    /// <summary>Comma- or pipe-separated tags as written to the YouTube metadata sheet.</summary>
    public string? TagsCsv { get; set; }

    /// <summary>Comma- or pipe-separated hashtags (no '#').</summary>
    public string? HashtagsCsv { get; set; }

    /// <summary>Final-render path — populated when <see cref="Phase"/> hits <c>RenderDone</c>.</summary>
    public string? FinalOutputPath { get; set; }

    public TimeSpan? RenderedDuration { get; set; }

    // -- Cost tracking (per-phase) --------------------------------------

    /// <summary>Tokens billed by the chat completion in <c>ScriptDone</c>.</summary>
    public int? ScriptTokens { get; set; }

    /// <summary>Characters sent to the TTS endpoint in <c>AudioDone</c>.</summary>
    public int? TtsCharacters { get; set; }

    /// <summary>Wall-clock seconds spent in FFmpeg during <c>RenderDone</c>.</summary>
    public double? RenderSeconds { get; set; }

    /// <summary>Aggregate cost in USD when known. Null when phases haven't billed yet.</summary>
    public decimal? CostUsd { get; set; }

    // -- Retry / timestamps ---------------------------------------------

    public int RetryCount { get; set; }

    public DateTime CreatedAtUtc { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedAtUtc { get; set; }
    public DateTime? CompletedAtUtc { get; set; }

    public ICollection<VideoAsset> Assets { get; set; } = new List<VideoAsset>();
    public ICollection<RenderError> Errors { get; set; } = new List<RenderError>();
}
