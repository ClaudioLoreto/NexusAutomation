# nexus-dashboard

Angular 19 standalone-components control panel for **Nexus-Shorts-Engine**.

The Angular workspace will be initialised in the next phase (Phase 4+) once
the `Nexus.API` contracts are stabilised. Stack:

- Angular 19 with **standalone components only** (no NgModules).
- **TailwindCSS** for styling.
- Talks to `Nexus.API` over HTTPS/JSON.
- Surfaces:
  - the State Machine queue (Pending → Completed),
  - the `Error_Requires_Human` queue with manual-resolve actions,
  - daily / hourly View Velocity charts per niche,
  - the Hangfire job dashboard (proxied),
  - one-click "force render this niche now" overrides.

To bootstrap the workspace once selectors and connection string are in place,
run from this directory on the developer workstation:

```bash
npx --yes @angular/cli@19 new nexus-dashboard --routing --style=scss --standalone --skip-git
cd nexus-dashboard
npx --yes tailwindcss init
```
