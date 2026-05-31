import { Component, inject, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterModule } from '@angular/router';
import {
  InstructorDashboardDto,
  LmsApiService,
  QuizListItemDto,
  TrainingClassListItemDto,
  CourseListItemDto,
} from '../../core/api/lms-api.service';
import { PagedList } from '../../core/models/paged-list.model';

@Component({
  selector: 'app-teach',
  standalone: true,
  imports: [CommonModule, RouterModule],
  templateUrl: './teach.component.html',
  styleUrls: ['./teach.component.scss'],
})
export class TeachComponent {
  private readonly api = inject(LmsApiService);

  readonly dashboard = signal<InstructorDashboardDto | null>(null);
  readonly classes = signal<TrainingClassListItemDto[]>([]);
  readonly courses = signal<CourseListItemDto[]>([]);
  readonly quizzes = signal<QuizListItemDto[]>([]);
  readonly error = signal<string | null>(null);

  constructor() {
    this.load();
  }

  load(): void {
    this.error.set(null);
    this.api.getInstructorDashboard().subscribe({
      next: (value) => this.dashboard.set(value),
      error: () => this.error.set('Unable to load teacher dashboard.'),
    });
    this.api.listTrainingClasses({ page: 1, pageSize: 4 }).subscribe({
      next: (value: PagedList<TrainingClassListItemDto>) => this.classes.set(value.items ?? []),
      error: () => this.classes.set([]),
    });
    this.api.listCourses({ page: 1, pageSize: 4, status: null, sort: 'Newest' }).subscribe({
      next: (value: PagedList<CourseListItemDto>) => this.courses.set(value.items ?? []),
      error: () => this.courses.set([]),
    });
    this.api.listQuizzes({ page: 1, pageSize: 4 }).subscribe({
      next: (value: PagedList<QuizListItemDto>) => this.quizzes.set(value.items ?? []),
      error: () => this.quizzes.set([]),
    });
  }
}
