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
    <div class="flex flex-column min-h-screen">
      <p-toolbar class="border-bottom-1 border-200 bg-white">
        <ng-template pTemplate="left">
          <h1 class="text-2xl font-semibold text-900 m-0">{{ title() }}</h1>
        </ng-template>
        <ng-template pTemplate="right">
          <ng-content select="[actions]" />
        </ng-template>
      </p-toolbar>

      <div class="flex-1 p-4 bg-gray-50">
        <ng-content />
      </div>
    </div>
  `,
  styles: [`
    :host {
      display: block;
    }
  `]
})
export class PageShellComponent {
  title = input.required<string>();
}