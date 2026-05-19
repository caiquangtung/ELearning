import { Component, inject, signal, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ActivatedRoute, Router, RouterLink } from '@angular/router';
import { LmsApiService, QuizDetailDto } from '../../../core/api/lms-api.service';
import { GlobalErrorService } from '../../../core/error/global-error.service';
import { UiButtonComponent, PageShellComponent } from '../../../shared/ui';
import { Tag } from 'primeng/tag';
import { ProgressSpinnerModule } from 'primeng/progressspinner';
import { Toast } from 'primeng/toast';
import { MessageService } from 'primeng/api';

@Component({
  selector: 'app-quiz-detail',
  standalone: true,
  imports: [
    CommonModule,
    PageShellComponent,
    UiButtonComponent,
    Tag,
    ProgressSpinnerModule,
    Toast
  ],
  providers: [MessageService],
  template: `
    <app-page-shell [title]="quiz()?.title || 'Quiz Detail'">
      <div actions>
        <app-ui-button
          label="Edit Quiz"
          icon="pi pi-pencil"
          severity="secondary"
          (onClick)="editQuiz()"
        />
        <app-ui-button
          label="Take Quiz"
          icon="pi pi-play"
          severity="success"
          (onClick)="takeQuiz()"
        />
        <app-ui-button
          label="View Analytics"
          icon="pi pi-chart-bar"
          severity="info"
          (onClick)="viewAnalytics()"
        />
      </div>

      <div *ngIf="quiz(); else loading" class="space-y-6">
        <div class="flex items-center gap-4">
          <p-tag
            [value]="quiz()!.status"
            [severity]="quiz()!.status === 'Published' ? 'success' : quiz()!.status === 'Draft' ? 'warn' : 'secondary'"
          />
          <span *ngIf="quiz()!.courseId" class="text-sm text-gray-600">
            Course ID: {{ quiz()!.courseId }}
          </span>
          <span *ngIf="quiz()!.lessonId" class="text-sm text-gray-600">
            Lesson ID: {{ quiz()!.lessonId }}
          </span>
        </div>

        <div class="space-y-4">
          <h3 class="text-xl font-semibold">{{ quiz()!.title }}</h3>
          <p *ngIf="quiz()!.description" class="text-gray-700">{{ quiz()!.description }}</p>

          <div class="grid grid-cols-1 md:grid-cols-2 gap-4 p-4 bg-gray-50 rounded">
            <div>
              <strong>Time Limit:</strong> {{ quiz()!.timeLimitMinutes || 'No limit' }} minutes
            </div>
            <div>
              <strong>Passing Score:</strong> {{ quiz()!.passingScore }}%
            </div>
          </div>
        </div>

        <div *ngIf="quiz()!.questions && quiz()!.questions.length > 0; else noQuestions" class="space-y-4">
          <h4 class="text-lg font-medium">Questions ({{ quiz()!.questions.length }})</h4>

          <div class="space-y-3">
            <div *ngFor="let question of quiz()!.questions; let i = index"
                 class="border border-gray-200 rounded p-4 bg-white">
              <div class="flex justify-between items-start mb-2">
                <span class="font-medium text-gray-900">{{ i + 1 }}. {{ question.text }}</span>
                <div class="flex gap-2 text-sm text-gray-600">
                  <span>{{ question.type }}</span>
                  <span>{{ question.points }} pts</span>
                </div>
              </div>

              <div *ngIf="question.options && question.options.length > 0" class="ml-4 space-y-1">
                <div *ngFor="let option of question.options" class="text-sm">
                  <span [class.font-semibold]="option.isCorrect" [class.text-green-600]="option.isCorrect">
                    {{ option.text }}
                    <i *ngIf="option.isCorrect" class="pi pi-check text-green-600 ml-1"></i>
                  </span>
                </div>
              </div>
            </div>
          </div>
        </div>

        <ng-template #noQuestions>
          <div class="text-center py-8 text-gray-500">
            <i class="pi pi-question-circle text-4xl mb-4"></i>
            <p>No questions added to this quiz yet.</p>
            <app-ui-button
              label="Add Questions"
              icon="pi pi-plus"
              severity="success"
              (onClick)="editQuiz()"
            />
          </div>
        </ng-template>
      </div>

      <ng-template #loading>
        <div class="text-center py-8">
          <i class="pi pi-spin pi-spinner text-2xl"></i>
          <p class="mt-2">Loading quiz...</p>
        </div>
      </ng-template>
    </app-page-shell>

    <p-toast />
  `,
  styleUrl: './quiz-detail.component.scss',
})
export class QuizDetailComponent implements OnInit {
  private readonly api = inject(LmsApiService);
  private readonly route = inject(ActivatedRoute);
  private readonly router = inject(Router);
  private readonly errors = inject(GlobalErrorService);
  private readonly messageService = inject(MessageService);

  readonly quiz = signal<QuizDetailDto | null>(null);
  private quizId: string | null = null;

  ngOnInit(): void {
    this.route.paramMap.subscribe(params => {
      const id = params.get('id');
      if (id) {
        this.quizId = id;
        this.loadQuiz(id);
      }
    });
  }

  loadQuiz(id: string): void {
    this.errors.clear();

    this.api.getQuiz(id).subscribe({
      next: (quiz) => {
        this.quiz.set(quiz);
      },
      error: (err) => {
        this.messageService.add({
          severity: 'error',
          summary: 'Error',
          detail: 'Failed to load quiz'
        });
      }
    });
  }

  editQuiz(): void {
    if (this.quizId) {
      this.router.navigate(['/quizzes', this.quizId, 'edit']);
    }
  }

  takeQuiz(): void {
    if (this.quizId) {
      this.router.navigate(['/quizzes', this.quizId, 'take']);
    }
  }

  viewAnalytics(): void {
    if (this.quizId) {
      this.router.navigate(['/quizzes', this.quizId, 'analytics']);
    }
  }
}