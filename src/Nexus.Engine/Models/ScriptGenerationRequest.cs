namespace Nexus.Engine.Models;

/// <summary>
/// Input to <see cref="Interfaces.IScriptGenerator.GenerateAsync"/>.
/// </summary>
/// <param name="Topic">
/// The niche topic / hook for the short (e.g. "lost cities of Sicily",
/// "useless animal facts"). Required.
/// </param>
/// <param name="LanguageCode">
/// BCP-47 language tag — for example <c>"it-IT"</c> or <c>"en-US"</c>.
/// Required. Drives both the system prompt language AND the per-rule
/// behaviour of the humaniser when the script feeds into TTS later.
/// </param>
/// <param name="Tone">
/// Optional editorial tone hint passed verbatim into the system prompt
/// (e.g. "dramatic", "casual brainrot", "wholesome", "deadpan").
/// </param>
/// <param name="TargetWordCount">
/// Soft target for the spoken script length. Maps roughly to seconds of
/// audio at ~3 words / sec. Optional; the generator decides a sensible
/// default if unset.
/// </param>
/// <param name="MaxWords">
/// Hard upper bound. The script will not exceed this length. Optional.
/// </param>
/// <param name="AdditionalInstructions">
/// Free-form text appended to the user prompt — useful for per-niche
/// editorial rules ("no clickbait questions", "always end on a hashtag",
/// niche-specific story beats). Optional.
/// </param>
public sealed record ScriptGenerationRequest(
    string Topic,
    string LanguageCode,
    string? Tone = null,
    int? TargetWordCount = null,
    int? MaxWords = null,
    string? AdditionalInstructions = null);
