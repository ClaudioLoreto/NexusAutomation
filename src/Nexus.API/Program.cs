using Microsoft.EntityFrameworkCore;
using Nexus.Data;

var builder = WebApplication.CreateBuilder(args);

// Configuration sources -------------------------------------------------------
// Local secrets live in secrets.json at the solution root and are gitignored.
// CI/CD picks up the same values from environment variables (see .env.example).
builder.Configuration
    .AddJsonFile("secrets.json", optional: true, reloadOnChange: true)
    .AddEnvironmentVariables(prefix: "NEXUS_");

// PostgreSQL via EF Core Code-First -------------------------------------------
var pgConnection =
    builder.Configuration.GetSection("PostgreSQL")["ConnectionString"]
    ?? builder.Configuration["PG_CONNECTION_STRING"];

builder.Services.AddDbContext<NexusDbContext>(options =>
{
    if (!string.IsNullOrWhiteSpace(pgConnection))
    {
        options.UseNpgsql(pgConnection);
    }
    else
    {
        options.UseNpgsql("Host=localhost;Port=5432;Database=nexus_shorts;Username=nexus;Password=changeme");
    }
});

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseAuthorization();
app.MapControllers();

app.Run();
