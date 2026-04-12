import { DatePipe } from '@angular/common';
import { Component, inject, OnInit, signal } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { ActivatedRoute, RouterLink } from '@angular/router';
import { Button } from 'primeng/button';
import { DropdownModule } from 'primeng/dropdown';
import { InputText } from 'primeng/inputtext';
import { Panel } from 'primeng/panel';
import { PrimeTemplate } from 'primeng/api';
import { TableModule } from 'primeng/table';
import { Tag } from 'primeng/tag';
import {
  LmsApiService,
  OrganizationDetailDto,
} from '../../core/api/lms-api.service';
import { GlobalErrorService } from '../../core/error/global-error.service';

@Component({
  selector: 'app-organization-detail',
  standalone: true,
  imports: [
    DatePipe,
    FormsModule,
    RouterLink,
    TableModule,
    Button,
    Panel,
    InputText,
    DropdownModule,
    Tag,
    PrimeTemplate,
  ],
  template: `
    <p-button label="Back" icon="pi pi-arrow-left" [text]="true" routerLink="/organizations" styleClass="mb-3" />
    @if (loading()) {
      <p>Loading…</p>
    } @else {
      @if (detail(); as d) {
        <h1 class="text-2xl font-semibold mt-0">{{ d.organization.name }}</h1>
        <p class="text-color-secondary flex align-items-center gap-2 flex-wrap">
          <span>{{ d.organization.slug }}</span>
          <p-tag [value]="d.organization.status" severity="info" />
        </p>
        <h2 class="text-xl">Members</h2>
        <p-table [value]="d.members" styleClass="p-datatable-sm mb-4" [tableStyle]="{ 'min-width': '40rem' }">
          <ng-template pTemplate="header">
            <tr>
              <th>User ID</th>
              <th>Role</th>
              <th>Joined</th>
            </tr>
          </ng-template>
          <ng-template pTemplate="body" let-m>
            <tr>
              <td class="font-mono text-sm">{{ m.userId }}</td>
              <td>{{ m.orgRole }}</td>
              <td>{{ m.joinedAt | date: 'medium' }}</td>
            </tr>
          </ng-template>
        </p-table>
        <p-panel header="Add member">
          <div class="flex flex-column md:flex-row flex-wrap gap-3 align-items-end">
            <input pInputText [(ngModel)]="memberUserId" placeholder="User ID (GUID)" class="w-full md:w-25rem" name="uid" />
            <p-dropdown
              [options]="roleOptions"
              [(ngModel)]="memberOrgRole"
              optionLabel="label"
              optionValue="value"
              placeholder="Org role"
              styleClass="w-full md:w-12rem"
              name="orgRole"
            />
            <p-button label="Add" icon="pi pi-user-plus" [disabled]="!memberUserId.trim() || adding()" [loading]="adding()" (onClick)="addMember()" />
          </div>
        </p-panel>
      }
    }
  `,
})
export class OrganizationDetailComponent implements OnInit {
  private readonly api = inject(LmsApiService);
  private readonly route = inject(ActivatedRoute);
  private readonly errors = inject(GlobalErrorService);

  readonly detail = signal<OrganizationDetailDto | null>(null);
  readonly loading = signal(true);
  readonly adding = signal(false);

  memberUserId = '';
  memberOrgRole = 'Member';

  readonly roleOptions = [
    { label: 'OrgAdmin', value: 'OrgAdmin' },
    { label: 'Member', value: 'Member' },
    { label: 'Instructor', value: 'Instructor' },
  ];

  ngOnInit(): void {
    this.errors.clear();
    const id = this.route.snapshot.paramMap.get('id');
    if (!id) {
      this.loading.set(false);
      return;
    }
    this.api.getOrganization(id).subscribe({
      next: (d) => {
        this.detail.set(d);
        this.loading.set(false);
      },
      error: () => this.loading.set(false),
    });
  }

  addMember(): void {
    const org = this.detail()?.organization;
    if (!org) return;
    const userId = this.memberUserId.trim();
    if (!userId) return;
    this.errors.clear();
    this.adding.set(true);
    this.api
      .addMember(org.id, {
        userId,
        orgRole: this.memberOrgRole,
        departmentId: null,
      })
      .subscribe({
        next: () => {
          this.api.getOrganization(org.id).subscribe({
            next: (d) => this.detail.set(d),
          });
          this.memberUserId = '';
          this.adding.set(false);
        },
        error: () => this.adding.set(false),
      });
  }
}
