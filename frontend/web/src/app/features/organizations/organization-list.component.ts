import { Component, inject, OnInit, signal } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { RouterLink } from '@angular/router';
import { Button } from 'primeng/button';
import { DialogModule } from 'primeng/dialog';
import { InputText } from 'primeng/inputtext';
import { Paginator, PaginatorState } from 'primeng/paginator';
import { PrimeTemplate } from 'primeng/api';
import { Skeleton } from 'primeng/skeleton';
import { TableModule } from 'primeng/table';
import { Tag } from 'primeng/tag';
import { LmsApiService, OrganizationDto } from '../../core/api/lms-api.service';
import { AuthService } from '../../core/auth/auth.service';
import { GlobalErrorService } from '../../core/error/global-error.service';
import { PagedList } from '../../core/models/paged-list.model';
import { PageShellComponent } from '../../shared/ui';

@Component({
  selector: 'app-organization-list',
  standalone: true,
  imports: [
    FormsModule,
    RouterLink,
    TableModule,
    Button,
    DialogModule,
    InputText,
    Paginator,
    Tag,
    PrimeTemplate,
    Skeleton,
    PageShellComponent,
  ],
  styleUrl: './organization-list.component.scss',
  template: `
    <app-page-shell
      title="Organizations"
      subtitle="Create and manage tenant organizations."
    >
      <ng-container pageActions>
        @if (isAdmin()) {
          <p-button
            label="New organization"
            icon="pi pi-plus"
            (onClick)="openCreateDialog()"
          />
        }
      </ng-container>

      @if (isAdmin()) {
        <p-dialog
          header="New organization"
          [(visible)]="createVisible"
          appendTo="body"
          [modal]="true"
          [draggable]="false"
          [resizable]="false"
          styleClass="app-dialog app-dialog--md"
          [style]="{ width: 'min(34rem, calc(100vw - 2rem))' }"
          (onHide)="closeCreateDialog()"
        >
          <form class="app-form" (ngSubmit)="createOrg()">
            <div class="field-block">
              <label for="org-name">Name</label>
              <input
                pInputText
                id="org-name"
                [(ngModel)]="newName"
                name="orgName"
                class="w-full"
              />
            </div>
            <div class="field-block">
              <label for="org-slug">Slug (optional)</label>
              <input
                pInputText
                id="org-slug"
                [(ngModel)]="newSlug"
                name="orgSlug"
                class="w-full"
              />
            </div>
          </form>

          <ng-template pTemplate="footer">
            <p-button
              label="Cancel"
              severity="secondary"
              [text]="true"
              (onClick)="closeCreateDialog()"
            />
            <p-button
              label="Create"
              icon="pi pi-plus"
              [disabled]="!newName.trim() || creating()"
              [loading]="creating()"
              (onClick)="createOrg()"
            />
          </ng-template>
        </p-dialog>
      }
      @if (loading()) {
        <div class="flex flex-column gap-2">
          @for (_ of skeletonRows; track $index) {
            <p-skeleton height="2.75rem" width="100%" />
          }
        </div>
      } @else {
        @if (page(); as p) {
          <p-table
            [value]="p.items"
            styleClass="p-datatable-sm"
            [tableStyle]="{ 'min-width': '50rem' }"
            [scrollable]="p.items.length > 25"
            [virtualScroll]="p.items.length > 25"
            [virtualScrollItemSize]="46"
            scrollHeight="420px"
          >
            <ng-template pTemplate="header">
              <tr>
                <th>Name</th>
                <th>Slug</th>
                <th>Status</th>
              </tr>
            </ng-template>
            <ng-template pTemplate="body" let-o>
              <tr>
                <td>
                  <a
                    [routerLink]="['/organizations', o.id]"
                    class="text-primary font-medium"
                    >{{ o.name }}</a
                  >
                </td>
                <td>{{ o.slug }}</td>
                <td><p-tag [value]="o.status" severity="secondary" /></td>
              </tr>
            </ng-template>
            <ng-template pTemplate="emptymessage">
              <tr>
                <td colspan="3">No organizations found.</td>
              </tr>
            </ng-template>
          </p-table>

          <p-paginator
            [rows]="p.pageSize"
            [totalRecords]="p.totalCount"
            [first]="(p.page - 1) * p.pageSize"
            [showCurrentPageReport]="true"
            currentPageReportTemplate="{first}–{last} of {totalRecords}"
            (onPageChange)="onPageChange($event)"
          />
        }
      }
    </app-page-shell>
  `,
})
export class OrganizationListComponent implements OnInit {
  private readonly api = inject(LmsApiService);
  private readonly errors = inject(GlobalErrorService);
  readonly auth = inject(AuthService);

  readonly page = signal<PagedList<OrganizationDto> | null>(null);
  readonly loading = signal(true);
  readonly creating = signal(false);
  readonly skeletonRows = Array.from({ length: 6 });

  createVisible = false;
  newName = '';
  newSlug = '';
  readonly pageSize = 20;
  private pageNum = 1;

  isAdmin(): boolean {
    return this.auth.user()?.roles?.some((r) => r === 'Admin') ?? false;
  }

  ngOnInit(): void {
    this.reload();
  }

  openCreateDialog(): void {
    this.newName = '';
    this.newSlug = '';
    this.createVisible = true;
  }

  closeCreateDialog(): void {
    if (this.creating()) return;
    this.createVisible = false;
  }

  onPageChange(event: PaginatorState): void {
    const first = event.first ?? 0;
    const rows = event.rows ?? this.pageSize;
    this.pageNum = Math.floor(first / rows) + 1;
    this.reload();
  }

  reload(): void {
    this.errors.clear();
    this.loading.set(true);
    this.api
      .listOrganizations({ page: this.pageNum, pageSize: this.pageSize })
      .subscribe({
        next: (page) => {
          this.page.set(page);
          this.loading.set(false);
        },
        error: () => this.loading.set(false),
      });
  }

  createOrg(): void {
    const name = this.newName.trim();
    if (!name) return;
    this.errors.clear();
    this.creating.set(true);
    const slug = this.newSlug.trim() || null;
    this.api.createOrganization({ name, slug }).subscribe({
      next: () => {
        this.newName = '';
        this.newSlug = '';
        this.createVisible = false;
        this.creating.set(false);
        this.reload();
      },
      error: () => this.creating.set(false),
    });
  }
}
