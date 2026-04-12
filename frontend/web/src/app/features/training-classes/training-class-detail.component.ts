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
  template: `
    <p-button label="Back" icon="pi pi-arrow-left" [text]="true" routerLink="/training-classes" styleClass="mb-3" />
    @if (loading()) {
      <p>Loading…</p>
    } @else {
      @if (tc(); as t) {
        <h1 class="text-2xl font-semibold mt-0">{{ t.title }}</h1>
        <p class="text-color-secondary flex align-items-center gap-2 flex-wrap">
          <p-tag [value]="t.status" severity="info" />
          <span>Max learners: {{ t.maxLearners }}</span>
        </p>
        <p-button label="View course" icon="pi pi-book" [text]="true" [routerLink]="['/courses', t.courseId]" styleClass="mb-3 p-0" />
        @if (canManageSessions()) {
          <p-panel header="Schedule / update session" [toggleable]="true" styleClass="mb-3">
            <div class="flex flex-column gap-3" style="max-width: 32rem">
              <input pInputText [(ngModel)]="sessTitle" placeholder="Session title" class="w-full" name="stitle" />
              <p-dropdown
                [options]="sessionTypeOptions"
                [(ngModel)]="sessType"
                optionLabel="label"
                optionValue="value"
                placeholder="Type"
                styleClass="w-full"
                name="stype"
              />
              <div class="flex flex-column gap-2">
                <label class="text-sm font-medium" for="sess-start">Start (local)</label>
                <input id="sess-start" type="datetime-local" pInputText [(ngModel)]="sessStart" class="w-full" name="sstart" />
              </div>
              <div class="flex flex-column gap-2">
                <label class="text-sm font-medium" for="sess-end">End (local)</label>
                <input id="sess-end" type="datetime-local" pInputText [(ngModel)]="sessEnd" class="w-full" name="send" />
              </div>
              <input pInputText [(ngModel)]="sessLocation" placeholder="Location (required for Offline)" class="w-full" name="sloc" />
              <div class="flex flex-wrap gap-2">
                <p-button
                  [label]="editingSessionId() ? 'Update session' : 'Schedule session'"
                  icon="pi pi-calendar-plus"
                  [loading]="sessionPending()"
                  (onClick)="saveSession()"
                />
                @if (editingSessionId()) {
                  <p-button label="Cancel edit" severity="secondary" [outlined]="true" type="button" (onClick)="clearEdit()" />
                }
              </div>
            </div>
          </p-panel>
          <p-panel header="Assign instructor" [toggleable]="true" styleClass="mb-4">
            <div class="flex flex-wrap gap-2 align-items-end">
              <input pInputText [(ngModel)]="instructorUserId" placeholder="User ID" class="w-full md:w-25rem" name="iid" />
              <p-button
                label="Assign"
                icon="pi pi-user-plus"
                [loading]="instructorPending()"
                (onClick)="assignInstructor()"
              />
            </div>
          </p-panel>
        }
        <h2 class="text-xl">Sessions</h2>
        <p-table [value]="t.sessions" styleClass="p-datatable-sm mb-4" [tableStyle]="{ 'min-width': '50rem' }">
          <ng-template pTemplate="header">
            <tr>
              <th>Title</th>
              <th>Type</th>
              <th>Start (UTC)</th>
              <th>End (UTC)</th>
              <th>Location / Zoom</th>
              <th>Status</th>
              <th></th>
            </tr>
          </ng-template>
          <ng-template pTemplate="body" let-s>
            <tr>
              <td>{{ s.title }}</td>
              <td>{{ s.sessionType }}</td>
              <td>{{ s.startUtc | date: 'medium' }}</td>
              <td>{{ s.endUtc | date: 'medium' }}</td>
              <td>
                @if (s.zoomJoinUrl) {
                  <a [href]="s.zoomJoinUrl" target="_blank" rel="noopener" class="text-primary">Join</a>
                } @else if (s.location) {
                  {{ s.location }}
                } @else {
                  —
                }
              </td>
              <td><p-tag [value]="s.status" [severity]="s.status === 'Cancelled' ? 'danger' : 'success'" /></td>
              <td>
                @if (canManageSessions() && s.status !== 'Cancelled') {
                  <p-button icon="pi pi-pencil" [rounded]="true" [text]="true" (onClick)="startEdit(s)" />
                  <p-button icon="pi pi-times" severity="danger" [rounded]="true" [text]="true" (onClick)="cancelSession(s.id)" />
                }
              </td>
            </tr>
          </ng-template>
        </p-table>
        <h2 class="text-xl">Instructors</h2>
        <p-table [value]="t.instructors" styleClass="p-datatable-sm" [tableStyle]="{ 'min-width': '28rem' }">
          <ng-template pTemplate="header">
            <tr>
              <th>User ID</th>
              <th>Assigned</th>
            </tr>
          </ng-template>
          <ng-template pTemplate="body" let-i>
            <tr>
              <td class="font-mono text-sm">{{ i.userId }}</td>
              <td>{{ i.assignedAt | date: 'short' }}</td>
            </tr>
          </ng-template>
        </p-table>
      }
    }
  `,
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
    return roles.some((r) => r === 'Admin' || r === 'OrgAdmin' || r === 'Instructor');
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
