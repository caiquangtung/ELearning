import { DatePipe } from '@angular/common';
import { Component, computed, inject, OnInit, signal } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { ActivatedRoute, Router, RouterLink } from '@angular/router';
import { Button } from 'primeng/button';
import { Divider } from 'primeng/divider';
import { InputTextarea } from 'primeng/inputtextarea';
import { Panel } from 'primeng/panel';
import { Tag } from 'primeng/tag';
import {
  CourseDetailDto,
  CourseRatingSummaryDto,
  LmsApiService,
  ReviewEligibilityDto,
  ReviewDto,
  VideoAssetDto,
  WatchProgressDto,
} from '../../core/api/lms-api.service';
import { AuthService } from '../../core/auth/auth.service';
import { GlobalErrorService } from '../../core/error/global-error.service';

function assetTypeLabel(t: number): string {
  switch (t) {
    case 0:
      return 'Video';
    case 1:
      return 'PDF';
    case 2:
      return 'SCORM';
    default:
      return 'Other';
  }
}

@Component({
  selector: 'app-course-detail',
  standalone: true,
  imports: [
    RouterLink,
    Button,
    DatePipe,
    Panel,
    Divider,
    FormsModule,
    InputTextarea,
    Tag,
  ],
  templateUrl: './course-detail.component.html',
  styleUrl: './course-detail.component.scss',
})
export class CourseDetailComponent implements OnInit {
  private readonly api = inject(LmsApiService);
  private readonly auth = inject(AuthService);
  private readonly route = inject(ActivatedRoute);
  private readonly router = inject(Router);
  private readonly errors = inject(GlobalErrorService);

  readonly course = signal<CourseDetailDto | null>(null);
  readonly loading = signal(true);
  readonly videos = signal<Record<string, VideoAssetDto>>({});
  readonly progress = signal<Record<string, WatchProgressDto>>({});
  readonly summary = signal<CourseRatingSummaryDto | null>(null);
  readonly eligibility = signal<ReviewEligibilityDto | null>(null);
  readonly reviews = signal<ReviewDto[]>([]);
  readonly includeRejected = signal(false);
  readonly submittingReview = signal(false);
  showReviewForm = false;
  readonly isAdmin = computed(
    () =>
      this.auth.user()?.roles.some((r) => r.toLowerCase() === 'admin') ?? false,
  );
  readonly ratingOptions = [5, 4, 3, 2, 1];
  reviewRating = 5;
  reviewComment = '';
  private readonly lastTrackedAt: Record<string, number> = {};

  readonly assetTypeLabel = assetTypeLabel;

  formatPrice(cents: number, currency: string): string {
    return `${(cents / 100).toFixed(2)} ${currency}`;
  }

  stars(value: number): string {
    const rounded = Math.round(value);
    return '★★★★★'.slice(0, rounded) + '☆☆☆☆☆'.slice(0, 5 - rounded);
  }

  askAiTutor(courseId: string): void {
    this.router.navigate(['/learn/ai-chat'], {
      queryParams: { courseId },
    });
  }

  ngOnInit(): void {
    this.errors.clear();
    const id = this.route.snapshot.paramMap.get('id');
    if (!id) {
      this.loading.set(false);
      return;
    }
    this.api.getCourse(id).subscribe({
      next: (c) => {
        this.course.set(c);
        this.loading.set(false);
        this.loadLessonVideos(c);
        this.loadReviewSummary(c.id);
        this.loadReviewEligibility(c.id);
        this.loadReviews(c.id);
      },
      error: () => this.loading.set(false),
    });
  }

  uploadVideo(
    courseId: string,
    sectionId: string,
    lessonId: string,
    event: Event,
  ): void {
    const input = event.target as HTMLInputElement;
    const file = input.files?.[0];
    if (!file) return;

    this.api.uploadVideo(courseId, sectionId, lessonId, file).subscribe({
      next: (video) => {
        this.videos.update((items) => ({ ...items, [lessonId]: video }));
        input.value = '';
      },
      error: () => {
        input.value = '';
      },
    });
  }

  trackVideo(video: VideoAssetDto, event: Event): void {
    const element = event.target as HTMLVideoElement;
    if (!Number.isFinite(element.duration) || element.duration <= 0) return;

    const now = Date.now();
    const last = this.lastTrackedAt[video.id] ?? 0;
    const current = Math.floor(element.currentTime);
    const duration = Math.floor(element.duration);
    const watched = Math.min(
      duration,
      Math.max(current, this.progress()[video.id]?.watchedSeconds ?? 0),
    );

    if (now - last < 30000 && watched < Math.ceil(duration * 0.8)) return;
    this.lastTrackedAt[video.id] = now;

    this.api
      .trackVideoProgress(video.id, {
        positionSeconds: current,
        durationSeconds: duration,
        watchedSeconds: watched,
      })
      .subscribe((p) => {
        this.progress.update((items) => ({ ...items, [video.id]: p }));
        if (p.isCompleted) {
          this.lastTrackedAt[video.id] = Number.MAX_SAFE_INTEGER;
        }
      });
  }

  submitReview(courseId: string): void {
    const comment = this.reviewComment.trim();
    if (!comment) return;

    this.submittingReview.set(true);
    this.api
      .submitCourseReview(courseId, { rating: this.reviewRating, comment })
      .subscribe({
        next: (review) => {
          this.submittingReview.set(false);
          this.reviewComment = '';
          this.reviews.update((items) => [
            review,
            ...items.filter((item) => item.id !== review.id),
          ]);
          this.loadReviewSummary(courseId);
          this.loadReviews(courseId);
        },
        error: () => this.submittingReview.set(false),
      });
  }

  toggleRejected(value: boolean): void {
    this.includeRejected.set(value);
    const courseId = this.course()?.id;
    if (courseId) {
      this.loadReviews(courseId);
    }
  }

  moderateReview(courseId: string, review: ReviewDto, status: string): void {
    const reason =
      status === 'Rejected' ? 'Rejected by admin moderation.' : null;
    this.api
      .moderateReview(review.id, { status, reason })
      .subscribe((updated) => {
        this.reviews.update((items) =>
          items.map((item) => (item.id === updated.id ? updated : item)),
        );
        this.loadReviewSummary(courseId);
      });
  }

  private loadLessonVideos(course: CourseDetailDto): void {
    for (const section of course.sections) {
      for (const lesson of section.lessons) {
        this.api.getLessonVideo(lesson.id).subscribe({
          next: (video) =>
            this.videos.update((items) => ({ ...items, [lesson.id]: video })),
          error: () => undefined,
        });
      }
    }
  }

  private loadReviewSummary(courseId: string): void {
    this.api.getCourseRatingSummary(courseId).subscribe({
      next: (summary) => this.summary.set(summary),
      error: () => undefined,
    });
  }

  private loadReviewEligibility(courseId: string): void {
    this.api.getCourseReviewEligibility(courseId).subscribe({
      next: (eligibility) => this.eligibility.set(eligibility),
      error: () =>
        this.eligibility.set({
          courseId,
          canReview: false,
          reason: 'Complete the course before submitting a review.',
        }),
    });
  }

  private loadReviews(courseId: string): void {
    this.api
      .listCourseReviews(courseId, {
        page: 1,
        pageSize: 20,
        includeRejected: this.includeRejected(),
      })
      .subscribe({
        next: (paged) => this.reviews.set(paged.items),
        error: () => undefined,
      });
  }
}
