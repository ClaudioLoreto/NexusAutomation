using Nexus.Core.Enums;

namespace Nexus.Data.Entities;

/// <summary>
/// Seeded niche configuration (voice, music path, script tone).
/// </summary>
public class Niche
{
    public int Id { get; set; }
    public NicheType Type { get; set; }
    public string Name { get; set; } = string.Empty;
    public string ScriptTone { get; set; } = string.Empty;
    public string ElevenLabsVoiceId { get; set; } = string.Empty;
    public string MusicDirectory { get; set; } = string.Empty;
    public bool IsActive { get; set; } = true;
    public int QueuePriority { get; set; } = 100;

    public ICollection<Video> Videos { get; set; } = new List<Video>();
    public ICollection<Trend> Trends { get; set; } = new List<Trend>();
}
