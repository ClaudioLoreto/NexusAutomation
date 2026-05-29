using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Nexus.Engine.Configuration;
using Nexus.Engine.Interfaces;
using Nexus.Engine.Services.AI;
using Nexus.Engine.Services.AI.Humanization;
using Nexus.Engine.Services.Media;

namespace Nexus.Engine.DependencyInjection;

/// <summary>
/// Single-call DI registration for the Nexus shorts-generation engine.
///
/// <para>
/// Usage from <c>Program.cs</c>:
/// <code>
/// builder.Services.AddNexusEngine(builder.Configuration);
/// </code>
/// </para>
///
/// <para>
/// Wires up:
/// <list type="bullet">
///   <item><see cref="OpenAiSettings"/>, <see cref="KaraokeStyle"/>,
///   <see cref="OverlayGifSettings"/> from configuration</item>
///   <item>Humanizer chain (<see cref="NumberSpellOutRule"/> → <see cref="RomanNumeralSpellOutRule"/>)
///   plus <see cref="TextHumanizer"/> orchestrator</item>
///   <item><see cref="OpenAIService"/> as both <see cref="IScriptGenerator"/>
///   AND <see cref="ITextToSpeechProvider"/></item>
///   <item><see cref="WhisperWordTimingSource"/> as the default
///   <see cref="IWordTimingSource"/></item>
///   <item><see cref="FFmpegVideoAssembler"/> + <see cref="AssKaraokeWriter"/>
///   + <see cref="FfmpegRunner"/></item>
/// </list>
/// </para>
/// </summary>
public static class EngineServiceCollectionExtensions
{
    public static IServiceCollection AddNexusEngine(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        ArgumentNullException.ThrowIfNull(services);
        ArgumentNullException.ThrowIfNull(configuration);

        // -- Configuration sections -------------------------------------------
        services
            .Configure<OpenAiSettings>(configuration.GetSection(OpenAiSettings.SectionName))
            .Configure<KaraokeStyle>(configuration.GetSection("Engine:KaraokeDefaults"))
            .Configure<OverlayGifSettings>(configuration.GetSection("Engine:SubscribeOverlayDefaults"));

        // -- Humanizer chain --------------------------------------------------
        services.AddSingleton<IHumanizationRule, NumberSpellOutRule>();
        services.AddSingleton<IHumanizationRule, RomanNumeralSpellOutRule>();
        services.AddSingleton<ITextHumanizer, TextHumanizer>();

        // -- OpenAI: one client, two roles -----------------------------------
        services.AddSingleton<OpenAIService>();
        services.AddSingleton<IScriptGenerator>(sp => sp.GetRequiredService<OpenAIService>());
        services.AddSingleton<ITextToSpeechProvider>(sp => sp.GetRequiredService<OpenAIService>());

        // -- Whisper word-timing source (typed HttpClient) -------------------
        services
            .AddHttpClient<IWordTimingSource, WhisperWordTimingSource>(c =>
            {
                c.Timeout = TimeSpan.FromMinutes(5);
            });

        // -- Media (ASS writer + FFmpeg assembler + FFmpeg runner) -----------
        services.AddSingleton<FfmpegRunner>();
        services.AddSingleton<AssKaraokeWriter>();
        services.AddSingleton<IVideoAssembler, FFmpegVideoAssembler>();

        return services;
    }
}
