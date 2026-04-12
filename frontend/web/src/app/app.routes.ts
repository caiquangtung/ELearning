import { Routes } from '@angular/router';
import { authGuard } from './core/auth/auth.guard';
import { guestGuard } from './core/auth/guest.guard';
import { MainLayoutComponent } from './shared/layout/main-layout.component';

export const routes: Routes = [
  {
    path: 'login',
    canActivate: [guestGuard],
    loadComponent: () => import('./features/auth/login.component').then((m) => m.LoginComponent),
  },
  {
    path: 'register',
    canActivate: [guestGuard],
    loadComponent: () => import('./features/auth/register.component').then((m) => m.RegisterComponent),
  },
  {
    path: '',
    component: MainLayoutComponent,
    canActivate: [authGuard],
    children: [
      { path: '', pathMatch: 'full', redirectTo: 'dashboard' },
      {
        path: 'dashboard',
        loadComponent: () =>
          import('./features/dashboard/dashboard.component').then((m) => m.DashboardComponent),
      },
      {
        path: 'profile',
        loadComponent: () => import('./features/profile/profile.component').then((m) => m.ProfileComponent),
      },
      {
        path: 'organizations',
        loadComponent: () =>
          import('./features/organizations/organization-list.component').then((m) => m.OrganizationListComponent),
      },
      {
        path: 'organizations/:id',
        loadComponent: () =>
          import('./features/organizations/organization-detail.component').then((m) => m.OrganizationDetailComponent),
      },
      {
        path: 'courses',
        loadComponent: () => import('./features/courses/course-list.component').then((m) => m.CourseListComponent),
      },
      {
        path: 'courses/:id',
        loadComponent: () =>
          import('./features/courses/course-detail.component').then((m) => m.CourseDetailComponent),
      },
      {
        path: 'training-classes',
        loadComponent: () =>
          import('./features/training-classes/training-class-list.component').then(
            (m) => m.TrainingClassListComponent,
          ),
      },
      {
        path: 'training-classes/:id',
        loadComponent: () =>
          import('./features/training-classes/training-class-detail.component').then(
            (m) => m.TrainingClassDetailComponent,
          ),
      },
    ],
  },
  { path: '**', redirectTo: '' },
];
