import { Component, OnInit, OnDestroy, inject, signal, computed } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { RouterLink } from '@angular/router';
import { Subscription, finalize } from 'rxjs';
import { NexusApiService } from './services/nexus-api.service';
import {
  DashboardSummary,
  Niche,
  NicheType,
  QueueVideoRequest,
  ScrapedMediaResult,
  Video,
  VIDEO_STATUSES
} from './models/nexus-models';

@Component({
  selector: 'app-dashboard-home',
  standalone: true,
  imports: [CommonModule, FormsModule, RouterLink],
  template: `
    <section class="space-y-6 max-w-6xl">

      <div *ngIf="loadError()" class="rounded border border-rose-700 bg-rose-950/50 text-rose-200 text-sm px-3 py-2">
        Failed to reach the API ({{ loadError() }}). Make sure the backend is running on
        <code class="text-rose-100">http://localhost:5239</code>.
      </div>

      <div class="grid grid-cols-1 md:grid-cols-3 gap-4">
        <div *ngFor="let s of statusCards()"
             class="rounded border border-slate-800 bg-slate-900/60 p-4">
          <div class="text-xs uppercase tracking-wider text-slate-400">{{ s.label }}</div>
          <div class="text-2xl font-semibold mt-1">{{ s.value }}</div>
        </div>
      </div>

      <section class="rounded border border-slate-800 bg-slate-900/40 p-4">
        <header class="flex items-center justify-between mb-3">
          <h2 class="text-lg font-semibold">Niches</h2>
          <div class="flex items-center gap-3">
            <a routerLink="/niches/new" class="text-xs text-sky-400 hover:text-sky-300">+ New niche</a>
            <button (click)="refreshAll()"
                    class="text-xs text-sky-400 hover:text-sky-300">
              Refresh
            </button>
          </div>
        </header>

        <div class="overflow-x-auto">
          <table class="w-full text-sm">
            <thead class="text-slate-400 text-xs uppercase">
              <tr>
                <th class="text-left py-2">Niche</th>
                <th class="text-left">Tone</th>
                <th class="text-right">Priority</th>
                <th class="text-right">Videos</th>
                <th class="text-right">Active</th>
              </tr>
            </thead>
            <tbody>
              <tr *ngFor="let n of summary()?.niches ?? []"
                  class="border-t border-slate-800">
                <td class="py-2 font-medium">{{ n.name }}</td>
                <td class="text-slate-400">{{ n.scriptTone }}</td>
                <td class="text-right tabular-nums">{{ n.queuePriority }}</td>
                <td class="text-right tabular-nums">{{ n.videoCount }}</td>
                <td class="text-right">
                  <button (click)="toggleNiche(n)"
                          [class.text-emerald-400]="n.isActive"
                          [class.text-slate-500]="!n.isActive"
                          class="text-xs hover:underline">
                    {{ n.isActive ? 'active' : 'inactive' }}
                  </button>
                </td>
              </tr>
              <tr *ngIf="!summary()?.niches?.length">
                <td colspan="5" class="py-3 text-slate-500 text-sm">No niches found.</td>
              </tr>
            </tbody>
          </table>
        </div>
      </section>

      <div class="grid grid-cols-1 lg:grid-cols-2 gap-4">

        <section class="rounded border border-slate-800 bg-slate-900/40 p-4">
          <h2 class="text-lg font-semibold mb-3">Queue a new video</h2>
          <form (ngSubmit)="queueVideo()" class="space-y-3">
            <label class="block text-xs uppercase tracking-wider text-slate-400">
              Niche
              <select [(ngModel)]="newVideo.nicheType" name="nicheType"
                      class="mt-1 block w-full bg-slate-800 border border-slate-700 rounded px-2 py-1 text-sm">
                <option *ngFor="let n of summary()?.niches ?? []" [value]="n.type">{{ n.name }}</option>
              </select>
            </label>
            <label class="block text-xs uppercase tracking-wider text-slate-400">
              Title (optional)
              <input [(ngModel)]="newVideo.title" name="title" type="text"
                     class="mt-1 block w-full bg-slate-800 border border-slate-700 rounded px-2 py-1 text-sm" />
            </label>
            <label class="block text-xs uppercase tracking-wider text-slate-400">
              Storyblocks query (optional)
              <input [(ngModel)]="newVideo.storyblocksQuery" name="storyblocksQuery" type="text"
                     placeholder="e.g. aerial city sunrise"
                     class="mt-1 block w-full bg-slate-800 border border-slate-700 rounded px-2 py-1 text-sm" />
            </label>
            <button type="submit"
                    [disabled]="queueing()"
                    class="px-3 py-1.5 rounded bg-sky-600 hover:bg-sky-500 disabled:opacity-50 text-sm">
              {{ queueing() ? 'Queueing...' : 'Queue video' }}
            </button>
            <div *ngIf="queueMessage()" class="text-xs"
                 [class.text-emerald-400]="queueMessage()?.kind === 'ok'"
                 [class.text-rose-400]="queueMessage()?.kind === 'err'">
              {{ queueMessage()?.text }}
            </div>
          </form>
        </section>

        <section class="rounded border border-slate-800 bg-slate-900/40 p-4">
          <h2 class="text-lg font-semibold mb-3">Test Storyblocks scrape</h2>
          <p class="text-xs text-slate-400 mb-3">
            Downloads up to <code>max</code> clips and saves them under
            <code>data/downloads/</code> on the server.
          </p>
          <form (ngSubmit)="runScrape()" class="space-y-3">
            <label class="block text-xs uppercase tracking-wider text-slate-400">
              Query
              <input [(ngModel)]="scrapeQuery" name="scrapeQuery" type="text"
                     placeholder="aerial city sunrise"
                     class="mt-1 block w-full bg-slate-800 border border-slate-700 rounded px-2 py-1 text-sm" />
            </label>
            <label class="block text-xs uppercase tracking-wider text-slate-400">
              Max clips
              <input [(ngModel)]="scrapeMax" name="scrapeMax" type="number" min="1" max="10"
                     class="mt-1 block w-24 bg-slate-800 border border-slate-700 rounded px-2 py-1 text-sm" />
            </label>
            <button type="submit"
                    [disabled]="scraping() || !scrapeQuery.trim()"
                    class="px-3 py-1.5 rounded bg-emerald-600 hover:bg-emerald-500 disabled:opacity-50 text-sm">
              {{ scraping() ? 'Running scraper...' : 'Run scraper' }}
            </button>
            <div *ngIf="scrapeMessage()" class="text-xs"
                 [class.text-emerald-400]="scrapeMessage()?.kind === 'ok'"
                 [class.text-rose-400]="scrapeMessage()?.kind === 'err'">
              {{ scrapeMessage()?.text }}
            </div>
            <ul *ngIf="scrapeResults().length" class="text-xs text-slate-300 mt-2 space-y-1 list-disc list-inside">
              <li *ngFor="let r of scrapeResults()">
                {{ r.title || '(untitled)' }} — <code>{{ r.filePath }}</code>
              </li>
            </ul>
          </form>
        </section>
      </div>

      <section class="rounded border border-slate-800 bg-slate-900/40 p-4">
        <h2 class="text-lg font-semibold mb-3">Recent videos</h2>
        <div class="overflow-x-auto">
          <table class="w-full text-sm">
            <thead class="text-slate-400 text-xs uppercase">
              <tr>
                <th class="text-left py-2">Created</th>
                <th class="text-left">Niche</th>
                <th class="text-left">Title</th>
                <th class="text-left">Status</th>
              </tr>
            </thead>
            <tbody>
              <tr *ngFor="let v of videos()" class="border-t border-slate-800">
                <td class="py-2 tabular-nums text-slate-400">{{ v.createdAtUtc | date:'short' }}</td>
                <td>{{ v.nicheType }}</td>
                <td class="text-slate-200">{{ v.title || '(untitled)' }}</td>
                <td>
                  <span class="px-2 py-0.5 rounded text-xs"
                        [class]="statusBadgeClass(v.status)">
                    {{ v.status }}
                  </span>
                </td>
              </tr>
              <tr *ngIf="!videos().length">
                <td colspan="4" class="py-3 text-slate-500 text-sm">
                  No videos in the queue yet — use the form above to create one.
                </td>
              </tr>
            </tbody>
          </table>
        </div>
      </section>

    </section>
  `
})
export class DashboardHomeComponent implements OnInit, OnDestroy {
  private readonly api = inject(NexusApiService);
  private readonly subs: Subscription[] = [];

  summary = signal<DashboardSummary | null>(null);
  videos = signal<Video[]>([]);
  loadError = signal<string | null>(null);

  newVideo: QueueVideoRequest = { nicheType: 'Finance', title: '', storyblocksQuery: '' };
  queueing = signal(false);
  queueMessage = signal<{ kind: 'ok' | 'err'; text: string } | null>(null);

  scrapeQuery = '';
  scrapeMax = 3;
  scraping = signal(false);
  scrapeMessage = signal<{ kind: 'ok' | 'err'; text: string } | null>(null);
  scrapeResults = signal<ScrapedMediaResult[]>([]);

  statusCards = computed(() => {
    const s = this.summary();
    const total = s?.totalVideos ?? 0;
    const pending = s?.countByStatus?.['Pending'] ?? 0;
    const completed = s?.countByStatus?.['Completed'] ?? 0;
    return [
      { label: 'Total videos', value: total },
      { label: 'Pending', value: pending },
      { label: 'Completed', value: completed }
    ];
  });

  ngOnInit(): void {
    this.refreshAll();
  }

  ngOnDestroy(): void {
    this.subs.forEach((s) => s.unsubscribe());
  }

  refreshAll(): void {
    this.loadError.set(null);

    this.subs.push(this.api.getDashboardSummary().subscribe({
      next: (s) => {
        this.summary.set(s);
        if (s.niches.length > 0 && !s.niches.some((n) => n.type === this.newVideo.nicheType)) {
          this.newVideo.nicheType = s.niches[0].type;
        }
      },
      error: (err) => this.loadError.set(this.describeError(err))
    }));

    this.subs.push(this.api.getVideos(undefined, 25).subscribe({
      next: (rows) => this.videos.set(rows),
      error: (err) => this.loadError.set(this.describeError(err))
    }));
  }

  toggleNiche(n: Niche): void {
    this.subs.push(this.api.setNicheActive(n.type, !n.isActive).subscribe({
      next: () => this.refreshAll(),
      error: (err) => this.loadError.set(this.describeError(err))
    }));
  }

  queueVideo(): void {
    if (this.queueing()) return;
    this.queueMessage.set(null);
    this.queueing.set(true);
    this.subs.push(
      this.api.queueVideo({
        nicheType: this.newVideo.nicheType,
        title: this.newVideo.title || null,
        storyblocksQuery: this.newVideo.storyblocksQuery || null
      })
        .pipe(finalize(() => this.queueing.set(false)))
        .subscribe({
          next: (v) => {
            this.queueMessage.set({ kind: 'ok', text: `Queued ${v.id.substring(0, 8)}.` });
            this.newVideo.title = '';
            this.newVideo.storyblocksQuery = '';
            this.refreshAll();
          },
          error: (err) => this.queueMessage.set({ kind: 'err', text: this.describeError(err) })
        })
    );
  }

  runScrape(): void {
    if (this.scraping() || !this.scrapeQuery.trim()) return;
    this.scrapeMessage.set(null);
    this.scrapeResults.set([]);
    this.scraping.set(true);
    this.subs.push(
      this.api.triggerScraperSearch(this.scrapeQuery.trim(), this.scrapeMax)
        .pipe(finalize(() => this.scraping.set(false)))
        .subscribe({
          next: (rows) => {
            this.scrapeResults.set(rows);
            this.scrapeMessage.set({
              kind: 'ok',
              text: rows.length
                ? `Downloaded ${rows.length} clip(s).`
                : 'Scraper finished but returned no clips — check selectors and login state.'
            });
          },
          error: (err) => this.scrapeMessage.set({ kind: 'err', text: this.describeError(err) })
        })
    );
  }

  statusBadgeClass(status: string): string {
    switch (status) {
      case 'Completed': return 'bg-emerald-900/60 text-emerald-300';
      case 'ErrorRequiresHuman': return 'bg-rose-900/60 text-rose-300';
      case 'Pending': return 'bg-slate-700/60 text-slate-200';
      default: return 'bg-sky-900/60 text-sky-200';
    }
  }

  private describeError(err: unknown): string {
    if (typeof err === 'object' && err !== null) {
      const e = err as { status?: number; message?: string; error?: { error?: string } };
      if (e.error?.error) return e.error.error;
      if (e.status) return `HTTP ${e.status}`;
      if (e.message) return e.message;
    }
    return 'Unknown error';
  }

  protected readonly VIDEO_STATUSES = VIDEO_STATUSES;
  protected readonly NicheType: NicheType[] = ['Finance', 'TechAndAi', 'LegalAndCourt'];
}
