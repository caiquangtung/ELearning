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
  templateUrl: './quiz-detail.component.html',
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
