namespace Nexus.Engine.Configuration;

/// <summary>
/// Strongly-typed binding for the <c>"OpenAI"</c> section in
/// <c>appsettings.json</c> / <c>appsettings.Development.json</c> /
/// <c>config/secrets.json</c>.
///
/// <para>
/// API keys MUST come from <c>secrets.json</c>, environment variables
/// (<c>OpenAI__ApiKey</c>) or User Secrets — NEVER from a tracked
/// <c>appsettings.json</c>. The placeholder fields in tracked config files
/// are deliberately left empty so a misconfigured environment fails fast
/// at startup with an obvious "key is missing" message instead of leaking
/// a sample key into git.
/// </para>
/// </summary>
public sealed class OpenAiSettings
{
    public const string SectionName = "OpenAI";

    /// <summary>
    /// OpenAI API key. Required. Empty in tracked config files.
    /// </summary>
    public string ApiKey { get; set; } = string.Empty;

    /// <summary>
    /// Chat-completion model used for script generation. Defaults to
    /// <c>gpt-4o-mini</c> — cheap, fast, plenty good for short-form scripts.
    /// Override in <c>appsettings.json</c> if you want full <c>gpt-4o</c>
    /// quality.
    /// </summary>
    public string ChatModel { get; set; } = "gpt-4o-mini";

    /// <summary>
    /// Sampling temperature for script generation. Higher → more varied
    /// scripts. Legacy YouTubeAutomation used <c>0.85</c>; we keep that as
    /// the default since the editorial style is the same.
    /// </summary>
    public float Temperature { get; set; } = 0.85f;

    /// <summary>
    /// Token cap for script generation. Far above what a 30–60s short needs;
    /// generous default so we don't accidentally truncate.
    /// </summary>
    public int MaxOutputTokens { get; set; } = 2000;

    /// <summary>
    /// Text-to-speech model. <c>tts-1</c> is fast/cheap and the legacy default;
    /// <c>tts-1-hd</c> is higher quality at higher latency and cost.
    /// </summary>
    public string TtsModel { get; set; } = "tts-1";

    /// <summary>
    /// Default OpenAI TTS voice when the per-call request leaves
    /// <see cref="Models.SpeechSynthesisRequest.Voice"/> null.
    /// One of: <c>alloy</c>, <c>echo</c>, <c>fable</c>, <c>onyx</c>,
    /// <c>nova</c>, <c>shimmer</c>.
    /// </summary>
    public string TtsVoice { get; set; } = "alloy";

    /// <summary>
    /// Default playback speed (0.25 – 4.0) for OpenAI TTS. <c>1.0</c> is
    /// natural pace; the legacy YouTubeAutomation pipeline used <c>1.0</c>
    /// for OpenAI and tweaked stability/similarity for ElevenLabs separately.
    /// </summary>
    public float TtsSpeed { get; set; } = 1.0f;

    /// <summary>
    /// Whisper transcription model used by <c>WhisperWordTimingSource</c>
    /// to derive per-word timings for karaoke. Legacy default: <c>whisper-1</c>.
    /// </summary>
    public string WhisperModel { get; set; } = "whisper-1";

    /// <summary>
    /// Optional override of the OpenAI base URL. Useful for proxy/Azure-OpenAI
    /// style deployments. Empty string means "use the SDK default".
    /// </summary>
    public string BaseUrl { get; set; } = string.Empty;
}
