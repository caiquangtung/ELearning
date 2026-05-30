import { DatePipe } from '@angular/common';
import { Component, inject, OnInit, signal } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { RouterLink } from '@angular/router';
import { InputTextModule } from 'primeng/inputtext';
import { DialogModule } from 'primeng/dialog';
import { DropdownModule } from 'primeng/dropdown';
import { PaginatorState } from 'primeng/paginator';
import { PrimeTemplate } from 'primeng/api';
import { Skeleton } from 'primeng/skeleton';
import {
  LmsApiService,
  CampaignListItemDto,
  CreateCampaignRequest,
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

const uuidRe =
  /^[0-9a-f]{8}-[0-9a-f]{4}-[1-5][0-9a-f]{3}-[89ab][0-9a-f]{3}-[0-9a-f]{12}$/i;

type ScopeOption = { label: string; value: string };

@Component({
  selector: 'app-campaign-list',
  standalone: true,
  imports: [
    DatePipe,
    FormsModule,
    RouterLink,
    DialogModule,
    InputTextModule,
    DropdownModule,
    PrimeTemplate,
    Skeleton,
    PageShellComponent,
    UiButtonComponent,
    UiDataTableComponent,
    UiDataTableHeaderTemplateDirective,
    UiDataTableBodyTemplateDirective,
  ],
  styleUrl: './campaign-list.component.scss',
  template: `
    <app-page-shell
      title="Campaigns"
      subtitle="Create and manage promotion campaigns"
    >
      <ng-container pageActions>
        <app-ui-button
          icon="pi pi-search"
          severity="secondary"
          [text]="true"
          (clicked)="searchDialogVisible = true"
        />
        <app-ui-button
          label="Refresh"
          icon="pi pi-refresh"
          severity="secondary"
          [text]="true"
          (clicked)="reload()"
        />
        <app-ui-button
          label="New campaign"
          icon="pi pi-plus"
          (clicked)="openCreateDialog()"
        />
      </ng-container>

      <p-dialog
        header="Search campaigns"
        [(visible)]="searchDialogVisible"
        appendTo="body"
        [modal]="true"
        [draggable]="false"
        [resizable]="false"
        styleClass="app-dialog app-dialog--sm"
        [style]="{ width: 'min(36rem, calc(100vw - 2rem))' }"
        (onHide)="searchDialogVisible = false"
      >
        <div class="field-block">
          <label for="campaign-search-name">Name</label>
          <input
            id="campaign-search-name"
            pInputText
            [(ngModel)]="searchName"
            name="searchName"
            placeholder="Search by name"
          />
        </div>

        <div class="field-block">
          <label for="campaign-search-scope">Scope</label>
          <p-dropdown
            id="campaign-search-scope"
            [options]="scopeOptions"
            [(ngModel)]="searchScope"
            optionLabel="label"
            optionValue="value"
            styleClass="w-full"
            name="searchScope"
          />
        </div>

        <ng-template pTemplate="footer">
          <app-ui-button
            label="Cancel"
            severity="secondary"
            [text]="true"
            (clicked)="searchDialogVisible = false"
          />
          <app-ui-button
            label="Apply"
            icon="pi pi-filter"
            (clicked)="searchDialogVisible = false; reload()"
          />
        </ng-template>
      </p-dialog>

      <p-dialog
        header="Create campaign"
        [(visible)]="formVisible"
        appendTo="body"
        [modal]="true"
        [draggable]="false"
        [resizable]="false"
        styleClass="app-dialog app-dialog--md"
        [style]="{ width: 'min(38rem, calc(100vw - 2rem))' }"
        (onHide)="closeDialog()"
      >
        <form class="app-form" (ngSubmit)="create()">
          <div class="field-block">
            <label for="name">Name</label>
            <input
              id="name"
              pInputText
              [(ngModel)]="name"
              name="name"
              placeholder="e.g. Spring sale"
            />
          </div>

          <div class="field-block">
            <label for="scope">Scope</label>
            <p-dropdown
              id="scope"
              [options]="scopeOptions"
              [(ngModel)]="scope"
              optionLabel="label"
              optionValue="value"
              styleClass="w-full"
              name="scope"
            />
          </div>

          @if (scope === 'Organization') {
            <div class="field-block">
              <label for="org">Organization ID</label>
              <input
                id="org"
                pInputText
                [(ngModel)]="orgId"
                name="orgId"
                placeholder="UUID"
              />
            </div>
          }

          <div class="app-form-grid">
            <div class="field-block">
              <label for="start">Start (UTC)</label>
              <input
                id="start"
                pInputText
                type="datetime-local"
                [(ngModel)]="startLocal"
                name="startLocal"
              />
            </div>

            <div class="field-block">
              <label for="end">End (UTC, optional)</label>
              <input
                id="end"
                pInputText
                type="datetime-local"
                [(ngModel)]="endLocal"
                name="endLocal"
              />
            </div>
          </div>
        </form>

        <ng-template pTemplate="footer">
          <app-ui-button
            label="Cancel"
            severity="secondary"
            [text]="true"
            (clicked)="closeDialog()"
          />
          <app-ui-button
            label="Create"
            icon="pi pi-plus"
            [loading]="creating()"
            [disabled]="creating() || !canCreate()"
            (clicked)="create()"
          />
        </ng-template>
      </p-dialog>

      @if (loading()) {
        <div class="flex flex-column gap-2">
          @for (_ of skeletonRows; track $index) {
            <p-skeleton height="2.75rem" width="100%" />
          }
        </div>
      } @else {
        @if (page(); as p) {
          <app-ui-data-table
            [value]="p.items"
            [emptyColspan]="6"
            [rows]="p.pageSize"
            [totalRecords]="p.totalCount"
            [first]="(p.page - 1) * p.pageSize"
            [tableStyle]="{ 'min-width': '58rem' }"
            [virtualScroll]="p.items.length > 25"
            (pageChange)="onPageChange($event)"
          >
            <ng-template uiDataTableHeader>
              <tr>
                <th>Name</th>
                <th>Scope</th>
                <th>Organization</th>
                <th>Status</th>
                <th>Window</th>
                <th></th>
              </tr>
            </ng-template>
            <ng-template uiDataTableBody let-c>
              <tr>
                <td>{{ c.name }}</td>
                <td>{{ c.scope }}</td>
                <td class="font-mono text-sm">{{ c.organizationId ?? '—' }}</td>
                <td>{{ c.status }}</td>
                <td>
                  {{ c.startUtc | date: 'mediumDate' }} →
                  {{ c.endUtc ? (c.endUtc | date: 'mediumDate') : '—' }}
                </td>
                <td class="text-right">
                  <a
                    [routerLink]="['/campaigns', c.id]"
                    class="text-primary font-medium"
                    >Manage</a
                  >
                </td>
              </tr>
            </ng-template>
          </app-ui-data-table>
        }
      }
    </app-page-shell>
  `,
})
export class CampaignListComponent implements OnInit {
  private readonly api = inject(LmsApiService);
  private readonly errors = inject(GlobalErrorService);

  readonly page = signal<PagedList<CampaignListItemDto> | null>(null);
  readonly loading = signal(true);
  readonly creating = signal(false);
  readonly skeletonRows = Array.from({ length: 6 });

  searchDialogVisible = false;
  searchName = '';
  searchScope: string | null = null;

  formVisible = false;
  name = '';
  scope = 'Global';
  orgId = '';
  startLocal = '';
  endLocal = '';
  readonly pageSize = 20;
  private pageNum = 1;

  readonly scopeOptions: ScopeOption[] = [
    { label: 'Global', value: 'Global' },
    { label: 'Organization', value: 'Organization' },
  ];

  ngOnInit(): void {
    this.resetForm();
    this.reload();
  }

  openCreateDialog(): void {
    this.resetForm();
    this.formVisible = true;
  }

  closeDialog(): void {
    if (this.creating()) return;
    this.formVisible = false;
  }

  reload(): void {
    this.errors.clear();
    this.loading.set(true);
    this.api
      .listCampaigns({
        organizationId: null,
        includeGlobal: true,
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

  canCreate(): boolean {
    if (!this.name.trim()) return false;
    if (!this.startLocal) return false;
    if (this.scope === 'Organization' && !uuidRe.test(this.orgId.trim()))
      return false;
    return true;
  }

  create(): void {
    if (!this.canCreate()) return;

    const org = this.orgId.trim();
    const body: CreateCampaignRequest = {
      name: this.name.trim(),
      scope: this.scope,
      organizationId: this.scope === 'Organization' ? org : null,
      startUtc: new Date(this.startLocal).toISOString(),
      endUtc: this.endLocal ? new Date(this.endLocal).toISOString() : null,
    };

    this.errors.clear();
    this.creating.set(true);
    this.api.createCampaign(body).subscribe({
      next: () => {
        this.creating.set(false);
        this.formVisible = false;
        this.resetForm();
        this.reload();
      },
      error: () => this.creating.set(false),
    });
  }

  private resetForm(): void {
    this.name = '';
    this.scope = 'Global';
    this.orgId = '';
    this.endLocal = '';
    this.startLocal = this.toLocalInput(new Date().toISOString());
  }

  private toLocalInput(iso: string): string {
    const d = new Date(iso);
    const pad = (n: number) => String(n).padStart(2, '0');
    return `${d.getFullYear()}-${pad(d.getMonth() + 1)}-${pad(d.getDate())}T${pad(d.getHours())}:${pad(d.getMinutes())}`;
  }
}
