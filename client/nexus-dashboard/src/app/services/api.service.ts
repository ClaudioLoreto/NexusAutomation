import { Injectable, inject } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { Video, NicheConfig, DashboardStats, CreateVideoRequest, NicheType } from '../models/video.model';
import { environment } from '../../environments/environment';

@Injectable({ providedIn: 'root' })
export class ApiService {
  private readonly http = inject(HttpClient);
  private readonly baseUrl = environment.apiUrl;

  getVideos(status?: string, niche?: string, page = 1, pageSize = 20): Observable<Video[]> {
    let params: Record<string, string> = { page: page.toString(), pageSize: pageSize.toString() };
    if (status) params['status'] = status;
    if (niche) params['niche'] = niche;
    return this.http.get<Video[]>(`${this.baseUrl}/api/videos`, { params });
  }

  getVideo(id: string): Observable<Video> {
    return this.http.get<Video>(`${this.baseUrl}/api/videos/${id}`);
  }

  createVideo(request: CreateVideoRequest): Observable<Video> {
    return this.http.post<Video>(`${this.baseUrl}/api/videos`, request);
  }

  retryVideo(id: string): Observable<void> {
    return this.http.post<void>(`${this.baseUrl}/api/videos/${id}/retry`, {});
  }

  getNiches(): Observable<NicheConfig[]> {
    return this.http.get<NicheConfig[]>(`${this.baseUrl}/api/niches`);
  }

  getNicheVelocities(): Observable<Record<NicheType, number>> {
    return this.http.get<Record<NicheType, number>>(`${this.baseUrl}/api/niches/velocities`);
  }

  getDashboardStats(): Observable<DashboardStats> {
    return this.http.get<DashboardStats>(`${this.baseUrl}/api/dashboard/stats`);
  }
}
