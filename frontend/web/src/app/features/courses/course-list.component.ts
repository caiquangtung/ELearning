import { DatePipe } from '@angular/common';
import { Component, inject, OnInit, signal } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { RouterLink } from '@angular/router';
import { Button } from 'primeng/button';
import { DropdownModule } from 'primeng/dropdown';
import { InputText } from 'primeng/inputtext';
import { InputTextarea } from 'primeng/inputtextarea';
import { Paginator, PaginatorState } from 'primeng/paginator';
import { Panel } from 'primeng/panel';
import { PrimeTemplate } from 'primeng/api';
import { TableModule } from 'primeng/table';
import { Tag } from 'primeng/tag';
import { Toolbar } from 'primeng/toolbar';
import { LmsApiService, CourseListItemDto } from '../../core/api/lms-api.service';
import { AuthService } from '../../core/auth/auth.service';
import { PagedList } from '../../core/models/paged-list.model';
import { GlobalErrorService } from '../../core/error/global-error.service';

@Component({
  selector: 'app-course-list',
  standalone: true,
  imports: [
    DatePipe,
    FormsModule,
    RouterLink,
    TableModule,
    Toolbar,
    Button,
    InputText,
    InputTextarea,
    DropdownModule,
    Paginator,
    Panel,
    Tag,
    PrimeTemplate,
  ],
  template: `
    <h1 class="text-2xl font-semibold mt-0">Courses</h1>
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
          <p-button
            label="Create draft"
            icon="pi pi-plus"
            [disabled]="!newTitle.trim() || creating()"
            [loading]="creating()"
            (onClick)="createCourse()"
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
        <p-button label="Apply" icon="pi pi-filter" class="ml-2" (onClick)="applyFilters()" />
      </ng-template>
    </p-toolbar>
    @if (loading()) {
      <p>Loading…</p>
    } @else {
      @if (page(); as p) {
        <p-table [value]="p.items" styleClass="p-datatable-sm" [tableStyle]="{ 'min-width': '40rem' }">
          <ng-template pTemplate="header">
            <tr>
              <th>Title</th>
              <th>Status</th>
              <th>Created</th>
            </tr>
          </ng-template>
          <ng-template pTemplate="body" let-c>
            <tr>
              <td>
                <a [routerLink]="['/courses', c.id]" class="text-primary font-medium">{{ c.title }}</a>
              </td>
              <td><p-tag [value]="c.status" [severity]="c.status === 'Published' ? 'success' : 'warn'" /></td>
              <td>{{ c.createdAt | date: 'mediumDate' }}</td>
            </tr>
          </ng-template>
          <ng-template pTemplate="emptymessage">
            <tr>
              <td colspan="3">No courses.</td>
            </tr>
          </ng-template>
        </p-table>
        <p-paginator
          [rows]="pageSize"
          [totalRecords]="p.totalCount"
          [first]="(p.page - 1) * pageSize"
          (onPageChange)="onPageChange($event)"
          [showCurrentPageReport]="true"
          currentPageReportTemplate="{first}–{last} of {totalRecords}"
        />
      }
    }
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

  onPageChange(event: PaginatorState): void {
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
