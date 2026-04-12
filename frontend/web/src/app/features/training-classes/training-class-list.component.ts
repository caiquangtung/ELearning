import { Component, inject, OnInit, signal } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { RouterLink } from '@angular/router';
import { Button } from 'primeng/button';
import { DropdownModule } from 'primeng/dropdown';
import { InputNumber } from 'primeng/inputnumber';
import { InputText } from 'primeng/inputtext';
import { Paginator, PaginatorState } from 'primeng/paginator';
import { Panel } from 'primeng/panel';
import { PrimeTemplate } from 'primeng/api';
import { TableModule } from 'primeng/table';
import { Tag } from 'primeng/tag';
import { Toolbar } from 'primeng/toolbar';
import { LmsApiService, CourseListItemDto, TrainingClassListItemDto } from '../../core/api/lms-api.service';
import { AuthService } from '../../core/auth/auth.service';
import { GlobalErrorService } from '../../core/error/global-error.service';
import { PagedList } from '../../core/models/paged-list.model';

@Component({
  selector: 'app-training-class-list',
  standalone: true,
  imports: [
    FormsModule,
    RouterLink,
    TableModule,
    Toolbar,
    Button,
    InputText,
    DropdownModule,
    InputNumber,
    Paginator,
    Panel,
    Tag,
    PrimeTemplate,
  ],
  template: `
    <h1 class="text-2xl font-semibold mt-0">Training classes</h1>
    @if (canCreate()) {
      <p-panel header="Create class" [toggleable]="true" styleClass="mb-4">
        <p class="text-sm text-color-secondary mt-0">Course must be <strong>Published</strong>.</p>
        <div class="flex flex-column md:flex-row flex-wrap gap-3 align-items-end">
          <p-dropdown
            [options]="publishedCourses()"
            [(ngModel)]="createCourseId"
            optionLabel="title"
            optionValue="id"
            placeholder="Select course"
            [filter]="true"
            filterPlaceholder="Search"
            styleClass="w-full md:w-20rem"
            name="ccourse"
          />
          <input pInputText [(ngModel)]="createTitle" placeholder="Title" class="w-full md:w-20rem" name="ctitle" />
          <p-inputNumber [(ngModel)]="createMax" [min]="1" placeholder="Max learners" name="cmax" />
          <p-button
            label="Create"
            icon="pi pi-plus"
            [disabled]="!createCourseId || !createTitle.trim() || creating()"
            [loading]="creating()"
            (onClick)="createClass()"
          />
        </div>
      </p-panel>
    }
    <p-toolbar styleClass="mb-3">
      <ng-template pTemplate="start">
        <input pInputText [(ngModel)]="search" placeholder="Search" class="mr-2" name="tsearch" />
        <p-button label="Apply" icon="pi pi-filter" (onClick)="applyFilters()" />
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
              <th>Max learners</th>
            </tr>
          </ng-template>
          <ng-template pTemplate="body" let-t>
            <tr>
              <td>
                <a [routerLink]="['/training-classes', t.id]" class="text-primary font-medium">{{ t.title }}</a>
              </td>
              <td><p-tag [value]="t.status" severity="secondary" /></td>
              <td>{{ t.maxLearners }}</td>
            </tr>
          </ng-template>
          <ng-template pTemplate="emptymessage">
            <tr>
              <td colspan="3">No training classes.</td>
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
export class TrainingClassListComponent implements OnInit {
  private readonly api = inject(LmsApiService);
  private readonly errors = inject(GlobalErrorService);
  readonly auth = inject(AuthService);

  readonly page = signal<PagedList<TrainingClassListItemDto> | null>(null);
  readonly publishedCourses = signal<CourseListItemDto[]>([]);
  readonly loading = signal(true);
  readonly creating = signal(false);

  search = '';
  createCourseId: string | null = null;
  createTitle = '';
  createMax = 30;

  private pageNum = 1;
  readonly pageSize = 20;

  canCreate(): boolean {
    const roles = this.auth.user()?.roles ?? [];
    return roles.some((r) => r === 'Admin' || r === 'OrgAdmin' || r === 'Instructor');
  }

  ngOnInit(): void {
    this.errors.clear();
    if (this.canCreate()) {
      this.api.listCourses(1, 100, undefined, 'Published').subscribe({
        next: (p) => this.publishedCourses.set(p.items),
      });
    }
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
    this.loading.set(true);
    this.api.listTrainingClasses(this.pageNum, this.pageSize, undefined, this.search || undefined).subscribe({
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
          this.creating.set(false);
          this.reload();
        },
        error: () => this.creating.set(false),
      });
  }
}
