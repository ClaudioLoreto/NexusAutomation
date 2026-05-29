using Nexus.Engine.Models;

namespace Nexus.Engine.Interfaces;

/// <summary>
/// Generates short-form video scripts from a niche/topic prompt.
///
/// <para>
/// Implementations are expected to be language-agnostic — the caller passes
/// a BCP-47 language tag (e.g. <c>"it-IT"</c>, <c>"en-US"</c>) on every call
/// and the implementation routes prompts/system messages accordingly.
/// </para>
///
/// <para>
/// The contract intentionally returns a structured <see cref="GeneratedScript"/>
/// (title + body + hashtags + tags + optional description) rather than a free
/// string so downstream consumers (TTS, YouTube metadata, SEO) can pick the
/// pieces they need without re-parsing model output.
/// </para>
/// </summary>
public interface IScriptGenerator
{
    Task<GeneratedScript> GenerateAsync(
        ScriptGenerationRequest request,
        CancellationToken cancellationToken = default);
}
