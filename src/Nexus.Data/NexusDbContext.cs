using Microsoft.EntityFrameworkCore;
using Nexus.Data.Configurations;
using Nexus.Data.Models;

namespace Nexus.Data;

public class NexusDbContext : DbContext
{
    public NexusDbContext(DbContextOptions<NexusDbContext> options) : base(options) { }

    public DbSet<VideoProject> VideoProjects => Set<VideoProject>();
    public DbSet<NicheConfig> NicheConfigs => Set<NicheConfig>();
    public DbSet<TrendSnapshot> TrendSnapshots => Set<TrendSnapshot>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfiguration(new VideoProjectConfiguration());
        modelBuilder.ApplyConfiguration(new NicheConfigConfiguration());
        modelBuilder.ApplyConfiguration(new TrendSnapshotConfiguration());

        base.OnModelCreating(modelBuilder);
    }
}
