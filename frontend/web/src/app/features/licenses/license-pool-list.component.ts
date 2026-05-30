import { DatePipe } from '@angular/common';
import { Component, inject, OnInit, signal } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { ActivatedRoute, RouterLink } from '@angular/router';
import { DialogModule } from 'primeng/dialog';
import { InputText } from 'primeng/inputtext';
import { PaginatorState } from 'primeng/paginator';
import { PrimeTemplate } from 'primeng/api';
import { Skeleton } from 'primeng/skeleton';
import { Tag } from 'primeng/tag';
import {
  LmsApiService,
  LicensePoolListItemDto,
} from '../../core/api/lms-api.service';
import { GlobalErrorService } from '../../core/error/global-error.service';
import { PagedList } from '../../core/models/paged-list.model';
import { PageShellComponent } from '../../shared/ui';
import { UiButtonComponent } from '../../shared/ui/ui-button/ui-button.component';
import { UiDataTableComponent } from '../../shared/ui/ui-data-table/ui-data-table.component';
import {
  UiDataTableBodyTemplateDirective,
  UiDataTableHeaderTemplateDirective,
} from '../../shared/ui/ui-data-table/ui-data-table-templates.directive';

@Component({
  selector: 'app-license-pool-list',
  standalone: true,
  imports: [
    DatePipe,
    FormsModule,
    RouterLink,
    DialogModule,
    InputText,
    PrimeTemplate,
    Skeleton,
    Tag,
    PageShellComponent,
    UiButtonComponent,
    UiDataTableComponent,
    UiDataTableHeaderTemplateDirective,
    UiDataTableBodyTemplateDirective,
  ],
  templateUrl: './license-pool-list.component.html',
})
export class LicensePoolListComponent implements OnInit {
  private readonly api = inject(LmsApiService);
  private readonly errors = inject(GlobalErrorService);
  private readonly route = inject(ActivatedRoute);

  orgId = '';
  readonly page = signal<PagedList<LicensePoolListItemDto> | null>(null);
  readonly loading = signal(true);
  readonly creating = signal(false);
  readonly skeletonRows = Array.from({ length: 6 });

  createVisible = false;
  name = '';
  totalSeats = '50';
  readonly pageSize = 20;
  private pageNum = 1;

  ngOnInit(): void {
    this.orgId = this.route.snapshot.paramMap.get('id') ?? '';
    this.reload();
  }

  openCreateDialog(): void {
    this.name = '';
    this.totalSeats = '50';
    this.createVisible = true;
  }

  closeCreateDialog(): void {
    if (this.creating()) return;
    this.createVisible = false;
  }

  reload(): void {
    this.errors.clear();
    if (!this.orgId) return;
    this.loading.set(true);
    this.api
      .listLicensePools(this.orgId, {
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

  create(): void {
    if (!this.orgId) return;
    const name = this.name.trim();
    const totalSeats = Number.parseInt(this.totalSeats, 10);
    if (!name || !Number.isFinite(totalSeats) || totalSeats <= 0) return;

    this.errors.clear();
    this.creating.set(true);
    this.api.createLicensePool(this.orgId, { name, totalSeats }).subscribe({
      next: () => {
        this.creating.set(false);
        this.name = '';
        this.totalSeats = '50';
        this.createVisible = false;
        this.reload();
      },
      error: () => this.creating.set(false),
    });
  }
}
