import { Component, inject, signal } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { Router, RouterModule } from '@angular/router';
import { MenuItem } from 'primeng/api';
import { Button } from 'primeng/button';
import { ProgressBar } from 'primeng/progressbar';
import { InputText } from 'primeng/inputtext';
import { LmsApiService } from '../../core/api/lms-api.service';
import { AuthService } from '../../core/auth/auth.service';
import { LoadingService } from '../../core/loading/loading.service';

@Component({
  selector: 'app-main-layout',
  standalone: true,
  imports: [FormsModule, RouterModule, Button, ProgressBar, InputText],
  template: `
    <div class="app-shell" [class.is-sidebar-collapsed]="isSidebarCollapsed()">
      <aside
        class="app-sidebar"
        [class.is-hidden]="isSidebarCollapsed()"
        aria-label="Primary navigation"
      >
        <div class="sidebar-brand">
          <span class="sidebar-brand__mark">E</span>
          <span class="sidebar-brand__text">ELearning</span>
        </div>

        <nav class="sidebar-nav">
          @for (item of navItems; track item.label) {
            @if (item.visible !== false) {
              <a
                class="sidebar-nav__link"
                [routerLink]="item.routerLink"
                routerLinkActive="is-active"
                [routerLinkActiveOptions]="{
                  exact: item.routerLink === '/dashboard',
                }"
              >
                <i [class]="item.icon"></i>
                <span>{{ item.label }}</span>
              </a>
            }
          }
        </nav>
      </aside>

      <p-button
        class="sidebar-slider-toggle"
        [icon]="isSidebarCollapsed() ? 'pi pi-angle-right' : 'pi pi-angle-left'"
        severity="secondary"
        [rounded]="true"
        [text]="true"
        [ariaLabel]="isSidebarCollapsed() ? 'Show sidebar' : 'Hide sidebar'"
        (onClick)="toggleSidebar()"
      />

      <main class="app-workspace">
        <header class="app-topbar">
          <form class="global-search" (ngSubmit)="submitGlobalSearch()">
            <i class="pi pi-search"></i>
            <input
              pInputText
              name="globalSearch"
              [(ngModel)]="globalSearch"
              placeholder="Search courses"
              aria-label="Search courses"
            />
          </form>
          <a
            class="notification-button"
            routerLink="/notifications"
            aria-label="Notifications"
          >
            <i class="pi pi-bell"></i>
            @if (unreadCount() > 0) {
              <span>{{ unreadCount() > 99 ? '99+' : unreadCount() }}</span>
            }
          </a>
          <div class="account-pill">
            <i class="pi pi-user"></i>
            <span>{{ auth.user()?.fullName ?? auth.user()?.email }}</span>
          </div>
          <p-button
            label="Sign out"
            icon="pi pi-sign-out"
            severity="secondary"
            [text]="true"
            (onClick)="signOut()"
          />
        </header>

        @if (loading.isLoading() > 0) {
          <p-progressBar mode="indeterminate" [style]="{ height: '3px' }" />
        }

        <div class="layout-main">
          <router-outlet />
        </div>
      </main>
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
  readonly isSidebarCollapsed = signal(false);
  globalSearch = '';

  readonly navItems: MenuItem[] = [
    { label: 'Dashboard', icon: 'pi pi-home', routerLink: '/dashboard' },
    { label: 'Profile', icon: 'pi pi-user', routerLink: '/profile' },
    {
      label: 'Organizations',
      icon: 'pi pi-building',
      routerLink: '/organizations',
    },
    { label: 'Courses', icon: 'pi pi-book', routerLink: '/courses' },
    {
      label: 'Classes',
      icon: 'pi pi-calendar',
      routerLink: '/training-classes',
    },
    { label: 'My orders', icon: 'pi pi-shopping-bag', routerLink: '/orders' },
    {
      label: 'Campaigns',
      icon: 'pi pi-ticket',
      routerLink: '/campaigns',
      visible: this.isAdmin(),
    },
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

  toggleSidebar(): void {
    this.isSidebarCollapsed.update((current) => !current);
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
