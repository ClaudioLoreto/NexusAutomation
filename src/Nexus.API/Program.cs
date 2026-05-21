using Microsoft.EntityFrameworkCore;
using Nexus.Core.Interfaces;
using Nexus.Data;
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
if (!string.IsNullOrWhiteSpace(connectionString))
{
    builder.Services.AddDbContext<NexusDbContext>(options =>
        options.UseNpgsql(connectionString));
}
else
{
    builder.Services.AddDbContext<NexusDbContext>(options =>
        options.UseNpgsql("Host=localhost;Database=nexus_shorts_placeholder;Username=;Password="));
}

builder.Services.AddScoped<IStoryblocksScraper, StoryblocksScraperStub>();

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
