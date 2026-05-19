import { Component, inject, signal, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ReactiveFormsModule, FormBuilder, FormGroup, Validators } from '@angular/forms';
import { ActivatedRoute, Router } from '@angular/router';
import { LmsApiService } from '../../../core/api/lms-api.service';
import { GlobalErrorService } from '../../../core/error/global-error.service';
import { UiButtonComponent, PageShellComponent } from '../../../shared/ui';
import { RadioButton } from 'primeng/radiobutton';
import { InputNumber } from 'primeng/inputnumber';
import { Tag } from 'primeng/tag';
import { ProgressSpinner } from 'primeng/progressspinner';
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
  template: `
    <app-page-shell title="Grade Quiz Attempt">
      <div *ngIf="attempt(); else loadingTemplate" class="space-y-6">
        <div class="flex items-center gap-4">
          <p-tag
            [value]="attempt()!.isCorrect === true ? 'Correct' : attempt()!.isCorrect === false ? 'Incorrect' : 'Not Graded'"
            [severity]="attempt()!.isCorrect === true ? 'success' : attempt()!.isCorrect === false ? 'danger' : 'warn'"
          />
        </div>

        <div class="space-y-4">
          <h3 class="text-xl font-semibold">{{ quizTitle() }}</h3>

          <div class="space-y-2">
            <p><strong>Question:</strong> {{ attempt()!.questionText }}</p>
            <p><strong>Points Possible:</strong> {{ attempt()!.points }}</p>
          </div>
        </div>

        <div class="space-y-4">
          <div>
            <p class="font-medium">Student Answer:</p>
            <div class="border border-gray-200 rounded p-4 bg-gray-50 min-h-[3rem]">
              {{ attempt()!.textAnswer || 'No answer provided' }}
            </div>
          </div>
        </div>

        <form [formGroup]="gradeForm" (ngSubmit)="onSubmit()" class="space-y-6">
          <div class="grid grid-cols-1 md:grid-cols-2 gap-4">
            <div class="space-y-2">
              <label for="score" class="block text-sm font-medium">Score (0-{{ attempt()!.points }}) *</label>
              <p-inputNumber
                inputId="score"
                formControlName="score"
                [min]="0"
                [max]="attempt()!.points"
                placeholder="Enter score"
                class="w-full"
                [class.ng-invalid]="gradeForm.get('score')?.invalid && gradeForm.get('score')?.touched"
              />
              <div *ngIf="gradeForm.get('score')?.invalid && gradeForm.get('score')?.touched" class="text-red-500 text-sm">
                Please enter a valid score between 0 and {{ attempt()!.points }}
              </div>
            </div>

            <div class="space-y-2">
              <label class="block text-sm font-medium">Is Correct? *</label>
              <div class="flex gap-4">
                <div class="flex items-center">
                  <p-radioButton
                    name="isCorrect"
                    [value]="true"
                    formControlName="isCorrect"
                    inputId="isCorrectYes"
                    class="mr-2"
                  />
                  <label for="isCorrectYes" class="cursor-pointer">Yes</label>
                </div>
                <div class="flex items-center">
                  <p-radioButton
                    name="isCorrect"
                    [value]="false"
                    formControlName="isCorrect"
                    inputId="isCorrectNo"
                    class="mr-2"
                  />
                  <label for="isCorrectNo" class="cursor-pointer">No</label>
                </div>
              </div>
            </div>
          </div>

          <div class="flex gap-3 pt-4">
            <app-ui-button
              type="button"
              label="Cancel"
              severity="secondary"
              (onClick)="onCancel()"
            />
            <app-ui-button
              type="submit"
              label="Submit Grade"
              severity="success"
              [loading]="loading()"
              [disabled]="gradeForm.invalid"
            />
          </div>
        </form>
      </div>

      <ng-template #loadingTemplate>
        <div class="text-center py-8">
          <i class="pi pi-spin pi-spinner text-2xl"></i>
          <p class="mt-2">Loading attempt...</p>
        </div>
      </ng-template>
    </app-page-shell>

    <p-toast />
  `,
  styleUrl: './quiz-grade.component.scss',
})
export class QuizGradeComponent implements OnInit {
  private readonly fb = inject(FormBuilder);
  private readonly api = inject(LmsApiService);
  private readonly route = inject(ActivatedRoute);
  private readonly router = inject(Router);
  private readonly errors = inject(GlobalErrorService);
  private readonly messageService = inject(MessageService);

  readonly attempt = signal<any>(null); // TODO: Use proper DTO when API is available
  readonly quizTitle = signal('');
  readonly gradeForm: FormGroup;
  readonly loading = signal(true);
  private attemptId: string | null = null;

  constructor() {
    this.gradeForm = this.fb.group({
      score: [null, Validators.required],
      isCorrect: [null, Validators.required]
    });
  }

  ngOnInit(): void {
    const quizId = this.route.snapshot.paramMap.get('quizId');
    const attemptId = this.route.snapshot.paramMap.get('attemptId');
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

    // This component is not fully implemented due to API limitations.
    // The backend doesn't have endpoints for grading individual questions yet.
    this.api.getQuiz(quizId).subscribe({
      next: (quiz) => {
        this.quizTitle.set(quiz.title);

        // TODO: When the API provides grading endpoints, implement proper data loading
        // For now, show a warning that this feature is not fully implemented
        this.loading.set(false);
        this.messageService.add({
          severity: 'warn',
          summary: 'Feature Not Implemented',
          detail: 'Grading functionality is not yet available due to API limitations.'
        });
      },
      error: (err) => {
        this.messageService.add({
          severity: 'error',
          summary: 'Error',
          detail: 'Failed to load quiz for grading'
        });
        this.loading.set(false);
      }
    });
  }

  onSubmit(): void {
    if (this.gradeForm.invalid) {
      this.gradeForm.markAllAsTouched();
      return;
    }

    this.errors.clear();
    this.loading.set(true);

    // TODO: Implement actual grading API call when available
    const gradeData = {
      questionId: this.attempt()?.questionId,
      score: this.gradeForm.value.score,
      isCorrect: this.gradeForm.value.isCorrect
    };

    // Simulate API call
    setTimeout(() => {
      this.messageService.add({
        severity: 'success',
        summary: 'Success',
        detail: 'Grade submitted (simulated)'
      });
      this.loading.set(false);
      this.router.navigate(['/quizzes']);
    }, 1000);
  }

  onCancel(): void {
    this.router.navigate(['/quizzes']);
  }
}