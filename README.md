# Nexus-Shorts-Engine (NexusAutomation)

Enterprise-grade, fully automated YouTube Shorts generator with niche auto-switching, view velocity analysis, and anti-reuse FFmpeg rendering.

## Architecture

```
NexusAutomation.sln
├── src/
│   ├── Nexus.Core         — Interfaces, Enums, DTOs (zero dependencies)
│   ├── Nexus.Data         — EF Core DbContext, Models, Seeding (PostgreSQL)
│   ├── Nexus.Analysis     — YouTube Data API v3 trend analysis
│   ├── Nexus.Scraper      — Playwright stealth bot for Storyblocks
│   ├── Nexus.Creative     — Claude Vision scripting + ElevenLabs TTS
│   ├── Nexus.Engine       — Xabe.FFmpeg video assembly, subtitles, ducking
│   └── Nexus.API          — ASP.NET Core Web API + Hangfire scheduler
├── client/
│   └── nexus-dashboard    — Angular 19 + TailwindCSS control panel
└── Assets/
    ├── Music/{Finance,TechAI,LegalCourt}/  — Royalty-free background music
    └── Luts/               — Color grading LUT files (.cube/.3dl)
```

## Video Pipeline State Machine

```
Pending → TrendAnalyzed → Scripting → MediaDownloaded → Rendering → Completed
                                                                  ↘ Error_Requires_Human
```

## Niches

| Niche | Script Tone | Voice Style | Music Genre |
|-------|-------------|-------------|-------------|
| Finance | Formal | Deep/Calm | Tension/Corporate |
| Tech & AI | Dynamic | Enthusiastic | Synthwave |
| Legal & Court | Narrative | Dramatic pauses | Dark Ambient |

## Quick Start

### Prerequisites
- .NET 8 SDK
- PostgreSQL
- Node.js 18+ & Angular CLI 19
- FFmpeg

### Backend
```bash
# Copy secrets template and fill in your API keys
cp src/Nexus.API/appsettings.Secrets.template.json src/Nexus.API/appsettings.Secrets.json

# Restore and build
dotnet restore
dotnet build

# Run the API (includes Hangfire dashboard at /hangfire)
cd src/Nexus.API
dotnet run
```

### Frontend
```bash
cd client/nexus-dashboard
npm install
ng serve
```

## Required Secrets (appsettings.Secrets.json)

| Key | Description |
|-----|-------------|
| `ConnectionStrings:NexusDb` | PostgreSQL connection string |
| `YouTubeApi:ApiKey` | YouTube Data API v3 key |
| `ClaudeApi:ApiKey` | Anthropic Claude API key |
| `ElevenLabs:ApiKey` | ElevenLabs TTS API key |
| `Scraper:Selectors:*` | Storyblocks CSS selectors (requires manual inspection) |

## Anti-Reuse Rendering

Every video processed by `Nexus.Engine` applies:
- Random micro-zoom (1–2%) to avoid pixel-identical frames
- Subtle color grading via LUT files
- Unique `.ass` subtitle overlay (word-by-word, yellow highlight)
- −22 dB audio ducking on background music during TTS

## License

Proprietary — All rights reserved.
