using Nexus.Core.DTOs;

namespace Nexus.Core.Interfaces;

public interface IScriptGenerator
{
    Task<ScriptGenerationResult> GenerateScriptAsync(ScriptGenerationRequest request, CancellationToken ct = default);
}
