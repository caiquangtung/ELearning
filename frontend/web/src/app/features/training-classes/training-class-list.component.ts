import { Component, inject, OnInit, signal } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { RouterLink } from '@angular/router';
import { DialogModule } from 'primeng/dialog';
import { DropdownModule } from 'primeng/dropdown';
import { InputNumber } from 'primeng/inputnumber';
import { InputText } from 'primeng/inputtext';
import { PaginatorState } from 'primeng/paginator';
import { PrimeTemplate } from 'primeng/api';
import { Skeleton } from 'primeng/skeleton';
import { Tag } from 'primeng/tag';
import {
  LmsApiService,
  CourseListItemDto,
  TrainingClassListItemDto,
} from '../../core/api/lms-api.service';
import { AuthService } from '../../core/auth/auth.service';
import { GlobalErrorService } from '../../core/error/global-error.service';
import { PagedList } from '../../core/models/paged-list.model';
import {
  PageShellComponent,
  UiButtonComponent,
  UiDataTableBodyTemplateDirective,
  UiDataTableComponent,
  UiDataTableHeaderTemplateDirective,
} from '../../shared/ui';

@Component({
  selector: 'app-training-class-list',
  standalone: true,
  imports: [
    FormsModule,
    RouterLink,
    DialogModule,
    InputText,
    DropdownModule,
    InputNumber,
    Tag,
    PrimeTemplate,
    Skeleton,
    PageShellComponent,
    UiButtonComponent,
    UiDataTableComponent,
    UiDataTableHeaderTemplateDirective,
    UiDataTableBodyTemplateDirective,
  ],
  styleUrl: './training-class-list.component.scss',
  templateUrl: './training-class-list.component.html',
})
export class TrainingClassListComponent implements OnInit {
  private readonly api = inject(LmsApiService);
  private readonly errors = inject(GlobalErrorService);
  readonly auth = inject(AuthService);

  readonly page = signal<PagedList<TrainingClassListItemDto> | null>(null);
  readonly publishedCourses = signal<CourseListItemDto[]>([]);
  readonly loading = signal(true);
  readonly creating = signal(false);
  readonly skeletonRows = Array.from({ length: 6 });

  createVisible = false;
  searchDialogVisible = false;
  search = '';
  createCourseId: string | null = null;
  createTitle = '';
  createMax = 30;

  private pageNum = 1;
  readonly pageSize = 20;

  canCreate(): boolean {
    const roles = this.auth.user()?.roles ?? [];
    return roles.some(
      (r) => r === 'Admin' || r === 'OrgAdmin' || r === 'Instructor',
    );
  }

  ngOnInit(): void {
    this.errors.clear();
    if (this.canCreate()) {
      this.api
        .listCourses({ page: 1, pageSize: 100, status: 'Published' })
        .subscribe({
          next: (p) => this.publishedCourses.set(p.items),
        });
    }
    this.reload();
  }

  openCreateDialog(): void {
    this.createCourseId = null;
    this.createTitle = '';
    this.createMax = 30;
    this.createVisible = true;
  }

  closeCreateDialog(): void {
    if (this.creating()) return;
    this.createVisible = false;
  }

  applyFilters(): void {
    this.pageNum = 1;
    this.reload();
  }

  onPageChange(event: PaginatorState): void {
    const first = event.first ?? 0;
    const rows = event.rows ?? this.pageSize;
    this.pageNum = Math.floor(first / rows) + 1;
    this.reload();
  }

  reload(): void {
    this.loading.set(true);
    this.api
      .listTrainingClasses({
        page: this.pageNum,
        pageSize: this.pageSize,
        search: this.search || null,
      })
      .subscribe({
        next: (p) => {
          this.page.set(p);
          this.loading.set(false);
        },
        error: () => this.loading.set(false),
      });
  }

  createClass(): void {
    if (!this.createCourseId || !this.createTitle.trim()) return;
    this.errors.clear();
    this.creating.set(true);
    this.api
      .createTrainingClass({
        courseId: this.createCourseId,
        title: this.createTitle.trim(),
        maxLearners: Math.max(1, this.createMax || 1),
      })
      .subscribe({
        next: () => {
          this.createTitle = '';
          this.createCourseId = null;
          this.createVisible = false;
          this.creating.set(false);
          this.reload();
        },
        error: () => this.creating.set(false),
      });
  }
}
