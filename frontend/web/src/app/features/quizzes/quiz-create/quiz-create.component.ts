import { Component, inject, signal, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ReactiveFormsModule, FormBuilder, FormGroup, Validators } from '@angular/forms';
import { ActivatedRoute, Router } from '@angular/router';
import { LmsApiService } from '../../../core/api/lms-api.service';
import { GlobalErrorService } from '../../../core/error/global-error.service';
import { UiButtonComponent, PageShellComponent } from '../../../shared/ui';
import { InputText } from 'primeng/inputtext';
import { InputTextarea } from 'primeng/inputtextarea';
import { InputNumber } from 'primeng/inputnumber';
import { Toast } from 'primeng/toast';
import { MessageService } from 'primeng/api';

@Component({
  selector: 'app-quiz-create',
  standalone: true,
  imports: [
    CommonModule,
    ReactiveFormsModule,
    PageShellComponent,
    UiButtonComponent,
    InputText,
    InputTextarea,
    InputNumber,
    Toast
  ],
  providers: [MessageService],
  templateUrl: './quiz-create.component.html',
  styleUrl: './quiz-create.component.scss',
})
export class QuizCreateComponent implements OnInit {
  private readonly fb = inject(FormBuilder);
  private readonly api = inject(LmsApiService);
  private readonly route = inject(ActivatedRoute);
  private readonly router = inject(Router);
  private readonly errors = inject(GlobalErrorService);
  private readonly messageService = inject(MessageService);

  readonly quizForm: FormGroup;
  readonly isEditMode = signal(false);
  readonly loading = signal(false);
  private quizId: string | null = null;

  constructor() {
    this.quizForm = this.fb.group({
      title: ['', Validators.required],
      courseId: [null],
      lessonId: [null],
      description: [''],
      timeLimitMinutes: [null],
      passingScore: [null]
    });
  }

  ngOnInit(): void {
    this.route.paramMap.subscribe(params => {
      const id = params.get('id');
      if (id && id !== 'create') {
        this.isEditMode.set(true);
        this.quizId = id;
        this.loadQuiz(id);
      }
    });
  }

  loadQuiz(id: string): void {
    this.errors.clear();
    this.loading.set(true);

    this.api.getQuiz(id).subscribe({
      next: (quiz) => {
        this.quizForm.patchValue({
          title: quiz.title,
          courseId: quiz.courseId,
          lessonId: quiz.lessonId,
          description: quiz.description,
          timeLimitMinutes: quiz.timeLimitMinutes,
          passingScore: quiz.passingScore
        });
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

  onSubmit(): void {
    if (this.quizForm.invalid) {
      this.quizForm.markAllAsTouched();
      return;
    }

    this.errors.clear();
    this.loading.set(true);
    const quizData = this.quizForm.value;

    const request = this.isEditMode()
      ? this.api.updateQuiz(this.quizId!, quizData)
      : this.api.createQuiz(quizData);

    request.subscribe({
      next: () => {
        this.messageService.add({
          severity: 'success',
          summary: 'Success',
          detail: `Quiz ${this.isEditMode() ? 'updated' : 'created'} successfully`
        });
        this.router.navigate(['/quizzes']);
      },
      error: (err) => {
        this.messageService.add({
          severity: 'error',
          summary: 'Error',
          detail: `Failed to ${this.isEditMode() ? 'update' : 'create'} quiz`
        });
        this.loading.set(false);
      }
    });
  }

  onCancel(): void {
    this.router.navigate(['/quizzes']);
  }
}