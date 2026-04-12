import { Component, inject, OnInit, signal } from '@angular/core';
import { ActivatedRoute, RouterLink } from '@angular/router';
import { Button } from 'primeng/button';
import { Divider } from 'primeng/divider';
import { Panel } from 'primeng/panel';
import { Tag } from 'primeng/tag';
import { CourseDetailDto, LmsApiService } from '../../core/api/lms-api.service';
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
  styles: `
    .content-block {
      white-space: pre-wrap;
      font-size: 0.9rem;
    }
  `,
})
export class CourseDetailComponent implements OnInit {
  private readonly api = inject(LmsApiService);
  private readonly route = inject(ActivatedRoute);
  private readonly errors = inject(GlobalErrorService);

  readonly course = signal<CourseDetailDto | null>(null);
  readonly loading = signal(true);

  readonly assetTypeLabel = assetTypeLabel;

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
      },
      error: () => this.loading.set(false),
    });
  }
}
