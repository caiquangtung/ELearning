import { Component, inject, signal, OnInit } from '@angular/core';
import { Router, RouterLink } from '@angular/router';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import {
  LmsApiService,
  QuizListItemDto,
} from '../../../core/api/lms-api.service';
import { PagedList } from '../../../core/models/paged-list.model';
import { GlobalErrorService } from '../../../core/error/global-error.service';
import {
  PageShellComponent,
  UiButtonComponent,
  UiDataTableBodyTemplateDirective,
  UiDataTableComponent,
  UiDataTableHeaderTemplateDirective,
} from '../../../shared/ui';
import { InputText } from 'primeng/inputtext';
import { DropdownModule } from 'primeng/dropdown';
import { Tag } from 'primeng/tag';
import { ConfirmDialog } from 'primeng/confirmdialog';
import { Toast } from 'primeng/toast';
import { PaginatorState } from 'primeng/paginator';
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
    UiDataTableComponent,
    UiDataTableHeaderTemplateDirective,
    UiDataTableBodyTemplateDirective,
  ],
  providers: [ConfirmationService, MessageService],
  templateUrl: './quiz-list.component.html',
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

  onPageChange(event: PaginatorState): void {
    const first = event.first ?? 0;
    const rows = event.rows ?? this.pageSize;
    if (rows !== this.pageSize) {
      this.pageSize = rows;
    }
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
            detail: 'Failed to load quizzes',
          });
          this.isLoading.set(false);
        },
      });
  }

  confirmDelete(quizId: string): void {
    this.confirmationService.confirm({
      message: 'Are you sure you want to delete this quiz?',
      header: 'Confirm Delete',
      icon: 'pi pi-exclamation-triangle',
      accept: () => this.deleteQuiz(quizId),
    });
  }

  deleteQuiz(quizId: string): void {
    this.errors.clear();

    this.api.deleteQuiz(quizId).subscribe({
      next: () => {
        this.messageService.add({
          severity: 'success',
          summary: 'Success',
          detail: 'Quiz deleted successfully',
        });
        this.loadQuizzes();
      },
      error: (err) => {
        this.messageService.add({
          severity: 'error',
          summary: 'Error',
          detail: 'Failed to delete quiz',
        });
      },
    });
  }

  navigateToCreate(): void {
    this.router.navigate(['/quizzes', 'create']);
  }
}
