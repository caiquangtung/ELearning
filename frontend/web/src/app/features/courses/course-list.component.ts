import { DatePipe } from '@angular/common';
import { Component, inject, OnInit, signal } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { ActivatedRoute, Router, RouterLink } from '@angular/router';
import { DropdownModule } from 'primeng/dropdown';
import { InputText } from 'primeng/inputtext';
import { InputTextarea } from 'primeng/inputtextarea';
import { Panel } from 'primeng/panel';
import { PrimeTemplate } from 'primeng/api';
import { Tag } from 'primeng/tag';
import { Toolbar } from 'primeng/toolbar';
import { LmsApiService, CourseListItemDto } from '../../core/api/lms-api.service';
import { AuthService } from '../../core/auth/auth.service';
import { PagedList } from '../../core/models/paged-list.model';
import { GlobalErrorService } from '../../core/error/global-error.service';
import { PageShellComponent } from '../../shared/ui/page-shell/page-shell.component';
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
    FormsModule,
    RouterLink,
    Toolbar,
    InputText,
    InputTextarea,
    DropdownModule,
    Panel,
    Tag,
    PrimeTemplate,
    PageShellComponent,
    UiButtonComponent,
    UiDataTableComponent,
    UiDataTableHeaderTemplateDirective,
    UiDataTableBodyTemplateDirective,
  ],
  template: `
    <app-page-shell title="Courses">
      <ng-container pageActions>
        <app-ui-button
          label="Refresh"
          icon="pi pi-refresh"
          severity="secondary"
          [text]="true"
          (clicked)="reload()"
        />
      </ng-container>

      @if (canCreate()) {
        <p-panel header="New course" [toggleable]="true" styleClass="mb-4">
          <div class="flex flex-column gap-3" style="max-width: 36rem">
            <input pInputText [(ngModel)]="newTitle" placeholder="Title" class="w-full" name="ctitle" />
            <textarea
              pInputTextarea
              [(ngModel)]="newDescription"
              rows="3"
              placeholder="Description"
              class="w-full"
              name="cdesc"
            ></textarea>
            <app-ui-button
              label="Create draft"
              icon="pi pi-plus"
              [disabled]="!newTitle.trim() || creating()"
              [loading]="creating()"
              (clicked)="createCourse()"
            />
          </div>
        </p-panel>
      }

      <p-toolbar styleClass="mb-3">
        <ng-template pTemplate="start">
          <input pInputText [(ngModel)]="search" placeholder="Search" class="mr-2" name="csearch" />
          <p-dropdown
            [options]="statusOptions"
            [(ngModel)]="status"
            optionLabel="label"
            optionValue="value"
            placeholder="Status"
            [showClear]="true"
            styleClass="w-12rem"
            name="cstatus"
          />
          <input
            pInputText
            type="number"
            min="0"
            [(ngModel)]="minPrice"
            placeholder="Min price"
            class="ml-2 w-8rem"
            name="minPrice"
          />
          <input
            pInputText
            type="number"
            min="0"
            [(ngModel)]="maxPrice"
            placeholder="Max price"
            class="ml-2 w-8rem"
            name="maxPrice"
          />
          <p-dropdown
            [options]="sortOptions"
            [(ngModel)]="sort"
            optionLabel="label"
            optionValue="value"
            styleClass="ml-2 w-12rem"
            name="csort"
          />
          <app-ui-button label="Apply" icon="pi pi-filter" class="ml-2" (clicked)="applyFilters()" />
        </ng-template>
      </p-toolbar>

      @if (page(); as p) {
        <app-ui-data-table
          [value]="p.items"
          [rows]="pageSize"
          [totalRecords]="p.totalCount"
          [first]="(p.page - 1) * pageSize"
          [emptyColspan]="4"
          (pageChange)="onPageChange($event)"
        >
          <ng-template uiDataTableHeader>
            <tr>
              <th>Title</th>
              <th>Status</th>
              <th>Price</th>
              <th>Created</th>
            </tr>
          </ng-template>

          <ng-template uiDataTableBody let-c>
            <tr>
              <td>
                <a [routerLink]="['/courses', c.id]" class="text-primary font-medium">{{ c.title }}</a>
              </td>
              <td><p-tag [value]="c.status" [severity]="c.status === 'Published' ? 'success' : 'warn'" /></td>
              <td>{{ formatPrice(c.priceCents, c.currency) }}</td>
              <td>{{ c.createdAt | date: 'mediumDate' }}</td>
            </tr>
          </ng-template>
        </app-ui-data-table>
      }
    </app-page-shell>
  `,
})
export class CourseListComponent implements OnInit {
  private readonly api = inject(LmsApiService);
  private readonly route = inject(ActivatedRoute);
  private readonly router = inject(Router);
  private readonly errors = inject(GlobalErrorService);
  readonly auth = inject(AuthService);

  readonly page = signal<PagedList<CourseListItemDto> | null>(null);
  readonly loading = signal(true);
  readonly creating = signal(false);

  search = '';
  status = '';
  minPrice: number | null = null;
  maxPrice: number | null = null;
  sort = 'Newest';
  newTitle = '';
  newDescription = '';

  readonly pageSize = 20;
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

  canCreate(): boolean {
    const roles = this.auth.user()?.roles ?? [];
    return roles.some((r) => r === 'Admin' || r === 'Instructor');
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
    this.pageNum = Math.floor(first / rows) + 1;
    this.reload();
  }

  reload(): void {
    this.errors.clear();
    this.loading.set(true);
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
    return value === null || value === undefined ? null : Math.round(value * 100);
  }
}
