namespace Nexus.Engine.Interfaces;

/// <summary>
/// A single deterministic text transformation applied to a script BEFORE
/// it reaches the TTS provider. Rules are chained by <see cref="ITextHumanizer"/>
/// in ascending <see cref="Order"/>; lower numbers run first.
///
/// <para>
/// This is the EXTENSIBILITY HOOK that lets us grow humanization without
/// touching <c>OpenAIService</c>. To add date normalization, acronym
/// expansion, currency spell-out, SSML pause hints, or a pronunciation
/// lexicon, drop a new <see cref="IHumanizationRule"/> implementation in
/// <c>Services/AI/Humanization/</c> and register it in DI — the rule
/// auto-joins the chain.
/// </para>
///
/// <para>
/// Rules MUST be pure: same <paramref name="text" />+<paramref name="languageCode"/>
/// always yields the same output. No I/O, no DB calls, no LLM round-trips.
/// Side effects belong in a different layer.
/// </para>
/// </summary>
public interface IHumanizationRule
{
    /// <summary>
    /// Suggested ordering for the legacy pipeline:
    /// <list type="bullet">
    ///   <item><c>10</c> — number spell-out (run before Roman so digits
    ///   inside dates/serials get normalised first)</item>
    ///   <item><c>20</c> — Roman numeral spell-out</item>
    ///   <item><c>30+</c> — future rules (dates, acronyms, currency, SSML)</item>
    /// </list>
    /// </summary>
    int Order { get; }

    /// <summary>
    /// Transforms <paramref name="text"/> for the given BCP-47
    /// <paramref name="languageCode"/> (e.g. <c>"it-IT"</c>, <c>"en-US"</c>).
    /// </summary>
    string Apply(string text, string languageCode);
}
