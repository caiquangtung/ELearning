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

  readonly showLoginMenu = signal(false);
  readonly showPortalMenu = signal(false);

  ngOnInit(): void {
    this.loadFeaturedCourses();
  }

  toggleLoginMenu() {
    this.showLoginMenu.update((v) => !v);
  }

  togglePortalMenu() {
    // close login menu when opening portal menu
    this.showLoginMenu.set(false);
    this.showPortalMenu.update((v) => !v);
  }

  enterPortal(_role: 'Student' | 'Instructor' | 'Admin') {
    const target =
      _role === 'Student'
        ? '/learn'
        : _role === 'Instructor'
          ? '/teach'
          : '/dashboard';
    if (this.isAuthenticated()) {
      void this.router.navigate([target]);
    } else {
      void this.router.navigate(['/login'], { queryParams: { role: _role } });
    }
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
            },
          ]);
          this.isLoadingCourses.set(false);
        },
      });
  }

  loginAs(_role: 'Student' | 'Instructor' | 'Admin') {
    // For demo: redirect to login with a role hint so developer can sign in quickly.
    void this.router.navigate(['/login'], { queryParams: { role: _role } });
  }

  logout() {
    this.auth.logout();
    this.router.navigate(['/']);
  }

  courseImage(course: PublicFeaturedCourseDto): string {
    return `https://picsum.photos/seed/${encodeURIComponent(course.id || course.title)}/640/360`;
  }

  price(course: PublicFeaturedCourseDto): string {
    if (course.priceCents <= 0) return 'FREE';
    return new Intl.NumberFormat(undefined, {
      style: 'currency',
      currency: course.currency || 'USD',
    }).format(course.priceCents / 100);
  }
}
