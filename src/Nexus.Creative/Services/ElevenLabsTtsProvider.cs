using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Nexus.Core.Configuration;
using Nexus.Core.DTOs;
using Nexus.Core.Enums;
using Nexus.Core.Interfaces;

namespace Nexus.Creative.Services;

public class ElevenLabsTtsProvider : ITtsProvider
{
    private readonly HttpClient _httpClient;
    private readonly ElevenLabsOptions _options;
    private readonly ILogger<ElevenLabsTtsProvider> _logger;

    public ElevenLabsTtsProvider(
        HttpClient httpClient,
        IOptions<ElevenLabsOptions> options,
        ILogger<ElevenLabsTtsProvider> logger)
    {
        _httpClient = httpClient;
        _options = options.Value;
        _logger = logger;

        _httpClient.BaseAddress = new Uri(_options.BaseUrl);
        _httpClient.DefaultRequestHeaders.Add("xi-api-key", _options.ApiKey);
    }

    public async Task<TtsResult> SynthesizeSpeechAsync(TtsRequest request, CancellationToken ct = default)
    {
        _logger.LogInformation("Synthesizing TTS for video {VideoId}, style: {VoiceStyle}",
            request.VideoId, request.VoiceStyle);

        try
        {
            var voiceId = ResolveVoiceId(request.VoiceStyle);
            var processedText = ConvertSsmlTagsToElevenLabs(request.SsmlText);

            var payload = new
            {
                text = processedText,
                model_id = "eleven_multilingual_v2",
                voice_settings = new
                {
                    stability = GetStability(request.VoiceStyle),
                    similarity_boost = 0.75,
                    style = GetStyleExaggeration(request.VoiceStyle),
                    use_speaker_boost = true
                }
            };

            var json = JsonSerializer.Serialize(payload);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            var response = await _httpClient.PostAsync(
                $"/v1/text-to-speech/{voiceId}",
                content,
                ct);

            if (!response.IsSuccessStatusCode)
            {
                var errorBody = await response.Content.ReadAsStringAsync(ct);
                _logger.LogError("ElevenLabs API returned {StatusCode}: {Body}",
                    response.StatusCode, errorBody);
                return new TtsResult
                {
                    Success = false,
                    ErrorMessage = $"ElevenLabs API error: {response.StatusCode}"
                };
            }

            var outputDir = Path.Combine("Output", "Audio");
            Directory.CreateDirectory(outputDir);
            var outputPath = Path.Combine(outputDir, $"{request.VideoId}.mp3");

            await using var stream = await response.Content.ReadAsStreamAsync(ct);
            await using var fileStream = File.Create(outputPath);
            await stream.CopyToAsync(fileStream, ct);

            var duration = await EstimateAudioDurationAsync(outputPath);

            _logger.LogInformation("TTS generated: {Path} ({Duration:F1}s)", outputPath, duration);

            return new TtsResult
            {
                Success = true,
                AudioFilePath = outputPath,
                DurationSeconds = duration
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "TTS synthesis failed for video {VideoId}", request.VideoId);
            return new TtsResult
            {
                Success = false,
                ErrorMessage = ex.Message
            };
        }
    }

    private string ResolveVoiceId(VoiceStyle style)
    {
        var styleKey = style.ToString();
        if (_options.VoiceMap.TryGetValue(styleKey, out var voiceId))
            return voiceId;

        return _options.DefaultVoiceId;
    }

    private static double GetStability(VoiceStyle style) => style switch
    {
        VoiceStyle.DeepCalm => 0.8,
        VoiceStyle.Enthusiastic => 0.5,
        VoiceStyle.DramaticPauses => 0.7,
        _ => 0.6
    };

    private static double GetStyleExaggeration(VoiceStyle style) => style switch
    {
        VoiceStyle.DeepCalm => 0.2,
        VoiceStyle.Enthusiastic => 0.6,
        VoiceStyle.DramaticPauses => 0.5,
        _ => 0.3
    };

    private static string ConvertSsmlTagsToElevenLabs(string ssmlText)
    {
        var result = ssmlText;
        result = result.Replace("[PAUSE:500ms]", "...");
        result = result.Replace("[PAUSE:1000ms]", "......");
        result = result.Replace("[EMPHASIS]", "");
        result = result.Replace("[/EMPHASIS]", "");
        return result;
    }

    private static Task<double> EstimateAudioDurationAsync(string filePath)
    {
        var fileInfo = new FileInfo(filePath);
        var estimatedBitrate = 128000.0;
        var durationSeconds = (fileInfo.Length * 8.0) / estimatedBitrate;
        return Task.FromResult(Math.Max(durationSeconds, 1.0));
    }
}
