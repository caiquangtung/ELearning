import { Component, inject, signal, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { ActivatedRoute, Router, RouterLink } from '@angular/router';
import { GeneratedQuizQuestionDto, LmsApiService, QuizDetailDto } from '../../../core/api/lms-api.service';
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
    FormsModule,
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
        <app-ui-button
          label="Generate with AI"
          icon="pi pi-sparkles"
          severity="help"
          [disabled]="!quiz()?.courseId || generatingAi()"
          [loading]="generatingAi()"
          (onClick)="generateWithAi()"
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

        <section class="border border-slate-200 rounded bg-white">
          <div class="flex flex-col gap-4 p-4 border-b border-slate-200 lg:flex-row lg:items-end lg:justify-between">
            <div>
              <h4 class="text-lg font-medium">AI question drafts</h4>
              <p class="text-sm text-slate-600">
                Provider: {{ aiProviderLabel() }}
              </p>
            </div>

            <div class="grid grid-cols-1 gap-3 sm:grid-cols-3">
              <label class="text-sm font-medium">
                Count
                <input
                  type="number"
                  min="1"
                  max="10"
                  [(ngModel)]="aiQuestionCount"
                  class="mt-1 w-full rounded border border-slate-300 px-3 py-2 text-sm"
                />
              </label>

              <label class="text-sm font-medium">
                Difficulty
                <select
                  [(ngModel)]="aiDifficulty"
                  class="mt-1 w-full rounded border border-slate-300 px-3 py-2 text-sm"
                >
                  <option value="Easy">Easy</option>
                  <option value="Medium">Medium</option>
                  <option value="Hard">Hard</option>
                </select>
              </label>

              <div class="text-sm font-medium">
                Types
                <div class="mt-1 flex min-h-10 flex-wrap items-center gap-3 rounded border border-slate-300 px-3 py-2">
                  <label class="flex items-center gap-1">
                    <input type="checkbox" [checked]="hasAiType('MultipleChoice')" (change)="toggleAiType('MultipleChoice')" />
                    MCQ
                  </label>
                  <label class="flex items-center gap-1">
                    <input type="checkbox" [checked]="hasAiType('Essay')" (change)="toggleAiType('Essay')" />
                    Essay
                  </label>
                  <label class="flex items-center gap-1">
                    <input type="checkbox" [checked]="hasAiType('Code')" (change)="toggleAiType('Code')" />
                    Code
                  </label>
                </div>
              </div>
            </div>
          </div>

          <div *ngIf="generatedQuestions().length > 0; else noAiDrafts" class="divide-y divide-slate-100">
            <article *ngFor="let question of generatedQuestions(); let i = index" class="p-4">
              <div class="flex flex-col gap-3 lg:flex-row lg:items-start lg:justify-between">
                <div class="space-y-2">
                  <div class="flex flex-wrap items-center gap-2 text-sm text-slate-600">
                    <p-tag [value]="question.type" severity="info" />
                    <p-tag [value]="question.difficulty" severity="secondary" />
                    <span>{{ question.points }} pts</span>
                  </div>
                  <h5 class="font-medium text-slate-950">{{ i + 1 }}. {{ question.text }}</h5>
                  <p class="text-sm text-slate-600">{{ question.explanation }}</p>
                  <ul *ngIf="question.options.length > 0" class="space-y-1 text-sm">
                    <li *ngFor="let option of question.options" [class.text-green-700]="option.isCorrect">
                      {{ option.sortOrder }}. {{ option.text }}
                      <i *ngIf="option.isCorrect" class="pi pi-check ml-1"></i>
                    </li>
                  </ul>
                </div>

                <div class="flex shrink-0 gap-2">
                  <app-ui-button
                    label="Accept"
                    icon="pi pi-check"
                    severity="success"
                    [disabled]="acceptingAiQuestion()"
                    [loading]="acceptingAiQuestion()"
                    (onClick)="acceptAiQuestion(question)"
                  />
                  <app-ui-button
                    label="Discard"
                    icon="pi pi-times"
                    severity="secondary"
                    [disabled]="acceptingAiQuestion()"
                    (onClick)="discardAiQuestion(question)"
                  />
                </div>
              </div>
            </article>
          </div>

          <ng-template #noAiDrafts>
            <div class="p-4 text-sm text-slate-600">
              No AI drafts generated yet.
            </div>
          </ng-template>
        </section>

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
  readonly generatedQuestions = signal<GeneratedQuizQuestionDto[]>([]);
  readonly generatingAi = signal(false);
  readonly acceptingAiQuestion = signal(false);
  readonly aiProviderLabel = signal('Local');
  aiQuestionCount = 5;
  aiDifficulty = 'Medium';
  aiQuestionTypes: string[] = ['MultipleChoice'];
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
        this.generatedQuestions.set([]);
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

  hasAiType(type: string): boolean {
    return this.aiQuestionTypes.includes(type);
  }

  toggleAiType(type: string): void {
    if (this.hasAiType(type)) {
      this.aiQuestionTypes = this.aiQuestionTypes.filter(t => t !== type);
      return;
    }

    this.aiQuestionTypes = [...this.aiQuestionTypes, type];
  }

  generateWithAi(): void {
    const quiz = this.quiz();
    if (!quiz?.courseId) {
      this.messageService.add({
        severity: 'warn',
        summary: 'Course required',
        detail: 'AI generation needs a course-linked quiz.'
      });
      return;
    }

    const questionTypes = this.aiQuestionTypes.length > 0 ? this.aiQuestionTypes : ['MultipleChoice'];
    this.generatingAi.set(true);
    this.api.generateQuizQuestions({
      courseId: quiz.courseId,
      lessonId: quiz.lessonId,
      questionCount: Math.min(Math.max(Number(this.aiQuestionCount) || 1, 1), 10),
      difficulty: this.aiDifficulty,
      questionTypes
    }).subscribe({
      next: (result) => {
        this.aiProviderLabel.set(`${result.provider} / ${result.model}`);
        this.generatedQuestions.set(result.questions);
        this.generatingAi.set(false);
        this.messageService.add({
          severity: 'success',
          summary: 'Drafts generated',
          detail: `${result.questions.length} AI question drafts are ready for review`
        });
      },
      error: () => {
        this.generatingAi.set(false);
        this.messageService.add({
          severity: 'error',
          summary: 'AI generation failed',
          detail: 'Could not generate quiz questions'
        });
      }
    });
  }

  acceptAiQuestion(question: GeneratedQuizQuestionDto): void {
    if (!this.quizId) return;

    const existingCount = this.quiz()?.questions.length ?? 0;
    this.acceptingAiQuestion.set(true);
    this.api.addQuestion(this.quizId, {
      text: question.text,
      type: question.type,
      points: question.points,
      sortOrder: existingCount + question.sortOrder,
      options: question.options.map(option => ({
        text: option.text,
        isCorrect: option.isCorrect,
        sortOrder: option.sortOrder
      }))
    }).subscribe({
      next: () => {
        this.discardAiQuestion(question);
        this.acceptingAiQuestion.set(false);
        this.messageService.add({
          severity: 'success',
          summary: 'Question accepted',
          detail: 'AI draft was added to the quiz'
        });
        this.loadQuiz(this.quizId!);
      },
      error: () => {
        this.acceptingAiQuestion.set(false);
        this.messageService.add({
          severity: 'error',
          summary: 'Save failed',
          detail: 'Could not add the AI question'
        });
      }
    });
  }

  discardAiQuestion(question: GeneratedQuizQuestionDto): void {
    this.generatedQuestions.set(this.generatedQuestions().filter(q => q !== question));
  }
}
