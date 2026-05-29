import { Component } from '@angular/core';
import { RouterOutlet } from '@angular/router';

@Component({
  selector: 'app-root',
  standalone: true,
  imports: [RouterOutlet],
  template: `
    <div class="min-h-screen flex flex-col">
      <header class="border-b border-slate-800 px-6 py-4 flex items-baseline justify-between">
        <div>
          <h1 class="text-xl font-bold tracking-tight">Nexus Shorts Engine</h1>
          <p class="text-sm text-slate-400">Control panel</p>
        </div>
        <a href="http://localhost:5239/swagger"
           target="_blank" rel="noopener"
           class="text-xs text-sky-400 hover:text-sky-300 underline">
          API · Swagger
        </a>
      </header>
      <main class="flex-1 p-6">
        <router-outlet />
      </main>
    </div>
  `
})
export class AppComponent {}
