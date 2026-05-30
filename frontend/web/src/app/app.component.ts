import { Component, inject } from '@angular/core';
import { RouterOutlet } from '@angular/router';
import { Message } from 'primeng/message';
import { GlobalErrorService } from './core/error/global-error.service';
import { LoadingService } from './core/loading/loading.service';

@Component({
  selector: 'app-root',
  imports: [RouterOutlet, Message],
  template: `
    @if (errors.message(); as msg) {
      <p-message
        severity="error"
        [text]="msg"
        [closable]="true"
        styleClass="w-full"
        (onClose)="errors.clear()"
      />
    }

    @if (loading.isLoading()) {
      <div class="global-loading" role="status" aria-live="polite" aria-label="Loading">
        <div class="global-loading__panel">
          <i class="pi pi-spin pi-spinner"></i>
        </div>
      </div>
    }

    <router-outlet />
  `,
  styleUrl: './app.component.scss',
})
export class AppComponent {
  readonly errors = inject(GlobalErrorService);
  readonly loading = inject(LoadingService);
}
