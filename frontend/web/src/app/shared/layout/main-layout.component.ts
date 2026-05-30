import { Component, DestroyRef, inject, signal } from '@angular/core';
import { FormsModule } from '@angular/forms';
import {
  ActivatedRouteSnapshot,
  NavigationEnd,
  Router,
  RouterModule,
} from '@angular/router';
import { MenuItem } from 'primeng/api';
import { Button } from 'primeng/button';
import { InputText } from 'primeng/inputtext';
import { LmsApiService } from '../../core/api/lms-api.service';
import { AuthService } from '../../core/auth/auth.service';
import { filter } from 'rxjs';
import { takeUntilDestroyed } from '@angular/core/rxjs-interop';

type SidebarItem = {
  label: string;
  icon: string;
  routerLink: string;
  visible?: boolean;
  exact?: boolean;
};

type SidebarGroup = {
  label?: string;
  items: SidebarItem[];
};

type BreadcrumbItem = {
  label: string;
  routerLink: string;
};

@Component({
  selector: 'app-main-layout',
  standalone: true,
  imports: [FormsModule, RouterModule, Button, InputText],
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
          @for (group of sidebarGroups; track group.label ?? $index) {
            @if (group.label) {
              <div class="sidebar-nav__group-label">{{ group.label }}</div>
            }

            @for (item of group.items; track item.label) {
              @if (item.visible !== false) {
                <a
                  class="sidebar-nav__link"
                  [class.sidebar-nav__link--child]="!!group.label"
                  [routerLink]="item.routerLink"
                  routerLinkActive="is-active"
                  [routerLinkActiveOptions]="{
                    exact: item.exact ?? item.routerLink === '/dashboard',
                  }"
                >
                  <i [class]="item.icon"></i>
                  <span>{{ item.label }}</span>
                </a>
              }
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
          <div
            class="account-menu"
            [class.is-open]="isAccountMenuOpen()"
            (mouseenter)="showAccountMenu()"
            (mouseleave)="hideAccountMenu()"
          >
            <button
              type="button"
              class="account-trigger"
              aria-label="Account menu"
              [attr.aria-expanded]="isAccountMenuOpen()"
              (click)="toggleAccountMenu()"
            >
              <i class="pi pi-user"></i>
            </button>

            @if (isAccountMenuOpen()) {
              <div class="account-popover" role="menu">
                <div class="account-popover__identity">
                  <span class="account-popover__avatar">
                    <i class="pi pi-user"></i>
                  </span>
                  <div>
                    <strong>{{
                      auth.user()?.fullName ?? 'System user'
                    }}</strong>
                    <span>{{ auth.user()?.email }}</span>
                  </div>
                </div>

                <a
                  class="account-popover__item"
                  routerLink="/profile"
                  role="menuitem"
                >
                  <i class="pi pi-id-card"></i>
                  <span>Profile</span>
                </a>

                <button
                  type="button"
                  class="account-popover__item account-popover__item--danger"
                  role="menuitem"
                  (click)="signOut()"
                >
                  <i class="pi pi-sign-out"></i>
                  <span>Sign out</span>
                </button>
              </div>
            }
          </div>
        </header>

        @if (breadcrumbs().length) {
          <nav class="app-breadcrumbs" aria-label="Breadcrumb">
            @for (
              crumb of breadcrumbs();
              track crumb.routerLink;
              let last = $last
            ) {
              @if (last) {
                <span class="app-breadcrumbs__current">{{ crumb.label }}</span>
              } @else {
                <a [routerLink]="crumb.routerLink">{{ crumb.label }}</a>
                <i class="pi pi-angle-right" aria-hidden="true"></i>
              }
            }
          </nav>
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
  private readonly destroyRef = inject(DestroyRef);
  readonly auth = inject(AuthService);
  private readonly router = inject(Router);
  private readonly api = inject(LmsApiService);
  readonly unreadCount = signal(0);
  readonly isSidebarCollapsed = signal(false);
  readonly isAccountMenuOpen = signal(false);
  readonly breadcrumbs = signal<BreadcrumbItem[]>([]);
  globalSearch = '';

  readonly sidebarGroups: SidebarGroup[] = [
    {
      items: [
        {
          label: 'Dashboard',
          icon: 'pi pi-home',
          routerLink: '/dashboard',
          exact: true,
        },
      ],
    },
    {
      label: 'Learning',
      items: [
        { label: 'Courses', icon: 'pi pi-book', routerLink: '/courses' },
        {
          label: 'Training classes',
          icon: 'pi pi-calendar',
          routerLink: '/training-classes',
        },
      ],
    },
    {
      label: 'Business',
      items: [
        {
          label: 'Organizations',
          icon: 'pi pi-building',
          routerLink: '/organizations',
        },
        { label: 'Orders', icon: 'pi pi-shopping-bag', routerLink: '/orders' },
        {
          label: 'Campaigns',
          icon: 'pi pi-ticket',
          routerLink: '/campaigns',
          visible: this.isAdmin(),
        },
      ],
    },
    {
      label: 'Activity',
      items: [
        {
          label: 'Notifications',
          icon: 'pi pi-bell',
          routerLink: '/notifications',
        },
      ],
    },
  ];

  constructor() {
    this.refreshUnreadCount();
    this.updateBreadcrumbs();
    this.router.events
      .pipe(
        filter(
          (event): event is NavigationEnd => event instanceof NavigationEnd,
        ),
        takeUntilDestroyed(this.destroyRef),
      )
      .subscribe(() => this.updateBreadcrumbs());
  }

  private isAdmin(): boolean {
    const roles = this.auth.user()?.roles ?? [];
    return roles.some((r) => r === 'Admin');
  }

  signOut(): void {
    this.auth.logout();
  }

  showAccountMenu(): void {
    this.isAccountMenuOpen.set(true);
  }

  hideAccountMenu(): void {
    this.isAccountMenuOpen.set(false);
  }

  toggleAccountMenu(): void {
    this.isAccountMenuOpen.update((isOpen) => !isOpen);
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

  private updateBreadcrumbs(): void {
    const items: BreadcrumbItem[] = [
      { label: 'Dashboard', routerLink: '/dashboard' },
    ];
    this.collectBreadcrumbs(this.router.routerState.snapshot.root, '', items);
    this.breadcrumbs.set(items);
  }

  private collectBreadcrumbs(
    route: ActivatedRouteSnapshot,
    url: string,
    items: BreadcrumbItem[],
  ): void {
    const path = route.url.map((segment) => segment.path).join('/');
    const nextUrl = path ? `${url}/${path}` : url;
    const breadcrumb = route.data['breadcrumb'] as string | undefined;

    if (
      breadcrumb &&
      nextUrl &&
      items[items.length - 1]?.routerLink !== nextUrl
    ) {
      items.push({ label: breadcrumb, routerLink: nextUrl });
    }

    for (const child of route.children) {
      this.collectBreadcrumbs(child, nextUrl, items);
    }
  }

  private refreshUnreadCount(): void {
    this.api.getUnreadNotificationCount().subscribe({
      next: (result) => this.unreadCount.set(result.count),
      error: () => this.unreadCount.set(0),
    });
  }
}
