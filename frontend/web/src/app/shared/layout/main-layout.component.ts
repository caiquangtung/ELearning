import { Component, inject, signal } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { Router, RouterModule } from '@angular/router';
import { MenuItem, PrimeTemplate } from 'primeng/api';
import { Button } from 'primeng/button';
import { Menubar } from 'primeng/menubar';
import { ProgressBar } from 'primeng/progressbar';
import { InputText } from 'primeng/inputtext';
import { LmsApiService } from '../../core/api/lms-api.service';
import { AuthService } from '../../core/auth/auth.service';
import { LoadingService } from '../../core/loading/loading.service';

@Component({
  selector: 'app-main-layout',
  standalone: true,
  imports: [FormsModule, RouterModule, Menubar, Button, PrimeTemplate, ProgressBar, InputText],
  template: `
    <p-menubar [model]="navItems" styleClass="mb-0">
      <ng-template pTemplate="start">
        <span class="font-bold text-xl mr-3">ELearning</span>
      </ng-template>
      <ng-template pTemplate="end">
        <form class="global-search" (ngSubmit)="submitGlobalSearch()">
          <input
            pInputText
            name="globalSearch"
            [(ngModel)]="globalSearch"
            placeholder="Search courses"
            aria-label="Search courses"
          />
        </form>
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
  styleUrl: './main-layout.component.scss',
})
export class MainLayoutComponent {
  readonly auth = inject(AuthService);
  readonly loading = inject(LoadingService);
  private readonly router = inject(Router);
  private readonly api = inject(LmsApiService);
  readonly unreadCount = signal(0);
  globalSearch = '';

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

  submitGlobalSearch(): void {
    const search = this.globalSearch.trim();
    void this.router.navigate(['/courses'], {
      queryParams: search ? { search, sort: 'Newest' } : {},
    });
  }

  private refreshUnreadCount(): void {
    this.api.getUnreadNotificationCount().subscribe({
      next: (result) => this.unreadCount.set(result.count),
      error: () => this.unreadCount.set(0),
    });
  }
}
