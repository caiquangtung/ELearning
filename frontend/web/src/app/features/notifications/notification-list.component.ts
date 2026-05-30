import { DatePipe } from '@angular/common';
import { Component, inject, signal } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { RouterLink } from '@angular/router';
import { Button } from 'primeng/button';
import { Card } from 'primeng/card';
import { Checkbox } from 'primeng/checkbox';
import { Tag } from 'primeng/tag';
import { LmsApiService, NotificationDto } from '../../core/api/lms-api.service';
import { PageShellComponent } from '../../shared/ui';

@Component({
  selector: 'app-notification-list',
  standalone: true,
  imports: [
    Button,
    Card,
    Checkbox,
    DatePipe,
    FormsModule,
    RouterLink,
    Tag,
    PageShellComponent,
  ],
  templateUrl: './notification-list.component.html',
  styleUrl: './notification-list.component.scss',
})
export class NotificationListComponent {
  private readonly api = inject(LmsApiService);

  readonly items = signal<NotificationDto[]>([]);
  readonly unreadOnly = signal(false);

  constructor() {
    this.load();
  }

  load(): void {
    this.api
      .listNotifications({
        page: 1,
        pageSize: 50,
        unreadOnly: this.unreadOnly(),
      })
      .subscribe((page) => this.items.set(page.items));
  }

  toggleUnread(value: boolean): void {
    this.unreadOnly.set(value);
    this.load();
  }

  markRead(item: NotificationDto): void {
    this.api.markNotificationRead(item.id).subscribe((updated) => {
      this.items.update((items) =>
        items.map((x) => (x.id === updated.id ? updated : x)),
      );
    });
  }

  severity(
    item: NotificationDto,
  ): 'success' | 'secondary' | 'info' | 'warn' | 'danger' | 'contrast' {
    if (item.type === 'Warning') return 'warn';
    if (item.type === 'Reminder') return 'info';
    if (item.type === 'Announcement') return 'success';
    return 'secondary';
  }
}
