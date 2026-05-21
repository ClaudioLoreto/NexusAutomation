using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Nexus.Data.Models;

namespace Nexus.Data.Configurations;

public class VideoProjectConfiguration : IEntityTypeConfiguration<VideoProject>
{
    public void Configure(EntityTypeBuilder<VideoProject> builder)
    {
        builder.ToTable("VideoProjects");
        builder.HasKey(v => v.Id);
        builder.Property(v => v.Id).HasDefaultValueSql("gen_random_uuid()");

        builder.Property(v => v.Title).HasMaxLength(500).IsRequired();
        builder.Property(v => v.Niche).HasConversion<string>().HasMaxLength(50);
        builder.Property(v => v.Status).HasConversion<string>().HasMaxLength(50);
        builder.Property(v => v.ScriptText).HasColumnType("text");
        builder.Property(v => v.SsmlText).HasColumnType("text");
        builder.Property(v => v.MediaTags).HasColumnType("text");
        builder.Property(v => v.ErrorMessage).HasColumnType("text");

        builder.Property(v => v.MediaFilePath).HasMaxLength(1000);
        builder.Property(v => v.AudioFilePath).HasMaxLength(1000);
        builder.Property(v => v.SubtitleFilePath).HasMaxLength(1000);
        builder.Property(v => v.OutputFilePath).HasMaxLength(1000);

        builder.Property(v => v.CreatedAtUtc).IsRequired();

        builder.HasIndex(v => v.Status);
        builder.HasIndex(v => v.Niche);
        builder.HasIndex(v => v.CreatedAtUtc);

        builder.HasOne(v => v.NicheConfig)
            .WithMany(n => n.Videos)
            .HasForeignKey(v => v.NicheConfigId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
