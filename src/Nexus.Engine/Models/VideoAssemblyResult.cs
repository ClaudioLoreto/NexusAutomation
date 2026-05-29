namespace Nexus.Engine.Models;

/// <summary>
/// Output of <see cref="Interfaces.IVideoAssembler.AssembleAsync"/>.
/// </summary>
/// <param name="OutputPath">
/// Absolute path to the final rendered MP4.
/// </param>
/// <param name="Duration">
/// Final video duration, measured from the rendered file. Useful for the
/// dashboard UI ("32s short generated") and for upload metadata.
/// </param>
public sealed record VideoAssemblyResult(
    string OutputPath,
    TimeSpan Duration);
