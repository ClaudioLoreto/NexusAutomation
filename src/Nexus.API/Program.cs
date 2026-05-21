using Hangfire;
using Hangfire.PostgreSql;
using Microsoft.EntityFrameworkCore;
using Nexus.Analysis;
using Nexus.API.Jobs;
using Nexus.API.Services;
using Nexus.Core.Configuration;
using Nexus.Core.Interfaces;
using Nexus.Creative;
using Nexus.Data;
using Nexus.Data.Seeding;
using Nexus.Engine;
using Nexus.Scraper;

var builder = WebApplication.CreateBuilder(args);

builder.Configuration
    .AddJsonFile("appsettings.json", optional: false)
    .AddJsonFile($"appsettings.{builder.Environment.EnvironmentName}.json", optional: true)
    .AddJsonFile("appsettings.Secrets.json", optional: true)
    .AddEnvironmentVariables();

var connectionString = builder.Configuration.GetConnectionString("NexusDb")
    ?? "Host=localhost;Database=nexus_automation;Username=postgres;Password=postgres";

builder.Services.AddDbContext<NexusDbContext>(options =>
    options.UseNpgsql(connectionString));

builder.Services.Configure<YouTubeApiOptions>(
    builder.Configuration.GetSection(YouTubeApiOptions.SectionName));
builder.Services.Configure<ClaudeApiOptions>(
    builder.Configuration.GetSection(ClaudeApiOptions.SectionName));
builder.Services.Configure<ElevenLabsOptions>(
    builder.Configuration.GetSection(ElevenLabsOptions.SectionName));
builder.Services.Configure<ScraperOptions>(
    builder.Configuration.GetSection(ScraperOptions.SectionName));
builder.Services.Configure<EngineOptions>(
    builder.Configuration.GetSection(EngineOptions.SectionName));

builder.Services.AddAnalysisServices();
builder.Services.AddScraperServices();
builder.Services.AddCreativeServices();
builder.Services.AddEngineServices();
builder.Services.AddScoped<INicheManager, NicheManager>();
builder.Services.AddScoped<IVideoPipelineOrchestrator, VideoPipelineOrchestrator>();

builder.Services.AddHangfire(config =>
    config.SetDataCompatibilityLevel(CompatibilityLevel.Version_180)
        .UseSimpleAssemblyNameTypeSerializer()
        .UseRecommendedSerializerSettings()
        .UsePostgreSqlStorage(options =>
            options.UseNpgsqlConnection(connectionString)));
builder.Services.AddHangfireServer();

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new() { Title = "Nexus Automation API", Version = "v1" });
});

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowDashboard", policy =>
        policy.WithOrigins("http://localhost:4200")
            .AllowAnyHeader()
            .AllowAnyMethod());
});

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<NexusDbContext>();
    var logger = scope.ServiceProvider.GetService<ILogger<Program>>();

    try
    {
        await db.Database.MigrateAsync();
        logger?.LogInformation("Database migrated successfully");
    }
    catch (Exception ex)
    {
        logger?.LogWarning(ex, "Migration failed (database may not be available). Continuing...");
    }

    try
    {
        await NicheSeeder.SeedAsync(db, scope.ServiceProvider.GetService<ILogger<NexusDbContext>>());
    }
    catch (Exception ex)
    {
        logger?.LogWarning(ex, "Seeding failed. Continuing...");
    }
}

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseCors("AllowDashboard");
app.UseAuthorization();
app.MapControllers();

app.UseHangfireDashboard("/hangfire");

RecurringJob.AddOrUpdate<VideoPipelineJob>(
    "process-pending-videos",
    job => job.ProcessPendingVideos(),
    "*/15 * * * *");

RecurringJob.AddOrUpdate<VideoPipelineJob>(
    "rebalance-niche-queue",
    job => job.RebalanceNicheQueue(),
    Cron.Hourly);

app.Run();
