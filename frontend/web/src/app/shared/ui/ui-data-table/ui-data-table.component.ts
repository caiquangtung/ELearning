import { NgTemplateOutlet } from '@angular/common';
import {
  AfterViewInit,
  ChangeDetectionStrategy,
  Component,
  ContentChild,
  ElementRef,
  Renderer2,
  inject,
  input,
  output,
} from '@angular/core';
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
    <div class="ui-data-table-shell">
      <p-table
        [value]="value()"
        [loading]="loading()"
        styleClass="p-datatable-sm"
        [tableStyle]="tableStyle()"
        [responsiveLayout]="'scroll'"
        [scrollable]="scrollable() || virtualScroll()"
        [scrollHeight]="scrollable() || virtualScroll() ? scrollHeight() : undefined"
        [virtualScroll]="virtualScroll()"
        [virtualScrollItemSize]="virtualScrollItemSize()"
        [attr.aria-label]="ariaLabel()"
        [attr.aria-busy]="loading()"
      >
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
    </div>

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
  styleUrl: './ui-data-table.component.scss',
})
export class UiDataTableComponent<T extends object> implements AfterViewInit {
  private readonly host = inject<ElementRef<HTMLElement>>(ElementRef);
  private readonly renderer = inject(Renderer2);

  readonly value = input<T[]>([]);
  readonly loading = input(false);
  readonly ariaLabel = input('Data table');
  readonly tableStyle = input<Record<string, string> | null>({ 'min-width': '40rem' });
  readonly scrollable = input(false);
  readonly scrollHeight = input('400px');
  readonly virtualScroll = input(false);
  readonly virtualScrollItemSize = input(46);

  readonly showPaginator = input(true);
  readonly rows = input(20);
  readonly totalRecords = input(0);
  readonly first = input(0);

  readonly emptyColspan = input(1);

  readonly pageChange = output<PaginatorState>();

  @ContentChild(UiDataTableHeaderTemplateDirective) headerTpl?: UiDataTableHeaderTemplateDirective;
  @ContentChild(UiDataTableBodyTemplateDirective) bodyTpl?: UiDataTableBodyTemplateDirective;
  @ContentChild(UiDataTableEmptyTemplateDirective) emptyTpl?: UiDataTableEmptyTemplateDirective;

  ngAfterViewInit(): void {
    queueMicrotask(() => {
      const scrollRegion = this.host.nativeElement.querySelector('.p-datatable-table-container');
      if (!scrollRegion) return;

      this.renderer.setAttribute(scrollRegion, 'tabindex', '0');
      this.renderer.setAttribute(scrollRegion, 'role', 'region');
      this.renderer.setAttribute(scrollRegion, 'aria-label', `${this.ariaLabel()} table`);
    });
  }
}
