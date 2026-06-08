import { Component, inject, computed, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { Router, RouterModule } from '@angular/router';
import { AuthService } from '../../core/auth/auth.service';
import { LmsApiService, PublicFeaturedCourseDto } from '../../core/api/lms-api.service';

@Component({
  selector: 'app-landing',
  standalone: true,
  imports: [CommonModule, RouterModule],
  templateUrl: './landing.component.html',
  styleUrls: ['./landing.component.scss'],
})
export class LandingComponent {
  readonly auth = inject(AuthService);
  private readonly router = inject(Router);
  private readonly api = inject(LmsApiService);

  readonly courses = signal<PublicFeaturedCourseDto[]>([]);
  readonly isLoadingCourses = signal(true);
  readonly isAuthenticated = computed(
    () => this.auth.isAuthenticated?.() ?? false,
  );

  ngOnInit(): void {
    this.loadFeaturedCourses();
  }

  loadFeaturedCourses(limit = 6) {
    this.isLoadingCourses.set(true);
    this.api.listPublicFeaturedCourses(limit).subscribe({
        next: (data) => {
          this.courses.set(data || []);
          this.isLoadingCourses.set(false);
        },
        error: () => {
          this.courses.set([
            {
              id: 'fallback-1',
              title: 'C# .NET 8 Enterprise Architecture',
              description:
                'Deploy scale models using SOLID programming practices and Domain repositories.',
              priceCents: 4999,
              currency: 'USD',
              level: 'Intermediate',
              category: 'Backend',
              thumbnailUrl: '/assets/public/course-technology.png',
              lessonCount: 14,
              sectionCount: 4,
              durationMinutes: 420,
            },
            {
              id: 'fallback-2',
              title: 'Angular Signals Core',
              description:
                'Configure fine-grained reactivity without heavy RxJS.',
              priceCents: 2999,
              currency: 'USD',
              level: 'Beginner',
              category: 'Frontend',
              thumbnailUrl: '/assets/public/course-design.png',
              lessonCount: 10,
              sectionCount: 3,
              durationMinutes: 300,
            },
          ]);
          this.isLoadingCourses.set(false);
        },
      });
  }

  openAiPath(): void {
    const returnUrl = '/learn/ai-path';
    if (this.isAuthenticated()) {
      void this.router.navigateByUrl(returnUrl);
      return;
    }
    void this.router.navigate(['/login'], { queryParams: { returnUrl } });
  }

  startCourse(course: PublicFeaturedCourseDto): void {
    void this.router.navigate(['/catalog', course.id]);
  }

  logout() {
    this.auth.logout();
    this.router.navigate(['/']);
  }

  courseImage(course: PublicFeaturedCourseDto): string {
    return course.thumbnailUrl;
  }

  price(course: PublicFeaturedCourseDto): string {
    if (course.priceCents <= 0) return 'FREE';
    return new Intl.NumberFormat(undefined, {
      style: 'currency',
      currency: course.currency || 'USD',
    }).format(course.priceCents / 100);
  }

  duration(course: PublicFeaturedCourseDto): string {
    return `${Math.max(1, Math.round(course.durationMinutes / 60))}h`;
  }
}
