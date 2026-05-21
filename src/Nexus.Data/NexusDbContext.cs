using Microsoft.EntityFrameworkCore;
using Nexus.Core.Enums;
using Nexus.Data.Entities;

namespace Nexus.Data;

/// <summary>
/// EF Core Code-First DbContext for the Nexus-Shorts-Engine PostgreSQL
/// database. The connection string is supplied at composition root time by
/// <c>Nexus.API</c>; <see cref="OnConfiguring(DbContextOptionsBuilder)"/>
/// is intentionally not implemented here.
/// </summary>
public class NexusDbContext : DbContext
{
    public NexusDbContext(DbContextOptions<NexusDbContext> options) : base(options)
    {
    }

    public DbSet<Niche> Niches => Set<Niche>();
    public DbSet<Video> Videos => Set<Video>();
    public DbSet<MediaAsset> MediaAssets => Set<MediaAsset>();
    public DbSet<TrendSnapshot> TrendSnapshots => Set<TrendSnapshot>();
    public DbSet<RenderJob> RenderJobs => Set<RenderJob>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<Niche>(b =>
        {
            b.HasIndex(n => n.Key).IsUnique();
            b.Property(n => n.Key).HasConversion<string>().HasMaxLength(32);
            b.Property(n => n.Tone).HasConversion<string>().HasMaxLength(32);
        });

        modelBuilder.Entity<Video>(b =>
        {
            b.Property(v => v.Status).HasConversion<string>().HasMaxLength(32);
            b.HasIndex(v => v.Status);
            b.HasIndex(v => new { v.NicheId, v.Status });
            b.HasOne(v => v.Niche)
                .WithMany(n => n.Videos)
                .HasForeignKey(v => v.NicheId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<MediaAsset>(b =>
        {
            b.Property(m => m.Kind).HasConversion<string>().HasMaxLength(32);
            b.HasIndex(m => new { m.VideoId, m.Kind });
            b.HasOne(m => m.Video)
                .WithMany(v => v.MediaAssets)
                .HasForeignKey(m => m.VideoId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<TrendSnapshot>(b =>
        {
            b.HasIndex(t => new { t.NicheId, t.CapturedAtUtc });
            b.HasOne(t => t.Niche)
                .WithMany(n => n.TrendSnapshots)
                .HasForeignKey(t => t.NicheId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<RenderJob>(b =>
        {
            b.Property(j => j.StatusAtStart).HasConversion<string>().HasMaxLength(32);
            b.Property(j => j.StatusAtEnd).HasConversion<string?>().HasMaxLength(32);
            b.HasIndex(j => j.HangfireJobId);
            b.HasOne(j => j.Video)
                .WithMany(v => v.RenderJobs)
                .HasForeignKey(j => j.VideoId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        SeedNiches(modelBuilder);
    }

    /// <summary>
    /// Seed the three canonical niches. Real ElevenLabs voice IDs are filled
    /// in at runtime from configuration; the values below are stable
    /// human-readable profile names that the <c>Nexus.Creative</c> module
    /// resolves to the actual voice IDs via the secrets configuration.
    /// </summary>
    private static void SeedNiches(ModelBuilder modelBuilder)
    {
        var now = new DateTimeOffset(2026, 1, 1, 0, 0, 0, TimeSpan.Zero);

        modelBuilder.Entity<Niche>().HasData(
            new
            {
                Id = 1,
                Key = NicheKey.Finance,
                DisplayName = "Finance",
                Tone = ScriptTone.FormalDataDriven,
                ElevenLabsVoiceProfile = "finance-deep-calm",
                MusicLibraryDirectory = "Assets/Music/Finance",
                IsActive = true,
                RenderWeight = 1.0,
                CreatedAtUtc = now,
                UpdatedAtUtc = now
            },
            new
            {
                Id = 2,
                Key = NicheKey.TechAI,
                DisplayName = "Tech & AI",
                Tone = ScriptTone.DynamicEnergetic,
                ElevenLabsVoiceProfile = "techai-enthusiastic",
                MusicLibraryDirectory = "Assets/Music/TechAI",
                IsActive = true,
                RenderWeight = 1.0,
                CreatedAtUtc = now,
                UpdatedAtUtc = now
            },
            new
            {
                Id = 3,
                Key = NicheKey.Legal,
                DisplayName = "Legal & Court",
                Tone = ScriptTone.NarrativeDramatic,
                ElevenLabsVoiceProfile = "legal-narrative-dramatic",
                MusicLibraryDirectory = "Assets/Music/Legal",
                IsActive = true,
                RenderWeight = 1.0,
                CreatedAtUtc = now,
                UpdatedAtUtc = now
            });
    }
}
