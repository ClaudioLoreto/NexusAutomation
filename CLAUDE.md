# CLAUDE.md — Nexus-Shorts-Engine Project Memory

## Project Identity
- **Name:** Nexus-Shorts-Engine (Solution: `NexusAutomation.sln`)
- **Purpose:** Fully automated, legal YouTube Shorts generator with niche auto-switching.
- **Runtime:** .NET 8, Angular 19, PostgreSQL, FFmpeg, Playwright

---

## Architectural Boundaries (STRICT)

### Clean Architecture — Segregated DLLs
| Layer | Project | Responsibility |
|-------|---------|----------------|
| Core | `Nexus.Core` | Interfaces, Enums, DTOs — **zero dependencies** |
| Data | `Nexus.Data` | EF Core DbContext, Migrations, Models — depends on Core only |
| Analysis | `Nexus.Analysis` | YouTube Data API v3 trend analysis — depends on Core |
| Scraper | `Nexus.Scraper` | Playwright stealth bot for Storyblocks — depends on Core |
| Creative | `Nexus.Creative` | Claude Vision scripting + ElevenLabs TTS — depends on Core |
| Engine | `Nexus.Engine` | Xabe.FFmpeg video assembly, subtitles, ducking — depends on Core |
| API | `Nexus.API` | ASP.NET Core Web API + Hangfire — orchestrates all layers |
| Client | `client/nexus-dashboard` | Angular 19 + TailwindCSS — standalone components |

### Rules
1. **NO business logic in the API layer.** The API only orchestrates calls to domain services.
2. **ALL database operations use Entity Framework Core (Code-First) targeting PostgreSQL.**
3. **FFmpeg operations MUST bypass YouTube's "Reused Content" filters** by applying:
   - Random micro-zooms (1–2%)
   - Subtle color grading via LUTs
   - Unique subtitle overlay per video
4. **Secrets are NEVER committed.** Use `appsettings.Secrets.json` (gitignored) or environment variables.

---

## State Machine — Video Pipeline
```
Pending → TrendAnalyzed → Scripting → MediaDownloaded → Rendering → Completed
                                                                  ↘ Error_Requires_Human
```

## Niche Configuration
| Niche | Script Tone | Voice Style | Music Genre |
|-------|-------------|-------------|-------------|
| Finance | Formal | Deep/Calm | Tension/Corporate |
| Tech & AI | Dynamic | Enthusiastic | Synthwave |
| Legal & Court | Narrative | Dramatic pauses | Dark Ambient |

## Niche Auto-Switching Rule
If niche A outperforms niche B by >200% in View Velocity (views/hour), the Hangfire queue
must autonomously reprioritize to produce more content for niche A.

## Anti-Ban Protocols (Scraper)
- Cookie persistence (JSON file)
- Human-like random delays (3000–7000ms)
- Filter for 9:16 vertical video only
- Headed browser fallback on CAPTCHA detection
- Console/log notification to human on intervention required

## Music Handling
- **No copyrighted music.** Use royalty-free tracks from `/Assets/Music/{Niche}/`.
- Apply **−22 dB audio ducking** whenever TTS voice is active.

## Subtitle Spec (.ass format)
- Centered, bold, large font
- Word-by-word or short-chunk display
- Active word highlighted in yellow (#FFD700)

## Required Human Inputs (Phase 5, Step 3)
Before completing integration, the human must provide:
- [ ] PostgreSQL Connection String
- [ ] Storyblocks CSS Selectors (Email, Password, Login Button, Search Bar, Download Button)
- [ ] YouTube Data API v3 Key
- [ ] ElevenLabs API Key
- [ ] Claude API Key
