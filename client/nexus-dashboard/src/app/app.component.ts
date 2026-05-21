import { Component } from '@angular/core';
import { RouterOutlet } from '@angular/router';

@Component({
  selector: 'app-root',
  standalone: true,
  imports: [RouterOutlet],
  template: `
    <div class="min-h-screen flex flex-col">
      <header class="border-b border-slate-800 px-6 py-4">
        <h1 class="text-xl font-bold tracking-tight">Nexus Shorts Engine</h1>
        <p class="text-sm text-slate-400">Control panel — API integration pending</p>
      </header>
      <main class="flex-1 p-6">
        <router-outlet />
      </main>
    </div>
  `
})
export class AppComponent {}
