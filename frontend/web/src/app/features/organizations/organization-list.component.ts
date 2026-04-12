import { Component, inject, OnInit, signal } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { RouterLink } from '@angular/router';
import { Button } from 'primeng/button';
import { FloatLabel } from 'primeng/floatlabel';
import { InputText } from 'primeng/inputtext';
import { Panel } from 'primeng/panel';
import { PrimeTemplate } from 'primeng/api';
import { TableModule } from 'primeng/table';
import { Tag } from 'primeng/tag';
import { LmsApiService, OrganizationDto } from '../../core/api/lms-api.service';
import { AuthService } from '../../core/auth/auth.service';
import { GlobalErrorService } from '../../core/error/global-error.service';

@Component({
  selector: 'app-organization-list',
  standalone: true,
  imports: [
    FormsModule,
    RouterLink,
    TableModule,
    Button,
    Panel,
    FloatLabel,
    InputText,
    Tag,
    PrimeTemplate,
  ],
  template: `
    <h1 class="text-2xl font-semibold mt-0">Organizations</h1>
    @if (isAdmin()) {
      <p-panel header="Create organization" [toggleable]="true" [collapsed]="false" styleClass="mb-4">
        <div class="flex flex-wrap gap-3 align-items-end">
          <p-floatlabel>
            <input pInputText id="org-name" [(ngModel)]="newName" name="orgName" class="w-full md:w-20rem" />
            <label for="org-name">Name</label>
          </p-floatlabel>
          <p-floatlabel>
            <input pInputText id="org-slug" [(ngModel)]="newSlug" name="orgSlug" class="w-full md:w-15rem" />
            <label for="org-slug">Slug (optional)</label>
          </p-floatlabel>
          <p-button label="Create" icon="pi pi-plus" [disabled]="!newName.trim() || creating()" [loading]="creating()" (onClick)="createOrg()" />
        </div>
      </p-panel>
    }
    <p-table [value]="orgs()" [loading]="loading()" styleClass="p-datatable-sm" [tableStyle]="{ 'min-width': '50rem' }">
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
            <a [routerLink]="['/organizations', o.id]" class="text-primary font-medium">{{ o.name }}</a>
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
  `,
})
export class OrganizationListComponent implements OnInit {
  private readonly api = inject(LmsApiService);
  private readonly errors = inject(GlobalErrorService);
  readonly auth = inject(AuthService);

  readonly orgs = signal<OrganizationDto[]>([]);
  readonly loading = signal(true);
  readonly creating = signal(false);

  newName = '';
  newSlug = '';

  isAdmin(): boolean {
    return this.auth.user()?.roles?.some((r) => r === 'Admin') ?? false;
  }

  ngOnInit(): void {
    this.errors.clear();
    this.api.listOrganizations().subscribe({
      next: (list) => {
        this.orgs.set(list);
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
      next: (o) => {
        this.orgs.update((list) => [...list, o]);
        this.newName = '';
        this.newSlug = '';
        this.creating.set(false);
      },
      error: () => this.creating.set(false),
    });
  }
}
