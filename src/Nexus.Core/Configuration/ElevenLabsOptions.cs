namespace Nexus.Core.Configuration;

public class ElevenLabsOptions
{
    public const string SectionName = "ElevenLabs";
    public string ApiKey { get; set; } = string.Empty;
    public string BaseUrl { get; set; } = "https://api.elevenlabs.io";
    public string DefaultVoiceId { get; set; } = string.Empty;
    public Dictionary<string, string> VoiceMap { get; set; } = new();
}
