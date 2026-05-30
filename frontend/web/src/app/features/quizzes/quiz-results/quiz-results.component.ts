import { Component, inject, signal, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ActivatedRoute, Router } from '@angular/router';
import { LmsApiService, QuizResultDto } from '../../../core/api/lms-api.service';
import { GlobalErrorService } from '../../../core/error/global-error.service';
import { UiButtonComponent, PageShellComponent } from '../../../shared/ui';
import { Tag } from 'primeng/tag';
import { ProgressSpinner } from 'primeng/progressspinner';
import { Toast } from 'primeng/toast';
import { MessageService } from 'primeng/api';

@Component({
  selector: 'app-quiz-results',
  standalone: true,
  imports: [
    CommonModule,
    PageShellComponent,
    UiButtonComponent,
    Tag,
    Toast
  ],
  providers: [MessageService],
  templateUrl: './quiz-results.component.html',
  styleUrl: './quiz-results.component.scss',
})
export class QuizResultsComponent implements OnInit {
  private readonly api = inject(LmsApiService);
  private readonly route = inject(ActivatedRoute);
  private readonly router = inject(Router);
  private readonly errors = inject(GlobalErrorService);
  private readonly messageService = inject(MessageService);

  readonly result = signal<QuizResultDto | null>(null);
  readonly loading = signal(true);

  ngOnInit(): void {
    const attemptId = this.route.snapshot.paramMap.get('attemptId');
    const userId = this.route.snapshot.queryParamMap.get('userId');
    if (attemptId && userId) {
      this.loadResult(attemptId, userId);
    } else {
      this.router.navigate(['/quizzes']);
    }
  }

  loadResult(attemptId: string, userId: string): void {
    this.errors.clear();

    this.api.getAttempt(attemptId, { userId }).subscribe({
      next: (result) => {
        this.result.set(result);
        this.loading.set(false);
      },
      error: (err) => {
        this.messageService.add({
          severity: 'error',
          summary: 'Error',
          detail: 'Failed to load quiz results'
        });
        this.loading.set(false);
      }
    });
  }

  backToQuiz(): void {
    const quizId = this.route.snapshot.paramMap.get('quizId');
    if (quizId) {
      this.router.navigate(['/quizzes', quizId]);
    } else {
      this.router.navigate(['/quizzes']);
    }
  }

  viewAnalytics(): void {
    const quizId = this.route.snapshot.paramMap.get('quizId');
    if (quizId) {
      this.router.navigate(['/quizzes', quizId, 'analytics']);
    }
  }
}
