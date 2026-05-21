namespace Nexus.Core.DTOs;

public record DashboardStatsDto
{
    public int TotalVideos { get; init; }
    public int PendingVideos { get; init; }
    public int CompletedVideos { get; init; }
    public int ErrorVideos { get; init; }
    public int RenderingVideos { get; init; }
    public Dictionary<string, double> NicheVelocities { get; init; } = new();
    public string TopNiche { get; init; } = string.Empty;
}
