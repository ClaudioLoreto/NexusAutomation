using Nexus.Core.Enums;

namespace Nexus.Data.Models;

public class NicheConfig
{
    public Guid Id { get; set; }
    public NicheType NicheType { get; set; }
    public string DisplayName { get; set; } = string.Empty;
    public ScriptTone ScriptTone { get; set; }
    public VoiceStyle VoiceStyle { get; set; }
    public MusicGenre MusicGenre { get; set; }
    public string MusicDirectoryPath { get; set; } = string.Empty;
    public string SearchKeywords { get; set; } = string.Empty;
    public bool IsActive { get; set; } = true;
    public int Priority { get; set; } = 1;

    public DateTime CreatedAtUtc { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedAtUtc { get; set; }

    public ICollection<VideoProject> Videos { get; set; } = new List<VideoProject>();
    public ICollection<TrendSnapshot> TrendSnapshots { get; set; } = new List<TrendSnapshot>();
}
