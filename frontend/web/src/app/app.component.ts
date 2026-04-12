import { Component, inject } from '@angular/core';
import { RouterOutlet } from '@angular/router';
import { Message } from 'primeng/message';
import { GlobalErrorService } from './core/error/global-error.service';

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
    <router-outlet />
  `,
  styles: `
    :host ::ng-deep .p-message {
      border-radius: 0;
    }
  `,
})
export class AppComponent {
  readonly errors = inject(GlobalErrorService);
}
