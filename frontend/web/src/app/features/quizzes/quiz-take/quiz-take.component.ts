import { Component, inject, signal, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ReactiveFormsModule, FormBuilder, FormGroup, FormArray, Validators } from '@angular/forms';
import { ActivatedRoute, Router } from '@angular/router';
import { LmsApiService, QuizDetailDto } from '../../../core/api/lms-api.service';
import { GlobalErrorService } from '../../../core/error/global-error.service';
import { UiButtonComponent, PageShellComponent } from '../../../shared/ui';
import { RadioButton } from 'primeng/radiobutton';
import { InputTextarea } from 'primeng/inputtextarea';
import { Tag } from 'primeng/tag';
import { ProgressSpinner } from 'primeng/progressspinner';
import { Toast } from 'primeng/toast';
import { MessageService } from 'primeng/api';

@Component({
  selector: 'app-quiz-take',
  standalone: true,
  imports: [
    CommonModule,
    ReactiveFormsModule,
    PageShellComponent,
    UiButtonComponent,
    RadioButton,
    InputTextarea,
    Tag,
    Toast
  ],
  providers: [MessageService],
  template: `
    <app-page-shell title="Take Quiz">
      <div *ngIf="quiz(); else loadingTemplate" class="space-y-6">
        <div class="flex items-center gap-4">
          <p-tag
            [value]="quiz()!.status"
            [severity]="quiz()!.status === 'Published' ? 'success' : 'warn'"
          />
        </div>

        <div class="space-y-4">
          <h3 class="text-xl font-semibold">{{ quiz()!.title }}</h3>
          <p *ngIf="quiz()!.description" class="text-gray-700">{{ quiz()!.description }}</p>

          <div *ngIf="quiz()!.timeLimitMinutes || quiz()!.passingScore" class="p-4 bg-blue-50 rounded">
            <p *ngIf="quiz()!.timeLimitMinutes">
              <strong>Time Limit:</strong> {{ quiz()!.timeLimitMinutes }} minutes
            </p>
            <p *ngIf="quiz()!.passingScore">
              <strong>Passing Score:</strong> {{ quiz()!.passingScore }}%
            </p>
          </div>
        </div>

        <form *ngIf="quiz()!.questions && quiz()!.questions.length > 0; else noQuestions"
              [formGroup]="quizForm"
              (ngSubmit)="onSubmit()"
              class="space-y-6">

          <div formArrayName="questions" class="space-y-4">
            <div *ngFor="let question of quiz()!.questions; let i = index"
                 class="border border-gray-200 rounded p-6 bg-white"
                 [formGroupName]="i">

              <div class="mb-4">
                <h5 class="font-medium text-lg mb-2">
                  Question {{ i + 1 }} ({{ question.points }} points)
                </h5>
                <p class="text-gray-800">{{ question.text }}</p>
              </div>

              <div [ngSwitch]="question.type" class="space-y-3">
                <!-- Multiple Choice -->
                <div *ngSwitchCase="'MultipleChoice'">
                  <div *ngFor="let option of question.options; let j = index" class="flex items-center">
                    <p-radioButton
                      [name]="'option_' + i"
                      [value]="option.id"
                      formControlName="selectedOptionId"
                      [inputId]="'option_' + i + '_' + j"
                      class="mr-3"
                    />
                    <label [for]="'option_' + i + '_' + j" class="cursor-pointer">
                      {{ option.text }}
                    </label>
                  </div>
                </div>

                <!-- Essay -->
                <div *ngSwitchCase="'Essay'">
                  <textarea
                    pInputTextarea
                    formControlName="textAnswer"
                    rows="4"
                    placeholder="Enter your answer here"
                    class="w-full"
                  ></textarea>
                </div>

                <!-- Code -->
                <div *ngSwitchCase="'Code'">
                  <textarea
                    pInputTextarea
                    formControlName="textAnswer"
                    rows="6"
                    placeholder="Enter your code here"
                    class="w-full font-mono"
                  ></textarea>
                </div>

                <!-- Default case -->
                <div *ngSwitchDefault>
                  <p class="text-gray-500">Unsupported question type: {{ question.type }}</p>
                </div>
              </div>
            </div>
          </div>

          <div class="flex gap-3 pt-4">
            <app-ui-button
              type="button"
              label="Cancel Quiz"
              severity="secondary"
              (onClick)="onCancel()"
            />
            <app-ui-button
              type="submit"
              label="Submit Quiz"
              severity="success"
              [loading]="loading()"
              [disabled]="quizForm.invalid"
            />
          </div>
        </form>

        <div *ngIf="quiz()!.questions && quiz()!.questions.length === 0" class="text-center py-8 text-gray-500">
          <p>This quiz has no questions yet.</p>
        </div>
      </div>

      <ng-template #loadingTemplate>
        <div class="text-center py-8">
          <i class="pi pi-spin pi-spinner text-2xl"></i>
          <p class="mt-2">Loading quiz...</p>
        </div>
      </ng-template>

      <ng-template #noQuestions>
        <div class="text-center py-8 text-gray-500">
          <p>This quiz has no questions yet.</p>
        </div>
      </ng-template>
    </app-page-shell>

    <p-toast />
  `,
  styleUrl: './quiz-take.component.scss',
})
export class QuizTakeComponent implements OnInit {
  private readonly fb = inject(FormBuilder);
  private readonly api = inject(LmsApiService);
  private readonly route = inject(ActivatedRoute);
  private readonly router = inject(Router);
  private readonly errors = inject(GlobalErrorService);
  private readonly messageService = inject(MessageService);

  readonly quiz = signal<QuizDetailDto | null>(null);
  readonly quizForm: FormGroup;
  readonly loading = signal(false);
  private userId = 'current-user'; // In a real app, this would come from auth service

  constructor() {
    this.quizForm = this.fb.group({
      questions: this.fb.array([])
    });
  }

  ngOnInit(): void {
    const quizId = this.route.snapshot.paramMap.get('id');
    if (quizId) {
      this.loadQuiz(quizId);
    } else {
      this.router.navigate(['/quizzes']);
    }
  }

  loadQuiz(id: string): void {
    this.errors.clear();
    this.loading.set(true);

    this.api.getQuiz(id).subscribe({
      next: (quiz) => {
        this.quiz.set(quiz);
        this.initForm();
        this.loading.set(false);
      },
      error: (err) => {
        this.messageService.add({
          severity: 'error',
          summary: 'Error',
          detail: 'Failed to load quiz'
        });
        this.loading.set(false);
      }
    });
  }

  initForm(): void {
    const questionArray = this.quizForm.get('questions') as FormArray;
    // Clear any existing controls
    while (questionArray.length) {
      questionArray.removeAt(0);
    }
    // Add a control group for each question
    const quiz = this.quiz();
    if (quiz && quiz.questions) {
      quiz.questions.forEach(() => {
        questionArray.push(this.fb.group({
          selectedOptionId: [null],
          textAnswer: [null]
        }));
      });
    }
  }

  onSubmit(): void {
    if (this.quizForm.invalid) {
      this.quizForm.markAllAsTouched();
      return;
    }

    this.errors.clear();
    this.loading.set(true);
    const quiz = this.quiz()!;
    const attemptData = {
      userId: this.userId,
      answers: this.quizForm.value.questions.map((answer: any, index: number) => ({
        questionId: quiz.questions[index].id,
        selectedOptionId: answer.selectedOptionId,
        textAnswer: answer.textAnswer
      }))
    };

    this.api.startAttempt(quiz.id, attemptData).subscribe({
      next: (attempt) => {
        // After starting, we immediately submit with the answers
        this.api.submitAttempt(attempt.id, attemptData).subscribe({
          next: () => {
            this.messageService.add({
              severity: 'success',
              summary: 'Success',
              detail: 'Quiz submitted successfully'
            });
            this.router.navigate(['/quizzes', quiz.id, 'results', attempt.id]);
          },
          error: (err) => {
            this.messageService.add({
              severity: 'error',
              summary: 'Error',
              detail: 'Failed to submit quiz'
            });
            this.loading.set(false);
          }
        });
      },
      error: (err) => {
        this.messageService.add({
          severity: 'error',
          summary: 'Error',
          detail: 'Failed to start quiz'
        });
        this.loading.set(false);
      }
    });
  }

  onCancel(): void {
    this.router.navigate(['/quizzes']);
  }
}