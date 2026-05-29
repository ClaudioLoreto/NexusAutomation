using Microsoft.EntityFrameworkCore;
using Nexus.Core.Enums;
using Nexus.Data.Entities;

namespace Nexus.Data;

public class NexusDbContext : DbContext
{
    public NexusDbContext(DbContextOptions<NexusDbContext> options)
        : base(options)
    {
    }

    public DbSet<Niche> Niches => Set<Niche>();
    public DbSet<Video> Videos => Set<Video>();
    public DbSet<Trend> Trends => Set<Trend>();
    public DbSet<VideoJob> VideoJobs => Set<VideoJob>();
    public DbSet<VideoAsset> VideoAssets => Set<VideoAsset>();
    public DbSet<RenderError> RenderErrors => Set<RenderError>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Niche>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.HasIndex(e => e.Type).IsUnique();
            entity.Property(e => e.Type).HasConversion<int>();
            entity.Property(e => e.Name).HasMaxLength(128);
            entity.Property(e => e.LanguageCode).HasMaxLength(16);
            entity.Property(e => e.ScriptTone).HasMaxLength(256);
            entity.Property(e => e.TtsVoice).HasMaxLength(64);
            entity.Property(e => e.ElevenLabsVoiceId).HasMaxLength(64);
            entity.Property(e => e.MusicDirectory).HasMaxLength(512);
            entity.Property(e => e.KaraokeFontFamily).HasMaxLength(128);
            entity.Property(e => e.KaraokeFillColor).HasMaxLength(16);
            entity.Property(e => e.KaraokeHighlightColor).HasMaxLength(16);
            entity.Property(e => e.KaraokeOutlineColor).HasMaxLength(16);
            entity.Property(e => e.KaraokeBackgroundColor).HasMaxLength(16);
            entity.Property(e => e.OverlayGifPath).HasMaxLength(512);
            entity.Property(e => e.AdditionalScriptInstructions).HasMaxLength(4000);

            entity.HasData(SeedNiches());
        });

        modelBuilder.Entity<Video>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Status).HasConversion<int>();
            entity.Property(e => e.Title).HasMaxLength(256);
            entity.Property(e => e.LocalMediaPath).HasMaxLength(1024);
            entity.Property(e => e.OutputPath).HasMaxLength(1024);
            entity.HasOne(e => e.Niche)
                .WithMany(n => n.Videos)
                .HasForeignKey(e => e.NicheId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<Trend>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.HasIndex(e => new { e.NicheId, e.MeasuredAtUtc });
            entity.Property(e => e.CompetitorChannelId).HasMaxLength(64);
            entity.Property(e => e.CompetitorVideoId).HasMaxLength(32);
            entity.HasOne(e => e.Niche)
                .WithMany(n => n.Trends)
                .HasForeignKey(e => e.NicheId)
                .OnDelete(DeleteBehavior.SetNull);
            entity.HasOne(e => e.Video)
                .WithMany(v => v.Trends)
                .HasForeignKey(e => e.VideoId)
                .OnDelete(DeleteBehavior.SetNull);
        });

        modelBuilder.Entity<VideoJob>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Phase).HasConversion<int>();
            entity.Property(e => e.Topic).HasMaxLength(512);
            entity.Property(e => e.StoryblocksQuery).HasMaxLength(512);
            entity.Property(e => e.Title).HasMaxLength(256);
            entity.Property(e => e.Description).HasMaxLength(2000);
            entity.Property(e => e.TagsCsv).HasMaxLength(1024);
            entity.Property(e => e.HashtagsCsv).HasMaxLength(512);
            entity.Property(e => e.FinalOutputPath).HasMaxLength(1024);
            entity.Property(e => e.CostUsd).HasColumnType("numeric(12,4)");
            entity.HasIndex(e => e.Phase);
            entity.HasIndex(e => new { e.NicheId, e.CreatedAtUtc });
            entity.HasOne(e => e.Niche)
                .WithMany(n => n.VideoJobs)
                .HasForeignKey(e => e.NicheId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<VideoAsset>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Kind).HasConversion<int>();
            entity.Property(e => e.Path).HasMaxLength(1024);
            entity.Property(e => e.MediaType).HasMaxLength(64);
            entity.HasIndex(e => new { e.VideoJobId, e.Kind });
            entity.HasOne(e => e.VideoJob)
                .WithMany(j => j.Assets)
                .HasForeignKey(e => e.VideoJobId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<RenderError>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.PhaseAtFailure).HasConversion<int>();
            entity.Property(e => e.ErrorCode).HasMaxLength(128);
            entity.Property(e => e.Message).HasMaxLength(2000);
            entity.HasIndex(e => new { e.VideoJobId, e.CreatedAtUtc });
            entity.HasOne(e => e.VideoJob)
                .WithMany(j => j.Errors)
                .HasForeignKey(e => e.VideoJobId)
                .OnDelete(DeleteBehavior.Cascade);
        });
    }

    /// <summary>
    /// Seed the six built-in niches: the original 3 (Finance / Tech-AI /
    /// Legal) PLUS three example typologies (Storia Antica IT, Brainrot
    /// Facts EN, Wholesome Animals EN) showcasing how karaoke and overlay
    /// settings differ between presets.
    /// </summary>
    private static IEnumerable<Niche> SeedNiches() => new[]
    {
        new Niche
        {
            Id = 1, Type = NicheType.Finance, Name = "Finance",
            LanguageCode = "en-US", ScriptTone = "Formal, authoritative",
            TargetWordCount = 130, MaxWords = 150,
            TtsVoice = "onyx", TtsSpeed = 1.0f,
            MusicDirectory = "Assets/Music/Finance",
            KaraokeFontFamily = "The Bold Font", KaraokeFontSize = 96, KaraokeHighlightFontSize = 140,
            KaraokeFillColor = "#FFFFFF", KaraokeHighlightColor = "#FFD166",
            KaraokeOutlineColor = "#000000", KaraokeBackgroundColor = "#0D1321",
            KaraokeYPositionPercent = 7.0,
            OverlayGifPath = "Assets/Overlays/subscribe.gif",
            OverlayGifPositionPercent = 95.3, OverlayGifTailSeconds = 5.0, OverlayGifLoopCount = 0,
            AdditionalScriptInstructions = "End every short with a single concrete takeaway.",
            QueuePriority = 100, IsActive = true,
        },
        new Niche
        {
            Id = 2, Type = NicheType.TechAndAi, Name = "Tech & AI",
            LanguageCode = "en-US", ScriptTone = "Dynamic, enthusiastic",
            TargetWordCount = 130, MaxWords = 150,
            TtsVoice = "nova", TtsSpeed = 1.05f,
            MusicDirectory = "Assets/Music/Tech",
            KaraokeFontFamily = "The Bold Font", KaraokeFontSize = 96, KaraokeHighlightFontSize = 140,
            KaraokeFillColor = "#FFFFFF", KaraokeHighlightColor = "#39FF14",
            KaraokeOutlineColor = "#000000", KaraokeBackgroundColor = "#0D1321",
            KaraokeYPositionPercent = 7.0,
            OverlayGifPath = "Assets/Overlays/subscribe.gif",
            OverlayGifPositionPercent = 95.3, OverlayGifTailSeconds = 5.0, OverlayGifLoopCount = 0,
            AdditionalScriptInstructions = "Open with a 'wait, what?' hook.",
            QueuePriority = 100, IsActive = true,
        },
        new Niche
        {
            Id = 3, Type = NicheType.LegalAndCourt, Name = "Legal & Court",
            LanguageCode = "en-US", ScriptTone = "Narrative, dramatic pauses",
            TargetWordCount = 130, MaxWords = 150,
            TtsVoice = "echo", TtsSpeed = 1.0f,
            MusicDirectory = "Assets/Music/Legal",
            KaraokeFontFamily = "The Bold Font", KaraokeFontSize = 96, KaraokeHighlightFontSize = 140,
            KaraokeFillColor = "#FFFFFF", KaraokeHighlightColor = "#FFD166",
            KaraokeOutlineColor = "#000000", KaraokeBackgroundColor = "#0D1321",
            KaraokeYPositionPercent = 7.0,
            OverlayGifPath = "Assets/Overlays/subscribe.gif",
            OverlayGifPositionPercent = 95.3, OverlayGifTailSeconds = 5.0, OverlayGifLoopCount = 0,
            AdditionalScriptInstructions = "Build to a single chilling fact halfway through.",
            QueuePriority = 100, IsActive = true,
        },

        // -- Example typologies (multi-niche scalability proof) ---------
        new Niche
        {
            Id = 4, Type = NicheType.StoriaAntica, Name = "Storia Antica",
            LanguageCode = "it-IT", ScriptTone = "Drammatico, epico",
            TargetWordCount = 150, MaxWords = 170,
            TtsVoice = "onyx", TtsSpeed = 0.95f,
            MusicDirectory = "Assets/Music/HistoryEpic",
            KaraokeFontFamily = "The Bold Font", KaraokeFontSize = 96, KaraokeHighlightFontSize = 140,
            KaraokeFillColor = "#FFFFFF", KaraokeHighlightColor = "#D4AF37",
            KaraokeOutlineColor = "#1A0F00", KaraokeBackgroundColor = "#1A0F00",
            KaraokeYPositionPercent = 8.0,
            OverlayGifPath = "Assets/Overlays/subscribe.gif",
            OverlayGifPositionPercent = 95.3, OverlayGifTailSeconds = 5.0, OverlayGifLoopCount = 0,
            AdditionalScriptInstructions = "Apri con una scena visiva concreta. Ogni short deve chiudere con una rivelazione storica sorprendente.",
            QueuePriority = 100, IsActive = true,
        },
        new Niche
        {
            Id = 5, Type = NicheType.BrainrotFacts, Name = "Brainrot Facts",
            LanguageCode = "en-US", ScriptTone = "Punchy, fast-paced, slang-friendly",
            TargetWordCount = 110, MaxWords = 130,
            TtsVoice = "nova", TtsSpeed = 1.15f,
            MusicDirectory = "Assets/Music/UpliftEpic",
            KaraokeFontFamily = "The Bold Font", KaraokeFontSize = 102, KaraokeHighlightFontSize = 150,
            KaraokeFillColor = "#FFFFFF", KaraokeHighlightColor = "#FF14C8",
            KaraokeOutlineColor = "#000000", KaraokeBackgroundColor = "#0D1321",
            KaraokeYPositionPercent = 6.0,
            OverlayGifPath = "Assets/Overlays/subscribe.gif",
            OverlayGifPositionPercent = 95.3, OverlayGifTailSeconds = 4.0, OverlayGifLoopCount = -1,
            AdditionalScriptInstructions = "Hook in 4 words or less. No filler.",
            QueuePriority = 100, IsActive = true,
        },
        new Niche
        {
            Id = 6, Type = NicheType.WholesomeAnimals, Name = "Wholesome Animals",
            LanguageCode = "en-US", ScriptTone = "Gentle, warm, comforting",
            TargetWordCount = 120, MaxWords = 140,
            TtsVoice = "shimmer", TtsSpeed = 0.95f,
            MusicDirectory = "Assets/Music/Wholesome",
            KaraokeFontFamily = "The Bold Font", KaraokeFontSize = 92, KaraokeHighlightFontSize = 130,
            KaraokeFillColor = "#FFFFFF", KaraokeHighlightColor = "#FFB5E8",
            KaraokeOutlineColor = "#000000", KaraokeBackgroundColor = "#1B0F2A",
            KaraokeYPositionPercent = 9.0,
            OverlayGifPath = "Assets/Overlays/subscribe.gif",
            OverlayGifPositionPercent = 95.3, OverlayGifTailSeconds = 5.0, OverlayGifLoopCount = 0,
            AdditionalScriptInstructions = "Always end on a kindness moment, never sad endings.",
            QueuePriority = 100, IsActive = true,
        },
    };
}
