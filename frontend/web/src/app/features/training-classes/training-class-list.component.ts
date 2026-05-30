import { Component, inject, OnInit, signal } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { RouterLink } from '@angular/router';
import { Button } from 'primeng/button';
import { DialogModule } from 'primeng/dialog';
import { DropdownModule } from 'primeng/dropdown';
import { InputNumber } from 'primeng/inputnumber';
import { InputText } from 'primeng/inputtext';
import { Paginator, PaginatorState } from 'primeng/paginator';
import { PrimeTemplate } from 'primeng/api';
import { TableModule } from 'primeng/table';
import { Tag } from 'primeng/tag';
import {
  LmsApiService,
  CourseListItemDto,
  TrainingClassListItemDto,
} from '../../core/api/lms-api.service';
import { AuthService } from '../../core/auth/auth.service';
import { GlobalErrorService } from '../../core/error/global-error.service';
import { PagedList } from '../../core/models/paged-list.model';
import { PageShellComponent } from '../../shared/ui';

@Component({
  selector: 'app-training-class-list',
  standalone: true,
  imports: [
    FormsModule,
    RouterLink,
    TableModule,
    Button,
    DialogModule,
    InputText,
    DropdownModule,
    InputNumber,
    Paginator,
    Tag,
    PrimeTemplate,
    PageShellComponent,
  ],
  styleUrl: './training-class-list.component.scss',
  template: `
    <app-page-shell
      title="Training classes"
      subtitle="Create cohorts and manage scheduled learning sessions."
    >
      <ng-container pageActions>
        <p-button
          icon="pi pi-search"
          severity="secondary"
          [text]="true"
          (onClick)="searchDialogVisible = true"
        />
        @if (canCreate()) {
          <p-button
            label="New class"
            icon="pi pi-plus"
            (onClick)="openCreateDialog()"
          />
        }
      </ng-container>

      <p-dialog
        header="Search training classes"
        [(visible)]="searchDialogVisible"
        appendTo="body"
        [modal]="true"
        [draggable]="false"
        [resizable]="false"
        styleClass="app-dialog app-dialog--sm"
        [style]="{ width: 'min(32rem, calc(100vw - 2rem))' }"
        (onHide)="searchDialogVisible = false"
      >
        <div class="field-block">
          <label for="training-class-search">Search</label>
          <input
            id="training-class-search"
            pInputText
            [(ngModel)]="search"
            placeholder="Search"
            name="tsearchPopup"
          />
        </div>

        <ng-template pTemplate="footer">
          <p-button
            label="Cancel"
            severity="secondary"
            [text]="true"
            (onClick)="searchDialogVisible = false"
          />
          <p-button
            label="Apply"
            icon="pi pi-filter"
            (onClick)="applyFilters(); searchDialogVisible = false"
          />
        </ng-template>
      </p-dialog>

      @if (canCreate()) {
        <p-dialog
          header="New training class"
          [(visible)]="createVisible"
          appendTo="body"
          [modal]="true"
          [draggable]="false"
          [resizable]="false"
          styleClass="app-dialog app-dialog--md"
          [style]="{ width: 'min(40rem, calc(100vw - 2rem))' }"
          (onHide)="closeCreateDialog()"
        >
          <form class="app-form" (ngSubmit)="createClass()">
            <p class="text-sm text-color-secondary mt-0 mb-0">
              Course must be <strong>Published</strong>.
            </p>
            <div class="field-block">
              <label for="class-course">Course</label>
              <p-dropdown
                inputId="class-course"
                [options]="publishedCourses()"
                [(ngModel)]="createCourseId"
                optionLabel="title"
                optionValue="id"
                placeholder="Select course"
                [filter]="true"
                filterPlaceholder="Search"
                styleClass="w-full"
                name="ccourse"
              />
            </div>
            <div class="app-form-grid">
              <div class="field-block">
                <label for="class-title">Title</label>
                <input
                  id="class-title"
                  pInputText
                  [(ngModel)]="createTitle"
                  placeholder="Title"
                  class="w-full"
                  name="ctitle"
                />
              </div>
              <div class="field-block">
                <label for="class-max">Max learners</label>
                <p-inputNumber
                  inputId="class-max"
                  [(ngModel)]="createMax"
                  [min]="1"
                  placeholder="Max learners"
                  name="cmax"
                />
              </div>
            </div>
          </form>

          <ng-template pTemplate="footer">
            <p-button
              label="Cancel"
              severity="secondary"
              [text]="true"
              (onClick)="closeCreateDialog()"
            />
            <p-button
              label="Create"
              icon="pi pi-plus"
              [disabled]="!createCourseId || !createTitle.trim() || creating()"
              [loading]="creating()"
              (onClick)="createClass()"
            />
          </ng-template>
        </p-dialog>
      }
      @if (loading()) {
        <p>Loading…</p>
      } @else {
        @if (page(); as p) {
          <p-table
            [value]="p.items"
            styleClass="p-datatable-sm"
            [tableStyle]="{ 'min-width': '40rem' }"
          >
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
                  <a
                    [routerLink]="['/training-classes', t.id]"
                    class="text-primary font-medium"
                    >{{ t.title }}</a
                  >
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
    </app-page-shell>
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
