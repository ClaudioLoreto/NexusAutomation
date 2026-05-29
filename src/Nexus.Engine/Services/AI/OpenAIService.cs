using System.ClientModel;
using System.Text.Json;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Nexus.Engine.Configuration;
using Nexus.Engine.Interfaces;
using Nexus.Engine.Models;
using OpenAI;
using OpenAI.Audio;
using OpenAI.Chat;

namespace Nexus.Engine.Services.AI;

/// <summary>
/// Combined OpenAI script generator + TTS provider.
///
/// <para>
/// Implements both <see cref="IScriptGenerator"/> and
/// <see cref="ITextToSpeechProvider"/> because the legacy YouTubeAutomation
/// pipeline already centralised both calls under one client (one API key,
/// one base URL, one retry/back-off policy). Splitting into two services
/// would mean configuring the same client twice — the user explicitly
/// chose <c>building_blocks_only</c> orchestration which keeps these tight.
/// </para>
///
/// <para>
/// The TTS path runs the registered <see cref="ITextHumanizer"/> chain over
/// the script BEFORE calling the API — that's the legacy "humanization
/// pre-pass" (number/Roman-numeral spell-out, future date/acronym rules)
/// which dramatically improves prosody on dates, big numbers, and Roman
/// monarch names.
/// </para>
/// </summary>
public sealed class OpenAIService : IScriptGenerator, ITextToSpeechProvider
{
    private readonly OpenAIClient _client;
    private readonly OpenAiSettings _settings;
    private readonly ITextHumanizer _humanizer;
    private readonly ILogger<OpenAIService> _logger;

    public OpenAIService(
        IOptions<OpenAiSettings> settings,
        ITextHumanizer humanizer,
        ILogger<OpenAIService> logger)
    {
        _settings = settings.Value;
        _humanizer = humanizer;
        _logger = logger;

        if (string.IsNullOrWhiteSpace(_settings.ApiKey))
            throw new InvalidOperationException(
                "OpenAI API key is missing. Set OpenAI:ApiKey in appsettings or via the OpenAI__ApiKey env var.");

        var credential = new ApiKeyCredential(_settings.ApiKey);
        var options = new OpenAIClientOptions();
        if (!string.IsNullOrWhiteSpace(_settings.BaseUrl))
            options.Endpoint = new Uri(_settings.BaseUrl);
        _client = new OpenAIClient(credential, options);
    }

    // ------------------------------------------------------------------
    //  IScriptGenerator
    // ------------------------------------------------------------------

    public async Task<GeneratedScript> GenerateAsync(
        ScriptGenerationRequest request,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);
        ArgumentException.ThrowIfNullOrWhiteSpace(request.Topic);
        ArgumentException.ThrowIfNullOrWhiteSpace(request.LanguageCode);

        var systemPrompt = BuildSystemPrompt(request);
        var userPrompt = BuildUserPrompt(request);

        var chatClient = _client.GetChatClient(_settings.ChatModel);
        var chatOptions = new ChatCompletionOptions
        {
            Temperature = _settings.Temperature,
            MaxOutputTokenCount = _settings.MaxOutputTokens,
            ResponseFormat = ChatResponseFormat.CreateJsonObjectFormat()
        };

        _logger.LogInformation(
            "Generating script via {Model} for topic={Topic} lang={Lang}",
            _settings.ChatModel, request.Topic, request.LanguageCode);

        var messages = new List<ChatMessage>
        {
            ChatMessage.CreateSystemMessage(systemPrompt),
            ChatMessage.CreateUserMessage(userPrompt)
        };
        var completion = await chatClient.CompleteChatAsync(
            messages,
            chatOptions,
            cancellationToken);

        var raw = completion.Value.Content[0].Text;
        if (string.IsNullOrWhiteSpace(raw))
            throw new InvalidOperationException("OpenAI returned an empty script payload.");

        return ParseScriptJson(raw);
    }

    private static string BuildSystemPrompt(ScriptGenerationRequest request)
    {
        var tone = string.IsNullOrWhiteSpace(request.Tone) ? "engaging, conversational" : request.Tone;
        return $$"""
            You are a senior short-form video scriptwriter for vertical 30-60s YouTube Shorts / TikTok / Instagram Reels.
            Write everything in language code "{{request.LanguageCode}}". Use natural spoken sentences (the result will be fed to a TTS engine).
            Tone: {{tone}}.
            Output a single JSON object with EXACTLY these fields:
              "title": string — short, hooky, no clickbait questions.
              "body": string — the spoken script. Plain prose. No SSML, no stage directions, no markdown.
              "description": string — a 1-2 sentence YouTube description.
              "hashtags": array of strings — 3-6 hashtags WITHOUT the leading '#'.
              "tags": array of strings — 3-8 SEO tags, lowercase.
            Return JSON only, no commentary, no code fences.
            """;
    }

    private static string BuildUserPrompt(ScriptGenerationRequest request)
    {
        var lines = new List<string> { $"Topic: {request.Topic}" };
        if (request.TargetWordCount.HasValue)
            lines.Add($"Target spoken length: ~{request.TargetWordCount.Value} words.");
        if (request.MaxWords.HasValue)
            lines.Add($"Hard maximum: {request.MaxWords.Value} words.");
        if (!string.IsNullOrWhiteSpace(request.AdditionalInstructions))
            lines.Add($"Additional editorial rules:\n{request.AdditionalInstructions}");
        return string.Join("\n", lines);
    }

    private static GeneratedScript ParseScriptJson(string raw)
    {
        try
        {
            using var doc = JsonDocument.Parse(raw);
            var root = doc.RootElement;
            var title = ReadString(root, "title") ?? string.Empty;
            var body = ReadString(root, "body") ?? string.Empty;
            var description = ReadString(root, "description");
            var hashtags = ReadStringArray(root, "hashtags");
            var tags = ReadStringArray(root, "tags");
            return new GeneratedScript(title, body, description, hashtags, tags);
        }
        catch (JsonException ex)
        {
            throw new InvalidOperationException(
                $"OpenAI returned malformed JSON: {raw}", ex);
        }
    }

    private static string? ReadString(JsonElement root, string name)
        => root.TryGetProperty(name, out var prop) && prop.ValueKind == JsonValueKind.String
            ? prop.GetString()
            : null;

    private static IReadOnlyList<string> ReadStringArray(JsonElement root, string name)
    {
        if (!root.TryGetProperty(name, out var prop) || prop.ValueKind != JsonValueKind.Array)
            return Array.Empty<string>();
        var list = new List<string>(prop.GetArrayLength());
        foreach (var item in prop.EnumerateArray())
        {
            if (item.ValueKind == JsonValueKind.String)
            {
                var s = item.GetString();
                if (!string.IsNullOrWhiteSpace(s)) list.Add(s);
            }
        }
        return list;
    }

    // ------------------------------------------------------------------
    //  ITextToSpeechProvider
    // ------------------------------------------------------------------

    public async Task<SpeechSynthesisResult> SynthesizeAsync(
        SpeechSynthesisRequest request,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);
        ArgumentException.ThrowIfNullOrWhiteSpace(request.Text);
        ArgumentException.ThrowIfNullOrWhiteSpace(request.LanguageCode);
        ArgumentException.ThrowIfNullOrWhiteSpace(request.OutputPath);

        var humanized = _humanizer.Humanize(request.Text, request.LanguageCode);

        var audioClient = _client.GetAudioClient(_settings.TtsModel);
        var voice = ResolveVoice(request.Voice ?? _settings.TtsVoice);
        var options = new SpeechGenerationOptions
        {
            ResponseFormat = GeneratedSpeechFormat.Mp3,
            SpeedRatio = request.Speed ?? _settings.TtsSpeed
        };

        _logger.LogInformation(
            "Synthesizing speech: model={Model} voice={Voice} chars={Chars} → {OutputPath}",
            _settings.TtsModel, voice, humanized.Length, request.OutputPath);

        var response = await audioClient.GenerateSpeechAsync(humanized, voice, options, cancellationToken);
        var audioBytes = response.Value.ToArray();

        var directory = Path.GetDirectoryName(Path.GetFullPath(request.OutputPath));
        if (!string.IsNullOrEmpty(directory))
            Directory.CreateDirectory(directory);
        await File.WriteAllBytesAsync(request.OutputPath, audioBytes, cancellationToken);

        return new SpeechSynthesisResult(
            FilePath: Path.GetFullPath(request.OutputPath),
            MediaType: "audio/mpeg",
            Duration: null, // OpenAI TTS doesn't report duration; FFprobe later if exact value is needed.
            HumanizedText: humanized);
    }

    private static GeneratedSpeechVoice ResolveVoice(string voice) => voice?.Trim().ToLowerInvariant() switch
    {
        "alloy"   => GeneratedSpeechVoice.Alloy,
        "echo"    => GeneratedSpeechVoice.Echo,
        "fable"   => GeneratedSpeechVoice.Fable,
        "onyx"    => GeneratedSpeechVoice.Onyx,
        "nova"    => GeneratedSpeechVoice.Nova,
        "shimmer" => GeneratedSpeechVoice.Shimmer,
        _         => GeneratedSpeechVoice.Alloy,
    };
}
