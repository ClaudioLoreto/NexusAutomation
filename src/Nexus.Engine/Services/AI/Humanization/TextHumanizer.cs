using Microsoft.Extensions.Logging;
using Nexus.Engine.Interfaces;

namespace Nexus.Engine.Services.AI.Humanization;

/// <summary>
/// Default <see cref="ITextHumanizer"/> implementation. Iterates every
/// registered <see cref="IHumanizationRule"/> in ascending <c>Order</c>
/// and applies it in sequence.
///
/// <para>
/// Adding a new transformation (dates, acronyms, currency, SSML) is a
/// one-file change: implement <see cref="IHumanizationRule"/> with an
/// appropriate <c>Order</c> and register it in DI — the rule auto-joins
/// the chain with no edits to <see cref="TextHumanizer"/> or any TTS
/// provider.
/// </para>
/// </summary>
public sealed class TextHumanizer : ITextHumanizer
{
    private readonly IReadOnlyList<IHumanizationRule> _rules;
    private readonly ILogger<TextHumanizer> _logger;

    public TextHumanizer(IEnumerable<IHumanizationRule> rules, ILogger<TextHumanizer> logger)
    {
        _rules = rules.OrderBy(r => r.Order).ToArray();
        _logger = logger;
    }

    public string Humanize(string text, string languageCode)
    {
        if (string.IsNullOrWhiteSpace(text))
            return string.Empty;

        var current = text;
        foreach (var rule in _rules)
        {
            try
            {
                current = rule.Apply(current, languageCode);
            }
            catch (Exception ex)
            {
                // A buggy rule must never break TTS — log + skip.
                _logger.LogWarning(
                    ex,
                    "Humanization rule {Rule} (order={Order}) threw; skipping.",
                    rule.GetType().Name, rule.Order);
            }
        }
        return current;
    }
}
