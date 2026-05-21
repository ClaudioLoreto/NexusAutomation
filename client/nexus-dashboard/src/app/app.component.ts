import { Component } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterModule } from '@angular/router';

@Component({
  selector: 'app-root',
  standalone: true,
  imports: [CommonModule, RouterModule],
  template: `
    <div class="min-h-screen bg-dark-900">
      <nav class="bg-dark-800 border-b border-dark-600 px-6 py-4">
        <div class="flex items-center justify-between max-w-7xl mx-auto">
          <div class="flex items-center gap-2">
            <span class="text-2xl font-bold bg-gradient-to-r from-nexus-400 to-nexus-600 bg-clip-text text-transparent">
              Nexus
            </span>
            <span class="text-sm text-gray-500">Shorts Engine</span>
          </div>
          <div class="flex gap-1">
            <a routerLink="/dashboard" routerLinkActive="bg-dark-600 text-nexus-400"
               class="px-4 py-2 rounded-lg text-sm text-gray-300 hover:text-white hover:bg-dark-700 transition-colors">
              Dashboard
            </a>
            <a routerLink="/videos" routerLinkActive="bg-dark-600 text-nexus-400"
               class="px-4 py-2 rounded-lg text-sm text-gray-300 hover:text-white hover:bg-dark-700 transition-colors">
              Videos
            </a>
            <a routerLink="/niches" routerLinkActive="bg-dark-600 text-nexus-400"
               class="px-4 py-2 rounded-lg text-sm text-gray-300 hover:text-white hover:bg-dark-700 transition-colors">
              Niches
            </a>
          </div>
        </div>
      </nav>
      <main class="max-w-7xl mx-auto">
        <router-outlet />
      </main>
    </div>
  `
})
export class AppComponent {}
