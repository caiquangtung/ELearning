import {
  AfterViewInit,
  ChangeDetectionStrategy,
  Component,
  ElementRef,
  Renderer2,
  inject,
  input,
  output,
} from '@angular/core';
import { Table, TableModule } from 'primeng/table';

/**
 * Data table wrapper with built-in pagination and sorting.
 * Uses OnPush change detection for performance.
 */
@Component({
  selector: 'app-ui-data-table',
  standalone: true,
  imports: [TableModule],
  changeDetection: ChangeDetectionStrategy.OnPush,
  template: `
    <div class="card">
      <p-table
        [value]="value()"
        [loading]="loading()"
        [paginator]="true"
        [rows]="rows()"
        [totalRecords]="totalRecords()"
        [lazy]="lazy()"
        [sortField]="sortField()"
        [sortOrder]="sortOrder()"
        [responsiveLayout]="'scroll'"
        [tableStyle]="tableStyle()"
        [showCurrentPageReport]="true"
        currentPageReportTemplate="Showing {first} to {last} of {totalRecords} entries"
        [rowsPerPageOptions]="[10, 25, 50]"
        [attr.aria-label]="ariaLabel()"
        [attr.aria-busy]="loading()"
        (onPage)="onPage.emit($event)"
        (onSort)="onSort.emit($event)"
        (onLazyLoad)="onLazyLoad.emit($event)"
      >
        <ng-content />
      </p-table>
    </div>
  `,
  styleUrl: './ui-data-table.component.scss',
})
export class UiDataTableComponent implements AfterViewInit {
  private readonly host = inject<ElementRef<HTMLElement>>(ElementRef);
  private readonly renderer = inject(Renderer2);

  // Data
  value = input<any[]>([]);
  totalRecords = input(0);
  loading = input(false);
  lazy = input(false);
  ariaLabel = input('Data table');
  tableStyle = input<Record<string, string> | null>({ 'min-width': '40rem' });

  // Pagination
  rows = input(10);

  // Sorting
  sortField = input<string>();
  sortOrder = input<1 | -1>(1);

  // Events
  onPage = output<any>();
  onSort = output<any>();
  onLazyLoad = output<any>();

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
