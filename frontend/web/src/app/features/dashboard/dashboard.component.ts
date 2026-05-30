import { Component, computed, inject, signal } from '@angular/core';
import { Button } from 'primeng/button';
import { Card } from 'primeng/card';
import {
  AdminDashboardDto,
  InstructorDashboardDto,
  LmsApiService,
  StudentDashboardDto,
} from '../../core/api/lms-api.service';
import { AuthService } from '../../core/auth/auth.service';
import { PageShellComponent } from '../../shared/ui';

@Component({
  selector: 'app-dashboard',
  standalone: true,
  imports: [Button, Card, PageShellComponent],
  templateUrl: './dashboard.component.html',
  styleUrl: './dashboard.component.scss',
})
export class DashboardComponent {
  readonly auth = inject(AuthService);
  private readonly api = inject(LmsApiService);

  readonly admin = signal<AdminDashboardDto | null>(null);
  readonly student = signal<StudentDashboardDto | null>(null);
  readonly instructor = signal<InstructorDashboardDto | null>(null);
  readonly error = signal<string | null>(null);

  readonly isAdmin = computed(() => this.hasRole('Admin'));
  readonly isInstructor = computed(() => this.hasRole('Instructor'));
  readonly greeting = computed(
    () => `Welcome, ${this.auth.user()?.fullName ?? 'learner'}`,
  );
  readonly subtitle = computed(() =>
    this.isAdmin()
      ? 'Platform performance, commerce, and learning activity at a glance.'
      : 'Your learning and teaching activity summary.',
  );

  constructor() {
    this.load();
  }

  load(): void {
    this.error.set(null);
    if (this.isAdmin()) {
      this.api.getAdminDashboard().subscribe({
        next: (x) => this.admin.set(x),
        error: () => this.error.set('Unable to load admin dashboard.'),
      });
      return;
    }

    this.api.getStudentDashboard().subscribe({
      next: (x) => this.student.set(x),
      error: () => this.error.set('Unable to load student dashboard.'),
    });

    if (this.isInstructor()) {
      this.api.getInstructorDashboard().subscribe({
        next: (x) => this.instructor.set(x),
        error: () => this.error.set('Unable to load instructor dashboard.'),
      });
    }
  }

  money(cents: number, currency: string): string {
    return new Intl.NumberFormat(undefined, {
      style: 'currency',
      currency: currency || 'USD',
    }).format(cents / 100);
  }

  private hasRole(role: string): boolean {
    return (this.auth.user()?.roles ?? []).some((x) => x === role);
  }
}
