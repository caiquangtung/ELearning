import { ChangeDetectionStrategy, Component, input } from '@angular/core';

@Component({
  selector: 'app-page-shell',
  standalone: true,
  changeDetection: ChangeDetectionStrategy.OnPush,
  template: `
    <header class="page-shell-header">
      <div class="page-shell-title-row">
        <div class="min-w-0">
          <h1 id="page-shell-title" class="page-shell-title">{{ title() }}</h1>
          @if (subtitle()) {
            <p class="page-shell-subtitle">{{ subtitle() }}</p>
          }
        </div>
        <div class="page-shell-actions">
          <ng-content select="[pageActions]" />
        </div>
      </div>
    </header>

    <section class="page-shell-content" aria-labelledby="page-shell-title">
      <ng-content />
    </section>
  `,
  styleUrl: './page-shell.component.scss',
})
export class PageShellComponent {
  readonly title = input.required<string>();
  readonly subtitle = input<string | null>(null);
}
