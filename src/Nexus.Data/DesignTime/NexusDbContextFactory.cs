using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace Nexus.Data.DesignTime;

/// <summary>
/// Used by <c>dotnet ef</c> at design time (e.g. when running
/// <c>dotnet ef migrations add &lt;Name&gt;</c>). At runtime the API supplies
/// the real connection string via DI, so this factory only needs a
/// placeholder good enough for the EF tooling to scan the model.
/// </summary>
public class NexusDbContextFactory : IDesignTimeDbContextFactory<NexusDbContext>
{
    public NexusDbContext CreateDbContext(string[] args)
    {
        var connectionString =
            Environment.GetEnvironmentVariable("NEXUS_PG_CONNECTION_STRING")
            ?? "Host=localhost;Port=5432;Database=nexus_shorts;Username=nexus;Password=designtime";

        var optionsBuilder = new DbContextOptionsBuilder<NexusDbContext>();
        optionsBuilder.UseNpgsql(connectionString);
        return new NexusDbContext(optionsBuilder.Options);
    }
}
