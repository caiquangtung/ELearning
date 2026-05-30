import { DatePipe } from '@angular/common';
import { Component, inject, OnInit, signal } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { ActivatedRoute, RouterLink } from '@angular/router';
import { Button } from 'primeng/button';
import { InputText } from 'primeng/inputtext';
import { Panel } from 'primeng/panel';
import { Tag } from 'primeng/tag';
import {
  LmsApiService,
  LicensePoolDetailDto,
} from '../../core/api/lms-api.service';
import { GlobalErrorService } from '../../core/error/global-error.service';
import { PageShellComponent } from '../../shared/ui';
import { UiButtonComponent } from '../../shared/ui/ui-button/ui-button.component';
import { UiDataTableComponent } from '../../shared/ui/ui-data-table/ui-data-table.component';
import {
  UiDataTableBodyTemplateDirective,
  UiDataTableHeaderTemplateDirective,
} from '../../shared/ui/ui-data-table/ui-data-table-templates.directive';

@Component({
  selector: 'app-license-pool-detail',
  standalone: true,
  imports: [
    DatePipe,
    FormsModule,
    RouterLink,
    Button,
    InputText,
    Panel,
    Tag,
    PageShellComponent,
    UiButtonComponent,
    UiDataTableComponent,
    UiDataTableHeaderTemplateDirective,
    UiDataTableBodyTemplateDirective,
  ],
  template: `
    @if (pool(); as p) {
      <app-page-shell title="{{ p.name }}" [subtitle]="'Pool: ' + p.id">
        <ng-container pageActions>
          <app-ui-button
            label="Refresh"
            icon="pi pi-refresh"
            severity="secondary"
            [text]="true"
            (clicked)="reload()"
          />
          @if (p.seatPriceCents > 0) {
            <p-button
              label="Buy seats"
              icon="pi pi-shopping-cart"
              [routerLink]="['/checkout']"
              [queryParams]="{ type: 'LicensePool', ref: p.id, qty: 1 }"
            />
          }
        </ng-container>

        <div class="flex gap-3 flex-wrap mb-3">
          <p-tag
            [value]="p.activeSeats + ' / ' + p.totalSeats + ' used'"
            severity="info"
          />
          <p-tag
            [value]="p.availableSeats + ' available'"
            [severity]="p.availableSeats > 0 ? 'success' : 'danger'"
          />
          <span class="text-600 text-sm"
            >Created: {{ p.createdAt | date: 'medium' }}</span
          >
        </div>

        <p-panel header="Assign license" styleClass="mb-4">
          <div class="flex flex-column gap-3" style="max-width: 36rem">
            <input
              pInputText
              [(ngModel)]="userId"
              placeholder="User ID (GUID)"
              class="w-full"
              name="lpuserid"
            />
            <app-ui-button
              label="Assign"
              icon="pi pi-user-plus"
              [disabled]="!userId.trim() || assigning()"
              [loading]="assigning()"
              (clicked)="assign()"
            />
          </div>
        </p-panel>

        <app-ui-data-table
          [value]="p.assignments"
          [emptyColspan]="4"
          [showPaginator]="false"
          [tableStyle]="{ 'min-width': '52rem' }"
        >
          <ng-template uiDataTableHeader>
            <tr>
              <th>User</th>
              <th>Assigned</th>
              <th>Status</th>
              <th></th>
            </tr>
          </ng-template>

          <ng-template uiDataTableBody let-a>
            <tr>
              <td class="font-mono text-sm">{{ a.userId }}</td>
              <td>{{ a.assignedAt | date: 'medium' }}</td>
              <td>
                <p-tag
                  [value]="a.revokedAt ? 'Revoked' : 'Active'"
                  [severity]="a.revokedAt ? 'secondary' : 'success'"
                />
              </td>
              <td class="text-right">
                @if (!a.revokedAt) {
                  <app-ui-button
                    label="Revoke"
                    icon="pi pi-ban"
                    severity="danger"
                    [text]="true"
                    [loading]="revokingUserId() === a.userId"
                    (clicked)="revoke(a.userId)"
                  />
                }
              </td>
            </tr>
          </ng-template>
        </app-ui-data-table>
      </app-page-shell>
    }
  `,
})
export class LicensePoolDetailComponent implements OnInit {
  private readonly api = inject(LmsApiService);
  private readonly errors = inject(GlobalErrorService);
  private readonly route = inject(ActivatedRoute);

  poolId = '';
  readonly pool = signal<LicensePoolDetailDto | null>(null);

  userId = '';
  readonly assigning = signal(false);
  readonly revokingUserId = signal<string | null>(null);

  ngOnInit(): void {
    this.poolId = this.route.snapshot.paramMap.get('id') ?? '';
    this.reload();
  }

  reload(): void {
    this.errors.clear();
    if (!this.poolId) return;
    this.api.getLicensePool(this.poolId).subscribe({
      next: (p) => this.pool.set(p),
    });
  }

  assign(): void {
    const userId = this.userId.trim();
    if (!this.poolId || !userId) return;
    this.errors.clear();
    this.assigning.set(true);
    this.api.assignLicense(this.poolId, userId).subscribe({
      next: () => {
        this.assigning.set(false);
        this.userId = '';
        this.reload();
      },
      error: () => this.assigning.set(false),
    });
  }

  revoke(userId: string): void {
    if (!this.poolId) return;
    this.errors.clear();
    this.revokingUserId.set(userId);
    this.api.revokeLicense(this.poolId, userId).subscribe({
      next: () => {
        this.revokingUserId.set(null);
        this.reload();
      },
      error: () => this.revokingUserId.set(null),
    });
  }
}
