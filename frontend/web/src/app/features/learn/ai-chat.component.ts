import { CommonModule } from '@angular/common';
import { Component, inject, signal } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { ActivatedRoute, Router, RouterModule } from '@angular/router';
import { DropdownModule } from 'primeng/dropdown';
import {
  AiAccessibleCourseDto,
  AiChatAnswerDto,
  AiChatMessageDto,
  AiChatSessionDto,
  LmsApiService,
} from '../../core/api/lms-api.service';

@Component({
  selector: 'app-ai-chat',
  standalone: true,
  imports: [CommonModule, FormsModule, RouterModule, DropdownModule],
  templateUrl: './ai-chat.component.html',
  styleUrls: ['./ai-chat.component.scss'],
})
export class AiChatComponent {
  private readonly api = inject(LmsApiService);
  private readonly route = inject(ActivatedRoute);
  private readonly router = inject(Router);

  readonly sessions = signal<AiChatSessionDto[]>([]);
  readonly activeSession = signal<AiChatSessionDto | null>(null);
  readonly messages = signal<AiChatMessageDto[]>([]);
  readonly courses = signal<AiAccessibleCourseDto[]>([]);
  readonly selectedCourseId = signal<string | null>(null);
  readonly isLoading = signal(true);
  readonly isLoadingCourses = signal(false);
  readonly isSending = signal(false);
  readonly error = signal<string | null>(null);

  draft = '';

  readonly courseOptions = () => {
    const items = this.courses();
    return [{ id: '', title: 'All course material', pinned: true }, ...items];
  };

  constructor() {
    this.loadSessions();
    this.loadCourses();
  }

  loadSessions(): void {
    this.isLoading.set(true);
    this.error.set(null);
    this.api.listAiChatSessions().subscribe({
      next: (sessions) => {
        this.sessions.set(sessions);
        const first = sessions[0] ?? null;
        this.activeSession.set(first);
        if (first) {
          this.loadMessages(first.id);
        } else {
          this.messages.set([]);
        }
      },
      error: () => this.error.set('Unable to load AI tutor sessions.'),
      complete: () => this.isLoading.set(false),
    });
  }

  loadCourses(): void {
    this.isLoadingCourses.set(true);
    this.api.listAccessibleAiCourses().subscribe({
      next: (courses) => this.courses.set(courses ?? []),
      error: () => this.courses.set([]),
      complete: () => this.isLoadingCourses.set(false),
    });

    const param = this.route.snapshot.queryParamMap.get('courseId');
    if (param) {
      this.selectedCourseId.set(param);
    }
  }

  createSession(): void {
    this.error.set(null);
    this.api
      .createAiChatSession({ courseId: this.resolveCourseId(), title: null })
      .subscribe({
        next: (session) => {
          this.sessions.set([session, ...this.sessions()]);
          this.activeSession.set(session);
          this.messages.set([]);
        },
        error: () => this.error.set('Unable to create an AI tutor session.'),
      });
  }

  selectSession(session: AiChatSessionDto): void {
    this.activeSession.set(session);
    this.loadMessages(session.id);
  }

  onCourseChange(): void {
    this.router.navigate([], {
      relativeTo: this.route,
      queryParams: { courseId: this.resolveCourseId() || null },
      queryParamsHandling: 'merge',
      replaceUrl: true,
    });
  }

  send(): void {
    const message = this.draft.trim();
    if (!message || this.isSending()) return;

    const session = this.activeSession();
    if (!session) {
      this.api
        .createAiChatSession({ courseId: this.resolveCourseId(), title: null })
        .subscribe({
          next: (created) => {
            this.sessions.set([created, ...this.sessions()]);
            this.activeSession.set(created);
            this.messages.set([]);
            this.sendToSession(created.id, message);
          },
          error: () => this.error.set('Unable to create an AI tutor session.'),
        });
      return;
    }

    this.sendToSession(session.id, message);
  }

  private resolveCourseId(): string | null {
    const value = this.selectedCourseId();
    return value ? value : null;
  }

  private sendToSession(sessionId: string, message: string): void {
    this.draft = '';
    this.error.set(null);
    this.isSending.set(true);
    const userMessage = this.localMessage('User', message);
    this.messages.set([...this.messages(), userMessage]);

    this.api.sendAiChatMessage(sessionId, { message }).subscribe({
      next: (answer) => {
        this.messages.set([...this.messages(), this.answerToMessage(answer)]);
        this.loadSessions();
      },
      error: () => {
        this.error.set('AI tutor could not answer right now.');
        this.messages.set(this.messages().filter((x) => x.id !== userMessage.id));
      },
      complete: () => this.isSending.set(false),
    });
  }

  private loadMessages(sessionId: string): void {
    this.api.getAiChatMessages(sessionId).subscribe({
      next: (messages) => this.messages.set(messages),
      error: () => this.error.set('Unable to load this AI tutor conversation.'),
    });
  }

  private localMessage(role: string, content: string): AiChatMessageDto {
    return {
      id: `local-${Date.now()}`,
      role,
      content,
      citations: [],
      provider: null,
      model: null,
      promptVersion: null,
      confidence: null,
      usedContext: false,
      createdAt: new Date().toISOString(),
    };
  }

  private answerToMessage(answer: AiChatAnswerDto): AiChatMessageDto {
    return {
      id: answer.messageId,
      role: 'Assistant',
      content: answer.answer,
      citations: answer.citations,
      provider: answer.provider,
      model: answer.model,
      promptVersion: answer.promptVersion,
      confidence: answer.confidence,
      usedContext: answer.usedContext,
      createdAt: new Date().toISOString(),
    };
  }
}
