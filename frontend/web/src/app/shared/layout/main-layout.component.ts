import { Component, inject } from '@angular/core';
import { RouterModule } from '@angular/router';
import { MenuItem, PrimeTemplate } from 'primeng/api';
import { Button } from 'primeng/button';
import { Menubar } from 'primeng/menubar';
import { AuthService } from '../../core/auth/auth.service';

@Component({
  selector: 'app-main-layout',
  standalone: true,
  imports: [RouterModule, Menubar, Button, PrimeTemplate],
  template: `
    <p-menubar [model]="navItems" styleClass="mb-0">
      <ng-template pTemplate="start">
        <span class="font-bold text-xl mr-3">ELearning</span>
      </ng-template>
      <ng-template pTemplate="end">
        <span class="text-sm mr-3 hidden md:inline">{{ auth.user()?.fullName ?? auth.user()?.email }}</span>
        <p-button label="Sign out" icon="pi pi-sign-out" severity="secondary" [text]="true" (onClick)="signOut()" />
      </ng-template>
    </p-menubar>
    <div class="layout-main">
      <router-outlet />
    </div>
  `,
})
export class MainLayoutComponent {
  readonly auth = inject(AuthService);

  readonly navItems: MenuItem[] = [
    { label: 'Dashboard', icon: 'pi pi-home', routerLink: '/dashboard' },
    { label: 'Profile', icon: 'pi pi-user', routerLink: '/profile' },
    { label: 'Organizations', icon: 'pi pi-building', routerLink: '/organizations' },
    { label: 'Courses', icon: 'pi pi-book', routerLink: '/courses' },
    { label: 'Classes', icon: 'pi pi-calendar', routerLink: '/training-classes' },
  ];

  signOut(): void {
    this.auth.logout();
  }
}
