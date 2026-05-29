using System.Globalization;
using System.Text.RegularExpressions;
using Humanizer;
using Nexus.Engine.Interfaces;

namespace Nexus.Engine.Services.AI.Humanization;

/// <summary>
/// Spells out bare integers ("123" → "centoventitré" / "one hundred
/// twenty-three"). Ported from the legacy YouTubeAutomation
/// <c>TextNormalizationService.NormalizeForTts</c>.
///
/// <para>
/// Runs FIRST in the chain (<see cref="Order"/> = 10) so downstream rules
/// (Roman numerals, future date/acronym rules) operate on already-spelled
/// digits. Pure regex: zero I/O, deterministic.
/// </para>
/// </summary>
public sealed partial class NumberSpellOutRule : IHumanizationRule
{
    public int Order => 10;

    [GeneratedRegex(@"\d+")]
    private static partial Regex DigitRun();

    public string Apply(string text, string languageCode)
    {
        if (string.IsNullOrWhiteSpace(text))
            return text;

        var culture = ResolveCulture(languageCode);
        return DigitRun().Replace(text, match =>
        {
            if (!int.TryParse(match.Value, NumberStyles.Integer, CultureInfo.InvariantCulture, out var n))
                return match.Value;
            return n.ToWords(culture);
        });
    }

    private static CultureInfo ResolveCulture(string languageCode)
    {
        if (string.IsNullOrWhiteSpace(languageCode))
            return CultureInfo.GetCultureInfo("en-US");

        // Accept BCP-47 ("it-IT", "en-US") AND legacy 3-letter codes
        // ("ITA", "ENG") so the rule survives a config carry-over.
        var lower = languageCode.Trim().ToLowerInvariant();
        if (lower.StartsWith("it") || lower == "ita")
            return CultureInfo.GetCultureInfo("it-IT");
        return CultureInfo.GetCultureInfo("en-US");
    }
}
