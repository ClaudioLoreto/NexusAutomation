using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Nexus.Core.Enums;
using Nexus.Data.Models;

namespace Nexus.Data.Seeding;

public static class NicheSeeder
{
    public static async Task SeedAsync(NexusDbContext context, ILogger? logger = null)
    {
        if (await context.NicheConfigs.AnyAsync())
        {
            logger?.LogInformation("Niche configs already seeded — skipping");
            return;
        }

        var niches = new List<NicheConfig>
        {
            new()
            {
                Id = Guid.NewGuid(),
                NicheType = NicheType.Finance,
                DisplayName = "Finance",
                ScriptTone = ScriptTone.Formal,
                VoiceStyle = VoiceStyle.DeepCalm,
                MusicGenre = MusicGenre.TensionCorporate,
                MusicDirectoryPath = "Assets/Music/Finance",
                SearchKeywords = "stock market,investing,financial news,economy,trading,crypto",
                IsActive = true,
                Priority = 1,
                CreatedAtUtc = DateTime.UtcNow
            },
            new()
            {
                Id = Guid.NewGuid(),
                NicheType = NicheType.TechAndAI,
                DisplayName = "Tech & AI",
                ScriptTone = ScriptTone.Dynamic,
                VoiceStyle = VoiceStyle.Enthusiastic,
                MusicGenre = MusicGenre.Synthwave,
                MusicDirectoryPath = "Assets/Music/TechAI",
                SearchKeywords = "artificial intelligence,technology,AI news,robotics,innovation,futuristic",
                IsActive = true,
                Priority = 1,
                CreatedAtUtc = DateTime.UtcNow
            },
            new()
            {
                Id = Guid.NewGuid(),
                NicheType = NicheType.LegalAndCourt,
                DisplayName = "Legal & Court",
                ScriptTone = ScriptTone.Narrative,
                VoiceStyle = VoiceStyle.DramaticPauses,
                MusicGenre = MusicGenre.DarkAmbient,
                MusicDirectoryPath = "Assets/Music/LegalCourt",
                SearchKeywords = "courtroom,legal drama,law,judge,trial,justice",
                IsActive = true,
                Priority = 1,
                CreatedAtUtc = DateTime.UtcNow
            }
        };

        context.NicheConfigs.AddRange(niches);
        await context.SaveChangesAsync();
        logger?.LogInformation("Seeded {Count} niche configurations", niches.Count);
    }
}
