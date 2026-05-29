using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Nexus.Engine.Interfaces;
using Nexus.Engine.Models;
using Nexus.Scraper.Storyblocks;

namespace Nexus.API.Controllers;

/// <summary>
/// TEMPORARY end-to-end smoke test endpoint for the rendering pipeline.
/// Wires:
///   1. <see cref="IScriptGenerator"/>      (OpenAI Chat Completions)
///   2. <see cref="ITextToSpeechProvider"/> (OpenAI TTS)
///   3. <see cref="IWordTimingSource"/>     (Whisper API word timings)
///   4. <see cref="IVideoAssembler"/>       (three-pass FFmpeg karaoke render)
///
/// Picks the most-recently-modified MP4 in the Storyblocks download directory
/// as the visual track, generates a fresh script + voiceover + word timings,
/// and renders a final <c>test_output.mp4</c> under <c>./data/test-render/</c>.
///
/// This controller is intentionally an integration smoke test, NOT production
/// orchestration. Real shorts generation will go through a Hangfire job that
/// pulls niche config from the DB and persists progress to <c>VideoJob</c>
/// rows — see the engine scaffold's TODO list. Delete this controller once
/// the production pipeline lands.
/// </summary>
[ApiController]
[Route("api/test-render")]
public sealed class TestRenderController : ControllerBase
{
    private const string DefaultTopic = "The mysterious dark forest";
    private const string DefaultLanguage = "en-US";
    private const int TargetWordCount = 30; // ~3 sentences at ~10 words each

    private readonly IScriptGenerator _scriptGenerator;
    private readonly ITextToSpeechProvider _tts;
    private readonly IWordTimingSource _wordTimings;
    private readonly IVideoAssembler _assembler;
    private readonly StoryblocksScraperOptions _storyblocksOptions;
    private readonly ILogger<TestRenderController> _logger;

    public TestRenderController(
        IScriptGenerator scriptGenerator,
        ITextToSpeechProvider tts,
        IWordTimingSource wordTimings,
        IVideoAssembler assembler,
        IOptions<StoryblocksScraperOptions> storyblocksOptions,
        ILogger<TestRenderController> logger)
    {
        _scriptGenerator = scriptGenerator;
        _tts = tts;
        _wordTimings = wordTimings;
        _assembler = assembler;
        _storyblocksOptions = storyblocksOptions.Value;
        _logger = logger;
    }

    /// <summary>
    /// GET /api/test-render — fire-and-walk-away integration test. Returns a
    /// JSON breakdown of every intermediate artifact so the operator can spot
    /// which stage failed if the final render isn't produced.
    /// </summary>
    [HttpGet]
    public async Task<IActionResult> Run(CancellationToken cancellationToken)
    {
        // === 1. Pick the most-recent MP4 from the Storyblocks downloads dir ===
        // Recursive search: the scraper writes downloads under nested per-query
        // folders (e.g. `data/downloads/dark-forest/clip-1.mp4`), so a flat
        // EnumerateFiles wouldn't see anything.
        var downloadDir = Path.GetFullPath(_storyblocksOptions.DownloadDirectory);
        if (!Directory.Exists(downloadDir))
        {
            return NotFound(new
            {
                stage = "pick-clip",
                error = "Storyblocks download directory does not exist yet — " +
                        "run /api/scraper/search to populate it first.",
                expectedDirectory = downloadDir
            });
        }

        var sourceClip = new DirectoryInfo(downloadDir)
            .EnumerateFiles("*.mp4", SearchOption.AllDirectories)
            .OrderByDescending(f => f.LastWriteTimeUtc)
            .FirstOrDefault();
        if (sourceClip is null)
        {
            return NotFound(new
            {
                stage = "pick-clip",
                error = "No .mp4 files found under the Storyblocks download " +
                        "directory. Run /api/scraper/search first.",
                searchedDirectory = downloadDir
            });
        }
        _logger.LogInformation(
            "TestRender: using Storyblocks clip {ClipPath} ({SizeKb} KB, modified {Modified:o})",
            sourceClip.FullName,
            sourceClip.Length / 1024,
            sourceClip.LastWriteTimeUtc);

        // === Prepare the test-render working directory ===
        // All intermediate artifacts (voiceover.mp3, the final MP4) live here
        // so the operator can hand the folder around for debugging.
        var workDir = Path.GetFullPath("./data/test-render");
        Directory.CreateDirectory(workDir);
        var voiceoverPath = Path.Combine(workDir, "voiceover.mp3");
        var outputPath = Path.Combine(workDir, "test_output.mp4");

        // === 2. Generate the script ===
        // 3-sentence target → ~30 words. MaxWords is the hard cap; the engine
        // will refuse to exceed it.
        _logger.LogInformation("TestRender: generating script for topic '{Topic}'...", DefaultTopic);
        var script = await _scriptGenerator.GenerateAsync(
            new ScriptGenerationRequest(
                Topic: DefaultTopic,
                LanguageCode: DefaultLanguage,
                Tone: "dramatic, atmospheric",
                TargetWordCount: TargetWordCount,
                MaxWords: TargetWordCount + 10,
                AdditionalInstructions:
                    "Exactly 3 short sentences. No call-to-action. No hashtags inside the body."),
            cancellationToken);
        _logger.LogInformation(
            "TestRender: script generated, title='{Title}', body length={Length} chars",
            script.Title,
            script.Body.Length);

        // === 3. Synthesize TTS ===
        // Implementations run the humanizer chain internally, so we pass the
        // raw script body untouched.
        _logger.LogInformation("TestRender: synthesizing TTS to {VoiceoverPath}...", voiceoverPath);
        var tts = await _tts.SynthesizeAsync(
            new SpeechSynthesisRequest(
                Text: script.Body,
                LanguageCode: DefaultLanguage,
                OutputPath: voiceoverPath),
            cancellationToken);
        _logger.LogInformation(
            "TestRender: TTS written, file={File}, mediaType={MediaType}, duration={Duration}",
            tts.FilePath,
            tts.MediaType,
            tts.Duration);

        // === 4. Word timings via Whisper (or whichever IWordTimingSource is wired) ===
        // We compute them HERE rather than letting the assembler do it on
        // demand, because the response should report the word count even if
        // the renderer fails downstream — a clear signal that the karaoke
        // alignment stage worked.
        _logger.LogInformation("TestRender: fetching word timings from Whisper...");
        var timings = await _wordTimings.GetWordTimingsAsync(
            tts.FilePath,
            DefaultLanguage,
            cancellationToken);
        _logger.LogInformation("TestRender: got {Count} word timings", timings.Count);

        // === 5. Render the final MP4 ===
        // Passing WordTimings explicitly skips the redundant Whisper call the
        // assembler would otherwise do internally. Per-niche karaoke styling
        // and Subscribe-GIF settings are intentionally LEFT NULL here so the
        // assembler falls back to its DI-registered defaults (the values bound
        // from Engine.KaraokeDefaults / Engine.SubscribeOverlayDefaults in
        // appsettings.json).
        _logger.LogInformation("TestRender: assembling final video to {OutputPath}...", outputPath);
        var render = await _assembler.AssembleAsync(
            new VideoAssemblyRequest(
                ClipPaths: new[] { sourceClip.FullName },
                VoiceoverPath: tts.FilePath,
                OutputPath: outputPath,
                LanguageCode: DefaultLanguage,
                WordTimings: timings),
            cancellationToken);
        _logger.LogInformation(
            "TestRender: render complete, file={Output}, duration={Duration}",
            render.OutputPath,
            render.Duration);

        return Ok(new
        {
            topic = DefaultTopic,
            languageCode = DefaultLanguage,
            sourceClip = sourceClip.FullName,
            script = new
            {
                title = script.Title,
                body = script.Body,
                wordCount = script.Body.Split(' ', StringSplitOptions.RemoveEmptyEntries).Length,
                hashtags = script.Hashtags,
                tags = script.Tags
            },
            voiceover = new
            {
                filePath = tts.FilePath,
                mediaType = tts.MediaType,
                duration = tts.Duration,
                humanizedText = tts.HumanizedText
            },
            wordTimings = new
            {
                count = timings.Count,
                first = timings.Take(3).Select(t => new { t.Word, t.StartSeconds, t.EndSeconds }),
                last = timings.TakeLast(3).Select(t => new { t.Word, t.StartSeconds, t.EndSeconds })
            },
            render = new
            {
                outputPath = render.OutputPath,
                durationSeconds = render.Duration.TotalSeconds
            }
        });
    }
}
