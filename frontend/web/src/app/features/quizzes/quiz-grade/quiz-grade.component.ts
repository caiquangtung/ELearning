import { Component, inject, signal, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ReactiveFormsModule, FormBuilder, FormGroup, Validators } from '@angular/forms';
import { ActivatedRoute, Router } from '@angular/router';
import {
  EssayGradeSuggestionDto,
  LmsApiService,
  QuestionResultDto,
  QuizResultDto,
} from '../../../core/api/lms-api.service';
import { GlobalErrorService } from '../../../core/error/global-error.service';
import { UiButtonComponent, PageShellComponent } from '../../../shared/ui';
import { RadioButton } from 'primeng/radiobutton';
import { InputNumber } from 'primeng/inputnumber';
import { Tag } from 'primeng/tag';
import { Toast } from 'primeng/toast';
import { MessageService } from 'primeng/api';

@Component({
  selector: 'app-quiz-grade',
  standalone: true,
  imports: [
    CommonModule,
    ReactiveFormsModule,
    PageShellComponent,
    UiButtonComponent,
    RadioButton,
    InputNumber,
    Tag,
    Toast
  ],
  providers: [MessageService],
  templateUrl: './quiz-grade.component.html',
  styleUrl: './quiz-grade.component.scss',
})
export class QuizGradeComponent implements OnInit {
  private readonly fb = inject(FormBuilder);
  private readonly api = inject(LmsApiService);
  private readonly route = inject(ActivatedRoute);
  private readonly router = inject(Router);
  private readonly errors = inject(GlobalErrorService);
  private readonly messageService = inject(MessageService);

  readonly result = signal<QuizResultDto | null>(null);
  readonly attempt = signal<QuestionResultDto | null>(null);
  readonly aiSuggestion = signal<EssayGradeSuggestionDto | null>(null);
  readonly quizTitle = signal('');
  readonly gradeForm: FormGroup;
  readonly loading = signal(true);
  readonly suggestingAi = signal(false);
  readonly submitting = signal(false);
  private attemptId: string | null = null;

  constructor() {
    this.gradeForm = this.fb.group({
      rubric: [''],
      score: [null, Validators.required],
      isCorrect: [null, Validators.required]
    });
  }

  ngOnInit(): void {
    const quizId = this.route.snapshot.paramMap.get('quizId') ?? this.route.snapshot.paramMap.get('id');
    const attemptId = this.route.snapshot.paramMap.get('attemptId') ?? this.route.snapshot.queryParamMap.get('attemptId');
    const userId = this.route.snapshot.queryParamMap.get('userId');

    if (quizId && attemptId && userId) {
      this.attemptId = attemptId;
      this.loadAttemptForGrading(quizId, attemptId, userId);
    } else {
      this.router.navigate(['/quizzes']);
    }
  }

  loadAttemptForGrading(quizId: string, attemptId: string, userId: string): void {
    this.errors.clear();

    this.api.getAttempt(attemptId, { userId }).subscribe({
      next: (result) => {
        this.result.set(result);
        this.quizTitle.set(result.quizTitle);

        const answer = result.questionResults.find((q) => q.textAnswer?.trim() && q.score === null)
          ?? result.questionResults.find((q) => q.textAnswer?.trim())
          ?? null;

        this.attempt.set(answer);
        if (answer) {
          this.gradeForm.patchValue({
            score: answer.score,
            isCorrect: answer.isCorrect
          });
        }

        if (!answer) {
          this.messageService.add({
            severity: 'warn',
            summary: 'No gradable answer',
            detail: 'This attempt has no essay or text answer to grade.'
          });
        }

        this.loading.set(false);
      },
      error: () => {
        this.messageService.add({
          severity: 'error',
          summary: 'Error',
          detail: 'Failed to load attempt for grading'
        });
        this.loading.set(false);
      }
    });
  }

  suggestWithAi(): void {
    if (!this.attemptId) return;

    this.suggestingAi.set(true);
    this.aiSuggestion.set(null);

    this.api.suggestEssayGrades(this.attemptId, {
      rubric: this.gradeForm.value.rubric?.trim() || null
    }).subscribe({
      next: (result) => {
        const currentQuestionId = this.attempt()?.questionId;
        const suggestion = result.suggestions.find((x) => x.questionId === currentQuestionId)
          ?? result.suggestions[0]
          ?? null;

        this.aiSuggestion.set(suggestion);
        if (suggestion) {
          this.gradeForm.patchValue({
            score: suggestion.suggestedScore,
            isCorrect: suggestion.suggestedScore >= Math.ceil(suggestion.maxScore * 0.6)
          });
        }

        this.suggestingAi.set(false);
        this.messageService.add({
          severity: 'success',
          summary: 'AI suggestion ready',
          detail: 'Review the suggested score before submitting the final grade.'
        });
      },
      error: () => {
        this.suggestingAi.set(false);
        this.messageService.add({
          severity: 'error',
          summary: 'AI suggestion failed',
          detail: 'Could not generate an AI grading suggestion.'
        });
      }
    });
  }

  onSubmit(): void {
    if (this.gradeForm.invalid) {
      this.gradeForm.markAllAsTouched();
      return;
    }

    this.errors.clear();
    const answer = this.attempt();
    if (!this.attemptId || !answer) return;

    this.submitting.set(true);

    this.api.gradeAttempt(this.attemptId, {
      grades: [{
        questionId: answer.questionId,
        score: this.gradeForm.value.score,
        isCorrect: this.gradeForm.value.isCorrect
      }]
    }).subscribe({
      next: () => {
        this.messageService.add({
          severity: 'success',
          summary: 'Success',
          detail: 'Grade submitted'
        });
        this.submitting.set(false);
        this.router.navigate(['/quizzes']);
      },
      error: () => {
        this.submitting.set(false);
        this.messageService.add({
          severity: 'error',
          summary: 'Submit failed',
          detail: 'Could not submit grade.'
        });
      }
    });
  }

  onCancel(): void {
    this.router.navigate(['/quizzes']);
  }
}
