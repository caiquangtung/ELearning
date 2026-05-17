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
  template: `
    <app-page-shell title="Quiz Results">
      <div *ngIf="result(); else loadingTemplate" class="space-y-6">
        <div class="flex items-center gap-4">
          <p-tag
            [value]="result()!.passed ? 'Passed' : 'Failed'"
            [severity]="result()!.passed ? 'success' : 'danger'"
          />
        </div>

        <div class="space-y-4">
          <h3 class="text-xl font-semibold">{{ result()!.quizTitle }}</h3>

          <div class="grid grid-cols-1 md:grid-cols-2 gap-4 p-4 bg-gray-50 rounded">
            <div>
              <strong>Score:</strong> {{ result()!.totalScore || 0 }} / {{ result()!.passingScore || 0 }}
            </div>
            <div>
              <strong>Submitted:</strong> {{ result()!.submittedAt | date:'medium' }}
            </div>
          </div>
        </div>

        <div *ngIf="result()!.questionResults && result()!.questionResults.length > 0; else noResults" class="space-y-4">
          <h4 class="text-lg font-medium">Question Breakdown</h4>

          <div class="overflow-x-auto">
            <table class="w-full border-collapse border border-gray-200">
              <thead class="bg-gray-50">
                <tr>
                  <th class="border border-gray-200 px-4 py-2 text-left">#</th>
                  <th class="border border-gray-200 px-4 py-2 text-left">Question</th>
                  <th class="border border-gray-200 px-4 py-2 text-left">Points</th>
                  <th class="border border-gray-200 px-4 py-2 text-left">Score</th>
                  <th class="border border-gray-200 px-4 py-2 text-left">Result</th>
                </tr>
              </thead>
              <tbody>
                <tr *ngFor="let qr of result()!.questionResults; let i = index" class="hover:bg-gray-50">
                  <td class="border border-gray-200 px-4 py-2">{{ i + 1 }}</td>
                  <td class="border border-gray-200 px-4 py-2">{{ qr.questionText }}</td>
                  <td class="border border-gray-200 px-4 py-2">{{ qr.points }}</td>
                  <td class="border border-gray-200 px-4 py-2">{{ qr.score || 0 }}</td>
                  <td class="border border-gray-200 px-4 py-2">
                    <p-tag
                      [value]="qr.isCorrect === true ? 'Correct' : qr.isCorrect === false ? 'Incorrect' : 'Not Graded'"
                      [severity]="qr.isCorrect === true ? 'success' : qr.isCorrect === false ? 'danger' : 'warn'"
                    />
                  </td>
                </tr>
              </tbody>
            </table>
          </div>
        </div>

        <ng-template #noResults>
          <div class="text-center py-8 text-gray-500">
            <p>No question results available.</p>
          </div>
        </ng-template>

        <div class="flex gap-3 pt-4">
          <app-ui-button
            label="Back to Quiz"
            icon="pi pi-arrow-left"
            severity="secondary"
            (onClick)="backToQuiz()"
          />
          <app-ui-button
            label="View Analytics"
            icon="pi pi-chart-bar"
            severity="info"
            (onClick)="viewAnalytics()"
          />
        </div>
      </div>

      <ng-template #loadingTemplate>
        <div class="text-center py-8">
          <i class="pi pi-spin pi-spinner text-2xl"></i>
          <p class="mt-2">Loading results...</p>
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
    .gap-4 > * + * {
      margin-left: 1rem;
    }
    .gap-3 > * + * {
      margin-left: 0.75rem;
    }
  `]
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

    this.api.getAttempt(attemptId, userId).subscribe({
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