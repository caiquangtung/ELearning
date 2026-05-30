import { DatePipe } from '@angular/common';
import { Component, inject, OnInit, signal } from '@angular/core';
import { ActivatedRoute, Router, RouterLink } from '@angular/router';
import { Button } from 'primeng/button';
import { Panel } from 'primeng/panel';
import { Tag } from 'primeng/tag';
import { finalize } from 'rxjs';
import {
  InvoiceDto,
  LmsApiService,
  OrderDto,
} from '../../core/api/lms-api.service';
import { GlobalErrorService } from '../../core/error/global-error.service';
import { PageShellComponent } from '../../shared/ui';
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
  templateUrl: './order-detail.component.html',
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
          void this.router.navigate(['/orders', this.orderId], {
            replaceUrl: true,
          });
        });
      } else if (autoPay) {
        void this.router.navigate(['/orders', this.orderId], {
          replaceUrl: true,
        });
      }
    });
  }

  statusSeverity(
    status: string,
  ): 'success' | 'warn' | 'danger' | 'info' | 'secondary' {
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
