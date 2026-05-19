import { Component, inject, signal } from '@angular/core';
import { RouterModule } from '@angular/router';
import { MenuItem, PrimeTemplate } from 'primeng/api';
import { Button } from 'primeng/button';
import { Menubar } from 'primeng/menubar';
import { ProgressBar } from 'primeng/progressbar';
import { LmsApiService } from '../../core/api/lms-api.service';
import { AuthService } from '../../core/auth/auth.service';
import { LoadingService } from '../../core/loading/loading.service';

@Component({
  selector: 'app-main-layout',
  standalone: true,
  imports: [RouterModule, Menubar, Button, PrimeTemplate, ProgressBar],
  template: `
    <p-menubar [model]="navItems" styleClass="mb-0">
      <ng-template pTemplate="start">
        <span class="font-bold text-xl mr-3">ELearning</span>
      </ng-template>
      <ng-template pTemplate="end">
        <a class="notification-button" routerLink="/notifications" aria-label="Notifications">
          <i class="pi pi-bell"></i>
          @if (unreadCount() > 0) {
            <span>{{ unreadCount() > 99 ? '99+' : unreadCount() }}</span>
          }
        </a>
        <span class="text-sm mr-3 hidden md:inline">{{ auth.user()?.fullName ?? auth.user()?.email }}</span>
        <p-button label="Sign out" icon="pi pi-sign-out" severity="secondary" [text]="true" (onClick)="signOut()" />
      </ng-template>
    </p-menubar>
    @if (loading.isLoading() > 0) {
      <p-progressBar mode="indeterminate" [style]="{ height: '3px' }" />
    }
    <div class="layout-main">
      <router-outlet />
    </div>
  `,
  styles: [`
    .notification-button {
      position: relative;
      display: inline-flex;
      align-items: center;
      justify-content: center;
      width: 2.25rem;
      height: 2.25rem;
      margin-right: .75rem;
      color: var(--text-color);
      text-decoration: none;
      border-radius: 50%;
    }
    .notification-button:hover { background: var(--surface-hover); }
    .notification-button span {
      position: absolute;
      top: .2rem;
      right: .15rem;
      min-width: 1rem;
      height: 1rem;
      padding: 0 .2rem;
      border-radius: 999px;
      background: var(--red-500);
      color: #fff;
      font-size: .65rem;
      line-height: 1rem;
      text-align: center;
      font-weight: 700;
    }
  `],
})
export class MainLayoutComponent {
  readonly auth = inject(AuthService);
  readonly loading = inject(LoadingService);
  private readonly api = inject(LmsApiService);
  readonly unreadCount = signal(0);

  readonly navItems: MenuItem[] = [
    { label: 'Dashboard', icon: 'pi pi-home', routerLink: '/dashboard' },
    { label: 'Profile', icon: 'pi pi-user', routerLink: '/profile' },
    { label: 'Organizations', icon: 'pi pi-building', routerLink: '/organizations' },
    { label: 'Courses', icon: 'pi pi-book', routerLink: '/courses' },
    { label: 'Classes', icon: 'pi pi-calendar', routerLink: '/training-classes' },
    { label: 'My orders', icon: 'pi pi-shopping-bag', routerLink: '/orders' },
    { label: 'Campaigns', icon: 'pi pi-ticket', routerLink: '/campaigns', visible: this.isAdmin() },
  ];

  constructor() {
    this.refreshUnreadCount();
  }

  private isAdmin(): boolean {
    const roles = this.auth.user()?.roles ?? [];
    return roles.some((r) => r === 'Admin');
  }

  signOut(): void {
    this.auth.logout();
  }

  private refreshUnreadCount(): void {
    this.api.getUnreadNotificationCount().subscribe({
      next: (result) => this.unreadCount.set(result.count),
      error: () => this.unreadCount.set(0),
    });
  }
}
