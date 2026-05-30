import { Routes } from '@angular/router';
import { authGuard } from './core/auth/auth.guard';
import { guestGuard } from './core/auth/guest.guard';
import { MainLayoutComponent } from './shared/layout/main-layout.component';

export const routes: Routes = [
  {
    path: 'login',
    canActivate: [guestGuard],
    loadComponent: () =>
      import('./features/auth/login.component').then((m) => m.LoginComponent),
  },
  {
    path: 'register',
    canActivate: [guestGuard],
    loadComponent: () =>
      import('./features/auth/register.component').then(
        (m) => m.RegisterComponent,
      ),
  },
  {
    path: '',
    component: MainLayoutComponent,
    canActivate: [authGuard],
    children: [
      { path: '', pathMatch: 'full', redirectTo: 'dashboard' },
      {
        path: 'dashboard',
        data: { breadcrumb: 'Dashboard' },
        loadComponent: () =>
          import('./features/dashboard/dashboard.component').then(
            (m) => m.DashboardComponent,
          ),
      },
      {
        path: 'profile',
        data: { breadcrumb: 'Profile' },
        loadComponent: () =>
          import('./features/profile/profile.component').then(
            (m) => m.ProfileComponent,
          ),
      },
      {
        path: 'organizations',
        data: { breadcrumb: 'Organizations' },
        loadComponent: () =>
          import('./features/organizations/organization-list.component').then(
            (m) => m.OrganizationListComponent,
          ),
      },
      {
        path: 'organizations/:id',
        data: { breadcrumb: 'Details' },
        loadComponent: () =>
          import('./features/organizations/organization-detail.component').then(
            (m) => m.OrganizationDetailComponent,
          ),
      },
      {
        path: 'courses',
        data: { breadcrumb: 'Courses' },
        loadComponent: () =>
          import('./features/courses/course-list.component').then(
            (m) => m.CourseListComponent,
          ),
      },
      {
        path: 'courses/:id',
        data: { breadcrumb: 'Details' },
        loadComponent: () =>
          import('./features/courses/course-detail.component').then(
            (m) => m.CourseDetailComponent,
          ),
      },
      {
        path: 'training-classes',
        data: { breadcrumb: 'Training classes' },
        loadComponent: () =>
          import('./features/training-classes/training-class-list.component').then(
            (m) => m.TrainingClassListComponent,
          ),
      },
      {
        path: 'training-classes/:id',
        data: { breadcrumb: 'Details' },
        loadComponent: () =>
          import('./features/training-classes/training-class-detail.component').then(
            (m) => m.TrainingClassDetailComponent,
          ),
      },
      {
        path: 'organizations/:id/license-pools',
        data: { breadcrumb: 'License pools' },
        loadComponent: () =>
          import('./features/licenses/license-pool-list.component').then(
            (m) => m.LicensePoolListComponent,
          ),
      },
      {
        path: 'license-pools/:id',
        data: { breadcrumb: 'Details' },
        loadComponent: () =>
          import('./features/licenses/license-pool-detail.component').then(
            (m) => m.LicensePoolDetailComponent,
          ),
      },
      {
        path: 'checkout',
        data: { breadcrumb: 'Checkout' },
        loadComponent: () =>
          import('./features/checkout/checkout.component').then(
            (m) => m.CheckoutComponent,
          ),
      },
      {
        path: 'orders',
        data: { breadcrumb: 'Orders' },
        loadComponent: () =>
          import('./features/orders/order-list.component').then(
            (m) => m.OrderListComponent,
          ),
      },
      {
        path: 'orders/:id',
        data: { breadcrumb: 'Details' },
        loadComponent: () =>
          import('./features/orders/order-detail.component').then(
            (m) => m.OrderDetailComponent,
          ),
      },
      {
        path: 'campaigns',
        data: { breadcrumb: 'Campaigns' },
        loadComponent: () =>
          import('./features/campaigns/campaign-list.component').then(
            (m) => m.CampaignListComponent,
          ),
      },
      {
        path: 'campaigns/:id',
        data: { breadcrumb: 'Details' },
        loadComponent: () =>
          import('./features/campaigns/campaign-detail.component').then(
            (m) => m.CampaignDetailComponent,
          ),
      },
      {
        path: 'notifications',
        data: { breadcrumb: 'Notifications' },
        loadComponent: () =>
          import('./features/notifications/notification-list.component').then(
            (m) => m.NotificationListComponent,
          ),
      },
      {
        path: 'notifications/announcements',
        data: { breadcrumb: 'Announcements' },
        loadComponent: () =>
          import('./features/notifications/announcement.component').then(
            (m) => m.AnnouncementComponent,
          ),
      },
      {
        path: 'quizzes',
        data: { breadcrumb: 'Quizzes' },
        loadChildren: () =>
          import('./features/quizzes/quizzes.routes').then(
            (m) => m.QUIZZES_ROUTES,
          ),
      },
    ],
  },
  { path: '**', redirectTo: '' },
];
