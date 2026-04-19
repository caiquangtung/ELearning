import { DatePipe } from '@angular/common';
import { Component, inject, OnInit, signal } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { RouterLink } from '@angular/router';
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
          <app-ui-button label="Apply" icon="pi pi-filter" class="ml-2" (clicked)="applyFilters()" />
        </ng-template>
      </p-toolbar>

      @if (page(); as p) {
        <app-ui-data-table
          [value]="p.items"
          [rows]="pageSize"
          [totalRecords]="p.totalCount"
          [first]="(p.page - 1) * pageSize"
          [emptyColspan]="3"
          (pageChange)="onPageChange($event)"
        >
          <ng-template uiDataTableHeader>
            <tr>
              <th>Title</th>
              <th>Status</th>
              <th>Created</th>
            </tr>
          </ng-template>

          <ng-template uiDataTableBody let-c>
            <tr>
              <td>
                <a [routerLink]="['/courses', c.id]" class="text-primary font-medium">{{ c.title }}</a>
              </td>
              <td><p-tag [value]="c.status" [severity]="c.status === 'Published' ? 'success' : 'warn'" /></td>
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
  private readonly errors = inject(GlobalErrorService);
  readonly auth = inject(AuthService);

  readonly page = signal<PagedList<CourseListItemDto> | null>(null);
  readonly loading = signal(true);
  readonly creating = signal(false);

  search = '';
  status = '';
  newTitle = '';
  newDescription = '';

  readonly pageSize = 20;
  private pageNum = 1;

  readonly statusOptions = [
    { label: 'Any', value: '' },
    { label: 'Draft', value: 'Draft' },
    { label: 'Published', value: 'Published' },
  ];

  canCreate(): boolean {
    const roles = this.auth.user()?.roles ?? [];
    return roles.some((r) => r === 'Admin' || r === 'Instructor');
  }

  ngOnInit(): void {
    this.reload();
  }

  applyFilters(): void {
    this.pageNum = 1;
    this.reload();
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
      .listCourses(this.pageNum, this.pageSize, this.search, this.status || undefined)
      .subscribe({
        next: (p) => {
          this.page.set(p);
          this.loading.set(false);
        },
        error: () => this.loading.set(false),
      });
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
}
