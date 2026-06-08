import { CommonModule } from '@angular/common';
import { Component, inject, signal } from '@angular/core';
import { ActivatedRoute, Router, RouterModule } from '@angular/router';
import {
  LmsApiService,
  PublicCourseDetailDto,
} from '../../core/api/lms-api.service';
import { AuthService } from '../../core/auth/auth.service';

@Component({
  selector: 'app-public-course-detail',
  standalone: true,
  imports: [CommonModule, RouterModule],
  templateUrl: './public-course-detail.component.html',
  styleUrl: './public-course-detail.component.scss',
})
export class PublicCourseDetailComponent {
  private readonly api = inject(LmsApiService);
  private readonly route = inject(ActivatedRoute);
  private readonly router = inject(Router);
  readonly auth = inject(AuthService);

  readonly course = signal<PublicCourseDetailDto | null>(null);
  readonly loading = signal(true);
  readonly error = signal<string | null>(null);

  constructor() {
    this.route.paramMap.subscribe((params) => {
      const id = params.get('id');
      if (!id) {
        this.error.set('Course not found.');
        this.loading.set(false);
        return;
      }
      this.load(id);
    });
  }

  primaryLabel(course: PublicCourseDetailDto): string {
    return course.priceCents <= 0 ? 'Start free' : 'Buy course';
  }

  priceLabel(course: PublicCourseDetailDto): string {
    if (course.priceCents <= 0) return 'Free';
    return new Intl.NumberFormat(undefined, {
      style: 'currency',
      currency: course.currency || 'USD',
    }).format(course.priceCents / 100);
  }

  durationLabel(minutes: number): string {
    const hours = Math.max(1, Math.round(minutes / 60));
    return `${hours} hours`;
  }

  stars(value: number): string {
    const rounded = Math.round(value);
    return '★★★★★'.slice(0, rounded).padEnd(5, '☆');
  }

  continuePrimary(course: PublicCourseDetailDto): void {
    const returnUrl = course.priceCents <= 0
      ? `/learn/courses/${course.id}`
      : `/learn/checkout?type=Course&ref=${course.id}&qty=1`;

    if (this.auth.isAuthenticated?.()) {
      void this.router.navigateByUrl(returnUrl);
      return;
    }

    void this.router.navigate(['/login'], { queryParams: { returnUrl } });
  }

  registerForCourse(course: PublicCourseDetailDto): void {
    const returnUrl = course.priceCents <= 0
      ? `/learn/courses/${course.id}`
      : `/learn/checkout?type=Course&ref=${course.id}&qty=1`;
    void this.router.navigate(['/register'], { queryParams: { returnUrl } });
  }

  private load(id: string): void {
    this.loading.set(true);
    this.error.set(null);
    this.api.getPublicCourse(id).subscribe({
      next: (course) => {
        this.course.set(course);
        this.loading.set(false);
      },
      error: () => {
        this.error.set('This course is not available in the public catalog.');
        this.course.set(null);
        this.loading.set(false);
      },
    });
  }
}
