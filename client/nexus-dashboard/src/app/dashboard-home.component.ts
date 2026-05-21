import { Component } from '@angular/core';

@Component({
  selector: 'app-dashboard-home',
  standalone: true,
  template: `
    <section class="max-w-2xl space-y-4">
      <h2 class="text-lg font-semibold">Pipeline status</h2>
      <ul class="list-disc list-inside text-slate-300 text-sm space-y-1">
        <li>Pending → TrendAnalyzed → Scripting → MediaDownloaded → Rendering → Completed</li>
        <li>Niches: Finance, Tech &amp; AI, Legal &amp; Court</li>
      </ul>
      <p class="text-amber-400 text-sm">
        Awaiting PostgreSQL connection string and Storyblocks selectors (Step 3).
      </p>
    </section>
  `
})
export class DashboardHomeComponent {}
