import {
  ChangeDetectionStrategy,
  Component,
  EventEmitter,
  Input,
  Output,
} from '@angular/core';
import { CommonModule } from '@angular/common';
import { PageShellComponent } from '../page-shell/page-shell.component';
import { FormFooterComponent } from '../form-footer/form-footer.component';

@Component({
  selector: 'app-form-shell',
  standalone: true,
  imports: [CommonModule, PageShellComponent, FormFooterComponent],
  changeDetection: ChangeDetectionStrategy.OnPush,
  template: `
    <app-page-shell [title]="title" [subtitle]="subtitle">
      <ng-container pageActions>
        <ng-content select="[formActions]"></ng-content>
      </ng-container>

      <!-- Inline search removed: prefer popup search. Use [formActions] to place a search button that opens a dialog. -->
      <div class="form-body">
        <ng-content></ng-content>
      </div>

      <div class="form-footer">
        <ng-content select="[formFooter]"></ng-content>
        <app-form-footer
          *ngIf="showDefaultFooter"
          (save)="save.emit()"
          (cancel)="cancel.emit()"
          (delete)="delete.emit()"
        ></app-form-footer>
      </div>
    </app-page-shell>
  `,
  styleUrls: ['./form-shell.component.scss'],
})
export class FormShellComponent {
  @Input() title = '';
  @Input() subtitle: string | null = null;
  @Input() showDefaultFooter = true;

  @Output() save = new EventEmitter<void>();
  @Output() cancel = new EventEmitter<void>();
  @Output() delete = new EventEmitter<void>();
}
