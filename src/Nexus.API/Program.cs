using Microsoft.EntityFrameworkCore;
using Nexus.Data;
using Nexus.Scraper.DependencyInjection;
using Nexus.Scraper.Storyblocks;

var builder = WebApplication.CreateBuilder(args);

builder.Configuration
    .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
    .AddJsonFile($"appsettings.{builder.Environment.EnvironmentName}.json", optional: true)
    .AddJsonFile(Path.Combine(builder.Environment.ContentRootPath, "..", "..", "config", "secrets.json"), optional: true)
    .AddEnvironmentVariables();

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var connectionString = builder.Configuration.GetConnectionString("PostgreSQL");
if (string.IsNullOrWhiteSpace(connectionString))
    throw new InvalidOperationException("ConnectionStrings:PostgreSQL is required.");

builder.Services.AddDbContext<NexusDbContext>(options => options.UseNpgsql(connectionString));

builder.Services.Configure<StoryblocksScraperOptions>(
    builder.Configuration.GetSection(StoryblocksScraperOptions.SectionName));
builder.Services.AddStoryblocksScraper();

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
