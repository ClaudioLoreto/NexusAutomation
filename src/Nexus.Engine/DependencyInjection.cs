using Microsoft.Extensions.DependencyInjection;
using Nexus.Core.Interfaces;
using Nexus.Engine.Services;
using Nexus.Engine.Subtitles;

namespace Nexus.Engine;

public static class DependencyInjection
{
    public static IServiceCollection AddEngineServices(this IServiceCollection services)
    {
        services.AddScoped<ISubtitleGenerator, AssSubtitleGenerator>();
        services.AddScoped<IVideoRenderer, FfmpegVideoRenderer>();
        return services;
    }
}
