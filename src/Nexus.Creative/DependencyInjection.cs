using Microsoft.Extensions.DependencyInjection;
using Nexus.Core.Interfaces;
using Nexus.Creative.Services;

namespace Nexus.Creative;

public static class DependencyInjection
{
    public static IServiceCollection AddCreativeServices(this IServiceCollection services)
    {
        services.AddHttpClient<IScriptGenerator, ClaudeScriptGenerator>();
        services.AddHttpClient<ITtsProvider, ElevenLabsTtsProvider>();
        return services;
    }
}
