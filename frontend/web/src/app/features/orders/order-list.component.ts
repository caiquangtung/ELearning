import { DatePipe } from '@angular/common';
import { Component, inject, OnInit, signal } from '@angular/core';
import { RouterLink } from '@angular/router';
import { LmsApiService, OrderListItemDto } from '../../core/api/lms-api.service';
import { AuthService } from '../../core/auth/auth.service';
import { GlobalErrorService } from '../../core/error/global-error.service';
import { PageShellComponent } from '../../shared/ui/page-shell/page-shell.component';
import { UiButtonComponent } from '../../shared/ui/ui-button/ui-button.component';
import { UiDataTableComponent } from '../../shared/ui/ui-data-table/ui-data-table.component';
import {
  UiDataTableBodyTemplateDirective,
  UiDataTableHeaderTemplateDirective,
} from '../../shared/ui/ui-data-table/ui-data-table-templates.directive';

function formatMoney(cents: number, currency: string): string {
  return `${(cents / 100).toFixed(2)} ${currency}`;
}

@Component({
  selector: 'app-order-list',
  standalone: true,
  imports: [
    DatePipe,
    RouterLink,
    PageShellComponent,
    UiButtonComponent,
    UiDataTableComponent,
    UiDataTableHeaderTemplateDirective,
    UiDataTableBodyTemplateDirective,
  ],
  template: `
    <app-page-shell title="My orders" subtitle="Recent purchases and pending checkouts">
      <ng-container pageActions>
        <app-ui-button label="Refresh" icon="pi pi-refresh" severity="secondary" [text]="true" (clicked)="reload()" />
      </ng-container>

      <app-ui-data-table [value]="items()" [emptyColspan]="5" [showPaginator]="false" [tableStyle]="{ 'min-width': '48rem' }">
        <ng-template uiDataTableHeader>
          <tr>
            <th>Order</th>
            <th>Status</th>
            <th>Total</th>
            <th>Created</th>
            <th></th>
          </tr>
        </ng-template>

        <ng-template uiDataTableBody let-o>
          <tr>
            <td class="font-mono text-sm">{{ o.id }}</td>
            <td>{{ o.status }}</td>
            <td>{{ formatMoney(o.totalCents, o.currency) }}</td>
            <td>{{ o.createdAt | date: 'medium' }}</td>
            <td class="text-right">
              <a [routerLink]="['/orders', o.id]" class="text-primary font-medium">View</a>
            </td>
          </tr>
        </ng-template>
      </app-ui-data-table>
    </app-page-shell>
  `,
})
export class OrderListComponent implements OnInit {
  private readonly api = inject(LmsApiService);
  private readonly auth = inject(AuthService);
  private readonly errors = inject(GlobalErrorService);

  readonly items = signal<OrderListItemDto[]>([]);
  readonly formatMoney = formatMoney;

  ngOnInit(): void {
    this.reload();
  }

  reload(): void {
    const user = this.auth.user();
    this.errors.clear();
    if (!user) return;
    this.api.listMyOrders(user.id, 100).subscribe({
      next: (rows) => this.items.set(rows),
    });
  }
}
