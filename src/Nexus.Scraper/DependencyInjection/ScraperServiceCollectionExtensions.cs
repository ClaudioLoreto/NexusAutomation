using Microsoft.Extensions.DependencyInjection;
using Nexus.Core.Interfaces;
using Nexus.Scraper.Storyblocks;

namespace Nexus.Scraper.DependencyInjection;

public static class ScraperServiceCollectionExtensions
{
    public static IServiceCollection AddStoryblocksScraper(
        this IServiceCollection services,
        Action<StoryblocksScraperOptions>? configure = null)
    {
        if (configure is not null)
            services.Configure(configure);
        else
            services.AddOptions<StoryblocksScraperOptions>();

        services.AddScoped<IStoryblocksScraper, StoryblocksScraper>();
        return services;
    }
}
