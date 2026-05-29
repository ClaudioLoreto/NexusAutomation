using Microsoft.Extensions.DependencyInjection;
using Nexus.Core.Interfaces;
using Nexus.Data.Services;

namespace Nexus.Data.DependencyInjection;

public static class DataServiceCollectionExtensions
{
    public static IServiceCollection AddNexusDataServices(this IServiceCollection services)
    {
        services.AddScoped<INicheService, NicheService>();
        services.AddScoped<IVideoQueueService, VideoQueueService>();
        return services;
    }
}
