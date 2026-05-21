using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Nexus.Data.Models;

namespace Nexus.Data.Configurations;

public class NicheConfigConfiguration : IEntityTypeConfiguration<NicheConfig>
{
    public void Configure(EntityTypeBuilder<NicheConfig> builder)
    {
        builder.ToTable("NicheConfigs");
        builder.HasKey(n => n.Id);
        builder.Property(n => n.Id).HasDefaultValueSql("gen_random_uuid()");

        builder.Property(n => n.NicheType).HasConversion<string>().HasMaxLength(50);
        builder.Property(n => n.DisplayName).HasMaxLength(200).IsRequired();
        builder.Property(n => n.ScriptTone).HasConversion<string>().HasMaxLength(50);
        builder.Property(n => n.VoiceStyle).HasConversion<string>().HasMaxLength(50);
        builder.Property(n => n.MusicGenre).HasConversion<string>().HasMaxLength(50);
        builder.Property(n => n.MusicDirectoryPath).HasMaxLength(500);
        builder.Property(n => n.SearchKeywords).HasColumnType("text");

        builder.HasIndex(n => n.NicheType).IsUnique();
    }
}
