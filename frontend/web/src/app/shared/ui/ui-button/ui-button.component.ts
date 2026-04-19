import { ChangeDetectionStrategy, Component, input, output } from '@angular/core';
import { Button } from 'primeng/button';

@Component({
  selector: 'app-ui-button',
  standalone: true,
  imports: [Button],
  changeDetection: ChangeDetectionStrategy.OnPush,
  template: `
    <p-button
      [label]="label()"
      [icon]="icon()"
      [severity]="severity()"
      [text]="text()"
      [outlined]="outlined()"
      [rounded]="rounded()"
      [disabled]="disabled() || loading()"
      [loading]="loading()"
      [type]="type()"
      [styleClass]="styleClass()"
      [attr.aria-busy]="loading()"
      (onClick)="clicked.emit()"
    >
      <ng-content />
    </p-button>
  `,
})
export class UiButtonComponent {
  readonly label = input<string | undefined>(undefined);
  readonly icon = input<string | undefined>(undefined);
  readonly severity = input<'secondary' | 'success' | 'info' | 'warn' | 'danger' | 'help' | 'contrast' | undefined>(
    undefined,
  );
  readonly text = input(false);
  readonly outlined = input(false);
  readonly rounded = input(false);
  readonly disabled = input(false);
  readonly loading = input(false);
  readonly type = input<'button' | 'submit'>('button');
  readonly styleClass = input<string | undefined>(undefined);

  readonly clicked = output<void>();
}

