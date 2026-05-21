# NexusAutomation (Nexus-Shorts-Engine)

Enterprise YouTube Shorts automation platform — Clean Architecture, .NET 8, PostgreSQL, Angular 19.

**Local development path:** `C:\Users\Clore\Sviluppo\NexusAutomation`

## Quick start

1. Read `CLAUDE.md` for architecture and agent memory.
2. Copy `config/secrets.template.json` → `config/secrets.json` (optional overrides).
3. Ensure PostgreSQL is running with database `nexus_shorts`.
4. Build and open the developer CLI:

```bash
dotnet build NexusAutomation.sln
dotnet run --project src/Nexus.CLI/Nexus.CLI.csproj
```

CLI menu: **1** = EF migrations, **2** = API, **3** = Angular, **4** = clean temp folders, **5** = Playwright browsers.

## Solution layout

| Project | Role |
|---------|------|
| `Nexus.Core` | Enums, DTOs, interfaces |
| `Nexus.Data` | EF Core + PostgreSQL entities |
| `Nexus.Analysis` | YouTube View Velocity |
| `Nexus.Scraper` | Playwright Storyblocks (Google OAuth cookies) |
| `Nexus.Creative` | Claude + ElevenLabs |
| `Nexus.Engine` | FFmpeg render pipeline |
| `Nexus.API` | Web API + Hangfire host |
| `Nexus.CLI` | Developer control panel |
| `client/nexus-dashboard` | Angular 19 UI |

## Storyblocks scraper (Google OAuth)

1. **No** `data/cookies.json` → Chromium opens **headed**. Sign in with **Sign in with Google** manually.
2. After login, cookies are saved to `data/cookies.json`.
3. **With** cookies → headless session, search, apply Footage / Vertical / 4K filters, hover card, download MP4 via `WaitForDownloadAsync`.

API endpoints: `POST /api/scraper/authenticate`, `POST /api/scraper/search?query=...&max=3`

### Playwright browsers (one-time)

```bash
dotnet build src/Nexus.Scraper/Nexus.Scraper.csproj
cd src/Nexus.Scraper/bin/Debug/net8.0
./playwright.sh install chromium
```

On Windows: `playwright.ps1 install chromium` from the same folder.

## Database

Default connection string (local dev) in `src/Nexus.API/appsettings.json`:

`Host=localhost;Port=5432;Database=nexus_shorts;Username=admin;Password=1234;`

Apply migrations:

```bash
dotnet tool restore
dotnet ef database update --project src/Nexus.Data --startup-project src/Nexus.API
```

Migration `InitialCreate` is included; run update when PostgreSQL is available.
