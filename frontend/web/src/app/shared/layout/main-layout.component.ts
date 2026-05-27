import { Component, ElementRef, HostListener, ViewChild, inject, signal } from '@angular/core';
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
    <a class="skip-link" href="#main-content">Skip to content</a>

    <p-menubar [model]="navItems" styleClass="desktop-menubar mb-0">
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

    <header class="mobile-appbar">
      <button
        type="button"
        class="mobile-menu-button"
        aria-label="Open navigation"
        [attr.aria-expanded]="mobileMenuOpen()"
        aria-controls="mobile-navigation-panel"
        (click)="openMobileMenu()"
      >
        <i class="pi pi-bars" aria-hidden="true"></i>
      </button>
      <a class="mobile-brand" routerLink="/" aria-label="ELearning dashboard">ELearning</a>
      <a class="notification-button mobile-notification" routerLink="/notifications" aria-label="Notifications">
        <i class="pi pi-bell" aria-hidden="true"></i>
        @if (unreadCount() > 0) {
          <span>{{ unreadCount() > 99 ? '99+' : unreadCount() }}</span>
        }
      </a>
    </header>

    @if (mobileMenuOpen()) {
      <div class="mobile-nav-overlay" aria-hidden="true" (click)="closeMobileMenu()"></div>
      <aside
        #mobilePanel
        id="mobile-navigation-panel"
        class="mobile-nav-drawer"
        role="dialog"
        aria-modal="true"
        aria-labelledby="mobile-navigation-title"
        (keydown.tab)="trapMobileFocus($event)"
      >
        <div class="mobile-nav-header">
          <h2 id="mobile-navigation-title">ELearning</h2>
          <button
            #mobileCloseButton
            type="button"
            class="mobile-close-button"
            aria-label="Close navigation"
            (click)="closeMobileMenu()"
          >
            <i class="pi pi-times" aria-hidden="true"></i>
          </button>
        </div>

        <form class="mobile-search" (ngSubmit)="submitGlobalSearch(true)">
          <label class="sr-only" for="mobile-global-search">Search courses</label>
          <input
            pInputText
            id="mobile-global-search"
            name="mobileGlobalSearch"
            [(ngModel)]="globalSearch"
            placeholder="Search courses"
          />
          <p-button type="submit" icon="pi pi-search" ariaLabel="Search courses" />
        </form>

        <nav id="mobile-navigation" class="mobile-nav-list" aria-label="Primary navigation">
          @for (item of visibleNavItems(); track item.label) {
            <a
              [routerLink]="item.routerLink"
              routerLinkActive="active"
              #activeLink="routerLinkActive"
              [attr.aria-current]="activeLink.isActive ? 'page' : null"
              (click)="closeMobileMenu()"
            >
              <i [class]="item.icon" aria-hidden="true"></i>
              <span>{{ item.label }}</span>
            </a>
          }
        </nav>

        <div class="mobile-user-panel">
          <span>{{ auth.user()?.fullName ?? auth.user()?.email }}</span>
          <p-button
            label="Sign out"
            icon="pi pi-sign-out"
            severity="secondary"
            [outlined]="true"
            styleClass="w-full"
            (onClick)="signOut()"
          />
        </div>
      </aside>
    }

    @if (loading.isLoading() > 0) {
      <p-progressBar mode="indeterminate" styleClass="layout-progress" />
    }
    <main id="main-content" class="layout-main" tabindex="-1">
      <router-outlet />
    </main>
  `,
  styleUrl: './main-layout.component.scss',
})
export class MainLayoutComponent {
  @ViewChild('mobilePanel') private mobilePanel?: ElementRef<HTMLElement>;
  @ViewChild('mobileCloseButton') private mobileCloseButton?: ElementRef<HTMLButtonElement>;

  readonly auth = inject(AuthService);
  readonly loading = inject(LoadingService);
  private readonly router = inject(Router);
  private readonly api = inject(LmsApiService);
  readonly unreadCount = signal(0);
  readonly mobileMenuOpen = signal(false);
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

  @HostListener('document:keydown.escape')
  closeMenuOnEscape(): void {
    this.closeMobileMenu();
  }

  private isAdmin(): boolean {
    const roles = this.auth.user()?.roles ?? [];
    return roles.some((r) => r === 'Admin');
  }

  signOut(): void {
    this.closeMobileMenu();
    this.auth.logout();
  }

  submitGlobalSearch(closeMenu = false): void {
    const search = this.globalSearch.trim();
    if (closeMenu) this.closeMobileMenu();
    void this.router.navigate(['/courses'], {
      queryParams: search ? { search, sort: 'Newest' } : {},
    });
  }

  openMobileMenu(): void {
    this.mobileMenuOpen.set(true);
    queueMicrotask(() => this.mobileCloseButton?.nativeElement.focus());
  }

  closeMobileMenu(): void {
    this.mobileMenuOpen.set(false);
  }

  visibleNavItems(): MenuItem[] {
    return this.navItems.filter((item) => item.visible !== false);
  }

  trapMobileFocus(event: Event): void {
    const keyboardEvent = event as KeyboardEvent;
    const panel = this.mobilePanel?.nativeElement;
    if (!panel) return;

    const focusableSelector =
      'a[href], button:not([disabled]), input:not([disabled]), select:not([disabled]), textarea:not([disabled]), [tabindex]:not([tabindex="-1"])';
    const focusable = Array.from(
      panel.querySelectorAll<HTMLElement>(focusableSelector),
    ).filter((element) => element.offsetParent !== null);

    if (focusable.length === 0) return;

    const first = focusable[0];
    const last = focusable[focusable.length - 1];

    if (keyboardEvent.shiftKey && document.activeElement === first) {
      keyboardEvent.preventDefault();
      last.focus();
    } else if (!keyboardEvent.shiftKey && document.activeElement === last) {
      keyboardEvent.preventDefault();
      first.focus();
    }
  }

  private refreshUnreadCount(): void {
    this.api.getUnreadNotificationCount().subscribe({
      next: (result) => this.unreadCount.set(result.count),
      error: () => this.unreadCount.set(0),
    });
  }
}
