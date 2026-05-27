import { Component, inject, signal, OnInit } from '@angular/core';
import { Router, RouterLink } from '@angular/router';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { LmsApiService, QuizListItemDto } from '../../../core/api/lms-api.service';
import { PagedList } from '../../../core/models/paged-list.model';
import { GlobalErrorService } from '../../../core/error/global-error.service';
import { UiButtonComponent, PageShellComponent } from '../../../shared/ui';
import { InputText } from 'primeng/inputtext';
import { DropdownModule } from 'primeng/dropdown';
import { Tag } from 'primeng/tag';
import { ConfirmDialog } from 'primeng/confirmdialog';
import { Toast } from 'primeng/toast';
import { TableModule } from 'primeng/table';
import { ConfirmationService, MessageService } from 'primeng/api';

@Component({
  selector: 'app-quiz-list',
  standalone: true,
  imports: [
    CommonModule,
    FormsModule,
    RouterLink,
    UiButtonComponent,
    PageShellComponent,
    InputText,
    DropdownModule,
    Tag,
    ConfirmDialog,
    Toast,
    TableModule
  ],
  providers: [ConfirmationService, MessageService],
  template: `
    <app-page-shell title="Quizzes">
      <div actions>
        <app-ui-button
          label="Create Quiz"
          icon="pi pi-plus"
          severity="success"
          (onClick)="navigateToCreate()"
        />
      </div>

      <div class="quiz-filter-panel">
        <div class="quiz-filter-row">
          <div class="quiz-filter-search">
            <span class="p-float-label">
              <input
                pInputText
                id="search"
                [(ngModel)]="searchTerm"
                placeholder="Search quizzes..."
                aria-label="Search quizzes"
                class="w-full"
              />
              <label for="search">Search</label>
            </span>
          </div>
          <div class="quiz-filter-status">
            <p-dropdown
              [options]="statusOptions"
              [(ngModel)]="statusFilter"
              optionLabel="label"
              optionValue="value"
              placeholder="Status"
              ariaLabel="Quiz status"
              [showClear]="true"
            />
          </div>
          <app-ui-button
            label="Apply"
            icon="pi pi-filter"
            (onClick)="applyFilters()"
          />
        </div>
      </div>

      <p-table
        [value]="page()?.items || []"
        [loading]="isLoading()"
        [paginator]="true"
        [rows]="pageSize"
        [totalRecords]="page()?.totalCount || 0"
        [lazy]="false"
        [responsiveLayout]="'scroll'"
        [showCurrentPageReport]="true"
        currentPageReportTemplate="Showing {first} to {last} of {totalRecords} entries"
        [rowsPerPageOptions]="[10, 25, 50]"
        aria-label="Quizzes"
        [attr.aria-busy]="isLoading()"
        (onPage)="onPageChange($event)"
      >
        <ng-template pTemplate="header">
          <tr>
            <th>Title</th>
            <th>Status</th>
            <th>Questions</th>
            <th>Created</th>
            <th>Actions</th>
          </tr>
        </ng-template>

        <ng-template pTemplate="body" let-quiz>
          <tr>
            <td>
              <a [routerLink]="['/quizzes', quiz.id]" class="text-primary font-medium">
                {{ quiz.title }}
              </a>
            </td>
            <td>
              <p-tag
                [value]="quiz.status"
                [severity]="quiz.status === 'Published' ? 'success' : quiz.status === 'Draft' ? 'warn' : 'secondary'"
              />
            </td>
            <td>{{ quiz.questionCount }}</td>
            <td>{{ quiz.createdAt | date:'mediumDate' }}</td>
            <td>
              <div class="flex gap-2">
                <a [routerLink]="['/quizzes', quiz.id, 'edit']" class="text-primary">
                  <i class="pi pi-pencil"></i> Edit
                </a>
                <button
                  type="button"
                  class="p-link text-red-500"
                  [attr.aria-label]="'Delete quiz ' + quiz.title"
                  (click)="confirmDelete(quiz.id)"
                >
                  <i class="pi pi-trash"></i> Delete
                </button>
              </div>
            </td>
          </tr>
        </ng-template>

        <ng-template pTemplate="emptymessage">
          <tr>
            <td colspan="5" class="text-center py-4">
              No quizzes found. <a [routerLink]="['/quizzes', 'create']" class="text-primary">Create your first quiz</a>
            </td>
          </tr>
        </ng-template>

        <ng-template pTemplate="loadingbody">
          <tr>
            <td colspan="5" class="text-center py-4">
              <i class="pi pi-spin pi-spinner"></i> Loading quizzes...
            </td>
          </tr>
        </ng-template>
      </p-table>
    </app-page-shell>

    <p-confirmDialog />
    <p-toast />
  `,
  styleUrl: './quiz-list.component.scss',
})
export class QuizListComponent implements OnInit {
  private readonly api = inject(LmsApiService);
  private readonly router = inject(Router);
  private readonly errors = inject(GlobalErrorService);
  private readonly confirmationService = inject(ConfirmationService);
  private readonly messageService = inject(MessageService);

  readonly page = signal<PagedList<QuizListItemDto> | null>(null);
  readonly isLoading = signal(false);

  searchTerm = '';
  statusFilter = '';
  pageSize = 10;
  currentPage = 1;

  readonly statusOptions = [
    { label: 'All', value: '' },
    { label: 'Draft', value: 'Draft' },
    { label: 'Published', value: 'Published' },
    { label: 'Archived', value: 'Archived' },
  ];

  ngOnInit(): void {
    this.loadQuizzes();
  }

  applyFilters(): void {
    this.currentPage = 1;
    this.loadQuizzes();
  }

  onPageChange(event: any): void {
    const first = event.first ?? 0;
    const rows = event.rows ?? this.pageSize;
    this.currentPage = Math.floor(first / rows) + 1;
    this.loadQuizzes();
  }

  loadQuizzes(): void {
    this.errors.clear();
    this.isLoading.set(true);

    this.api
      .listQuizzes({
        page: this.currentPage,
        pageSize: this.pageSize,
        search: this.searchTerm || null,
        status: this.statusFilter || null,
      })
      .subscribe({
        next: (result) => {
          this.page.set(result);
          this.isLoading.set(false);
        },
        error: (err) => {
          this.messageService.add({
            severity: 'error',
            summary: 'Error',
            detail: 'Failed to load quizzes'
          });
          this.isLoading.set(false);
        }
      });
  }

  confirmDelete(quizId: string): void {
    this.confirmationService.confirm({
      message: 'Are you sure you want to delete this quiz?',
      header: 'Confirm Delete',
      icon: 'pi pi-exclamation-triangle',
      accept: () => this.deleteQuiz(quizId)
    });
  }

  deleteQuiz(quizId: string): void {
    this.errors.clear();

    this.api.deleteQuiz(quizId).subscribe({
      next: () => {
        this.messageService.add({
          severity: 'success',
          summary: 'Success',
          detail: 'Quiz deleted successfully'
        });
        this.loadQuizzes();
      },
      error: (err) => {
        this.messageService.add({
          severity: 'error',
          summary: 'Error',
          detail: 'Failed to delete quiz'
        });
      }
    });
  }

  navigateToCreate(): void {
    this.router.navigate(['/quizzes', 'create']);
  }
}
