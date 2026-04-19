import { NgTemplateOutlet } from '@angular/common';
import { ChangeDetectionStrategy, Component, ContentChild, input, output } from '@angular/core';
import { Paginator, PaginatorState } from 'primeng/paginator';
import { PrimeTemplate } from 'primeng/api';
import { TableModule } from 'primeng/table';
import {
  UiDataTableBodyTemplateDirective,
  UiDataTableEmptyTemplateDirective,
  UiDataTableHeaderTemplateDirective,
} from './ui-data-table-templates.directive';

@Component({
  selector: 'app-ui-data-table',
  standalone: true,
  imports: [TableModule, Paginator, PrimeTemplate, NgTemplateOutlet],
  changeDetection: ChangeDetectionStrategy.OnPush,
  template: `
    <p-table [value]="value()" styleClass="p-datatable-sm" [tableStyle]="tableStyle()">
      <ng-template pTemplate="header">
        <ng-container *ngTemplateOutlet="headerTpl?.templateRef ?? null" />
      </ng-template>

      <ng-template pTemplate="body" let-row let-ri="rowIndex">
        <ng-container
          *ngTemplateOutlet="bodyTpl?.templateRef ?? null; context: { $implicit: row, index: ri }"
        />
      </ng-template>

      <ng-template pTemplate="emptymessage">
        <ng-container *ngTemplateOutlet="emptyTpl?.templateRef ?? defaultEmpty" />
      </ng-template>
    </p-table>

    @if (showPaginator()) {
      <p-paginator
        [rows]="rows()"
        [totalRecords]="totalRecords()"
        [first]="first()"
        (onPageChange)="pageChange.emit($event)"
        [showCurrentPageReport]="true"
        currentPageReportTemplate="{first}–{last} of {totalRecords}"
      />
    }

    <ng-template #defaultEmpty>
      <tr>
        <td [attr.colspan]="emptyColspan()">No records.</td>
      </tr>
    </ng-template>
  `,
})
export class UiDataTableComponent<T extends object> {
  readonly value = input<T[]>([]);
  readonly tableStyle = input<Record<string, string> | null>({ 'min-width': '40rem' });

  readonly showPaginator = input(true);
  readonly rows = input(20);
  readonly totalRecords = input(0);
  readonly first = input(0);

  readonly emptyColspan = input(1);

  readonly pageChange = output<PaginatorState>();

  @ContentChild(UiDataTableHeaderTemplateDirective) headerTpl?: UiDataTableHeaderTemplateDirective;
  @ContentChild(UiDataTableBodyTemplateDirective) bodyTpl?: UiDataTableBodyTemplateDirective;
  @ContentChild(UiDataTableEmptyTemplateDirective) emptyTpl?: UiDataTableEmptyTemplateDirective;
}

