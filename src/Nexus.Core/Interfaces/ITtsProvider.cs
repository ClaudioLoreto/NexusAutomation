using Nexus.Core.DTOs;

namespace Nexus.Core.Interfaces;

public interface ITtsProvider
{
    Task<TtsResult> SynthesizeSpeechAsync(TtsRequest request, CancellationToken ct = default);
}
