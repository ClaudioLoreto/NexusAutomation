using Nexus.Core.Dtos;

namespace Nexus.Core.Interfaces;

public interface IRenderEngine
{
    Task<string> RenderAsync(RenderRequestDto request, CancellationToken ct = default);
}
