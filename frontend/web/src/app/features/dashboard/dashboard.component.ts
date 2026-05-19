import { Component, computed, inject, signal } from '@angular/core';
import { Button } from 'primeng/button';
import { Card } from 'primeng/card';
import { AdminDashboardDto, InstructorDashboardDto, LmsApiService, StudentDashboardDto } from '../../core/api/lms-api.service';
import { AuthService } from '../../core/auth/auth.service';

@Component({
  selector: 'app-dashboard',
  standalone: true,
  imports: [Button, Card],
  template: `
    <section class="dashboard">
      <div class="dashboard-header">
        <div>
          <p class="eyebrow">Dashboard</p>
          <h1>{{ greeting() }}</h1>
          <p>{{ subtitle() }}</p>
        </div>
        <p-button icon="pi pi-refresh" label="Refresh" severity="secondary" [outlined]="true" (onClick)="load()" />
      </div>

      @if (isAdmin() && admin(); as stats) {
        <div class="metric-grid">
          <p-card styleClass="metric-card strong">
            <span>Revenue</span>
            <strong>{{ money(stats.revenueCents, stats.currency) }}</strong>
            <small>{{ stats.paidOrders }} paid orders</small>
          </p-card>
          <p-card styleClass="metric-card">
            <span>Users</span>
            <strong>{{ stats.totalUsers }}</strong>
            <small>{{ stats.activeUsers }} active</small>
          </p-card>
          <p-card styleClass="metric-card">
            <span>Courses</span>
            <strong>{{ stats.totalCourses }}</strong>
            <small>{{ stats.publishedCourses }} published</small>
          </p-card>
          <p-card styleClass="metric-card">
            <span>Classes</span>
            <strong>{{ stats.totalClasses }}</strong>
            <small>{{ stats.scheduledClasses }} scheduled</small>
          </p-card>
          <p-card styleClass="metric-card">
            <span>Certificates</span>
            <strong>{{ stats.certificatesIssued }}</strong>
            <small>Issued completions</small>
          </p-card>
          <p-card styleClass="metric-card">
            <span>Checkout</span>
            <strong>{{ stats.pendingOrders }}</strong>
            <small>Pending payment</small>
          </p-card>
        </div>
      } @else {
        <div class="metric-grid">
          @if (student(); as stats) {
            <p-card styleClass="metric-card strong">
              <span>Learning orders</span>
              <strong>{{ stats.paidOrders }}</strong>
              <small>{{ stats.coursePurchases }} courses, {{ stats.classPurchases }} classes</small>
            </p-card>
            <p-card styleClass="metric-card">
              <span>Upcoming sessions</span>
              <strong>{{ stats.upcomingSessions }}</strong>
              <small>From purchased classes</small>
            </p-card>
            <p-card styleClass="metric-card">
              <span>Certificates</span>
              <strong>{{ stats.certificatesIssued }}</strong>
              <small>Issued to you</small>
            </p-card>
          }

          @if (isInstructor() && instructor(); as stats) {
            <p-card styleClass="metric-card">
              <span>Assigned classes</span>
              <strong>{{ stats.assignedClasses }}</strong>
              <small>{{ stats.scheduledClasses }} scheduled</small>
            </p-card>
            <p-card styleClass="metric-card">
              <span>Upcoming teaching</span>
              <strong>{{ stats.upcomingSessions }}</strong>
              <small>{{ stats.completedSessions }} past sessions</small>
            </p-card>
            <p-card styleClass="metric-card">
              <span>Draft classes</span>
              <strong>{{ stats.draftClasses }}</strong>
              <small>Need scheduling</small>
            </p-card>
          }
        </div>
      }

      @if (error()) {
        <p-card styleClass="notice-card">
          <p>{{ error() }}</p>
        </p-card>
      }
    </section>
  `,
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
  readonly greeting = computed(() => `Welcome, ${this.auth.user()?.fullName ?? 'learner'}`);
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
