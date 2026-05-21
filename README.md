# Nexus-Shorts-Engine

> Enterprise-grade, fully automated, legal and highly profitable YouTube Shorts
> generator. .NET 8 + EF Core (PostgreSQL) + Hangfire on the backend, Angular 19
> + TailwindCSS on the frontend, `Xabe.FFmpeg` for assembly, Playwright for
> stealth scraping, Claude for scripting, ElevenLabs for narration.

Physical workstation path: `C:\Users\Clore\Sviluppo\NexusAutomation`

The persistent operating memory for the agent that works on this codebase is
[`CLAUDE.md`](./CLAUDE.md). **Read it first.**

---

## Solution Layout (Phase 2 — scaffolded)

```
NexusAutomation.sln
├── src/
│   ├── Nexus.Core         Interfaces, Enums, DTOs
│   ├── Nexus.Data         EF Core Code-First (PostgreSQL) — models + DbContext
│   ├── Nexus.Analysis     YouTube Data API v3 (View Velocity)            [stub]
│   ├── Nexus.Scraper      Playwright stealth Storyblocks bot             [stub]
│   ├── Nexus.Creative     Claude scripting + ElevenLabs TTS              [stub]
│   ├── Nexus.Engine       Xabe.FFmpeg assembly + subtitles               [stub]
│   └── Nexus.API          ASP.NET Core 8 + Hangfire
├── client/
│   └── nexus-dashboard    Angular 19 control panel                       [pending]
├── Assets/                Local royalty-free music + LUTs + fonts        [pending]
├── CLAUDE.md              Agent persistent memory
├── .mcp.json              Model Context Protocol config
├── secrets.example.json   Redacted template for secrets.json
└── .env.example           Redacted environment template
```

The solution builds with `dotnet build NexusAutomation.sln` (0 warnings,
0 errors at the Phase 2 checkpoint).

---

## State Machine

```
Pending → TrendAnalyzed → Scripting → MediaDownloaded → Rendering → Completed
                                  ↘                              ↗
                                   Error_Requires_Human (terminal until human ack)
```

See [`CLAUDE.md`](./CLAUDE.md) for the canonical specification.

---

## ⚠️ HUMAN INPUT REQUIRED (Phase 5 / Step 3)

The agent has paused at the checkpoint mandated by the execution plan. To
proceed with `Nexus.Scraper` and the database wiring, the following items are
needed from the human operator:

1. **PostgreSQL connection string.** Will be stored in `secrets.json` under
   `PostgreSQL:ConnectionString` (or in `.env` as `NEXUS_PG_CONNECTION_STRING`).
2. **Storyblocks CSS / HTML selectors.** Open
   [storyblocks.com](https://www.storyblocks.com), use the browser DevTools
   "Inspect" panel on the relevant pages and provide:
   - Email login input — selector / outer HTML
   - Password input — selector / outer HTML
   - Login submit button — selector / outer HTML
   - Main search bar — selector / outer HTML
   - Video "Download" button — selector / outer HTML

Once provided, the agent will implement `Nexus.Scraper`, generate the first
EF Core migration, and continue with the remaining pipeline integrations.
