import { DatePipe } from '@angular/common';
import { Component, inject, signal } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { RouterLink } from '@angular/router';
import { Button } from 'primeng/button';
import { Card } from 'primeng/card';
import { Checkbox } from 'primeng/checkbox';
import { Tag } from 'primeng/tag';
import { LmsApiService, NotificationDto } from '../../core/api/lms-api.service';

@Component({
  selector: 'app-notification-list',
  standalone: true,
  imports: [Button, Card, Checkbox, DatePipe, FormsModule, RouterLink, Tag],
  template: `
    <div class="page">
      <div class="page-header">
        <div>
          <h1>Notifications</h1>
          <p>Recent updates, reminders, and announcements.</p>
        </div>
        <div class="actions">
          <label class="unread-filter">
            <p-checkbox [binary]="true" [ngModel]="unreadOnly()" (ngModelChange)="toggleUnread($event)" inputId="unreadOnly" />
            <span>Unread only</span>
          </label>
          <p-button icon="pi pi-refresh" severity="secondary" [outlined]="true" (onClick)="load()" ariaLabel="Refresh notifications" />
          <p-button label="Announcement" icon="pi pi-send" routerLink="/notifications/announcements" />
        </div>
      </div>

      <div class="notification-list">
        @for (item of items(); track item.id) {
          <p-card [styleClass]="item.isRead ? 'notification-card' : 'notification-card unread'">
            <div class="notification-row">
              <div>
                <div class="notification-title">
                  <span>{{ item.title }}</span>
                  <p-tag [value]="item.type" [severity]="severity(item)" />
                </div>
                <p>{{ item.body }}</p>
                <div class="meta">{{ item.createdAt | date: 'medium' }}</div>
              </div>
              <div class="row-actions">
                @if (item.actionUrl) {
                  <a class="action-link" [routerLink]="item.actionUrl">Open</a>
                }
                @if (!item.isRead) {
                  <p-button label="Mark read" icon="pi pi-check" size="small" [outlined]="true" (onClick)="markRead(item)" />
                }
              </div>
            </div>
          </p-card>
        } @empty {
          <p-card>
            <p class="empty">No notifications found.</p>
          </p-card>
        }
      </div>
    </div>
  `,
  styles: [`
    .page { padding: 1.5rem; }
    .page-header { display: flex; justify-content: space-between; align-items: flex-start; gap: 1rem; margin-bottom: 1rem; }
    h1 { margin: 0; font-size: 1.75rem; }
    .page-header p { margin: .25rem 0 0; color: var(--text-color-secondary); }
    .actions { display: flex; align-items: center; gap: .75rem; flex-wrap: wrap; justify-content: flex-end; }
    .unread-filter { display: flex; align-items: center; gap: .5rem; white-space: nowrap; }
    .notification-list { display: grid; gap: .75rem; }
    .notification-row { display: grid; grid-template-columns: minmax(0, 1fr) auto; gap: 1rem; align-items: start; }
    .notification-title { display: flex; align-items: center; gap: .5rem; font-weight: 700; }
    .notification-row p { margin: .5rem 0; color: var(--text-color); }
    .meta, .empty { color: var(--text-color-secondary); }
    .row-actions { display: flex; align-items: center; gap: .5rem; }
    .action-link { color: var(--primary-color); text-decoration: none; font-weight: 600; }
    :host ::ng-deep .notification-card.unread { border-left: 4px solid var(--primary-color); }
    @media (max-width: 720px) {
      .page-header, .notification-row { display: block; }
      .actions, .row-actions { justify-content: flex-start; margin-top: .75rem; }
    }
  `],
})
export class NotificationListComponent {
  private readonly api = inject(LmsApiService);

  readonly items = signal<NotificationDto[]>([]);
  readonly unreadOnly = signal(false);

  constructor() {
    this.load();
  }

  load(): void {
    this.api.listNotifications(1, 50, this.unreadOnly()).subscribe((page) => this.items.set(page.items));
  }

  toggleUnread(value: boolean): void {
    this.unreadOnly.set(value);
    this.load();
  }

  markRead(item: NotificationDto): void {
    this.api.markNotificationRead(item.id).subscribe((updated) => {
      this.items.update((items) => items.map((x) => (x.id === updated.id ? updated : x)));
    });
  }

  severity(item: NotificationDto): 'success' | 'secondary' | 'info' | 'warn' | 'danger' | 'contrast' {
    if (item.type === 'Warning') return 'warn';
    if (item.type === 'Reminder') return 'info';
    if (item.type === 'Announcement') return 'success';
    return 'secondary';
  }
}
