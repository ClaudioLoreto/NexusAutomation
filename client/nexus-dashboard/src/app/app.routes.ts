import { Routes } from '@angular/router';
import { DashboardHomeComponent } from './dashboard-home.component';

export const routes: Routes = [
  { path: '', component: DashboardHomeComponent },
  { path: '**', redirectTo: '' }
];
