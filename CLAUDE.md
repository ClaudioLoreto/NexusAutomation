# Nexus-Shorts-Engine — Core Memory

> **Local path (authoritative):** `C:\Users\Clore\Sviluppo\NexusAutomation`  
> **Repository:** Enterprise YouTube Shorts automation platform (`NexusAutomation.sln`)

---

## Mission

Build **Nexus-Shorts-Engine**: a legal, enterprise-grade, fully automated YouTube Shorts pipeline — trend analysis → scripting → media acquisition → FFmpeg render → publish — with human fallback only where automation cannot proceed (CAPTCHA, missing DOM selectors).

---

## Architectural Boundaries (Non-Negotiable)

### Clean Architecture + Segregated DLLs

| Layer | Project | Responsibility |
|-------|---------|----------------|
| Domain contracts | `Nexus.Core` | Interfaces, enums, DTOs only — **no** EF, HTTP, Playwright, FFmpeg |
| Persistence | `Nexus.Data` | EF Core Code-First, `DbContext`, entities, migrations |
| Analysis | `Nexus.Analysis` | YouTube Data API v3 — View Velocity (views/hour) |
| Acquisition | `Nexus.Scraper` | Playwright stealth Storyblocks bot |
| Creative | `Nexus.Creative` | Claude Vision (scripts) + ElevenLabs TTS |
| Rendering | `Nexus.Engine` | Xabe.FFmpeg — assembly, ASS subtitles, audio ducking |
| Host | `Nexus.API` | ASP.NET Core Web API, Hangfire jobs, DI composition root |
| UI | `client/nexus-dashboard` | Angular 19 — **presentation only**, no business logic |

**Rules:**

- Business logic lives in class libraries; `Nexus.API` wires DI and exposes endpoints.
- UI calls API only; never references `Nexus.Scraper`, `Nexus.Engine`, etc. directly.
- Cross-cutting concerns (logging, config) via interfaces in `Nexus.Core`, implementations in respective projects.

### Database

- **PostgreSQL** only.
- **Entity Framework Core Code-First** for all persistence.
- Migrations run from `Nexus.Data` / `Nexus.API` host — not ad-hoc SQL.

### FFmpeg Anti–Reused Content

All downloaded media processed through `Nexus.Engine` must:

1. Apply **random micro-zoom** (1–2%) per clip.
2. Apply **subtle color grading** (LUT or equivalent) to vary pixel fingerprint.
3. Goal: reduce YouTube "Reused Content" false positives while staying within legal/fair-use bounds.

### Music

- **Never** use trending/copyrighted tracks.
- Background music from local paths: `Assets/Music/{Niche}/`.
- **-22 dB ducking** under active TTS (voice-over).

### Scraper Anti-Ban

- Playwright with stealth principles.
- Cookie persistence (JSON file after login).
- Human-like delays: **3000–7000 ms** between clicks.
- Filter: **9:16 vertical** only.
- On login failure / CAPTCHA: pause, **headed** browser, log for human, save cookies after solve.
- **Do not guess Storyblocks selectors** — human must supply exact CSS/HTML.

---

## Video State Machine

```
Pending → TrendAnalyzed → Scripting → MediaDownloaded → Rendering → Completed
                                                              ↘ Error_Requires_Human
```

Statuses defined in `Nexus.Core` (`VideoStatus` enum). Hangfire workers advance transitions; failures land in `Error_Requires_Human`.

---

## Niche Dynamics

Three seeded niches:

| Niche | Script tone | Voice | Music folder |
|-------|-------------|-------|--------------|
| Finance | Formal | Deep/calm ElevenLabs | `Assets/Music/Finance/` |
| Tech & AI | Dynamic | Enthusiastic | `Assets/Music/Tech/` |
| Legal & Court | Narrative, dramatic pauses | Dramatic | `Assets/Music/Legal/` |

**Auto-switch:** `Nexus.Analysis` compares View Velocity per niche. If Tech velocity > Finance by **>200%** today, Hangfire queue reprioritizes Tech renders.

---

## Creative Pipeline

1. Scraper saves media + tags (and optional first frame).
2. `Nexus.Creative` → Claude: ~55s script aligned to visuals; SSML/tags for ElevenLabs pauses/emphasis.
3. `Nexus.Engine` → `.ass` subtitles: centered, bold, large, word/chunk karaoke, active word **#FFD700**.

---

## Secrets & MCP

- Copy `config/secrets.template.json` → `config/secrets.json` (gitignored).
- Copy `.env.template` → `.env` for local/docker overrides.
- Optional MCP: `.cursor/mcp.json` — document external tool hooks; API keys stay in `secrets.json`, not in repo.

Required keys:

- `ConnectionStrings:PostgreSQL`
- `YouTube:ApiKey`
- `ElevenLabs:ApiKey`
- `Anthropic:ApiKey` (Claude)
- `Storyblocks` credentials (never commit)

---

## Execution Phases (Agent Protocol)

| Step | Status | Action |
|------|--------|--------|
| 1 | Done | `CLAUDE.md` + solution scaffold |
| 2 | Done | EF models in `Nexus.Data` (migrations **not** run yet) |
| 3 | **BLOCKED** | Human: PostgreSQL connection string + Storyblocks selectors |
| 4 | Pending | Implement `Nexus.Scraper` after selectors received |

---

## Human Inputs Still Required

1. PostgreSQL connection string.
2. Storyblocks Inspect selectors: email, password, submit, search bar, download button.
3. CAPTCHA / 2FA when scraper opens headed browser.

---

## Folder Layout

```
NexusAutomation/
├── CLAUDE.md
├── NexusAutomation.sln
├── config/
│   ├── secrets.template.json
│   └── secrets.json          (gitignored)
├── .env.template
├── Assets/
│   └── Music/{Finance,Tech,Legal}/
├── src/
│   ├── Nexus.Core/
│   ├── Nexus.Data/
│   ├── Nexus.Analysis/
│   ├── Nexus.Scraper/
│   ├── Nexus.Creative/
│   ├── Nexus.Engine/
│   └── Nexus.API/
└── client/
    └── nexus-dashboard/
```

---

*Last updated: Phase 1–2 scaffold. Do not run EF migrations until connection string is provided.*
