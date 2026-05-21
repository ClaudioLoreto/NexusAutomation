using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Nexus.Data.Models;

namespace Nexus.Data.Configurations;

public class TrendSnapshotConfiguration : IEntityTypeConfiguration<TrendSnapshot>
{
    public void Configure(EntityTypeBuilder<TrendSnapshot> builder)
    {
        builder.ToTable("TrendSnapshots");
        builder.HasKey(t => t.Id);
        builder.Property(t => t.Id).HasDefaultValueSql("gen_random_uuid()");

        builder.Property(t => t.Niche).HasConversion<string>().HasMaxLength(50);
        builder.Property(t => t.TopVideoIds).HasColumnType("text");
        builder.Property(t => t.AnalyzedAtUtc).IsRequired();

        builder.HasIndex(t => t.Niche);
        builder.HasIndex(t => t.AnalyzedAtUtc);

        builder.HasOne(t => t.NicheConfig)
            .WithMany(n => n.TrendSnapshots)
            .HasForeignKey(t => t.NicheConfigId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
