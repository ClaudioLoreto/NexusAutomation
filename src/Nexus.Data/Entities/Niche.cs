using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Nexus.Core.Enums;

namespace Nexus.Data.Entities;

/// <summary>
/// A content niche (Finance, Tech &amp; AI, Legal &amp; Court). Seeded on
/// first run via <see cref="NexusDbContext.OnModelCreating(Microsoft.EntityFrameworkCore.ModelBuilder)"/>.
/// </summary>
public class Niche
{
    [Key]
    public int Id { get; set; }

    [Required, MaxLength(32)]
    public NicheKey Key { get; set; }

    [Required, MaxLength(128)]
    public string DisplayName { get; set; } = string.Empty;

    [Required]
    public ScriptTone Tone { get; set; }

    [Required, MaxLength(64)]
    public string ElevenLabsVoiceProfile { get; set; } = string.Empty;

    [Required, MaxLength(256)]
    public string MusicLibraryDirectory { get; set; } = string.Empty;

    [Required]
    public bool IsActive { get; set; } = true;

    /// <summary>
    /// Current weight in the Hangfire renderer queue. Recomputed daily by the
    /// Analysis worker; if niche A's View Velocity exceeds niche B by more
    /// than 200% on the same calendar day, A's weight is increased.
    /// </summary>
    public double RenderWeight { get; set; } = 1.0;

    [Column(TypeName = "timestamp with time zone")]
    public DateTimeOffset CreatedAtUtc { get; set; } = DateTimeOffset.UtcNow;

    [Column(TypeName = "timestamp with time zone")]
    public DateTimeOffset UpdatedAtUtc { get; set; } = DateTimeOffset.UtcNow;

    public ICollection<Video> Videos { get; set; } = new List<Video>();
    public ICollection<TrendSnapshot> TrendSnapshots { get; set; } = new List<TrendSnapshot>();
}
