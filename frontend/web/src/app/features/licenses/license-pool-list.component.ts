import { DatePipe } from '@angular/common';
import { Component, inject, OnInit, signal } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { ActivatedRoute, RouterLink } from '@angular/router';
import { InputText } from 'primeng/inputtext';
import { Panel } from 'primeng/panel';
import { PaginatorState } from 'primeng/paginator';
import { Skeleton } from 'primeng/skeleton';
import { Tag } from 'primeng/tag';
import { LmsApiService, LicensePoolListItemDto } from '../../core/api/lms-api.service';
import { GlobalErrorService } from '../../core/error/global-error.service';
import { PagedList } from '../../core/models/paged-list.model';
import { PageShellComponent } from '../../shared/ui/page-shell/page-shell.component';
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
    InputText,
    Panel,
    Skeleton,
    Tag,
    PageShellComponent,
    UiButtonComponent,
    UiDataTableComponent,
    UiDataTableHeaderTemplateDirective,
    UiDataTableBodyTemplateDirective,
  ],
  template: `
    <app-page-shell title="License pools" [subtitle]="'Organization: ' + orgId">
      <ng-container pageActions>
        <app-ui-button label="Refresh" icon="pi pi-refresh" severity="secondary" [text]="true" (clicked)="reload()" />
      </ng-container>

      <p-panel header="Create pool" [toggleable]="true" styleClass="mb-4">
        <div class="flex flex-column gap-3" style="max-width: 36rem">
          <input pInputText [(ngModel)]="name" placeholder="Pool name" class="w-full" name="lpname" />
          <input
            pInputText
            [(ngModel)]="totalSeats"
            placeholder="Total seats"
            class="w-full"
            name="lpseats"
            inputmode="numeric"
          />
          <app-ui-button
            label="Create"
            icon="pi pi-plus"
            [disabled]="!name.trim() || creating()"
            [loading]="creating()"
            (clicked)="create()"
          />
        </div>
      </p-panel>

      @if (loading()) {
        <div class="flex flex-column gap-2">
          @for (_ of skeletonRows; track $index) {
            <p-skeleton height="2.75rem" width="100%" />
          }
        </div>
      } @else if (page(); as page) {
        <app-ui-data-table
          [value]="page.items"
          [emptyColspan]="5"
          [rows]="page.pageSize"
          [totalRecords]="page.totalCount"
          [first]="(page.page - 1) * page.pageSize"
          [tableStyle]="{ 'min-width': '52rem' }"
          [virtualScroll]="page.items.length > 25"
          (pageChange)="onPageChange($event)"
        >
          <ng-template uiDataTableHeader>
            <tr>
              <th>Name</th>
              <th>Seats</th>
              <th>Available</th>
              <th>Expiry</th>
              <th>Created</th>
            </tr>
          </ng-template>

          <ng-template uiDataTableBody let-p>
            <tr>
              <td>
                <a [routerLink]="['/license-pools', p.id]" class="text-primary font-medium">{{ p.name }}</a>
              </td>
              <td>{{ p.activeSeats }} / {{ p.totalSeats }}</td>
              <td>
                <p-tag [value]="p.availableSeats.toString()" [severity]="p.availableSeats > 0 ? 'success' : 'danger'" />
              </td>
              <td>{{ p.expiresAt ? (p.expiresAt | date: 'mediumDate') : '—' }}</td>
              <td>{{ p.createdAt | date: 'mediumDate' }}</td>
            </tr>
          </ng-template>
        </app-ui-data-table>
      }
    </app-page-shell>
  `,
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

  name = '';
  totalSeats = '50';
  readonly pageSize = 20;
  private pageNum = 1;

  ngOnInit(): void {
    this.orgId = this.route.snapshot.paramMap.get('id') ?? '';
    this.reload();
  }

  reload(): void {
    this.errors.clear();
    if (!this.orgId) return;
    this.loading.set(true);
    this.api.listLicensePools(this.orgId, { page: this.pageNum, pageSize: this.pageSize }).subscribe({
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
        this.reload();
      },
      error: () => this.creating.set(false),
    });
  }
}
