import { Routes } from '@angular/router';
import { DashboardHomeComponent } from './dashboard-home.component';
import { NicheEditorComponent } from './niche-editor.component';

export const routes: Routes = [
  { path: '', component: DashboardHomeComponent },
  { path: 'niches/new', component: NicheEditorComponent },
  { path: '**', redirectTo: '' }
];
