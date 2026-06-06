import { Component, DestroyRef, computed, inject, signal } from '@angular/core';
import { FormsModule } from '@angular/forms';
import {
  ActivatedRouteSnapshot,
  NavigationEnd,
  Router,
  RouterModule,
} from '@angular/router';
import { Button } from 'primeng/button';
import { InputText } from 'primeng/inputtext';
import { LmsApiService } from '../../core/api/lms-api.service';
import { AuthService } from '../../core/auth/auth.service';
import { Roles } from '../../core/auth/roles';
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

type PortalKey = 'learn' | 'teach' | 'admin';

type PortalOption = {
  key: PortalKey;
  label: string;
  icon: string;
  path: string;
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
          @for (group of sidebarGroups(); track group.label ?? $index) {
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
          <div class="header-portal">
            <button
              type="button"
              class="portal-menu-icon"
              aria-label="Open portal switcher"
              [attr.aria-expanded]="isPortalMenuOpen()"
              (click)="togglePortalMenu()"
            >
              <i class="pi pi-th-large"></i>
            </button>

            @if (isPortalMenuOpen()) {
              <div class="portal-menu" role="menu">
                @for (portal of availablePortals(); track portal.path) {
                  <button
                    type="button"
                    class="portal-cta"
                    [class.is-active]="currentPortal() === portal.key"
                    role="menuitem"
                    (click)="enterPortal(portal.path)"
                  >
                    <i [class]="portal.icon"></i>
                    <span>{{ portal.label }}</span>
                  </button>
                }
              </div>
            }
          </div>
          <a
            class="notification-button"
            [routerLink]="notificationLink()"
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
  readonly isPortalMenuOpen = signal(false);
  readonly breadcrumbs = signal<BreadcrumbItem[]>([]);
  globalSearch = '';

  readonly currentRole = computed(() => {
    const roles = this.auth.user()?.roles ?? [];
    if (roles.some((role) => role === Roles.Admin)) return Roles.Admin;
    if (roles.some((role) => role === Roles.Instructor))
      return Roles.Instructor;
    return Roles.Student;
  });

  readonly currentPortal = signal<PortalKey>('learn');
  readonly notificationLink = computed(() => `/${this.currentPortal()}/notifications`);
  readonly availablePortals = computed(() => {
    const role = this.currentRole();
    const portals: PortalOption[] = [
      {
        key: 'learn',
        label: 'Learner Portal',
        icon: 'pi pi-user',
        path: '/learn',
      },
    ];

    if (role === Roles.Instructor || role === Roles.Admin) {
      portals.push({
        key: 'teach',
        label: 'Teacher Portal',
        icon: 'pi pi-briefcase',
        path: '/teach',
      });
    }

    if (role === Roles.Admin) {
      portals.push({
        key: 'admin',
        label: 'Admin Portal',
        icon: 'pi pi-shield',
        path: '/admin',
      });
    }

    return portals;
  });

  readonly sidebarGroups = computed<SidebarGroup[]>(() => {
    const portal = this.currentPortal();
    const role = this.currentRole();

    const learnerGroups: SidebarGroup[] = [
      {
        items: [
          {
            label: 'Learn Home',
            icon: 'pi pi-home',
            routerLink: '/learn',
            exact: true,
          },
        ],
      },
      {
        label: 'Learning',
        items: [
          {
            label: 'My Courses',
            icon: 'pi pi-book',
            routerLink: '/learn/courses',
          },
          {
            label: 'My Classes',
            icon: 'pi pi-calendar',
            routerLink: '/learn/classes',
          },
          {
            label: 'AI Learning Path',
            icon: 'pi pi-sparkles',
            routerLink: '/learn/ai-path',
          },
          {
            label: 'AI Tutor',
            icon: 'pi pi-comments',
            routerLink: '/learn/ai-chat',
          },
          {
            label: 'Orders',
            icon: 'pi pi-shopping-bag',
            routerLink: '/learn/orders',
          },
          {
            label: 'Notifications',
            icon: 'pi pi-bell',
            routerLink: '/learn/notifications',
          },
        ],
      },
    ];

    const teacherGroups: SidebarGroup[] = [
      {
        items: [
          {
            label: 'Teach Home',
            icon: 'pi pi-home',
            routerLink: '/teach',
            exact: true,
          },
        ],
      },
      {
        label: 'Teaching',
        items: [
          {
            label: 'Classes',
            icon: 'pi pi-calendar',
            routerLink: '/teach/classes',
          },
          {
            label: 'Courses',
            icon: 'pi pi-book',
            routerLink: '/teach/courses',
          },
          {
            label: 'Quizzes',
            icon: 'pi pi-question-circle',
            routerLink: '/teach/quizzes',
          },
          {
            label: 'AI Quiz Generation',
            icon: 'pi pi-sparkles',
            routerLink: '/teach/quizzes/create',
          },
          {
            label: 'AI Grading',
            icon: 'pi pi-pen-to-square',
            routerLink: '/teach/quizzes',
          },
          {
            label: 'Notifications',
            icon: 'pi pi-bell',
            routerLink: '/teach/notifications',
          },
        ],
      },
    ];

    const adminGroups: SidebarGroup[] = [
      {
        items: [
          {
            label: 'Admin Home',
            icon: 'pi pi-home',
            routerLink: '/admin',
            exact: true,
          },
        ],
      },
      {
        label: 'Administration',
        items: [
          {
            label: 'Organizations',
            icon: 'pi pi-building',
            routerLink: '/admin/organizations',
          },
          {
            label: 'Campaigns',
            icon: 'pi pi-ticket',
            routerLink: '/admin/campaigns',
          },
          {
            label: 'License Pools',
            icon: 'pi pi-id-card',
            routerLink: '/admin/license-pools',
          },
          {
            label: 'Reports',
            icon: 'pi pi-chart-line',
            routerLink: '/admin/reports',
          },
          {
            label: 'Announcements',
            icon: 'pi pi-send',
            routerLink: '/admin/announcements',
          },
          {
            label: 'Notifications',
            icon: 'pi pi-bell',
            routerLink: '/admin/notifications',
          },
        ],
      },
    ];

    if (portal === 'admin' && role === Roles.Admin) return adminGroups;
    if (portal === 'teach' && role !== Roles.Student) return teacherGroups;
    return learnerGroups;
  });

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

  togglePortalMenu(): void {
    this.isPortalMenuOpen.update((isOpen) => !isOpen);
    this.isAccountMenuOpen.set(false);
  }

  enterPortal(path: string): void {
    this.isPortalMenuOpen.set(false);
    void this.router.navigate([path]);
  }

  toggleSidebar(): void {
    this.isSidebarCollapsed.update((current) => !current);
  }

  submitGlobalSearch(): void {
    const search = this.globalSearch.trim();
    const target =
      this.currentPortal() === 'teach'
        ? '/teach/courses'
        : this.currentPortal() === 'admin'
          ? '/admin/reports'
          : '/learn/courses';
    void this.router.navigate([target], {
      queryParams: search ? { search, sort: 'Newest' } : {},
    });
  }

  private updateBreadcrumbs(): void {
    this.currentPortal.set(this.resolvePortal(this.router.url));
    const items: BreadcrumbItem[] = [
      {
        label: this.portalLabel(),
        routerLink: `/${this.currentPortal()}`,
      },
    ];
    this.collectBreadcrumbs(this.router.routerState.snapshot.root, '', items);
    this.breadcrumbs.set(items);
  }

  private resolvePortal(url: string): PortalKey {
    if (url.startsWith('/admin')) return 'admin';
    if (url.startsWith('/teach')) return 'teach';
    return 'learn';
  }

  private portalLabel(): string {
    const portal = this.currentPortal();
    if (portal === 'admin') return 'Admin';
    if (portal === 'teach') return 'Teach';
    return 'Learn';
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
