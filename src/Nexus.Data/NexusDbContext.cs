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

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Niche>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.HasIndex(e => e.Type).IsUnique();
            entity.Property(e => e.Name).HasMaxLength(128);
            entity.Property(e => e.ScriptTone).HasMaxLength(256);
            entity.Property(e => e.ElevenLabsVoiceId).HasMaxLength(64);
            entity.Property(e => e.MusicDirectory).HasMaxLength(512);

            entity.HasData(
                new Niche
                {
                    Id = 1,
                    Type = NicheType.Finance,
                    Name = "Finance",
                    ScriptTone = "Formal, authoritative",
                    ElevenLabsVoiceId = "",
                    MusicDirectory = "Assets/Music/Finance",
                    QueuePriority = 100
                },
                new Niche
                {
                    Id = 2,
                    Type = NicheType.TechAndAi,
                    Name = "Tech & AI",
                    ScriptTone = "Dynamic, enthusiastic",
                    ElevenLabsVoiceId = "",
                    MusicDirectory = "Assets/Music/Tech",
                    QueuePriority = 100
                },
                new Niche
                {
                    Id = 3,
                    Type = NicheType.LegalAndCourt,
                    Name = "Legal & Court",
                    ScriptTone = "Narrative, dramatic pauses",
                    ElevenLabsVoiceId = "",
                    MusicDirectory = "Assets/Music/Legal",
                    QueuePriority = 100
                });
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
    }
}
