using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Nexus.Data.Entities;

/// <summary>
/// Single observation of competitor performance for a niche. The Analysis
/// worker writes one row per niche per hour. Aggregations against this table
/// drive the autonomous Hangfire reprioritisation rule.
/// </summary>
public class TrendSnapshot
{
    [Key]
    public Guid Id { get; set; } = Guid.NewGuid();

    public int NicheId { get; set; }
    public Niche Niche { get; set; } = null!;

    /// <summary>Mean views per hour across the sampled competitor Shorts.</summary>
    [Required]
    public double ViewVelocity { get; set; }

    [Required]
    public int SampleSize { get; set; }

    /// <summary>Comma-separated top keywords/tags driving the velocity.</summary>
    [MaxLength(2048)]
    public string? TopKeywordsCsv { get; set; }

    [Column(TypeName = "timestamp with time zone")]
    public DateTimeOffset CapturedAtUtc { get; set; } = DateTimeOffset.UtcNow;
}
