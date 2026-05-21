using Microsoft.Extensions.DependencyInjection;
using Nexus.Core.Interfaces;
using Nexus.Scraper.Services;

namespace Nexus.Scraper;

public static class DependencyInjection
{
    public static IServiceCollection AddScraperServices(this IServiceCollection services)
    {
        services.AddScoped<IMediaScraper, StoryblocksScraper>();
        return services;
    }
}
