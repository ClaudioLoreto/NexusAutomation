using Nexus.Core.DTOs;

namespace Nexus.Core.Interfaces;

public interface IVideoRenderer
{
    Task<RenderResult> RenderVideoAsync(RenderRequest request, CancellationToken ct = default);
}
