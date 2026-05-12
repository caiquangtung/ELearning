import { Routes } from '@angular/router';

export const QUIZZES_ROUTES: Routes = [
  {
    path: '',
    loadComponent: () => import('./quiz-list/quiz-list.component').then(m => m.QuizListComponent)
  },
  {
    path: 'create',
    loadComponent: () => import('./quiz-create/quiz-create.component').then(m => m.QuizCreateComponent)
  },
  {
    path: ':id',
    loadComponent: () => import('./quiz-detail/quiz-detail.component').then(m => m.QuizDetailComponent)
  },
  {
    path: ':id/edit',
    loadComponent: () => import('./quiz-create/quiz-create.component').then(m => m.QuizCreateComponent)
  },
  {
    path: ':id/take',
    loadComponent: () => import('./quiz-take/quiz-take.component').then(m => m.QuizTakeComponent)
  },
  {
    path: ':id/results',
    loadComponent: () => import('./quiz-results/quiz-results.component').then(m => m.QuizResultsComponent)
  },
  {
    path: ':id/analytics',
    loadComponent: () => import('./quiz-analytics/quiz-analytics.component').then(m => m.QuizAnalyticsComponent)
  },
  {
    path: ':id/grade',
    loadComponent: () => import('./quiz-grade/quiz-grade.component').then(m => m.QuizGradeComponent)
  }
];