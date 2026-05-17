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
  template: `
    <app-page-shell title="Quiz Analytics">
      <div *ngIf="analytics(); else loadingTemplate" class="space-y-6">
        <div class="space-y-4">
          <h3 class="text-xl font-semibold">{{ analytics()!.quizTitle }}</h3>
        </div>

        <!-- Key Metrics -->
        <div class="grid grid-cols-1 md:grid-cols-4 gap-4">
          <div class="text-center p-4 bg-blue-50 rounded">
            <div class="text-2xl font-semibold text-blue-600">{{ analytics()!.totalAttempts }}</div>
            <div class="text-sm text-gray-600">Total Attempts</div>
          </div>
          <div class="text-center p-4 bg-green-50 rounded">
            <div class="text-2xl font-semibold text-green-600">{{ analytics()!.completedAttempts }}</div>
            <div class="text-sm text-gray-600">Completed Attempts</div>
          </div>
          <div class="text-center p-4 bg-purple-50 rounded">
            <div class="text-2xl font-semibold text-purple-600">{{ analytics()!.averageScore | number:'1.0-1' }}</div>
            <div class="text-sm text-gray-600">Average Score</div>
          </div>
          <div class="text-center p-4 bg-orange-50 rounded">
            <div class="text-2xl font-semibold text-orange-600">{{ analytics()!.passRate | number:'1.0-1' }}%</div>
            <div class="text-sm text-gray-600">Pass Rate</div>
          </div>
        </div>

        <!-- Detailed Analytics -->
        <div class="grid grid-cols-1 md:grid-cols-2 gap-6">
          <p-fieldset legend="Score Distribution" class="space-y-3">
            <div class="space-y-2">
              <p><strong>Highest Score:</strong> {{ analytics()!.highestScore }}</p>
              <p><strong>Lowest Score:</strong> {{ analytics()!.lowestScore }}</p>
            </div>
          </p-fieldset>

          <p-fieldset legend="Attempts Overview" class="space-y-3">
            <div class="space-y-2">
              <p><strong>Completed:</strong> {{ analytics()!.completedAttempts }} / {{ analytics()!.totalAttempts }}</p>
              <p><strong>In Progress:</strong> {{ analytics()!.totalAttempts - analytics()!.completedAttempts }}</p>
            </div>
          </p-fieldset>
        </div>

        <div class="flex gap-3 pt-4">
          <app-ui-button
            label="Back to Quiz"
            icon="pi pi-arrow-left"
            severity="secondary"
            (onClick)="backToQuiz()"
          />
          <app-ui-button
            label="View Quiz List"
            icon="pi pi-list"
            severity="info"
            (onClick)="backToList()"
          />
        </div>
      </div>

      <ng-template #loadingTemplate>
        <div class="text-center py-8">
          <i class="pi pi-spin pi-spinner text-2xl"></i>
          <p class="mt-2">Loading analytics...</p>
        </div>
      </ng-template>
    </app-page-shell>

    <p-toast />
  `,
  styles: [`
    .space-y-6 > * + * {
      margin-top: 1.5rem;
    }
    .space-y-4 > * + * {
      margin-top: 1rem;
    }
    .space-y-3 > * + * {
      margin-top: 0.75rem;
    }
    .space-y-2 > * + * {
      margin-top: 0.5rem;
    }
    .gap-4 > * + * {
      margin-left: 1rem;
    }
    .gap-6 > * + * {
      margin-left: 1.5rem;
    }
    .gap-3 > * + * {
      margin-left: 0.75rem;
    }
  `]
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