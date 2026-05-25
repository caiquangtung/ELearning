import { DatePipe } from '@angular/common';
import { Component, computed, inject, OnInit, signal } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { ActivatedRoute, RouterLink } from '@angular/router';
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
  imports: [RouterLink, Button, DatePipe, Panel, Divider, FormsModule, InputTextarea, Tag],
  template: `
    <p-button label="Back to courses" icon="pi pi-arrow-left" [text]="true" routerLink="/courses" styleClass="mb-3" />
    @if (loading()) {
      <p>Loading…</p>
    } @else {
      @if (course(); as c) {
        <div class="flex align-items-center gap-2 flex-wrap mb-2">
          <h1 class="text-2xl font-semibold m-0">{{ c.title }}</h1>
          <p-tag [value]="c.status" [severity]="c.status === 'Published' ? 'success' : 'warn'" />
        </div>
        @if (c.status === 'Published' && c.priceCents > 0) {
          <p class="text-600 mb-2">
            Price: <strong>{{ formatPrice(c.priceCents, c.currency) }}</strong>
          </p>
          <p-button
            label="Buy course"
            icon="pi pi-shopping-cart"
            styleClass="mb-3"
            [routerLink]="['/checkout']"
            [queryParams]="{ type: 'Course', ref: c.id, qty: 1 }"
          />
        }
        @if (c.description) {
          <p class="text-color-secondary">{{ c.description }}</p>
        }
        <section class="rating-summary">
          <div>
            <div class="stars" aria-label="Average rating">{{ stars(summary()?.averageRating ?? 0) }}</div>
            <strong>{{ (summary()?.averageRating ?? 0).toFixed(1) }}/5</strong>
            <span class="text-color-secondary">({{ summary()?.reviewCount ?? 0 }} reviews)</span>
          </div>
        </section>
        @for (s of c.sections; track s.id) {
          <p-panel [header]="s.title" styleClass="mb-3">
            @for (l of s.lessons; track l.id; let last = $last) {
              <div class="mb-3">
                <h3 class="text-lg mt-0 mb-2">{{ l.title }}</h3>
                @if (l.content) {
                  <p class="content-block">{{ l.content }}</p>
                }
                @if (l.assets.length) {
                  <ul class="pl-4 m-0">
                    @for (a of l.assets; track a.id) {
                      <li>
                        <a [href]="a.url" target="_blank" rel="noopener" class="text-primary">{{ a.fileName }}</a>
                        <span class="text-color-secondary text-sm"> ({{ assetTypeLabel(a.assetType) }})</span>
                      </li>
                    }
                  </ul>
                }
                @if (videos()[l.id]; as video) {
                  <div class="video-box">
                    <video
                      controls
                      preload="metadata"
                      [src]="video.url"
                      (timeupdate)="trackVideo(video, $event)"
                    ></video>
                    <div class="video-meta">
                      <span>{{ video.fileName }}</span>
                      @if (progress()[video.id]; as p) {
                        <strong>{{ p.progressPercent }}% watched</strong>
                      }
                    </div>
                  </div>
                }
                <div class="video-upload">
                  <input
                    type="file"
                    accept="video/*"
                    [id]="'video-' + l.id"
                    (change)="uploadVideo(c.id, s.id, l.id, $event)"
                  />
                </div>
              </div>
              @if (!last) {
                <p-divider />
              }
            }
          </p-panel>
        }
        <section class="reviews">
          <div class="reviews-header">
            <h2>Course reviews</h2>
            @if (isAdmin()) {
              <label class="review-toggle">
                <input type="checkbox" [ngModel]="includeRejected()" (ngModelChange)="toggleRejected($event)" />
                Include pending/rejected
              </label>
            }
          </div>

          @if (eligibility()?.canReview) {
            <form class="review-form" (ngSubmit)="submitReview(c.id)">
              <label for="review-rating">Your rating</label>
              <select id="review-rating" name="reviewRating" [(ngModel)]="reviewRating">
                @for (value of ratingOptions; track value) {
                  <option [ngValue]="value">{{ value }} stars</option>
                }
              </select>
              <label for="review-comment">Your review</label>
              <textarea
                id="review-comment"
                pInputTextarea
                rows="4"
                name="reviewComment"
                [(ngModel)]="reviewComment"
                maxlength="4000"
                placeholder="Share what helped you learn from this course"
              ></textarea>
              <p-button type="submit" label="Submit review" icon="pi pi-star" [disabled]="submittingReview()" />
            </form>
          } @else {
            <p class="review-locked">{{ eligibility()?.reason ?? 'Complete the course before submitting a review.' }}</p>
          }

          @if (reviews().length) {
            <div class="review-list">
              @for (review of reviews(); track review.id) {
                <article class="review-item">
                  <div class="review-topline">
                    <span class="stars">{{ stars(review.rating) }}</span>
                    <p-tag [value]="review.status" [severity]="review.status === 'Published' ? 'success' : review.status === 'Pending' ? 'warn' : 'danger'" />
                  </div>
                  <p>{{ review.comment }}</p>
                  <small class="text-color-secondary">Submitted {{ review.submittedAt | date: 'mediumDate' }}</small>
                  @if (review.moderationReason) {
                    <small class="text-color-secondary">Reason: {{ review.moderationReason }}</small>
                  }
                  @if (isAdmin()) {
                    <div class="review-actions">
                      <p-button
                        label="Publish"
                        size="small"
                        [outlined]="true"
                        [disabled]="review.status === 'Published'"
                        (onClick)="moderateReview(c.id, review, 'Published')"
                      />
                      <p-button
                        label="Reject"
                        size="small"
                        severity="danger"
                        [outlined]="true"
                        [disabled]="review.status === 'Rejected'"
                        (onClick)="moderateReview(c.id, review, 'Rejected')"
                      />
                    </div>
                  }
                </article>
              }
            </div>
          } @else {
            <p class="text-color-secondary">No reviews yet.</p>
          }
        </section>
      }
    }
  `,
  styleUrl: './course-detail.component.scss',
})
export class CourseDetailComponent implements OnInit {
  private readonly api = inject(LmsApiService);
  private readonly auth = inject(AuthService);
  private readonly route = inject(ActivatedRoute);
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
  readonly isAdmin = computed(() => this.auth.user()?.roles.some((r) => r.toLowerCase() === 'admin') ?? false);
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

  uploadVideo(courseId: string, sectionId: string, lessonId: string, event: Event): void {
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
    const watched = Math.min(duration, Math.max(current, this.progress()[video.id]?.watchedSeconds ?? 0));

    if (now - last < 30000 && watched < Math.ceil(duration * 0.8)) return;
    this.lastTrackedAt[video.id] = now;

    this.api.trackVideoProgress(video.id, {
      positionSeconds: current,
      durationSeconds: duration,
      watchedSeconds: watched,
    }).subscribe((p) => {
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
    this.api.submitCourseReview(courseId, { rating: this.reviewRating, comment }).subscribe({
      next: (review) => {
        this.submittingReview.set(false);
        this.reviewComment = '';
        this.reviews.update((items) => [review, ...items.filter((item) => item.id !== review.id)]);
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
    const reason = status === 'Rejected' ? 'Rejected by admin moderation.' : null;
    this.api.moderateReview(review.id, { status, reason }).subscribe((updated) => {
      this.reviews.update((items) => items.map((item) => (item.id === updated.id ? updated : item)));
      this.loadReviewSummary(courseId);
    });
  }

  private loadLessonVideos(course: CourseDetailDto): void {
    for (const section of course.sections) {
      for (const lesson of section.lessons) {
        this.api.getLessonVideo(lesson.id).subscribe({
          next: (video) => this.videos.update((items) => ({ ...items, [lesson.id]: video })),
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
      error: () => this.eligibility.set({
        courseId,
        canReview: false,
        reason: 'Complete the course before submitting a review.',
      }),
    });
  }

  private loadReviews(courseId: string): void {
    this.api.listCourseReviews(courseId, {
      page: 1,
      pageSize: 20,
      includeRejected: this.includeRejected(),
    }).subscribe({
      next: (paged) => this.reviews.set(paged.items),
      error: () => undefined,
    });
  }
}
