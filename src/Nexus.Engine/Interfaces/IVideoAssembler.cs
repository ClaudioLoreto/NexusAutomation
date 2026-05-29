using Nexus.Engine.Models;

namespace Nexus.Engine.Interfaces;

/// <summary>
/// Assembles a final MP4 from a set of downloaded video clips, a voiceover
/// track, and optional background music. Implementations encapsulate
/// FFmpeg (or any future render backend) invocation.
///
/// <para>
/// The contract is intentionally narrow at this stage — we expose the
/// minimum surface needed to drive the renderer end-to-end. Richer features
/// (subtitles, overlays, transitions, ken-burns effects, voice ducking
/// curves) will land as additional fields on <see cref="VideoAssemblyRequest"/>
/// so existing call sites don't break when new knobs are added.
/// </para>
/// </summary>
public interface IVideoAssembler
{
    Task<VideoAssemblyResult> AssembleAsync(
        VideoAssemblyRequest request,
        CancellationToken cancellationToken = default);
}
