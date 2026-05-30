import { DatePipe, PercentPipe } from '@angular/common';
import { Component, computed, inject, OnInit, signal } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { ActivatedRoute, Router, RouterLink } from '@angular/router';
import { DialogModule } from 'primeng/dialog';
import { DropdownModule } from 'primeng/dropdown';
import { InputText } from 'primeng/inputtext';
import { InputTextarea } from 'primeng/inputtextarea';
import { PrimeTemplate } from 'primeng/api';
import { Skeleton } from 'primeng/skeleton';
import { Tag } from 'primeng/tag';
import {
  LmsApiService,
  CourseListItemDto,
  LearningPathDraftDto,
  SemanticCourseSearchResultDto,
} from '../../core/api/lms-api.service';
import { AuthService } from '../../core/auth/auth.service';
import { PagedList } from '../../core/models/paged-list.model';
import { GlobalErrorService } from '../../core/error/global-error.service';
import { PageShellComponent } from '../../shared/ui';
import { UiButtonComponent } from '../../shared/ui/ui-button/ui-button.component';
import { UiDataTableComponent } from '../../shared/ui/ui-data-table/ui-data-table.component';
import {
  UiDataTableBodyTemplateDirective,
  UiDataTableHeaderTemplateDirective,
} from '../../shared/ui/ui-data-table/ui-data-table-templates.directive';

@Component({
  selector: 'app-course-list',
  standalone: true,
  imports: [
    DatePipe,
    PercentPipe,
    FormsModule,
    RouterLink,
    DialogModule,
    InputText,
    InputTextarea,
    DropdownModule,
    Skeleton,
    Tag,
    PrimeTemplate,
    PageShellComponent,
    UiButtonComponent,
    UiDataTableComponent,
    UiDataTableHeaderTemplateDirective,
    UiDataTableBodyTemplateDirective,
  ],
  styleUrl: './course-list.component.scss',
  templateUrl: './course-list.component.html',
})
export class CourseListComponent implements OnInit {
  private readonly api = inject(LmsApiService);
  private readonly route = inject(ActivatedRoute);
  private readonly router = inject(Router);
  private readonly errors = inject(GlobalErrorService);
  readonly auth = inject(AuthService);

  readonly page = signal<PagedList<CourseListItemDto> | null>(null);
  readonly semanticResults = signal<SemanticCourseSearchResultDto[] | null>(null);
  readonly learningPath = signal<LearningPathDraftDto | null>(null);
  readonly loading = signal(true);
  readonly creating = signal(false);
  readonly semanticLoading = signal(false);
  readonly generatingPath = signal(false);
  readonly summary = computed(() => {
    const items = this.page()?.items ?? [];
    const published = items.filter(
      (item) => item.status === 'Published',
    ).length;
    const averagePriceCents = items.length
      ? Math.round(
          items.reduce((total, item) => total + item.priceCents, 0) /
            items.length,
        )
      : 0;

    return {
      total: this.page()?.totalCount ?? 0,
      published,
      averagePriceCents,
    };
  });

  search = '';
  searchMode: 'Keyword' | 'Semantic' = 'Keyword';
  searchDialogVisible = false;
  learningPathVisible = false;
  createVisible = false;
  status = '';
  minPrice: number | null = null;
  maxPrice: number | null = null;
  sort = 'Newest';
  newTitle = '';
  newDescription = '';
  pathGoal = '';
  pathCurrentSkills = '';
  pathTargetRole = '';
  pathMaxCourses = 5;

  pageSize = 10;
  readonly pageSizeOptions = [5, 10, 20, 50];
  readonly skeletonRows = Array.from({ length: 8 }, (_, index) => index);
  private pageNum = 1;

  readonly statusOptions = [
    { label: 'Any', value: '' },
    { label: 'Draft', value: 'Draft' },
    { label: 'Published', value: 'Published' },
  ];

  readonly sortOptions = [
    { label: 'Newest', value: 'Newest' },
    { label: 'Oldest', value: 'Oldest' },
    { label: 'Title A-Z', value: 'TitleAsc' },
    { label: 'Title Z-A', value: 'TitleDesc' },
    { label: 'Price low-high', value: 'PriceAsc' },
    { label: 'Price high-low', value: 'PriceDesc' },
  ];

  readonly searchModeOptions = [
    { label: 'Keyword', value: 'Keyword' },
    { label: 'Semantic AI', value: 'Semantic' },
  ];

  canCreate(): boolean {
    const roles = this.auth.user()?.roles ?? [];
    return roles.some((r) => r === 'Admin' || r === 'Instructor');
  }

  openCreateDialog(): void {
    this.newTitle = '';
    this.newDescription = '';
    this.createVisible = true;
  }

  closeCreateDialog(): void {
    if (this.creating()) return;
    this.createVisible = false;
  }

  ngOnInit(): void {
    this.route.queryParamMap.subscribe((query) => {
      this.search = query.get('search') ?? '';
      this.status = query.get('status') ?? '';
      this.sort = query.get('sort') ?? 'Newest';
      this.minPrice = this.parsePriceParam(query.get('minPrice'));
      this.maxPrice = this.parsePriceParam(query.get('maxPrice'));
      this.pageNum = 1;
      this.reload();
    });
  }

  applyFilters(): void {
    this.pageNum = 1;
    if (this.searchMode === 'Semantic') {
      this.searchDialogVisible = false;
      this.semanticSearch();
      return;
    }

    this.semanticResults.set(null);
    void this.router.navigate([], {
      relativeTo: this.route,
      queryParams: {
        search: this.search.trim() || null,
        status: this.status || null,
        sort: this.sort === 'Newest' ? null : this.sort,
        minPrice: this.minPrice ?? null,
        maxPrice: this.maxPrice ?? null,
      },
      queryParamsHandling: 'merge',
    });
  }

  onPageChange(event: import('primeng/paginator').PaginatorState): void {
    const first = event.first ?? 0;
    const rows = event.rows ?? this.pageSize;
    if (rows !== this.pageSize) {
      this.pageSize = rows;
    }
    this.pageNum = Math.floor(first / rows) + 1;
    this.reload();
  }

  reload(): void {
    if (this.searchMode === 'Semantic' && this.search.trim()) {
      this.semanticSearch();
      return;
    }

    this.errors.clear();
    this.loading.set(true);
    this.semanticResults.set(null);
    this.api
      .listCourses({
        page: this.pageNum,
        pageSize: this.pageSize,
        search: this.search,
        status: this.status || null,
        minPriceCents: this.priceToCents(this.minPrice),
        maxPriceCents: this.priceToCents(this.maxPrice),
        sort: this.sort,
      })
      .subscribe({
        next: (p) => {
          this.page.set(p);
          this.loading.set(false);
        },
        error: () => this.loading.set(false),
      });
  }

  semanticSearch(): void {
    const query = this.search.trim();
    if (!query) {
      this.semanticResults.set([]);
      return;
    }

    this.errors.clear();
    this.semanticLoading.set(true);
    this.loading.set(false);
    this.api.semanticCourseSearch(query, this.pageSize).subscribe({
      next: (result) => {
        this.semanticResults.set(result.results);
        this.semanticLoading.set(false);
      },
      error: () => this.semanticLoading.set(false),
    });
  }

  openLearningPathDialog(): void {
    this.pathGoal = this.search.trim();
    this.pathCurrentSkills = '';
    this.pathTargetRole = '';
    this.pathMaxCourses = 5;
    this.learningPath.set(null);
    this.learningPathVisible = true;
  }

  generateLearningPath(): void {
    const goal = this.pathGoal.trim();
    if (!goal) return;

    this.generatingPath.set(true);
    this.errors.clear();
    this.api.generateLearningPath({
      goal,
      currentSkills: this.pathCurrentSkills.trim() || null,
      targetRole: this.pathTargetRole.trim() || null,
      organizationId: null,
      maxCourses: this.pathMaxCourses,
    }).subscribe({
      next: (draft) => {
        this.learningPath.set(draft);
        this.generatingPath.set(false);
      },
      error: () => this.generatingPath.set(false),
    });
  }

  formatPrice(cents: number, currency: string): string {
    if (cents <= 0) return 'Free';
    return new Intl.NumberFormat(undefined, {
      style: 'currency',
      currency: currency || 'USD',
    }).format(cents / 100);
  }

  createCourse(): void {
    const title = this.newTitle.trim();
    if (!title) return;
    this.errors.clear();
    this.creating.set(true);
    this.api
      .createCourse({ title, description: this.newDescription.trim() || null })
      .subscribe({
        next: () => {
          this.newTitle = '';
          this.newDescription = '';
          this.createVisible = false;
          this.creating.set(false);
          this.reload();
        },
        error: () => this.creating.set(false),
      });
  }

  private parsePriceParam(value: string | null): number | null {
    if (!value) return null;
    const parsed = Number(value);
    return Number.isFinite(parsed) && parsed >= 0 ? parsed : null;
  }

  private priceToCents(value: number | null): number | null {
    return value === null || value === undefined
      ? null
      : Math.round(value * 100);
  }
}
