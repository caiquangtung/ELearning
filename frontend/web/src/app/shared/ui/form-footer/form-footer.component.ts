import {
  ChangeDetectionStrategy,
  Component,
  EventEmitter,
  Input,
  Output,
} from '@angular/core';
import { CommonModule } from '@angular/common';
import { UiButtonComponent } from '../ui-button.component';

@Component({
  selector: 'app-form-footer',
  standalone: true,
  imports: [CommonModule, UiButtonComponent],
  changeDetection: ChangeDetectionStrategy.OnPush,
  template: `
    <div class="form-footer-actions">
      <app-ui-button
        *ngIf="showCancel"
        label="{{ cancelLabel }}"
        severity="secondary"
        [text]="true"
        (clicked)="cancel.emit()"
      ></app-ui-button>
      <app-ui-button
        *ngIf="showDelete"
        label="{{ deleteLabel }}"
        icon="pi pi-trash"
        severity="danger"
        (clicked)="delete.emit()"
      ></app-ui-button>
      <app-ui-button
        *ngIf="showSave"
        label="{{ saveLabel }}"
        icon="pi pi-save"
        severity="success"
        (clicked)="save.emit()"
      ></app-ui-button>
    </div>
  `,
  styleUrls: ['./form-footer.component.scss'],
})
export class FormFooterComponent {
  @Input() showSave = true;
  @Input() showCancel = true;
  @Input() showDelete = false;

  @Input() saveLabel = 'Save';
  @Input() cancelLabel = 'Cancel';
  @Input() deleteLabel = 'Delete';

  @Output() save = new EventEmitter<void>();
  @Output() cancel = new EventEmitter<void>();
  @Output() delete = new EventEmitter<void>();
}
