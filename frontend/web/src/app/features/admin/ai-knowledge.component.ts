import { CommonModule, DatePipe } from '@angular/common';
import { Component, OnInit, computed, inject, signal } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { InputTextModule } from 'primeng/inputtext';
import {
  AiKnowledgeStatusDto,
  LmsApiService,
  ReindexAiKnowledgeDto,
} from '../../core/api/lms-api.service';
import { GlobalErrorService } from '../../core/error/global-error.service';
import { PageShellComponent } from '../../shared/ui';
import { UiButtonComponent } from '../../shared/ui/ui-button/ui-button.component';

@Component({
  selector: 'app-ai-knowledge',
  standalone: true,
  imports: [
    CommonModule,
    DatePipe,
    FormsModule,
    InputTextModule,
    PageShellComponent,
    UiButtonComponent,
  ],
  templateUrl: './ai-knowledge.component.html',
  styleUrl: './ai-knowledge.component.scss',
})
export class AiKnowledgeComponent implements OnInit {
  private readonly api = inject(LmsApiService);
  private readonly errors = inject(GlobalErrorService);

  readonly status = signal<AiKnowledgeStatusDto | null>(null);
  readonly loading = signal(true);
  readonly reindexing = signal(false);
  readonly evaluating = signal(false);
  readonly lastResult = signal<ReindexAiKnowledgeDto | null>(null);
  courseId = '';

  readonly vectorCoverage = computed(() => {
    const value = this.status();
    if (!value || value.totalChunks === 0) return 0;
    return Math.round((value.vectorizedChunks / value.totalChunks) * 100);
  });

  ngOnInit(): void {
    this.reload();
  }

  reload(): void {
    this.errors.clear();
    this.loading.set(true);
    this.api.getAiKnowledgeStatus().subscribe({
      next: (status) => {
        this.status.set(status);
        this.loading.set(false);
      },
      error: () => this.loading.set(false),
    });
  }

  reindexAll(): void {
    this.runReindex(null);
  }

  reindexCourse(): void {
    const value = this.courseId.trim();
    this.runReindex(value.length === 0 ? null : value);
  }

  runEvaluation(): void {
    if (this.evaluating()) return;
    this.errors.clear();
    this.evaluating.set(true);
    this.api.runRagEvaluation().subscribe({
      next: () => {
        this.evaluating.set(false);
        this.reload();
      },
      error: () => this.evaluating.set(false),
    });
  }

  percent(value: number): number {
    return Math.round(value * 100);
  }

  private runReindex(courseId: string | null): void {
    if (this.reindexing()) return;
    this.errors.clear();
    this.reindexing.set(true);
    this.lastResult.set(null);
    this.api.reindexAiKnowledge({ courseId }).subscribe({
      next: (result) => {
        this.lastResult.set(result);
        this.reindexing.set(false);
        this.reload();
      },
      error: () => this.reindexing.set(false),
    });
  }
}
