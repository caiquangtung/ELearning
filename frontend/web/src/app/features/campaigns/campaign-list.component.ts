import { DatePipe } from '@angular/common';
import { Component, inject, OnInit, signal } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { RouterLink } from '@angular/router';
import { InputTextModule } from 'primeng/inputtext';
import { Panel } from 'primeng/panel';
import { DropdownModule } from 'primeng/dropdown';
import { LmsApiService, CampaignListItemDto, CreateCampaignRequest } from '../../core/api/lms-api.service';
import { GlobalErrorService } from '../../core/error/global-error.service';
import { PageShellComponent } from '../../shared/ui/page-shell/page-shell.component';
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
    Panel,
    InputTextModule,
    DropdownModule,
    PageShellComponent,
    UiButtonComponent,
    UiDataTableComponent,
    UiDataTableHeaderTemplateDirective,
    UiDataTableBodyTemplateDirective,
  ],
  template: `
    <app-page-shell title="Campaigns" subtitle="Create and manage promotion campaigns">
      <ng-container pageActions>
        <app-ui-button label="Refresh" icon="pi pi-refresh" severity="secondary" [text]="true" (clicked)="reload()" />
      </ng-container>

      <p-panel header="Create campaign" [toggleable]="true" styleClass="mb-4">
        <div class="flex flex-column gap-3" style="max-width: 40rem">
          <label class="text-sm font-medium" for="name">Name</label>
          <input id="name" pInputText [(ngModel)]="name" placeholder="e.g. Spring sale" />

          <label class="text-sm font-medium" for="scope">Scope</label>
          <p-dropdown
            id="scope"
            [options]="scopeOptions"
            [(ngModel)]="scope"
            optionLabel="label"
            optionValue="value"
            styleClass="w-full"
          />

          <label class="text-sm font-medium" for="org">Organization ID (required for Organization scope)</label>
          <input id="org" pInputText [(ngModel)]="orgId" placeholder="UUID" />

          <label class="text-sm font-medium" for="start">Start (UTC)</label>
          <input id="start" pInputText type="datetime-local" [(ngModel)]="startLocal" />

          <label class="text-sm font-medium" for="end">End (UTC, optional)</label>
          <input id="end" pInputText type="datetime-local" [(ngModel)]="endLocal" />

          <app-ui-button
            label="Create"
            icon="pi pi-plus"
            [loading]="creating()"
            [disabled]="creating() || !canCreate()"
            (clicked)="create()"
          />
        </div>
      </p-panel>

      <app-ui-data-table [value]="items()" [emptyColspan]="6" [showPaginator]="false" [tableStyle]="{ 'min-width': '58rem' }">
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
            <td>{{ c.startUtc | date: 'mediumDate' }} → {{ c.endUtc ? (c.endUtc | date: 'mediumDate') : '—' }}</td>
            <td class="text-right">
              <a [routerLink]="['/campaigns', c.id]" class="text-primary font-medium">Manage</a>
            </td>
          </tr>
        </ng-template>
      </app-ui-data-table>
    </app-page-shell>
  `,
})
export class CampaignListComponent implements OnInit {
  private readonly api = inject(LmsApiService);
  private readonly errors = inject(GlobalErrorService);

  readonly items = signal<CampaignListItemDto[]>([]);
  readonly creating = signal(false);

  name = '';
  scope = 'Global';
  orgId = '';
  startLocal = '';
  endLocal = '';

  readonly scopeOptions: ScopeOption[] = [
    { label: 'Global', value: 'Global' },
    { label: 'Organization', value: 'Organization' },
  ];

  ngOnInit(): void {
    const now = new Date();
    this.startLocal = this.toLocalInput(now.toISOString());
    this.reload();
  }

  reload(): void {
    this.errors.clear();
    this.api.listCampaigns(null, true, 200).subscribe({
      next: (rows) => this.items.set(rows),
    });
  }

  canCreate(): boolean {
    if (!this.name.trim()) return false;
    if (!this.startLocal) return false;
    if (this.scope === 'Organization' && !uuidRe.test(this.orgId.trim())) return false;
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
        this.name = '';
        this.orgId = '';
        this.endLocal = '';
        this.reload();
      },
      error: () => this.creating.set(false),
    });
  }

  private toLocalInput(iso: string): string {
    const d = new Date(iso);
    const pad = (n: number) => String(n).padStart(2, '0');
    return `${d.getFullYear()}-${pad(d.getMonth() + 1)}-${pad(d.getDate())}T${pad(d.getHours())}:${pad(d.getMinutes())}`;
  }
}

