import { DatePipe } from '@angular/common';
import { Component, inject, OnInit, signal } from '@angular/core';
import { RouterLink } from '@angular/router';
import { PaginatorState } from 'primeng/paginator';
import { Skeleton } from 'primeng/skeleton';
import {
  LmsApiService,
  OrderListItemDto,
} from '../../core/api/lms-api.service';
import { AuthService } from '../../core/auth/auth.service';
import { GlobalErrorService } from '../../core/error/global-error.service';
import { PagedList } from '../../core/models/paged-list.model';
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
  selector: 'app-order-list',
  standalone: true,
  imports: [
    DatePipe,
    RouterLink,
    Skeleton,
    PageShellComponent,
    UiButtonComponent,
    UiDataTableComponent,
    UiDataTableHeaderTemplateDirective,
    UiDataTableBodyTemplateDirective,
  ],
  templateUrl: './order-list.component.html',
})
export class OrderListComponent implements OnInit {
  private readonly api = inject(LmsApiService);
  private readonly auth = inject(AuthService);
  private readonly errors = inject(GlobalErrorService);

  readonly page = signal<PagedList<OrderListItemDto> | null>(null);
  readonly loading = signal(true);
  readonly skeletonRows = Array.from({ length: 6 });
  readonly formatMoney = formatMoney;
  readonly pageSize = 20;
  private pageNum = 1;

  ngOnInit(): void {
    this.reload();
  }

  reload(): void {
    const user = this.auth.user();
    this.errors.clear();
    if (!user) return;
    this.loading.set(true);
    this.api
      .listMyOrders({
        buyerUserId: user.id,
        page: this.pageNum,
        pageSize: this.pageSize,
      })
      .subscribe({
        next: (page) => {
          this.page.set(page);
          this.loading.set(false);
        },
        error: () => this.loading.set(false),
      });
  }

  onPageChange(event: PaginatorState): void {
    const first = event.first ?? 0;
    const rows = event.rows ?? this.pageSize;
    this.pageNum = Math.floor(first / rows) + 1;
    this.reload();
  }
}
