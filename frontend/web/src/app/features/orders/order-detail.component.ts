import { DatePipe } from '@angular/common';
import { Component, inject, OnInit, signal } from '@angular/core';
import { ActivatedRoute, Router, RouterLink } from '@angular/router';
import { Button } from 'primeng/button';
import { Panel } from 'primeng/panel';
import { Tag } from 'primeng/tag';
import { finalize } from 'rxjs';
import { InvoiceDto, LmsApiService, OrderDto } from '../../core/api/lms-api.service';
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
  selector: 'app-order-detail',
  standalone: true,
  imports: [
    DatePipe,
    RouterLink,
    Button,
    Panel,
    Tag,
    PageShellComponent,
    UiButtonComponent,
    UiDataTableComponent,
    UiDataTableHeaderTemplateDirective,
    UiDataTableBodyTemplateDirective,
  ],
  template: `
    <p-button label="Back to orders" icon="pi pi-arrow-left" [text]="true" routerLink="/orders" styleClass="mb-3" />
    @if (loading()) {
      <p>Loading…</p>
    } @else {
      @if (order(); as o) {
        <app-page-shell title="Order" [subtitle]="o.id">
          <ng-container pageActions>
            <app-ui-button label="Refresh" icon="pi pi-refresh" severity="secondary" [text]="true" (clicked)="reload()" />
          </ng-container>

          <div class="flex gap-2 flex-wrap align-items-center mb-3">
            <p-tag [value]="o.status" [severity]="statusSeverity(o.status)" />
            <span class="text-600 text-sm">Total: {{ formatMoney(o.totalCents, o.currency) }}</span>
            @if (o.checkoutExpiresAtUtc && o.status === 'PendingPayment') {
              <span class="text-600 text-sm">Pay before: {{ o.checkoutExpiresAtUtc | date: 'medium' }}</span>
            }
          </div>

          @if (o.status === 'PendingPayment') {
            <p-panel header="Payment" styleClass="mb-4">
              <app-ui-button
                label="Pay now (NoOp)"
                icon="pi pi-credit-card"
                [loading]="paying()"
                [disabled]="paying()"
                (clicked)="pay()"
              />
              <p class="text-sm text-color-secondary mt-2 mb-0">
                Development uses the NoOp payment provider; payment completes immediately after you confirm.
              </p>
            </p-panel>
          }

          @if (invoice(); as inv) {
            <p-panel header="Invoice" styleClass="mb-4">
              <p class="mt-0"><strong>{{ inv.invoiceNumber }}</strong></p>
              <p class="text-sm text-color-secondary mb-0">
                Issued {{ inv.issuedAt | date: 'medium' }} · {{ formatMoney(inv.totalCents, inv.currency) }}
              </p>
            </p-panel>
          }

          <h2 class="text-xl">Line items</h2>
          <app-ui-data-table [value]="o.items" [emptyColspan]="5" [showPaginator]="false" [tableStyle]="{ 'min-width': '52rem' }">
            <ng-template uiDataTableHeader>
              <tr>
                <th>Type</th>
                <th>Reference</th>
                <th>Qty</th>
                <th>Unit</th>
                <th>Line</th>
              </tr>
            </ng-template>
            <ng-template uiDataTableBody let-i>
              <tr>
                <td>{{ i.itemType }}</td>
                <td class="font-mono text-sm">{{ i.referenceId }}</td>
                <td>{{ i.quantity }}</td>
                <td>{{ formatMoney(i.unitPriceCents, i.currency) }}</td>
                <td>{{ formatMoney(i.lineTotalCents, i.currency) }}</td>
              </tr>
            </ng-template>
          </app-ui-data-table>
        </app-page-shell>
      }
    }
  `,
})
export class OrderDetailComponent implements OnInit {
  private readonly api = inject(LmsApiService);
  private readonly route = inject(ActivatedRoute);
  private readonly router = inject(Router);
  private readonly errors = inject(GlobalErrorService);

  orderId = '';
  readonly order = signal<OrderDto | null>(null);
  readonly invoice = signal<InvoiceDto | null>(null);
  readonly loading = signal(true);
  readonly paying = signal(false);
  readonly formatMoney = formatMoney;

  ngOnInit(): void {
    this.orderId = this.route.snapshot.paramMap.get('id') ?? '';
    const autoPay = this.route.snapshot.queryParamMap.get('pay') === '1';
    this.reload(() => {
      if (autoPay && this.order()?.status === 'PendingPayment') {
        this.pay(() => {
          void this.router.navigate(['/orders', this.orderId], { replaceUrl: true });
        });
      } else if (autoPay) {
        void this.router.navigate(['/orders', this.orderId], { replaceUrl: true });
      }
    });
  }

  statusSeverity(status: string): 'success' | 'warn' | 'danger' | 'info' | 'secondary' {
    switch (status) {
      case 'Paid':
        return 'success';
      case 'PendingPayment':
        return 'warn';
      case 'Cancelled':
        return 'secondary';
      default:
        return 'info';
    }
  }

  reload(done?: () => void): void {
    if (!this.orderId) {
      this.loading.set(false);
      done?.();
      return;
    }
    this.errors.clear();
    this.loading.set(true);
    this.invoice.set(null);
    this.api.getOrder(this.orderId).subscribe({
      next: (o) => {
        this.order.set(o);
        this.loading.set(false);
        if (o.status === 'Paid') {
          this.api.getOrderInvoice(this.orderId).subscribe({
            next: (inv) => this.invoice.set(inv),
            error: () => this.invoice.set(null),
          });
        }
        done?.();
      },
      error: () => {
        this.loading.set(false);
        done?.();
      },
    });
  }

  pay(after?: () => void): void {
    if (!this.orderId) return;
    this.errors.clear();
    this.paying.set(true);
    this.api.payOrder(this.orderId).subscribe({
      next: (o) => {
        this.order.set(o);
        this.paying.set(false);
        if (o.status === 'Paid') {
          this.api
            .getOrderInvoice(this.orderId)
            .pipe(finalize(() => after?.()))
            .subscribe({
              next: (inv) => this.invoice.set(inv),
              error: () => this.invoice.set(null),
            });
        } else {
          after?.();
        }
      },
      error: () => {
        this.paying.set(false);
        after?.();
      },
    });
  }
}
