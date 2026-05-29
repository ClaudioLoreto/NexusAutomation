using System.Text.Json.Serialization;
using Microsoft.EntityFrameworkCore;
using Nexus.Data;
using Nexus.Data.DependencyInjection;
using Nexus.Engine.DependencyInjection;
using Nexus.Scraper.DependencyInjection;
using Nexus.Scraper.Storyblocks;

const string AngularDevCorsPolicy = "AngularDev";

var builder = WebApplication.CreateBuilder(args);

builder.Configuration
    .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
    .AddJsonFile($"appsettings.{builder.Environment.EnvironmentName}.json", optional: true)
    .AddJsonFile(Path.Combine(builder.Environment.ContentRootPath, "..", "..", "config", "secrets.json"), optional: true)
    .AddEnvironmentVariables();

builder.Services
    .AddControllers()
    .AddJsonOptions(options =>
    {
        // Serialize enums as their string names ("Pending", "Finance") instead of ints.
        // This keeps the Angular models (VideoStatus / NicheType union types) honest and
        // lets dictionaries keyed by enum (e.g. DashboardSummaryDto.CountByStatus)
        // render as { "Pending": 3, ... } instead of { "0": 3, ... }.
        options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());
        options.JsonSerializerOptions.DictionaryKeyPolicy = null;
    });

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddCors(options =>
{
    options.AddPolicy(AngularDevCorsPolicy, policy =>
    {
        var allowedOrigins = builder.Configuration
            .GetSection("Cors:AllowedOrigins")
            .Get<string[]>()
            ?? new[] { "http://localhost:4200", "https://localhost:4200" };

        policy.WithOrigins(allowedOrigins)
              .AllowAnyHeader()
              .AllowAnyMethod()
              .AllowCredentials();
    });
});

var connectionString = builder.Configuration.GetConnectionString("PostgreSQL");
if (string.IsNullOrWhiteSpace(connectionString))
    throw new InvalidOperationException("ConnectionStrings:PostgreSQL is required.");

builder.Services.AddDbContext<NexusDbContext>(options => options.UseNpgsql(connectionString));
builder.Services.AddNexusDataServices();

builder.Services.Configure<StoryblocksScraperOptions>(
    builder.Configuration.GetSection(StoryblocksScraperOptions.SectionName));
builder.Services.AddStoryblocksScraper();

builder.Services.AddNexusEngine(builder.Configuration);

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(options =>
    {
        options.SwaggerEndpoint("/swagger/v1/swagger.json", "Nexus API v1");
        options.DocumentTitle = "Nexus API";
    });

    app.MapGet("/", () => Results.Redirect("/swagger"))
       .ExcludeFromDescription();
}

app.UseHttpsRedirection();
app.UseCors(AngularDevCorsPolicy);
app.UseAuthorization();
app.MapControllers();

app.Run();
