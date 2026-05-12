import { Component, input, output, ChangeDetectionStrategy } from '@angular/core';
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
        [showCurrentPageReport]="true"
        currentPageReportTemplate="Showing {first} to {last} of {totalRecords} entries"
        [rowsPerPageOptions]="[10, 25, 50]"
        (onPage)="onPage.emit($event)"
        (onSort)="onSort.emit($event)"
        (onLazyLoad)="onLazyLoad.emit($event)"
      >
        <ng-content />
      </p-table>
    </div>
  `
})
export class UiDataTableComponent {
  // Data
  value = input<any[]>([]);
  totalRecords = input(0);
  loading = input(false);
  lazy = input(false);

  // Pagination
  rows = input(10);

  // Sorting
  sortField = input<string>();
  sortOrder = input<1 | -1>(1);

  // Events
  onPage = output<any>();
  onSort = output<any>();
  onLazyLoad = output<any>();
}