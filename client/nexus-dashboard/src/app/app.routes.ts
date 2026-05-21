import { Routes } from '@angular/router';

export const routes: Routes = [
  { path: '', redirectTo: 'dashboard', pathMatch: 'full' },
  {
    path: 'dashboard',
    loadComponent: () => import('./components/dashboard/dashboard.component')
      .then(m => m.DashboardComponent)
  },
  {
    path: 'videos',
    loadComponent: () => import('./components/video-list/video-list.component')
      .then(m => m.VideoListComponent)
  },
  {
    path: 'niches',
    loadComponent: () => import('./components/niche-panel/niche-panel.component')
      .then(m => m.NichePanelComponent)
  }
];
