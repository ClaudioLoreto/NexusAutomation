namespace Nexus.Engine.Models;

/// <summary>
/// Structured output of <see cref="Interfaces.IScriptGenerator.GenerateAsync"/>.
/// Modelled on the legacy YouTubeAutomation XML schema (TITOLO, DESCRIZIONE,
/// HASHTAGS, TAGS, PROMPT_AUDIO) but flattened into a plain record so it
/// serialises cleanly through the API and into the EF Core layer.
/// </summary>
/// <param name="Title">
/// The video title (used for YouTube/Shorts metadata). Should ALREADY contain
/// any required suffix like <c>#shorts</c> if the generator was instructed to
/// add one.
/// </param>
/// <param name="Body">
/// The spoken script — this is what feeds the TTS provider. Newlines separate
/// paragraphs; the legacy convention is one blank line between beats.
/// </param>
/// <param name="Description">
/// Long-form description for the video. Optional — not every generator/use case
/// needs one.
/// </param>
/// <param name="Hashtags">
/// Hashtags (no leading <c>#</c>) ready to be joined into the description or
/// the YouTube metadata sheet. Empty list rather than null when none.
/// </param>
/// <param name="Tags">
/// SEO tags (no leading symbols). Empty list rather than null when none.
/// </param>
public sealed record GeneratedScript(
    string Title,
    string Body,
    string? Description,
    IReadOnlyList<string> Hashtags,
    IReadOnlyList<string> Tags);
