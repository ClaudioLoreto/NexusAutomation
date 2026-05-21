using Nexus.Core.Dtos;

namespace Nexus.Core.Interfaces;

public interface IScriptGenerator
{
    Task<ScriptResponseDto> GenerateAsync(ScriptRequestDto request, CancellationToken ct = default);
}
