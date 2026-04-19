import { ChangeDetectionStrategy, Component, input } from '@angular/core';

@Component({
  selector: 'app-page-shell',
  standalone: true,
  changeDetection: ChangeDetectionStrategy.OnPush,
  template: `
    <header class="flex flex-column gap-2 mb-3">
      <div class="flex align-items-start justify-content-between gap-3 flex-wrap">
        <div class="min-w-0">
          <h1 class="text-2xl font-semibold m-0">{{ title() }}</h1>
          @if (subtitle()) {
            <p class="text-600 mt-1 mb-0">{{ subtitle() }}</p>
          }
        </div>
        <div class="flex align-items-center gap-2">
          <ng-content select="[pageActions]" />
        </div>
      </div>
    </header>

    <section>
      <ng-content />
    </section>
  `,
})
export class PageShellComponent {
  readonly title = input.required<string>();
  readonly subtitle = input<string | null>(null);
}

