# CLAUDE.md — Nexus-Shorts-Engine Persistent Memory

> This file is the **single source of truth** for the agent (Claude/Cursor) that
> works on this codebase. Read it **at the start of every session** before
> generating, refactoring, or executing any code. Every architectural decision
> in this repo must be reconcilable with the rules described below.

Physical location on developer workstation:
`C:\Users\Clore\Sviluppo\NexusAutomation`

Operating context:
- The codebase is developed inside this Git repository and physically materialised
  on the developer's Windows workstation at the path above.
- The agent is expected to operate **autonomously** but must STOP and ask the
  human for input whenever it would otherwise be forced to hallucinate
  third-party selectors, secrets, or proprietary UI structure (Storyblocks
  layout, CAPTCHA challenges, etc.).

---

## 1. Mission

Build **Nexus-Shorts-Engine**, an enterprise-grade, fully automated, legal and
highly profitable YouTube Shorts generator. The engine continuously:

1. Analyses competitor YouTube Shorts in three niches (Finance, Tech & AI,
   Legal & Court) and computes the **View Velocity** (views per hour) of the
   top performers.
2. Picks the niche that is currently overperforming and queues new videos for
   that niche through Hangfire.
3. Scrapes Storyblocks (vertical 9:16) for matching b-roll using a stealth
   Playwright bot with cookie persistence and human fallback.
4. Generates a 55-second narrated script via Claude (Haiku Vision-grade
   reasoning over the downloaded media tags / first frames), with SSML/prompt
   hints for ElevenLabs.
5. Synthesises the voice-over with ElevenLabs (deep voice for Finance,
   enthusiastic for Tech & AI, narrative/dramatic for Legal & Court).
6. Assembles the final 9:16 short with `Xabe.FFmpeg` — applying anti-reused-content
   transforms, ducked background music, dynamic word-by-word `.ass` subtitles
   with the active word highlighted in `#FFD700`.
7. Publishes / hands off the video via the YouTube Data API.

---

## 2. Hard Architectural Boundaries (NEVER violate)

The solution follows **Clean Architecture** and is **strictly compartmentalised**
into segregated DLLs (Class Libraries). Business logic NEVER leaks into the UI
or the API hosting layer.

```
NexusAutomation.sln
├── src/
│   ├── Nexus.Core         (Class Library)   Interfaces, Enums, DTOs only
│   ├── Nexus.Data         (Class Library)   EF Core Code-First (PostgreSQL)
│   ├── Nexus.Analysis     (Class Library)   YouTube Data API v3 + View Velocity
│   ├── Nexus.Scraper      (Class Library)   Playwright stealth bot (Storyblocks)
│   ├── Nexus.Creative     (Class Library)   Claude Vision + ElevenLabs TTS
│   ├── Nexus.Engine       (Class Library)   Xabe.FFmpeg assembly + subtitles
│   └── Nexus.API          (ASP.NET Core)    Endpoints + Hangfire job queue
└── client/
    └── nexus-dashboard    (Angular 19)      Standalone components + Tailwind
```

Reference rules (compile-time enforced via `.csproj` ProjectReferences):

- `Nexus.Core` references **nothing** internal. Pure contracts.
- `Nexus.Data` references only `Nexus.Core`.
- `Nexus.Analysis`, `Nexus.Scraper`, `Nexus.Creative`, `Nexus.Engine` reference
  `Nexus.Core` and (when persistence is required) `Nexus.Data`. They MUST NOT
  reference each other.
- `Nexus.API` references all of the above. It is the **only** project that
  orchestrates them via Hangfire jobs and DI.
- `client/nexus-dashboard` is a separate Angular workspace and communicates
  with `Nexus.API` exclusively over HTTPS/JSON.

### 2.1 Persistence rules

- All database operations use **Entity Framework Core (Code-First)** targeting
  **PostgreSQL** via `Npgsql.EntityFrameworkCore.PostgreSQL`.
- No raw ADO.NET. No Dapper. No "magic strings" SQL inside business code.
- Migrations live in `src/Nexus.Data/Migrations/` and are produced by
  `dotnet ef migrations add <Name> -p src/Nexus.Data -s src/Nexus.API`.
- Connection strings come from configuration (env vars or `secrets.json`),
  never from a hard-coded literal.

### 2.2 FFmpeg / Anti "Reused Content" rules

Every video produced by `Nexus.Engine` MUST pass through the anti-reused-content
pipeline so that YouTube's duplicate-content classifier does not flag the
account:

- Random **micro-zoom** between **1% and 2%** applied per clip (`zoompan` or
  scale + crop with a random ratio in `[1.01, 1.02]`).
- Subtle **color grading** via a randomly selected LUT
  (`.cube` files under `/Assets/LUTs/`) — strength clamped to 25–40%.
- Small random **speed jitter** of ±2% on b-roll segments (no audio pitch
  change, audio is rebuilt from the TTS track).
- Audio **ducking** of background music to **-22 dB** whenever the ElevenLabs
  TTS track is active (sidechain compressor on the music bus).
- Output is **vertical 9:16** at **1080x1920** at **30 fps**, H.264 high
  profile, AAC 192 kbps.

These transforms are mandatory; they are not toggles for "fast mode".

### 2.3 Music handling rules

- **Never** use trending or copyrighted music. The engine does NOT touch the
  YouTube Audio Library at runtime.
- Background music is loaded exclusively from local structured directories:
  `/Assets/Music/Finance/`, `/Assets/Music/TechAI/`, `/Assets/Music/Legal/`.
- Each track must be either originally produced or covered by a perpetual
  royalty-free licence whose proof-of-licence file lives next to the audio.
- Ducking to **-22 dB** under the TTS bus is non-negotiable.

---

## 3. State Machine (canonical)

A `Video` row in the database moves through these statuses in order. The
Hangfire workers ONLY pick up the work appropriate for the current status.

```
Pending
   └─► TrendAnalyzed
          └─► Scripting
                 └─► MediaDownloaded
                        └─► Rendering
                               └─► Completed
                                      └─► (terminal)
   any of the above on failure ──► Error_Requires_Human
```

`Error_Requires_Human` is a **first-class state**: when a worker cannot make
progress (selectors broken, CAPTCHA, API quota exhausted, file write failed,
etc.), it must move the entity to this status and surface the reason in the
`LastError` field. The Angular dashboard exposes a queue of these for manual
intervention.

---

## 4. Niches (seeded by `Nexus.Data` on first run)

1. **Finance**  
   Script tone: formal, concise, data-driven.  
   ElevenLabs: deep / calm voice (e.g. a Daniel/Adam-style male voice).  
   Background music: Tension / Corporate from `/Assets/Music/Finance/`.

2. **Tech & AI**  
   Script tone: dynamic, hook-heavy, energetic.  
   ElevenLabs: enthusiastic voice.  
   Background music: Synthwave from `/Assets/Music/TechAI/`.

3. **Legal & Court**  
   Script tone: narrative, with dramatic pauses (SSML `<break>` tags).  
   ElevenLabs: deep narrator voice.  
   Background music: Dark Ambient from `/Assets/Music/Legal/`.

The Analysis worker computes daily View Velocity per niche. **If one niche
overperforms another by more than 200% on the same calendar day, the Hangfire
queue is autonomously reprioritised** so that the overperforming niche
consumes the larger share of render slots.

---

## 5. Anti-Ban / Stealth Protocol (Storyblocks)

The Storyblocks scraper in `Nexus.Scraper` MUST:

- Use `Microsoft.Playwright` configured with stealth principles (no
  `navigator.webdriver`, real user-agent, locale, viewport 1280x800, etc.).
- Persist cookies to `cookies/storyblocks.json` after a successful login and
  reload them on subsequent runs.
- Insert randomised human-like delays of **3000 ms to 7000 ms** between
  navigations and clicks.
- Filter the search results page strictly to **9:16 vertical** clips.
- On login failure or CAPTCHA detection: **pause the bot**, relaunch in
  **headed mode**, write a clear console + log message asking the human to
  solve the challenge in the visible window, then resume and save cookies.
- Never hammer the service: maximum **one download per 30 seconds** per
  account session.

---

## 6. Creative Engine

- `Nexus.Creative` calls the Claude API with a prompt that includes:
  - the media tags returned by Storyblocks,
  - optionally the first frame of each clip encoded as base64,
  - a system prompt forcing a **55-second**, hook-first vertical Short script,
  - SSML / prompt hints (`[pause]`, `[emphasize]`) that ElevenLabs understands.
- Script length is validated against the synthesised audio length and re-rolled
  if the rendered TTS exceeds 58 seconds.

---

## 7. Subtitles

`.ass` subtitle files are generated from the ElevenLabs word-level timestamps.
Constraints:

- Centered horizontally and vertically.
- Massive bold font (`Anton`, `Bebas Neue`, or `Montserrat ExtraBold`).
- Display **word-by-word** (or short 2–3 word chunks).
- The currently spoken word is highlighted in `#FFD700` (gold).
- White fill, black outline, 4-pixel shadow.

---

## 8. Stop-and-Ask Policy

The agent is FORBIDDEN from inventing:

- third-party CSS / XPath selectors (especially Storyblocks),
- third-party HTML structure,
- API keys or connection strings,
- account credentials.

When any of these are required, the agent stops and emits a clearly formatted
"HUMAN INPUT REQUIRED" block, listing each item it needs.

---

## 9. Secrets

Local secrets live in `secrets.json` (gitignored) at the solution root and are
loaded by `Nexus.API` via the standard .NET configuration pipeline. A redacted
template is committed as `secrets.example.json`. MCP-compatible tooling reads
the same keys from environment variables when running in CI/CD.

Required keys:

- `PostgreSQL:ConnectionString`
- `YouTube:DataApiKey`
- `ElevenLabs:ApiKey`
- `Anthropic:ApiKey`
- `Storyblocks:Email`
- `Storyblocks:Password`

---

## 10. Execution Plan Checkpoints

1. **DONE** — Phase 1: `CLAUDE.md` + secrets template committed.
2. **DONE** — Phase 2: `.sln`, segregated projects, Angular folder scaffolded.
3. **DONE** — Phase 2/Step 2: EF Core models (`Niche`, `Video`, `Trend`, etc.)
   defined. **Migrations not yet generated.**
4. **NEXT** — Phase 5/Step 3: STOP and ask the human for the PostgreSQL
   connection string and the Storyblocks selectors. **Do not implement
   `Nexus.Scraper` before these arrive.**
5. After human input: implement `Nexus.Scraper`, then the rest of the
   pipeline (Analysis → Creative → Engine → API/Hangfire → Dashboard).
