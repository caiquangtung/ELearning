import { Component, inject, signal, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ActivatedRoute, Router } from '@angular/router';
import { LmsApiService, QuizAnalyticsDto } from '../../../core/api/lms-api.service';
import { GlobalErrorService } from '../../../core/error/global-error.service';
import { UiButtonComponent, PageShellComponent } from '../../../shared/ui';
import { Fieldset } from 'primeng/fieldset';
import { ProgressSpinner } from 'primeng/progressspinner';
import { Toast } from 'primeng/toast';
import { MessageService } from 'primeng/api';

@Component({
  selector: 'app-quiz-analytics',
  standalone: true,
  imports: [
    CommonModule,
    PageShellComponent,
    UiButtonComponent,
    Fieldset,
    Toast
  ],
  providers: [MessageService],
  templateUrl: './quiz-analytics.component.html',
  styleUrl: './quiz-analytics.component.scss',
})
export class QuizAnalyticsComponent implements OnInit {
  private readonly api = inject(LmsApiService);
  private readonly route = inject(ActivatedRoute);
  private readonly router = inject(Router);
  private readonly errors = inject(GlobalErrorService);
  private readonly messageService = inject(MessageService);

  readonly analytics = signal<QuizAnalyticsDto | null>(null);
  readonly loading = signal(true);

  ngOnInit(): void {
    const quizId = this.route.snapshot.paramMap.get('id');
    if (quizId) {
      this.loadAnalytics(quizId);
    } else {
      this.router.navigate(['/quizzes']);
    }
  }

  loadAnalytics(quizId: string): void {
    this.errors.clear();

    this.api.getQuizAnalytics(quizId).subscribe({
      next: (analytics) => {
        this.analytics.set(analytics);
        this.loading.set(false);
      },
      error: (err) => {
        this.messageService.add({
          severity: 'error',
          summary: 'Error',
          detail: 'Failed to load quiz analytics'
        });
        this.loading.set(false);
      }
    });
  }

  backToQuiz(): void {
    const quizId = this.route.snapshot.paramMap.get('id');
    if (quizId) {
      this.router.navigate(['/quizzes', quizId]);
    } else {
      this.router.navigate(['/quizzes']);
    }
  }

  backToList(): void {
    this.router.navigate(['/quizzes']);
  }
}