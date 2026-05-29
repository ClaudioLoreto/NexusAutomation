using System.Text.RegularExpressions;
using Nexus.Engine.Interfaces;

namespace Nexus.Engine.Services.AI.Humanization;

/// <summary>
/// Replaces standalone Roman numerals (I–XXX) with their spoken ordinal in
/// the target language. Ported from the legacy YouTubeAutomation
/// <c>TextNormalizationService</c>.
///
/// <para>
/// Runs SECOND (<see cref="Order"/> = 20), after digit spell-out, so we
/// don't accidentally turn "Henry VIII" into "Henry eight" if a future
/// rule rewrites it as digits first. Uses a static map (1–30) which
/// covers the practical range for short-form content (king/pope numbering,
/// chapter references, etc.).
/// </para>
/// </summary>
public sealed partial class RomanNumeralSpellOutRule : IHumanizationRule
{
    public int Order => 20;

    [GeneratedRegex(@"\b(?:M{0,3})(?:CM|CD|D?C{0,3})(?:XC|XL|L?X{0,3})(?:IX|IV|V?I{0,3})\b")]
    private static partial Regex RomanNumeralPattern();

    /// <summary>
    /// Maps Roman numerals 1–30 to (Italian, English) ordinal pairs.
    /// Verbatim port from the legacy <c>RomanToWordMap</c>.
    /// </summary>
    private static readonly IReadOnlyDictionary<string, (string Italian, string English)> RomanMap
        = new Dictionary<string, (string Italian, string English)>(StringComparer.Ordinal)
        {
            ["I"]     = ("primo", "first"),
            ["II"]    = ("secondo", "second"),
            ["III"]   = ("terzo", "third"),
            ["IV"]    = ("quarto", "fourth"),
            ["V"]     = ("quinto", "fifth"),
            ["VI"]    = ("sesto", "sixth"),
            ["VII"]   = ("settimo", "seventh"),
            ["VIII"]  = ("ottavo", "eighth"),
            ["IX"]    = ("nono", "ninth"),
            ["X"]     = ("decimo", "tenth"),
            ["XI"]    = ("undicesimo", "eleventh"),
            ["XII"]   = ("dodicesimo", "twelfth"),
            ["XIII"]  = ("tredicesimo", "thirteenth"),
            ["XIV"]   = ("quattordicesimo", "fourteenth"),
            ["XV"]    = ("quindicesimo", "fifteenth"),
            ["XVI"]   = ("sedicesimo", "sixteenth"),
            ["XVII"]  = ("diciassettesimo", "seventeenth"),
            ["XVIII"] = ("diciottesimo", "eighteenth"),
            ["XIX"]   = ("diciannovesimo", "nineteenth"),
            ["XX"]    = ("ventesimo", "twentieth"),
            ["XXI"]   = ("ventunesimo", "twenty-first"),
            ["XXII"]  = ("ventiduesimo", "twenty-second"),
            ["XXIII"] = ("ventitreesimo", "twenty-third"),
            ["XXIV"]  = ("ventiquattresimo", "twenty-fourth"),
            ["XXV"]   = ("venticinquesimo", "twenty-fifth"),
            ["XXVI"]  = ("ventiseiesimo", "twenty-sixth"),
            ["XXVII"] = ("ventisettesimo", "twenty-seventh"),
            ["XXVIII"] = ("ventottesimo", "twenty-eighth"),
            ["XXIX"]  = ("ventinovesimo", "twenty-ninth"),
            ["XXX"]   = ("trentesimo", "thirtieth"),
        };

    public string Apply(string text, string languageCode)
    {
        if (string.IsNullOrWhiteSpace(text))
            return text;

        var isItalian = IsItalian(languageCode);

        return RomanNumeralPattern().Replace(text, match =>
        {
            if (string.IsNullOrEmpty(match.Value))
                return match.Value;
            if (!RomanMap.TryGetValue(match.Value, out var pair))
                return match.Value;
            return isItalian ? pair.Italian : pair.English;
        });
    }

    private static bool IsItalian(string languageCode)
    {
        if (string.IsNullOrWhiteSpace(languageCode))
            return false;
        var lower = languageCode.Trim().ToLowerInvariant();
        return lower.StartsWith("it") || lower == "ita";
    }
}
