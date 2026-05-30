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
  templateUrl: './quiz-take.component.html',
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