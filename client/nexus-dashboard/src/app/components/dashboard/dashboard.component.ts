import { Component, OnInit, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ApiService } from '../../services/api.service';
import { DashboardStats } from '../../models/video.model';

@Component({
  selector: 'app-dashboard',
  standalone: true,
  imports: [CommonModule],
  template: `
    <div class="p-6">
      <h1 class="text-3xl font-bold text-nexus-400 mb-8">Nexus Shorts Engine</h1>

      @if (stats) {
        <div class="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-5 gap-4 mb-8">
          <div class="bg-dark-700 rounded-xl p-5 border border-dark-500">
            <p class="text-sm text-gray-400 uppercase tracking-wide">Total Videos</p>
            <p class="text-3xl font-bold text-white mt-2">{{ stats.totalVideos }}</p>
          </div>
          <div class="bg-dark-700 rounded-xl p-5 border border-dark-500">
            <p class="text-sm text-gray-400 uppercase tracking-wide">Pending</p>
            <p class="text-3xl font-bold text-yellow-400 mt-2">{{ stats.pendingVideos }}</p>
          </div>
          <div class="bg-dark-700 rounded-xl p-5 border border-dark-500">
            <p class="text-sm text-gray-400 uppercase tracking-wide">Rendering</p>
            <p class="text-3xl font-bold text-blue-400 mt-2">{{ stats.renderingVideos }}</p>
          </div>
          <div class="bg-dark-700 rounded-xl p-5 border border-dark-500">
            <p class="text-sm text-gray-400 uppercase tracking-wide">Completed</p>
            <p class="text-3xl font-bold text-green-400 mt-2">{{ stats.completedVideos }}</p>
          </div>
          <div class="bg-dark-700 rounded-xl p-5 border border-dark-500">
            <p class="text-sm text-gray-400 uppercase tracking-wide">Errors</p>
            <p class="text-3xl font-bold text-red-400 mt-2">{{ stats.errorVideos }}</p>
          </div>
        </div>

        <div class="bg-dark-700 rounded-xl p-6 border border-dark-500 mb-8">
          <h2 class="text-xl font-semibold text-white mb-4">Niche View Velocity (views/hr)</h2>
          <p class="text-sm text-nexus-300 mb-4">Top Performing: <span class="font-bold text-nexus-400">{{ stats.topNiche }}</span></p>
          <div class="space-y-3">
            @for (entry of velocityEntries; track entry.key) {
              <div>
                <div class="flex justify-between text-sm mb-1">
                  <span class="text-gray-300">{{ entry.key }}</span>
                  <span class="text-gray-400">{{ entry.value | number:'1.0-0' }}</span>
                </div>
                <div class="w-full bg-dark-900 rounded-full h-3">
                  <div class="bg-nexus-500 h-3 rounded-full transition-all duration-500"
                       [style.width.%]="getBarWidth(entry.value)"></div>
                </div>
              </div>
            }
          </div>
        </div>
      }
    </div>
  `
})
export class DashboardComponent implements OnInit {
  private api = inject(ApiService);
  stats: DashboardStats | null = null;
  velocityEntries: { key: string; value: number }[] = [];
  maxVelocity = 1;

  ngOnInit() {
    this.api.getDashboardStats().subscribe(stats => {
      this.stats = stats;
      this.velocityEntries = Object.entries(stats.nicheVelocities)
        .map(([key, value]) => ({ key, value }));
      this.maxVelocity = Math.max(...this.velocityEntries.map(e => e.value), 1);
    });
  }

  getBarWidth(value: number): number {
    return (value / this.maxVelocity) * 100;
  }
}
