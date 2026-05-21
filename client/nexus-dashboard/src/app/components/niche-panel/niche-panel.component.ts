import { Component, OnInit, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ApiService } from '../../services/api.service';
import { NicheConfig } from '../../models/video.model';

@Component({
  selector: 'app-niche-panel',
  standalone: true,
  imports: [CommonModule],
  template: `
    <div class="p-6">
      <h2 class="text-2xl font-bold text-white mb-6">Niche Configuration</h2>

      <div class="grid grid-cols-1 md:grid-cols-3 gap-6">
        @for (niche of niches; track niche.id) {
          <div class="bg-dark-700 rounded-xl p-6 border border-dark-500">
            <div class="flex items-center justify-between mb-4">
              <h3 class="text-lg font-semibold text-nexus-300">{{ niche.displayName }}</h3>
              <span class="text-xs px-2 py-1 rounded-full"
                    [class]="niche.isActive ? 'bg-green-600 text-green-100' : 'bg-gray-600 text-gray-300'">
                {{ niche.isActive ? 'Active' : 'Inactive' }}
              </span>
            </div>

            <div class="space-y-2 text-sm">
              <div class="flex justify-between">
                <span class="text-gray-400">Script Tone</span>
                <span class="text-gray-200">{{ niche.scriptTone }}</span>
              </div>
              <div class="flex justify-between">
                <span class="text-gray-400">Voice Style</span>
                <span class="text-gray-200">{{ niche.voiceStyle }}</span>
              </div>
              <div class="flex justify-between">
                <span class="text-gray-400">Music Genre</span>
                <span class="text-gray-200">{{ niche.musicGenre }}</span>
              </div>
              <div class="flex justify-between">
                <span class="text-gray-400">Priority</span>
                <span class="text-nexus-400 font-bold">{{ niche.priority }}</span>
              </div>
            </div>

            <div class="mt-4">
              <p class="text-xs text-gray-500 mb-1">Keywords</p>
              <div class="flex flex-wrap gap-1">
                @for (keyword of niche.searchKeywords; track keyword) {
                  <span class="text-xs bg-dark-900 text-gray-300 px-2 py-0.5 rounded">{{ keyword }}</span>
                }
              </div>
            </div>
          </div>
        }
      </div>
    </div>
  `
})
export class NichePanelComponent implements OnInit {
  private api = inject(ApiService);
  niches: NicheConfig[] = [];

  ngOnInit() {
    this.api.getNiches().subscribe(niches => this.niches = niches);
  }
}
