import { Component, input } from '@angular/core';
import { Toolbar } from 'primeng/toolbar';

/**
 * Page shell wrapper for consistent page layout.
 * Provides title, actions slot, and main content area.
 */
@Component({
  selector: 'app-page-shell',
  standalone: true,
  imports: [Toolbar],
  template: `
    <div class="legacy-page-shell">
      <p-toolbar styleClass="legacy-page-toolbar">
        <ng-template pTemplate="left">
          <h1 id="legacy-page-title">{{ title() }}</h1>
        </ng-template>
        <ng-template pTemplate="right">
          <ng-content select="[actions]" />
          <ng-content select="[pageActions]" />
        </ng-template>
      </p-toolbar>

      <section class="legacy-page-content" aria-labelledby="legacy-page-title">
        <ng-content />
      </section>
    </div>
  `,
  styleUrl: './page-shell.component.scss',
})
export class PageShellComponent {
  title = input.required<string>();
}
