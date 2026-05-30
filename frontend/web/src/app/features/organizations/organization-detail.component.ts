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
  LearnerRiskDto,
  LmsApiService,
  OrganizationDetailDto,
  OrganizationRiskReportDto,
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
  templateUrl: './organization-detail.component.html',
})
export class OrganizationDetailComponent implements OnInit {
  private readonly api = inject(LmsApiService);
  private readonly route = inject(ActivatedRoute);
  private readonly errors = inject(GlobalErrorService);

  readonly detail = signal<OrganizationDetailDto | null>(null);
  readonly riskReport = signal<OrganizationRiskReportDto | null>(null);
  readonly selectedRisk = signal<LearnerRiskDto | null>(null);
  readonly loading = signal(true);
  readonly adding = signal(false);
  readonly loadingRisk = signal(false);
  showAddMember = false;

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

  loadRiskReport(): void {
    const org = this.detail()?.organization;
    if (!org) return;

    this.loadingRisk.set(true);
    this.api.getOrganizationRiskReport(org.id).subscribe({
      next: (report) => {
        this.riskReport.set(report);
        this.loadingRisk.set(false);
      },
      error: () => this.loadingRisk.set(false),
    });
  }

  riskFor(userId: string): LearnerRiskDto | null {
    return this.riskReport()?.learners.find((x) => x.userId === userId) ?? null;
  }

  riskSeverity(level: string): 'success' | 'warn' | 'danger' | 'info' {
    if (level === 'High') return 'danger';
    if (level === 'Medium') return 'warn';
    if (level === 'Low') return 'success';
    return 'info';
  }
}
