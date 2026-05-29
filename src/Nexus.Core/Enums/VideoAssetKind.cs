namespace Nexus.Core.Enums;

/// <summary>
/// Type discriminator for a <c>VideoAsset</c> row — which artifact does
/// this file path represent in the pipeline?
/// </summary>
public enum VideoAssetKind
{
    /// <summary>Single Storyblocks .mp4 clip.</summary>
    StoryblocksClip = 1,

    /// <summary>OpenAI / ElevenLabs voiceover audio.</summary>
    Voiceover = 2,

    /// <summary>ASS karaoke subtitle file.</summary>
    KaraokeSubtitle = 3,

    /// <summary>Background music track selected for the job.</summary>
    BackgroundMusic = 4,

    /// <summary>Final MP4 produced by FFmpegVideoAssembler.</summary>
    FinalRender = 5,

    /// <summary>Generated thumbnail PNG/JPG.</summary>
    Thumbnail = 6
}
