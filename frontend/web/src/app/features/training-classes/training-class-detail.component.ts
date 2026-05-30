import { DatePipe } from '@angular/common';
import { Component, inject, OnInit, signal } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { ActivatedRoute, RouterLink, RouterModule } from '@angular/router';
import { Button } from 'primeng/button';
import { DropdownModule } from 'primeng/dropdown';
import { InputText } from 'primeng/inputtext';
import { Panel } from 'primeng/panel';
import { PrimeTemplate } from 'primeng/api';
import { TableModule } from 'primeng/table';
import { Tag } from 'primeng/tag';
import {
  ClassSessionDto,
  LmsApiService,
  TrainingClassDetailDto,
} from '../../core/api/lms-api.service';
import { AuthService } from '../../core/auth/auth.service';
import { GlobalErrorService } from '../../core/error/global-error.service';

function toLocalInput(iso: string): string {
  const d = new Date(iso);
  const pad = (n: number) => String(n).padStart(2, '0');
  return `${d.getFullYear()}-${pad(d.getMonth() + 1)}-${pad(d.getDate())}T${pad(d.getHours())}:${pad(d.getMinutes())}`;
}

function toIso(local: string): string {
  return new Date(local).toISOString();
}

@Component({
  selector: 'app-training-class-detail',
  standalone: true,
  imports: [
    DatePipe,
    FormsModule,
    RouterLink,
    RouterModule,
    TableModule,
    Button,
    Panel,
    InputText,
    DropdownModule,
    Tag,
    PrimeTemplate,
  ],
  templateUrl: './training-class-detail.component.html',
})
export class TrainingClassDetailComponent implements OnInit {
  private readonly api = inject(LmsApiService);
  private readonly route = inject(ActivatedRoute);
  private readonly errors = inject(GlobalErrorService);
  readonly auth = inject(AuthService);

  readonly tc = signal<TrainingClassDetailDto | null>(null);
  readonly loading = signal(true);
  readonly editingSessionId = signal<string | null>(null);
  readonly sessionPending = signal(false);
  readonly instructorPending = signal(false);
  showSessionForm = false;
  showInstructorForm = false;

  classId = '';

  sessTitle = '';
  sessType = 'Zoom';
  sessStart = '';
  sessEnd = '';
  sessLocation = '';
  instructorUserId = '';

  readonly sessionTypeOptions = [
    { label: 'Zoom', value: 'Zoom' },
    { label: 'Offline', value: 'Offline' },
    { label: 'Vod', value: 'Vod' },
  ];

  canManageSessions(): boolean {
    const roles = this.auth.user()?.roles ?? [];
    return roles.some(
      (r) => r === 'Admin' || r === 'OrgAdmin' || r === 'Instructor',
    );
  }

  canCheckout(t: TrainingClassDetailDto): boolean {
    return t.priceCents > 0 && t.status !== 'Cancelled';
  }

  ngOnInit(): void {
    this.errors.clear();
    const id = this.route.snapshot.paramMap.get('id');
    if (!id) {
      this.loading.set(false);
      return;
    }
    this.classId = id;
    this.load();
  }

  load(): void {
    this.loading.set(true);
    this.api.getTrainingClass(this.classId).subscribe({
      next: (t) => {
        this.tc.set(t);
        this.loading.set(false);
      },
      error: () => this.loading.set(false),
    });
  }

  startEdit(s: ClassSessionDto): void {
    if (s.status === 'Cancelled') return;
    this.editingSessionId.set(s.id);
    this.sessTitle = s.title;
    this.sessType = s.sessionType;
    this.sessStart = toLocalInput(s.startUtc);
    this.sessEnd = toLocalInput(s.endUtc);
    this.sessLocation = s.location ?? '';
  }

  clearEdit(): void {
    this.editingSessionId.set(null);
    this.sessTitle = '';
    this.sessStart = '';
    this.sessEnd = '';
    this.sessLocation = '';
  }

  saveSession(): void {
    if (!this.sessTitle.trim() || !this.sessStart || !this.sessEnd) return;
    const body = {
      title: this.sessTitle.trim(),
      sessionType: this.sessType,
      startUtc: toIso(this.sessStart),
      endUtc: toIso(this.sessEnd),
      location: this.sessLocation.trim() || null,
    };
    this.errors.clear();
    this.sessionPending.set(true);
    const editId = this.editingSessionId();
    const req = editId
      ? this.api.updateSession(this.classId, editId, body)
      : this.api.scheduleSession(this.classId, body);
    req.subscribe({
      next: () => {
        this.sessionPending.set(false);
        this.clearEdit();
        this.load();
      },
      error: () => this.sessionPending.set(false),
    });
  }

  cancelSession(sessionId: string): void {
    this.errors.clear();
    this.api.cancelSession(this.classId, sessionId).subscribe({
      next: () => this.load(),
    });
  }

  assignInstructor(): void {
    const uid = this.instructorUserId.trim();
    if (!uid) return;
    this.errors.clear();
    this.instructorPending.set(true);
    this.api.assignInstructor(this.classId, uid).subscribe({
      next: () => {
        this.instructorUserId = '';
        this.instructorPending.set(false);
        this.load();
      },
      error: () => this.instructorPending.set(false),
    });
  }
}
