import { Component, inject, signal } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { RouterLink } from '@angular/router';
import { Button } from 'primeng/button';
import { Card } from 'primeng/card';
import { DropdownModule } from 'primeng/dropdown';
import { InputText } from 'primeng/inputtext';
import { InputTextarea } from 'primeng/inputtextarea';
import { Message } from 'primeng/message';
import { LmsApiService } from '../../core/api/lms-api.service';

@Component({
  selector: 'app-announcement',
  standalone: true,
  imports: [
    Button,
    Card,
    DropdownModule,
    FormsModule,
    InputText,
    InputTextarea,
    Message,
    RouterLink,
  ],
  templateUrl: './announcement.component.html',
  styleUrl: './announcement.component.scss',
})
export class AnnouncementComponent {
  private readonly api = inject(LmsApiService);

  readonly scopeOptions = [
    { label: 'Direct', value: 'Direct' },
    { label: 'Course', value: 'Course' },
    { label: 'Training class', value: 'TrainingClass' },
    { label: 'Organization', value: 'Organization' },
    { label: 'Platform', value: 'Platform' },
  ];

  recipientText = '';
  scope = 'Direct';
  actionUrl = '';
  subject = '';
  body = '';
  readonly submitting = signal(false);
  readonly success = signal<string | null>(null);
  readonly error = signal<string | null>(null);

  submit(): void {
    const recipientUserIds = this.recipientText
      .split(/\r?\n|,/)
      .map((x) => x.trim())
      .filter(Boolean);

    if (!recipientUserIds.length || !this.subject.trim() || !this.body.trim()) {
      this.error.set('Recipients, subject, and body are required.');
      return;
    }

    this.submitting.set(true);
    this.error.set(null);
    this.success.set(null);
    this.api
      .sendAnnouncement({
        recipientUserIds,
        subject: this.subject.trim(),
        body: this.body.trim(),
        scope: this.scope,
        organizationId: null,
        courseId: null,
        trainingClassId: null,
        actionUrl: this.actionUrl.trim() || null,
      })
      .subscribe({
        next: (message) => {
          this.success.set(
            `Announcement sent to ${message.recipientCount} recipient(s).`,
          );
          this.submitting.set(false);
        },
        error: () => this.submitting.set(false),
      });
  }
}
