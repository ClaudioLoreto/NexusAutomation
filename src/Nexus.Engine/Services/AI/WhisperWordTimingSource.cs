using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json.Serialization;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Nexus.Engine.Configuration;
using Nexus.Engine.Interfaces;
using Nexus.Engine.Models;

namespace Nexus.Engine.Services.AI;

/// <summary>
/// <see cref="IWordTimingSource"/> backed by OpenAI's Whisper API
/// (<c>POST /v1/audio/transcriptions</c> with
/// <c>response_format=verbose_json</c> and
/// <c>timestamp_granularities[]=word</c>).
///
/// <para>
/// Logic is a direct port of the legacy YouTubeAutomation V.4
/// <c>WhisperTranscriptionService.GetTranscriptionWithWordTimestampsAsync</c>,
/// including the Italian-apostrophe merge that stitches tokens like
/// <c>l'</c> + <c>Italia</c> back into <c>l'Italia</c>.
/// </para>
/// </summary>
public sealed class WhisperWordTimingSource : IWordTimingSource
{
    private readonly HttpClient _httpClient;
    private readonly OpenAiSettings _settings;
    private readonly ILogger<WhisperWordTimingSource> _logger;

    public WhisperWordTimingSource(
        HttpClient httpClient,
        IOptions<OpenAiSettings> settings,
        ILogger<WhisperWordTimingSource> logger)
    {
        _httpClient = httpClient;
        _settings = settings.Value;
        _logger = logger;
    }

    public async Task<IReadOnlyList<WordTiming>> GetWordTimingsAsync(
        string audioPath,
        string languageCode,
        CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(audioPath);
        if (!File.Exists(audioPath))
            throw new FileNotFoundException("Audio file for transcription not found.", audioPath);

        if (string.IsNullOrWhiteSpace(_settings.ApiKey))
            throw new InvalidOperationException(
                "OpenAI API key is not configured. Set OpenAI:ApiKey in appsettings.");

        using var content = new MultipartFormDataContent();
        var stream = File.OpenRead(audioPath);
        var streamContent = new StreamContent(stream);
        streamContent.Headers.ContentType = new MediaTypeHeaderValue("audio/mpeg");
        content.Add(streamContent, "file", Path.GetFileName(audioPath));
        content.Add(new StringContent(_settings.WhisperModel), "model");
        content.Add(new StringContent("verbose_json"), "response_format");
        content.Add(new StringContent(NormalizeLanguage(languageCode)), "language");
        content.Add(new StringContent("word"), "timestamp_granularities[]");

        var endpoint = string.IsNullOrWhiteSpace(_settings.BaseUrl)
            ? "https://api.openai.com/v1/audio/transcriptions"
            : _settings.BaseUrl.TrimEnd('/') + "/audio/transcriptions";

        using var request = new HttpRequestMessage(HttpMethod.Post, endpoint);
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", _settings.ApiKey);
        request.Content = content;

        using var response = await _httpClient.SendAsync(request, HttpCompletionOption.ResponseContentRead, cancellationToken);
        if (!response.IsSuccessStatusCode)
        {
            var body = await response.Content.ReadAsStringAsync(cancellationToken);
            throw new InvalidOperationException(
                $"Whisper transcription failed ({(int)response.StatusCode}): {body}");
        }

        var payload = await response.Content.ReadFromJsonAsync<WhisperResponse>(cancellationToken: cancellationToken);
        if (payload?.Words is null || payload.Words.Count == 0)
        {
            _logger.LogWarning("Whisper returned no word timings for {AudioPath}", audioPath);
            return Array.Empty<WordTiming>();
        }

        var words = payload.Words
            .Select(w => new WordTiming(w.Word ?? string.Empty, w.Start, w.End))
            .ToList();

        if (IsItalian(languageCode))
            words = MergeItalianApostrophes(words);

        return words;
    }

    /// <summary>
    /// Maps incoming language codes (BCP-47, ISO-639-1, legacy 3-letter)
    /// to the 2-letter codes Whisper expects.
    /// </summary>
    private static string NormalizeLanguage(string languageCode)
    {
        if (string.IsNullOrWhiteSpace(languageCode))
            return "en";
        var lower = languageCode.Trim().ToLowerInvariant();
        if (lower.StartsWith("it") || lower == "ita") return "it";
        if (lower.StartsWith("es") || lower == "spa") return "es";
        if (lower.StartsWith("fr") || lower == "fra") return "fr";
        if (lower.StartsWith("de") || lower == "deu" || lower == "ger") return "de";
        return "en";
    }

    private static bool IsItalian(string languageCode)
    {
        if (string.IsNullOrWhiteSpace(languageCode)) return false;
        var lower = languageCode.Trim().ToLowerInvariant();
        return lower.StartsWith("it") || lower == "ita";
    }

    /// <summary>
    /// Whisper sometimes splits Italian elisions like <c>l' Italia</c>
    /// into two tokens. This re-stitches them so the karaoke highlight
    /// covers the whole article+noun pair, matching how a viewer reads
    /// it. Direct port of legacy <c>FixItalianApostrophes</c>.
    /// </summary>
    private static List<WordTiming> MergeItalianApostrophes(List<WordTiming> words)
    {
        if (words.Count == 0) return words;
        var merged = new List<WordTiming>(words.Count);
        for (var i = 0; i < words.Count; i++)
        {
            var current = words[i];
            if (i + 1 < words.Count
                && current.Word.EndsWith('\'')
                && current.Word.Length > 1)
            {
                var next = words[i + 1];
                merged.Add(new WordTiming(
                    current.Word + next.Word,
                    current.StartSeconds,
                    next.EndSeconds));
                i++;
            }
            else
            {
                merged.Add(current);
            }
        }
        return merged;
    }

    private sealed class WhisperResponse
    {
        [JsonPropertyName("text")]
        public string? Text { get; set; }

        [JsonPropertyName("words")]
        public List<WhisperWord>? Words { get; set; }
    }

    private sealed class WhisperWord
    {
        [JsonPropertyName("word")]
        public string? Word { get; set; }

        [JsonPropertyName("start")]
        public double Start { get; set; }

        [JsonPropertyName("end")]
        public double End { get; set; }
    }
}
