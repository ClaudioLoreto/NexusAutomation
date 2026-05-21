import { Component, OnInit, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { ApiService } from '../../services/api.service';
import { Video, VideoStatus, NicheType, CreateVideoRequest } from '../../models/video.model';

@Component({
  selector: 'app-video-list',
  standalone: true,
  imports: [CommonModule, FormsModule],
  template: `
    <div class="p-6">
      <div class="flex items-center justify-between mb-6">
        <h2 class="text-2xl font-bold text-white">Video Pipeline</h2>
        <button (click)="showCreateForm = !showCreateForm"
                class="px-4 py-2 bg-nexus-600 hover:bg-nexus-700 text-white rounded-lg font-medium transition-colors">
          + New Video
        </button>
      </div>

      @if (showCreateForm) {
        <div class="bg-dark-700 rounded-xl p-6 border border-dark-500 mb-6">
          <h3 class="text-lg font-semibold text-white mb-4">Create New Video</h3>
          <div class="grid grid-cols-1 md:grid-cols-2 gap-4">
            <input [(ngModel)]="newVideo.title"
                   placeholder="Video Title"
                   class="bg-dark-900 border border-dark-500 rounded-lg px-4 py-2 text-white placeholder-gray-500 focus:border-nexus-500 focus:outline-none" />
            <select [(ngModel)]="newVideo.niche"
                    class="bg-dark-900 border border-dark-500 rounded-lg px-4 py-2 text-white focus:border-nexus-500 focus:outline-none">
              <option value="Finance">Finance</option>
              <option value="TechAndAI">Tech & AI</option>
              <option value="LegalAndCourt">Legal & Court</option>
            </select>
          </div>
          <button (click)="createVideo()"
                  class="mt-4 px-6 py-2 bg-green-600 hover:bg-green-700 text-white rounded-lg font-medium transition-colors">
            Create & Enqueue
          </button>
        </div>
      }

      <div class="space-y-3">
        @for (video of videos; track video.id) {
          <div class="bg-dark-700 rounded-xl p-4 border border-dark-500 flex items-center justify-between">
            <div class="flex-1">
              <div class="flex items-center gap-3">
                <span class="text-white font-medium">{{ video.title }}</span>
                <span class="text-xs px-2 py-0.5 rounded-full"
                      [class]="getStatusClass(video.status)">
                  {{ video.status }}
                </span>
                <span class="text-xs text-gray-500">{{ video.niche }}</span>
              </div>
              @if (video.errorMessage) {
                <p class="text-sm text-red-400 mt-1">{{ video.errorMessage }}</p>
              }
              <p class="text-xs text-gray-500 mt-1">Created: {{ video.createdAtUtc | date:'medium' }}</p>
            </div>
            <div class="flex gap-2">
              @if (video.status === 'ErrorRequiresHuman') {
                <button (click)="retryVideo(video.id)"
                        class="px-3 py-1 bg-yellow-600 hover:bg-yellow-700 text-white text-sm rounded-lg transition-colors">
                  Retry
                </button>
              }
            </div>
          </div>
        }
      </div>
    </div>
  `
})
export class VideoListComponent implements OnInit {
  private api = inject(ApiService);
  videos: Video[] = [];
  showCreateForm = false;
  newVideo: CreateVideoRequest = { title: '', niche: NicheType.TechAndAI };

  ngOnInit() {
    this.loadVideos();
  }

  loadVideos() {
    this.api.getVideos().subscribe(videos => this.videos = videos);
  }

  createVideo() {
    if (!this.newVideo.title) return;
    this.api.createVideo(this.newVideo).subscribe(() => {
      this.showCreateForm = false;
      this.newVideo = { title: '', niche: NicheType.TechAndAI };
      this.loadVideos();
    });
  }

  retryVideo(id: string) {
    this.api.retryVideo(id).subscribe(() => this.loadVideos());
  }

  getStatusClass(status: VideoStatus): string {
    const map: Record<string, string> = {
      Pending: 'bg-gray-600 text-gray-200',
      TrendAnalyzed: 'bg-purple-600 text-purple-100',
      Scripting: 'bg-blue-600 text-blue-100',
      MediaDownloaded: 'bg-cyan-600 text-cyan-100',
      Rendering: 'bg-yellow-600 text-yellow-100',
      Completed: 'bg-green-600 text-green-100',
      ErrorRequiresHuman: 'bg-red-600 text-red-100',
    };
    return map[status] || 'bg-gray-600 text-gray-200';
  }
}
