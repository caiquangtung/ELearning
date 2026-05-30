import { ChangeDetectionStrategy, Component, Input } from '@angular/core';
import { CommonModule } from '@angular/common';

@Component({
  selector: 'app-page-shell',
  standalone: true,
  imports: [CommonModule],
  changeDetection: ChangeDetectionStrategy.OnPush,
  template: `
    <header class="flex align-items-start gap-3 mb-3">
      <div class="flex-1 min-w-0">
        <h1 class="text-2xl font-semibold m-0">{{ title }}</h1>
        <p *ngIf="subtitle" class="text-600 mt-1 mb-0">{{ subtitle }}</p>
      </div>

      <div
        class="flex flex-wrap align-items-center justify-content-end gap-2 ml-auto page-actions"
      >
        <ng-content select="[pageActions]"></ng-content>
      </div>
    </header>

    <section class="page-content">
      <ng-content></ng-content>
    </section>
  `,
  styleUrls: ['./page-shell.component.scss'],
})
export class PageShellComponent {
  @Input() title = '';
  @Input() subtitle: string | null = null;
}
