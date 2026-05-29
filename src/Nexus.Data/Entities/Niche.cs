using Nexus.Core.Enums;

namespace Nexus.Data.Entities;

/// <summary>
/// A "Niche" is a content typology preset — language, tone, voice, karaoke
/// styling, GIF overlay, and editorial constraints. The render engine reads
/// every parameter from this row at job time, which is why one engine can
/// produce a dramatic Italian history short and a punchy English brainrot
/// short in parallel.
/// </summary>
public class Niche
{
    public int Id { get; set; }

    public NicheType Type { get; set; }

    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// BCP-47 language code, e.g. <c>"it-IT"</c>, <c>"en-US"</c>. Drives:
    /// (a) script generation system prompt, (b) Whisper transcription
    /// language hint, (c) per-locale humanizer rules.
    /// </summary>
    public string LanguageCode { get; set; } = "en-US";

    /// <summary>
    /// Editorial tone hint passed verbatim to the script generator
    /// (<c>"dramatic"</c>, <c>"casual brainrot"</c>, <c>"wholesome"</c>, etc.).
    /// </summary>
    public string ScriptTone { get; set; } = string.Empty;

    /// <summary>
    /// Soft target for spoken script length, in words.
    /// </summary>
    public int? TargetWordCount { get; set; }

    /// <summary>
    /// Hard upper bound for the spoken script length, in words.
    /// </summary>
    public int? MaxWords { get; set; }

    /// <summary>
    /// OpenAI TTS voice ID (one of <c>alloy</c>, <c>echo</c>, <c>fable</c>,
    /// <c>onyx</c>, <c>nova</c>, <c>shimmer</c>) OR an ElevenLabs voice ID
    /// when the niche uses ElevenLabs.
    /// </summary>
    public string TtsVoice { get; set; } = "alloy";

    /// <summary>Playback speed for OpenAI TTS, range 0.25–4.0.</summary>
    public float TtsSpeed { get; set; } = 1.0f;

    /// <summary>Legacy ElevenLabs voice ID kept for backward compat.</summary>
    public string ElevenLabsVoiceId { get; set; } = string.Empty;

    /// <summary>Folder of background music tracks to pick from at render time.</summary>
    public string MusicDirectory { get; set; } = string.Empty;

    // -- Karaoke styling -------------------------------------------------

    public string KaraokeFontFamily { get; set; } = "The Bold Font";
    public int KaraokeFontSize { get; set; } = 96;
    public int KaraokeHighlightFontSize { get; set; } = 140;
    public string KaraokeFillColor { get; set; } = "#FFFFFF";
    public string KaraokeHighlightColor { get; set; } = "#FFFF00";
    public string KaraokeOutlineColor { get; set; } = "#000000";
    public string KaraokeBackgroundColor { get; set; } = "#0D1321";

    /// <summary>0 = top, 100 = bottom. Where the karaoke line sits vertically.</summary>
    public double KaraokeYPositionPercent { get; set; } = 7.0;

    // -- Subscribe-GIF overlay ------------------------------------------

    public string OverlayGifPath { get; set; } = string.Empty;

    /// <summary>Y position as % of canvas height for the GIF overlay.</summary>
    public double OverlayGifPositionPercent { get; set; } = 95.3;

    /// <summary>How many seconds before the end the GIF starts playing. Legacy: 5.</summary>
    public double OverlayGifTailSeconds { get; set; } = 5.0;

    /// <summary>0 = play once, -1 = loop forever, &gt;0 = explicit loop count.</summary>
    public int OverlayGifLoopCount { get; set; } = 0;

    // -- Editorial knobs -------------------------------------------------

    /// <summary>Free-form text appended to the LLM user prompt for this niche.</summary>
    public string AdditionalScriptInstructions { get; set; } = string.Empty;

    public bool IsActive { get; set; } = true;

    public int QueuePriority { get; set; } = 100;

    // -- Navigation properties ------------------------------------------

    public ICollection<Video> Videos { get; set; } = new List<Video>();
    public ICollection<Trend> Trends { get; set; } = new List<Trend>();
    public ICollection<VideoJob> VideoJobs { get; set; } = new List<VideoJob>();
}
