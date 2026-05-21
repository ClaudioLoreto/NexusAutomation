# NexusAutomation (Nexus-Shorts-Engine)

Enterprise YouTube Shorts automation platform — Clean Architecture, .NET 8, PostgreSQL, Angular 19.

**Local development path:** `C:\Users\Clore\Sviluppo\NexusAutomation`

## Quick start

1. Read `CLAUDE.md` for architecture and agent memory.
2. Copy `config/secrets.template.json` → `config/secrets.json` and `.env.template` → `.env`.
3. Build backend:

```bash
dotnet build NexusAutomation.sln
```

4. Dashboard (after `npm install` in `client/nexus-dashboard`):

```bash
cd client/nexus-dashboard && npm install && npm start
```

## Solution layout

| Project | Role |
|---------|------|
| `Nexus.Core` | Enums, DTOs, interfaces |
| `Nexus.Data` | EF Core + PostgreSQL entities |
| `Nexus.Analysis` | YouTube View Velocity |
| `Nexus.Scraper` | Playwright Storyblocks |
| `Nexus.Creative` | Claude + ElevenLabs |
| `Nexus.Engine` | FFmpeg render pipeline |
| `Nexus.API` | Web API + Hangfire host |
| `client/nexus-dashboard` | Angular 19 UI |

**Do not run EF migrations until PostgreSQL connection string is configured.**
