import { ChangeDetectionStrategy, Component, Input } from '@angular/core';
import { CommonModule } from '@angular/common';

@Component({
  selector: 'app-page-shell',
  standalone: true,
  imports: [CommonModule],
  changeDetection: ChangeDetectionStrategy.OnPush,
  template: `
    <header class="page-shell__header">
      <div class="page-shell__title-block">
        <h1>{{ title }}</h1>
        <p *ngIf="subtitle">{{ subtitle }}</p>
      </div>

      <div class="page-shell__actions">
        <ng-content select="[pageActions]"></ng-content>
      </div>
    </header>

    <section class="page-shell__content page-content">
      <ng-content></ng-content>
    </section>
  `,
  styleUrls: ['./page-shell.component.scss'],
})
export class PageShellComponent {
  @Input() title = '';
  @Input() subtitle: string | null = null;
}
