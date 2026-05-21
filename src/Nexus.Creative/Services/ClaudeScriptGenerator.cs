using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Nexus.Core.Configuration;
using Nexus.Core.DTOs;
using Nexus.Core.Enums;
using Nexus.Core.Interfaces;

namespace Nexus.Creative.Services;

public class ClaudeScriptGenerator : IScriptGenerator
{
    private readonly HttpClient _httpClient;
    private readonly ClaudeApiOptions _options;
    private readonly ILogger<ClaudeScriptGenerator> _logger;

    public ClaudeScriptGenerator(
        HttpClient httpClient,
        IOptions<ClaudeApiOptions> options,
        ILogger<ClaudeScriptGenerator> logger)
    {
        _httpClient = httpClient;
        _options = options.Value;
        _logger = logger;

        _httpClient.BaseAddress = new Uri(_options.BaseUrl);
        _httpClient.DefaultRequestHeaders.Add("x-api-key", _options.ApiKey);
        _httpClient.DefaultRequestHeaders.Add("anthropic-version", "2023-06-01");
    }

    public async Task<ScriptGenerationResult> GenerateScriptAsync(
        ScriptGenerationRequest request,
        CancellationToken ct = default)
    {
        _logger.LogInformation("Generating script for video {VideoId}, niche: {Niche}",
            request.VideoId, request.Niche);

        try
        {
            var toneDescription = request.Tone switch
            {
                ScriptTone.Formal => "formal, authoritative, and professional",
                ScriptTone.Dynamic => "dynamic, enthusiastic, and energetic",
                ScriptTone.Narrative => "narrative, dramatic, with strategic pauses",
                _ => "engaging and clear"
            };

            var systemPrompt = BuildSystemPrompt(toneDescription);
            var userPrompt = BuildUserPrompt(request);

            var content = new List<object>();

            if (!string.IsNullOrEmpty(request.FirstFrameBase64))
            {
                content.Add(new
                {
                    type = "image",
                    source = new
                    {
                        type = "base64",
                        media_type = "image/jpeg",
                        data = request.FirstFrameBase64
                    }
                });
            }

            content.Add(new { type = "text", text = userPrompt });

            var payload = new
            {
                model = _options.Model,
                max_tokens = _options.MaxTokens,
                system = systemPrompt,
                messages = new[]
                {
                    new { role = "user", content }
                }
            };

            var json = JsonSerializer.Serialize(payload, new JsonSerializerOptions
            {
                DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
            });

            var httpContent = new StringContent(json, Encoding.UTF8, "application/json");
            var response = await _httpClient.PostAsync("/v1/messages", httpContent, ct);

            var responseBody = await response.Content.ReadAsStringAsync(ct);

            if (!response.IsSuccessStatusCode)
            {
                _logger.LogError("Claude API returned {StatusCode}: {Body}",
                    response.StatusCode, responseBody);
                return new ScriptGenerationResult
                {
                    Success = false,
                    ErrorMessage = $"Claude API error: {response.StatusCode}"
                };
            }

            var result = JsonSerializer.Deserialize<JsonElement>(responseBody);
            var scriptText = result.GetProperty("content")[0].GetProperty("text").GetString() ?? "";

            var (plainScript, ssmlScript) = ParseScriptOutput(scriptText);

            _logger.LogInformation("Script generated successfully for video {VideoId} ({Length} chars)",
                request.VideoId, plainScript.Length);

            return new ScriptGenerationResult
            {
                Success = true,
                ScriptText = plainScript,
                SsmlText = ssmlScript
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Script generation failed for video {VideoId}", request.VideoId);
            return new ScriptGenerationResult
            {
                Success = false,
                ErrorMessage = ex.Message
            };
        }
    }

    private static string BuildSystemPrompt(string toneDescription) =>
        $"""
        You are an expert YouTube Shorts scriptwriter. Your scripts are designed for TTS narration 
        over stock footage. Each script MUST:
        
        1. Be exactly 55 seconds when read aloud at a natural pace (~150 words).
        2. Use a {toneDescription} tone.
        3. Start with a powerful hook in the first 3 seconds.
        4. Include SSML-compatible tags for the TTS engine:
           - Use [PAUSE:500ms] for half-second pauses
           - Use [PAUSE:1000ms] for dramatic one-second pauses
           - Use [EMPHASIS]word[/EMPHASIS] for words that should be stressed
        5. End with a call-to-action encouraging follows/likes.
        6. Output the script in two sections separated by "---SSML---":
           - First section: plain text (for subtitles)
           - Second section: SSML-annotated text (for TTS)
        """;

    private static string BuildUserPrompt(ScriptGenerationRequest request)
    {
        var sb = new StringBuilder();
        sb.AppendLine("Write a YouTube Shorts script based on the following:");
        sb.AppendLine();

        if (request.MediaTags.Length > 0)
        {
            sb.AppendLine($"Visual asset tags: {string.Join(", ", request.MediaTags)}");
        }

        sb.AppendLine($"Niche: {request.Niche}");
        sb.AppendLine($"Target duration: {request.TargetDurationSeconds} seconds");

        if (!string.IsNullOrEmpty(request.FirstFrameBase64))
        {
            sb.AppendLine("I've also attached the first frame of the video for visual context.");
        }

        return sb.ToString();
    }

    private static (string plainScript, string ssmlScript) ParseScriptOutput(string raw)
    {
        var parts = raw.Split("---SSML---", StringSplitOptions.TrimEntries);
        var plain = parts[0];
        var ssml = parts.Length > 1 ? parts[1] : plain;
        return (plain, ssml);
    }
}
