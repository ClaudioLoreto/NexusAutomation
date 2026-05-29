import { Injectable, inject } from '@angular/core';
import { HttpClient, HttpParams } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '../../environments/environment';
import {
  DashboardSummary,
  Niche,
  NicheType,
  QueueVideoRequest,
  ScrapedMediaResult,
  Video,
  VideoStatus
} from '../models/nexus-models';

@Injectable({ providedIn: 'root' })
export class NexusApiService {
  private readonly http = inject(HttpClient);
  private readonly base = environment.apiBaseUrl;

  health(): Observable<{ status: string; service: string; nichesSeeded: number; databaseConfigured: boolean }> {
    return this.http.get<{ status: string; service: string; nichesSeeded: number; databaseConfigured: boolean }>(
      `${this.base}/health`
    );
  }

  getDashboardSummary(): Observable<DashboardSummary> {
    return this.http.get<DashboardSummary>(`${this.base}/dashboard/summary`);
  }

  getNiches(): Observable<Niche[]> {
    return this.http.get<Niche[]>(`${this.base}/niches`);
  }

  setNicheActive(type: NicheType, isActive: boolean): Observable<Niche> {
    return this.http.patch<Niche>(`${this.base}/niches/${type}/active`, { isActive });
  }

  setNichePriority(type: NicheType, queuePriority: number): Observable<Niche> {
    return this.http.patch<Niche>(`${this.base}/niches/${type}/priority`, { queuePriority });
  }

  getVideos(status?: VideoStatus, take = 50): Observable<Video[]> {
    let params = new HttpParams().set('take', take.toString());
    if (status) {
      params = params.set('status', status);
    }
    return this.http.get<Video[]>(`${this.base}/videos`, { params });
  }

  queueVideo(request: QueueVideoRequest): Observable<Video> {
    return this.http.post<Video>(`${this.base}/videos`, request);
  }

  triggerScraperSearch(query: string, max = 3): Observable<ScrapedMediaResult[]> {
    const params = new HttpParams().set('query', query).set('max', max.toString());
    return this.http.post<ScrapedMediaResult[]>(`${this.base}/scraper/search`, null, { params });
  }

  authenticateScraper(): Observable<{ authenticated: boolean }> {
    return this.http.post<{ authenticated: boolean }>(`${this.base}/scraper/authenticate`, null);
  }
}
