import { DatePipe } from '@angular/common';
import { Component, inject, OnInit, signal } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { ActivatedRoute, RouterLink } from '@angular/router';
import { Button } from 'primeng/button';
import { InputTextModule } from 'primeng/inputtext';
import { Panel } from 'primeng/panel';
import { MultiSelectModule } from 'primeng/multiselect';
import {
  LmsApiService,
  CampaignAnalyticsDto,
  CampaignDto,
  PreviewCampaignQuoteRequest,
  PromotionQuoteDto,
} from '../../core/api/lms-api.service';
import { AuthService } from '../../core/auth/auth.service';
import { GlobalErrorService } from '../../core/error/global-error.service';
import { PageShellComponent } from '../../shared/ui';
import { UiButtonComponent } from '../../shared/ui/ui-button/ui-button.component';
import { UiDataTableComponent } from '../../shared/ui/ui-data-table/ui-data-table.component';
import {
  UiDataTableBodyTemplateDirective,
  UiDataTableHeaderTemplateDirective,
} from '../../shared/ui/ui-data-table/ui-data-table-templates.directive';

@Component({
  selector: 'app-campaign-detail',
  standalone: true,
  imports: [
    DatePipe,
    FormsModule,
    RouterLink,
    Button,
    Panel,
    InputTextModule,
    MultiSelectModule,
    PageShellComponent,
    UiButtonComponent,
    UiDataTableComponent,
    UiDataTableHeaderTemplateDirective,
    UiDataTableBodyTemplateDirective,
  ],
  templateUrl: './campaign-detail.component.html',
})
export class CampaignDetailComponent implements OnInit {
  private readonly api = inject(LmsApiService);
  private readonly errors = inject(GlobalErrorService);
  private readonly route = inject(ActivatedRoute);
  private readonly auth = inject(AuthService);

  campaignId = '';
  readonly campaign = signal<CampaignDto | null>(null);
  readonly analytics = signal<CampaignAnalyticsDto | null>(null);
  readonly preview = signal<PromotionQuoteDto | null>(null);
  readonly loading = signal(true);
  readonly savingRule = signal(false);
  readonly savingCoupon = signal(false);
  readonly previewing = signal(false);
  showPreviewForm = false;
  showRuleForm = false;
  showCouponForm = false;

  percentOff = 20;
  selectedItemTypes: string[] = ['Course'];

  couponCode = '';
  couponExpiresLocal = '';
  perBuyerMax = 1;

  previewItemTypeArr: string[] = ['Course'];
  previewReferenceId = '';
  previewQuantity = 1;
  previewOrgId = '';
  previewCoupon = '';

  readonly itemTypeOptions = [
    { label: 'Course', value: 'Course' },
    { label: 'TrainingClass', value: 'TrainingClass' },
    { label: 'LicensePool', value: 'LicensePool' },
  ];

  ngOnInit(): void {
    this.campaignId = this.route.snapshot.paramMap.get('id') ?? '';
    this.reload();
  }

  reload(): void {
    if (!this.campaignId) return;
    this.errors.clear();
    this.loading.set(true);
    this.preview.set(null);
    this.api.getCampaign(this.campaignId).subscribe({
      next: (c) => {
        this.campaign.set(c);
        this.loading.set(false);
        this.api.getCampaignAnalytics(this.campaignId).subscribe({
          next: (a) => this.analytics.set(a),
          error: () => this.analytics.set(null),
        });
      },
      error: () => this.loading.set(false),
    });
  }

  previewQuote(): void {
    const user = this.auth.user();
    if (!user || !this.campaignId) return;

    const itemType = this.previewItemTypeArr[0] ?? 'Course';
    const ref = this.previewReferenceId.trim();
    if (!ref) return;

    const org = this.previewOrgId.trim();
    const body: PreviewCampaignQuoteRequest = {
      buyerUserId: user.id,
      organizationId: org ? org : null,
      currency: 'USD',
      couponCode: this.previewCoupon.trim() ? this.previewCoupon.trim() : null,
      items: [
        {
          itemType,
          referenceId: ref,
          quantity: Math.max(1, Math.floor(Number(this.previewQuantity) || 1)),
        },
      ],
    };

    this.errors.clear();
    this.previewing.set(true);
    this.api.previewCampaign(this.campaignId, body).subscribe({
      next: (q) => {
        this.preview.set(q);
        this.previewing.set(false);
      },
      error: () => {
        this.preview.set(null);
        this.previewing.set(false);
      },
    });
  }

  canAddRule(): boolean {
    return (
      this.percentOff >= 1 &&
      this.percentOff <= 100 &&
      this.selectedItemTypes.length > 0
    );
  }

  addRule(): void {
    if (!this.campaignId || !this.canAddRule()) return;
    this.errors.clear();
    this.savingRule.set(true);
    this.api
      .addCampaignRule(this.campaignId, {
        percentOff: Math.floor(this.percentOff),
        appliesToItemTypes: this.selectedItemTypes,
      })
      .subscribe({
        next: (c) => {
          this.campaign.set(c);
          this.savingRule.set(false);
        },
        error: () => this.savingRule.set(false),
      });
  }

  canCreateCoupon(): boolean {
    return !!this.couponCode.trim() && this.perBuyerMax >= 1;
  }

  createCoupon(): void {
    if (!this.campaignId || !this.canCreateCoupon()) return;
    this.errors.clear();
    this.savingCoupon.set(true);
    this.api
      .createCampaignCoupon(this.campaignId, {
        code: this.couponCode.trim(),
        expiresUtc: this.couponExpiresLocal
          ? new Date(this.couponExpiresLocal).toISOString()
          : null,
        perBuyerMaxRedemptions: Math.floor(this.perBuyerMax),
      })
      .subscribe({
        next: (c) => {
          this.campaign.set(c);
          this.savingCoupon.set(false);
          this.couponCode = '';
          this.couponExpiresLocal = '';
          this.perBuyerMax = 1;
        },
        error: () => this.savingCoupon.set(false),
      });
  }
}
