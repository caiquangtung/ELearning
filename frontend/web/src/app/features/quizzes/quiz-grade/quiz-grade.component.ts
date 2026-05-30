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