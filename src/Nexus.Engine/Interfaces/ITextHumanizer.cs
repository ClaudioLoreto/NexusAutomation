namespace Nexus.Engine.Interfaces;

/// <summary>
/// The orchestrator that runs the registered <see cref="IHumanizationRule"/>
/// chain over a raw script before TTS. Injected into every
/// <see cref="ITextToSpeechProvider"/> implementation so they don't each
/// re-invent text normalisation.
/// </summary>
public interface ITextHumanizer
{
    /// <summary>
    /// Applies every registered rule in <see cref="IHumanizationRule.Order"/>
    /// ascending order to <paramref name="text"/>.
    /// </summary>
    string Humanize(string text, string languageCode);
}
