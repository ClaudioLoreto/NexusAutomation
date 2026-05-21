namespace Nexus.Data.Entities;

/// <summary>
/// View velocity and competitor metrics for niche prioritization.
/// </summary>
public class Trend
{
    public long Id { get; set; }
    public int? NicheId { get; set; }
    public Niche? Niche { get; set; }
    public Guid? VideoId { get; set; }
    public Video? Video { get; set; }
    public string? CompetitorChannelId { get; set; }
    public string? CompetitorVideoId { get; set; }
    public long ViewCount { get; set; }
    public double ViewsPerHour { get; set; }
    public DateTime MeasuredAtUtc { get; set; } = DateTime.UtcNow;
    public string? RawPayloadJson { get; set; }
}
