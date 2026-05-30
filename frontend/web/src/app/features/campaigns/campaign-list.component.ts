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
  templateUrl: './campaign-list.component.html',
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
