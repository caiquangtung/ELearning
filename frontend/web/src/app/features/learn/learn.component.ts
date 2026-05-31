import { Component, inject, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterModule } from '@angular/router';
import { FormsModule } from '@angular/forms';
import { DialogModule } from 'primeng/dialog';
import {
  CourseRecommendationDto,
  LearningPathDraftDto,
  LmsApiService,
  StudentDashboardDto,
} from '../../core/api/lms-api.service';

@Component({
  selector: 'app-learn',
  standalone: true,
  imports: [CommonModule, RouterModule, FormsModule, DialogModule],
  templateUrl: './learn.component.html',
  styleUrls: ['./learn.component.scss'],
})
export class LearnComponent {
  private readonly api = inject(LmsApiService);

  readonly dashboard = signal<StudentDashboardDto | null>(null);
  readonly recommendations = signal<CourseRecommendationDto[]>([]);
  readonly learningPath = signal<LearningPathDraftDto | null>(null);
  readonly isLoading = signal(true);
  readonly isGenerating = signal(false);
  readonly error = signal<string | null>(null);
  readonly isPathDialogOpen = signal(false);

  pathForm = {
    goal: 'I want to become a backend developer',
    currentSkills: 'Basic programming, SQL',
    targetRole: 'Backend Developer',
    maxCourses: 5,
  };

  constructor() {
    this.load();
  }

  load(): void {
    this.isLoading.set(true);
    this.error.set(null);

    this.api.getStudentDashboard().subscribe({
      next: (value) => this.dashboard.set(value),
      error: () => this.error.set('Unable to load learner dashboard.'),
    });

    this.api.getCourseRecommendations(6).subscribe({
      next: (value) => this.recommendations.set(value.items ?? []),
      error: () => this.recommendations.set([]),
      complete: () => this.isLoading.set(false),
    });
  }

  openPathDialog(): void {
    this.learningPath.set(null);
    this.isPathDialogOpen.set(true);
  }

  closePathDialog(): void {
    this.isPathDialogOpen.set(false);
  }

  generatePath(): void {
    if (!this.pathForm.goal.trim()) {
      this.error.set('Learning goal is required.');
      return;
    }

    this.isGenerating.set(true);
    this.error.set(null);
    this.api
      .generateLearningPath({
        goal: this.pathForm.goal.trim(),
        currentSkills: this.pathForm.currentSkills.trim() || null,
        targetRole: this.pathForm.targetRole.trim() || null,
        organizationId: null,
        maxCourses: Number(this.pathForm.maxCourses) || 5,
      })
      .subscribe({
        next: (value) => this.learningPath.set(value),
        error: () => this.error.set('Unable to generate learning path.'),
        complete: () => this.isGenerating.set(false),
      });
  }

  money(cents: number, currency: string): string {
    return new Intl.NumberFormat(undefined, {
      style: 'currency',
      currency: currency || 'USD',
    }).format(cents / 100);
  }
}
