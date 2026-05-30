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
  templateUrl: './license-pool-detail.component.html',
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
  showAssignForm = false;

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
