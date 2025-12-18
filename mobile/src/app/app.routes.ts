import { Routes } from '@angular/router';
import { AuthGuard } from './services/auth.guard'

export const routes: Routes = [
  {
    path: '',
    redirectTo: '/home',
    pathMatch: 'full'
  },
  {
    path: 'login',
    loadComponent: () => import('./login/login.component').then(m => m.LoginComponent)
  },
  {
    path: 'register',
    loadComponent: () => import('./register/register.component').then(m => m.RegisterComponent)
  },
  {
    path: 'home',
    canActivate: [AuthGuard],
    loadComponent: () => import('./home/home.component').then(m => m.HomeComponent)
  },
  {
    path: 'forgot-password',
    loadComponent: () => import('./forgot-password/forgot-password.component').then(m => m.ForgotPasswordComponent)
  },
  {
    path: 'search',
    canActivate: [AuthGuard],
    loadComponent: () => import('./search/search.component').then(m => m.SearchComponent)
  },
  { 
    path: 'perfume/:id', 
    canActivate: [AuthGuard],
    loadComponent: () => import('./perfume/perfume.component').then(m => m.PerfumeComponent) 
  }
];
