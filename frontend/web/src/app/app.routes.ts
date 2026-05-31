import { Routes } from '@angular/router';
import { adminGuard } from './core/auth/admin.guard';
import { authGuard } from './core/auth/auth.guard';
import { guestGuard } from './core/auth/guest.guard';
import { instructorGuard } from './core/auth/instructor.guard';
import { learnerGuard } from './core/auth/learner.guard';
import {
  defaultPortalRedirectGuard,
  legacyCheckoutRedirectGuard,
  legacyAdminRedirectGuard,
  legacyClassesRedirectGuard,
  legacyCoursesRedirectGuard,
  legacyNotificationsRedirectGuard,
  legacyOrdersRedirectGuard,
  legacyQuizzesRedirectGuard,
} from './core/auth/portal-redirect.guard';
import { MainLayoutComponent } from './shared/layout/main-layout.component';

const routeData = (portal: 'learn' | 'teach' | 'admin', breadcrumb: string, pageMode?: string) => ({
  portal,
  breadcrumb,
  pageMode,
});

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
    pathMatch: 'full',
    loadComponent: () =>
      import('./features/landing/landing.component').then(
        (m) => m.LandingComponent,
      ),
  },
  { path: 'landing', pathMatch: 'full', redirectTo: '' },
  {
    path: '',
    component: MainLayoutComponent,
    canActivate: [authGuard],
    children: [
      {
        path: 'dashboard',
        canActivate: [defaultPortalRedirectGuard],
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
        path: 'learn',
        canActivate: [learnerGuard],
        children: [
          {
            path: '',
            data: routeData('learn', 'Learn'),
            loadComponent: () =>
              import('./features/learn/learn.component').then(
                (m) => m.LearnComponent,
              ),
          },
          {
            path: 'courses',
            data: routeData('learn', 'My Courses', 'learner-catalog'),
            loadComponent: () =>
              import('./features/courses/course-list.component').then(
                (m) => m.CourseListComponent,
              ),
          },
          {
            path: 'courses/:id',
            data: routeData('learn', 'Course details', 'learner-course-detail'),
            loadComponent: () =>
              import('./features/courses/course-detail.component').then(
                (m) => m.CourseDetailComponent,
              ),
          },
          {
            path: 'classes',
            data: routeData('learn', 'My Classes', 'learner-classes'),
            loadComponent: () =>
              import('./features/training-classes/training-class-list.component').then(
                (m) => m.TrainingClassListComponent,
              ),
          },
          {
            path: 'classes/:id',
            data: routeData('learn', 'Class details', 'learner-class-detail'),
            loadComponent: () =>
              import('./features/training-classes/training-class-detail.component').then(
                (m) => m.TrainingClassDetailComponent,
              ),
          },
          {
            path: 'checkout',
            data: routeData('learn', 'Checkout'),
            loadComponent: () =>
              import('./features/checkout/checkout.component').then(
                (m) => m.CheckoutComponent,
              ),
          },
          {
            path: 'orders',
            data: routeData('learn', 'Orders'),
            loadComponent: () =>
              import('./features/orders/order-list.component').then(
                (m) => m.OrderListComponent,
              ),
          },
          {
            path: 'orders/:id',
            data: routeData('learn', 'Order details'),
            loadComponent: () =>
              import('./features/orders/order-detail.component').then(
                (m) => m.OrderDetailComponent,
              ),
          },
          {
            path: 'notifications',
            data: routeData('learn', 'Notifications'),
            loadComponent: () =>
              import('./features/notifications/notification-list.component').then(
                (m) => m.NotificationListComponent,
              ),
          },
          {
            path: 'ai-path',
            data: routeData('learn', 'AI Learning Path'),
            loadComponent: () =>
              import('./features/learn/learn.component').then(
                (m) => m.LearnComponent,
              ),
          },
        ],
      },

      {
        path: 'teach',
        canActivate: [instructorGuard],
        children: [
          {
            path: '',
            data: routeData('teach', 'Teach'),
            loadComponent: () =>
              import('./features/teach/teach.component').then(
                (m) => m.TeachComponent,
              ),
          },
          {
            path: 'classes',
            data: routeData('teach', 'Classes', 'teacher-classes'),
            loadComponent: () =>
              import('./features/training-classes/training-class-list.component').then(
                (m) => m.TrainingClassListComponent,
              ),
          },
          {
            path: 'classes/:id',
            data: routeData('teach', 'Class details', 'teacher-class-detail'),
            loadComponent: () =>
              import('./features/training-classes/training-class-detail.component').then(
                (m) => m.TrainingClassDetailComponent,
              ),
          },
          {
            path: 'courses',
            data: routeData('teach', 'Courses', 'teacher-courses'),
            loadComponent: () =>
              import('./features/courses/course-list.component').then(
                (m) => m.CourseListComponent,
              ),
          },
          {
            path: 'courses/:id',
            data: routeData('teach', 'Course details', 'teacher-course-detail'),
            loadComponent: () =>
              import('./features/courses/course-detail.component').then(
                (m) => m.CourseDetailComponent,
              ),
          },
          {
            path: 'quizzes',
            data: routeData('teach', 'Quizzes'),
            loadChildren: () =>
              import('./features/quizzes/quizzes.routes').then(
                (m) => m.QUIZZES_ROUTES,
              ),
          },
          {
            path: 'notifications',
            data: routeData('teach', 'Notifications'),
            loadComponent: () =>
              import('./features/notifications/notification-list.component').then(
                (m) => m.NotificationListComponent,
              ),
          },
        ],
      },

      {
        path: 'admin',
        canActivate: [adminGuard],
        children: [
          {
            path: '',
            data: routeData('admin', 'Admin'),
            loadComponent: () =>
              import('./features/dashboard/dashboard.component').then(
                (m) => m.DashboardComponent,
              ),
          },
          {
            path: 'organizations',
            data: routeData('admin', 'Organizations'),
            loadComponent: () =>
              import('./features/organizations/organization-list.component').then(
                (m) => m.OrganizationListComponent,
              ),
          },
          {
            path: 'organizations/:id',
            data: routeData('admin', 'Organization details'),
            loadComponent: () =>
              import('./features/organizations/organization-detail.component').then(
                (m) => m.OrganizationDetailComponent,
              ),
          },
          {
            path: 'organizations/:id/license-pools',
            data: routeData('admin', 'License pools'),
            loadComponent: () =>
              import('./features/licenses/license-pool-list.component').then(
                (m) => m.LicensePoolListComponent,
              ),
          },
          {
            path: 'license-pools',
            data: routeData('admin', 'License pools'),
            loadComponent: () =>
              import('./features/organizations/organization-list.component').then(
                (m) => m.OrganizationListComponent,
              ),
          },
          {
            path: 'license-pools/:id',
            data: routeData('admin', 'License pool details'),
            loadComponent: () =>
              import('./features/licenses/license-pool-detail.component').then(
                (m) => m.LicensePoolDetailComponent,
              ),
          },
          {
            path: 'campaigns',
            data: routeData('admin', 'Campaigns'),
            loadComponent: () =>
              import('./features/campaigns/campaign-list.component').then(
                (m) => m.CampaignListComponent,
              ),
          },
          {
            path: 'campaigns/:id',
            data: routeData('admin', 'Campaign details'),
            loadComponent: () =>
              import('./features/campaigns/campaign-detail.component').then(
                (m) => m.CampaignDetailComponent,
              ),
          },
          {
            path: 'reports',
            data: routeData('admin', 'Reports'),
            loadComponent: () =>
              import('./features/dashboard/dashboard.component').then(
                (m) => m.DashboardComponent,
              ),
          },
          {
            path: 'notifications',
            data: routeData('admin', 'Notifications'),
            loadComponent: () =>
              import('./features/notifications/notification-list.component').then(
                (m) => m.NotificationListComponent,
              ),
          },
          {
            path: 'announcements',
            data: routeData('admin', 'Announcements'),
            loadComponent: () =>
              import('./features/notifications/announcement.component').then(
                (m) => m.AnnouncementComponent,
              ),
          },
        ],
      },

      {
        path: 'courses',
        canActivate: [legacyCoursesRedirectGuard],
        loadComponent: () =>
          import('./features/courses/course-list.component').then(
            (m) => m.CourseListComponent,
          ),
      },
      {
        path: 'courses/:id',
        canActivate: [legacyCoursesRedirectGuard],
        loadComponent: () =>
          import('./features/courses/course-detail.component').then(
            (m) => m.CourseDetailComponent,
          ),
      },
      {
        path: 'training-classes',
        canActivate: [legacyClassesRedirectGuard],
        loadComponent: () =>
          import('./features/training-classes/training-class-list.component').then(
            (m) => m.TrainingClassListComponent,
          ),
      },
      {
        path: 'training-classes/:id',
        canActivate: [legacyClassesRedirectGuard],
        loadComponent: () =>
          import('./features/training-classes/training-class-detail.component').then(
            (m) => m.TrainingClassDetailComponent,
          ),
      },
      {
        path: 'checkout',
        canActivate: [legacyCheckoutRedirectGuard],
        loadComponent: () =>
          import('./features/checkout/checkout.component').then(
            (m) => m.CheckoutComponent,
          ),
      },
      {
        path: 'orders',
        canActivate: [legacyOrdersRedirectGuard],
        loadComponent: () =>
          import('./features/orders/order-list.component').then(
            (m) => m.OrderListComponent,
          ),
      },
      {
        path: 'orders/:id',
        canActivate: [legacyOrdersRedirectGuard],
        loadComponent: () =>
          import('./features/orders/order-detail.component').then(
            (m) => m.OrderDetailComponent,
          ),
      },
      {
        path: 'organizations',
        canActivate: [legacyAdminRedirectGuard],
        loadComponent: () =>
          import('./features/organizations/organization-list.component').then(
            (m) => m.OrganizationListComponent,
          ),
      },
      {
        path: 'organizations/:id',
        canActivate: [legacyAdminRedirectGuard],
        loadComponent: () =>
          import('./features/organizations/organization-detail.component').then(
            (m) => m.OrganizationDetailComponent,
          ),
      },
      {
        path: 'campaigns',
        canActivate: [legacyAdminRedirectGuard],
        loadComponent: () =>
          import('./features/campaigns/campaign-list.component').then(
            (m) => m.CampaignListComponent,
          ),
      },
      {
        path: 'campaigns/:id',
        canActivate: [legacyAdminRedirectGuard],
        loadComponent: () =>
          import('./features/campaigns/campaign-detail.component').then(
            (m) => m.CampaignDetailComponent,
          ),
      },
      {
        path: 'notifications',
        canActivate: [legacyNotificationsRedirectGuard],
        loadComponent: () =>
          import('./features/notifications/notification-list.component').then(
            (m) => m.NotificationListComponent,
          ),
      },
      {
        path: 'quizzes',
        canActivate: [legacyQuizzesRedirectGuard],
        loadChildren: () =>
          import('./features/quizzes/quizzes.routes').then(
            (m) => m.QUIZZES_ROUTES,
          ),
      },
    ],
  },
  { path: '**', redirectTo: '' },
];
