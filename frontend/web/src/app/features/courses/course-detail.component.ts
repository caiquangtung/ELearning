import { Component, inject, OnInit, signal } from '@angular/core';
import { ActivatedRoute, RouterLink } from '@angular/router';
import { Button } from 'primeng/button';
import { Divider } from 'primeng/divider';
import { Panel } from 'primeng/panel';
import { Tag } from 'primeng/tag';
import { CourseDetailDto, LmsApiService, VideoAssetDto, WatchProgressDto } from '../../core/api/lms-api.service';
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
  imports: [RouterLink, Button, Panel, Divider, Tag],
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
      }
    }
  `,
  styleUrl: './course-detail.component.scss',
})
export class CourseDetailComponent implements OnInit {
  private readonly api = inject(LmsApiService);
  private readonly route = inject(ActivatedRoute);
  private readonly errors = inject(GlobalErrorService);

  readonly course = signal<CourseDetailDto | null>(null);
  readonly loading = signal(true);
  readonly videos = signal<Record<string, VideoAssetDto>>({});
  readonly progress = signal<Record<string, WatchProgressDto>>({});
  private readonly lastTrackedAt: Record<string, number> = {};

  readonly assetTypeLabel = assetTypeLabel;

  formatPrice(cents: number, currency: string): string {
    return `${(cents / 100).toFixed(2)} ${currency}`;
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
}
