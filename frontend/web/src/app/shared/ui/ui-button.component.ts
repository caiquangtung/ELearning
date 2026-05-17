import { Component, input, output } from '@angular/core';
import { Button } from 'primeng/button';

/**
 * Thin wrapper around PrimeNG Button component.
 * Enforces team conventions for loading states, accessibility, and consistent sizing.
 */
@Component({
  selector: 'app-ui-button',
  standalone: true,
  imports: [Button],
  template: `
    <p-button
      [label]="label()"
      [icon]="icon()"
      [loading]="loading() || pending()"
      [disabled]="disabled() || loading() || pending()"
      [severity]="severity()"
      [size]="size()"
      [outlined]="outlined()"
      [text]="text()"
      [raised]="raised()"
      [rounded]="rounded()"
      [type]="type()"
      [attr.aria-busy]="loading() || pending() ? 'true' : null"
      (onClick)="onClick.emit($event)"
    />
  `,
})
export class UiButtonComponent {
  // Required
  label = input.required<string>();

  // Optional
  icon = input<string>();
  loading = input(false);
  pending = input(false);
  disabled = input(false);
  severity = input<'success' | 'info' | 'warn' | 'danger' | 'help' | 'secondary' | 'contrast'>('secondary');
  size = input<'small' | 'large'>('small');
  outlined = input(false);
  text = input(false);
  raised = input(false);
  rounded = input(false);
  type = input<'button' | 'submit' | 'reset'>('button');

  // Events
  onClick = output<MouseEvent>();
}