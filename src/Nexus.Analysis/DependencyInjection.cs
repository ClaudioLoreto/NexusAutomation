using Microsoft.Extensions.DependencyInjection;
using Nexus.Analysis.Services;
using Nexus.Core.Interfaces;

namespace Nexus.Analysis;

public static class DependencyInjection
{
    public static IServiceCollection AddAnalysisServices(this IServiceCollection services)
    {
        services.AddScoped<ITrendAnalyzer, YouTubeTrendAnalyzer>();
        return services;
    }
}
