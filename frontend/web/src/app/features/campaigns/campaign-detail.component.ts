import { DatePipe } from '@angular/common';
import { Component, inject, OnInit, signal } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { ActivatedRoute, RouterLink } from '@angular/router';
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
    Panel,
    InputTextModule,
    MultiSelectModule,
    PageShellComponent,
    UiButtonComponent,
    UiDataTableComponent,
    UiDataTableHeaderTemplateDirective,
    UiDataTableBodyTemplateDirective,
  ],
  template: `
    <a
      routerLink="/campaigns"
      class="text-primary font-medium inline-block mb-3"
      >← Back to campaigns</a
    >

    @if (loading()) {
      <p>Loading…</p>
    } @else {
      @if (campaign(); as c) {
        <app-page-shell title="{{ c.name }}" [subtitle]="c.id">
          <ng-container pageActions>
            <app-ui-button
              label="Refresh"
              icon="pi pi-refresh"
              severity="secondary"
              [text]="true"
              (clicked)="reload()"
            />
          </ng-container>

          <div class="flex gap-3 flex-wrap mb-3">
            <span class="text-sm text-color-secondary"
              >Scope: <strong>{{ c.scope }}</strong></span
            >
            <span class="text-sm text-color-secondary"
              >Status: <strong>{{ c.status }}</strong></span
            >
            <span class="text-sm text-color-secondary">
              Window: <strong>{{ c.startUtc | date: 'medium' }}</strong> →
              <strong>{{
                c.endUtc ? (c.endUtc | date: 'medium') : '—'
              }}</strong>
            </span>
          </div>

          @if (analytics(); as a) {
            <p-panel header="Analytics" styleClass="mb-4">
              <div class="flex gap-4 flex-wrap">
                <div>
                  <div class="text-sm text-color-secondary">
                    Total redemptions
                  </div>
                  <div class="text-lg font-medium">
                    {{ a.totalRedemptions }}
                  </div>
                </div>
                <div>
                  <div class="text-sm text-color-secondary">Unique buyers</div>
                  <div class="text-lg font-medium">{{ a.uniqueBuyers }}</div>
                </div>
                <div>
                  <div class="text-sm text-color-secondary">
                    Discount total (cents)
                  </div>
                  <div class="text-lg font-medium">
                    {{ a.totalDiscountCents }}
                  </div>
                </div>
                <div>
                  <div class="text-sm text-color-secondary">Last redeemed</div>
                  <div class="text-lg font-medium">
                    {{
                      a.lastRedeemedAtUtc
                        ? (a.lastRedeemedAtUtc | date: 'medium')
                        : '—'
                    }}
                  </div>
                </div>
              </div>
            </p-panel>
          }

          <p-panel header="Preview (quote)" styleClass="mb-4">
            <div class="flex flex-column gap-3" style="max-width: 40rem">
              <label class="text-sm font-medium" for="p-itemType"
                >Item type</label
              >
              <p-multiSelect
                id="p-itemType"
                [options]="itemTypeOptions"
                [(ngModel)]="previewItemTypeArr"
                optionLabel="label"
                optionValue="value"
                [maxSelectedLabels]="1"
                styleClass="w-full"
              />

              <label class="text-sm font-medium" for="p-ref"
                >Reference ID (GUID)</label
              >
              <input
                id="p-ref"
                pInputText
                [(ngModel)]="previewReferenceId"
                placeholder="Course/Class/Pool ID"
              />

              <label class="text-sm font-medium" for="p-qty">Quantity</label>
              <input
                id="p-qty"
                pInputText
                type="number"
                min="1"
                [(ngModel)]="previewQuantity"
              />

              <label class="text-sm font-medium" for="p-org"
                >Organization ID (optional)</label
              >
              <input
                id="p-org"
                pInputText
                [(ngModel)]="previewOrgId"
                placeholder="UUID (enables B2B volume tiers)"
              />

              <label class="text-sm font-medium" for="p-coupon"
                >Coupon code (optional)</label
              >
              <input
                id="p-coupon"
                pInputText
                [(ngModel)]="previewCoupon"
                placeholder="e.g. SPRING20"
              />

              <app-ui-button
                label="Preview quote"
                icon="pi pi-search"
                [loading]="previewing()"
                [disabled]="previewing() || !previewReferenceId.trim()"
                (clicked)="previewQuote()"
              />

              @if (preview(); as q) {
                <p class="m-0 text-sm text-color-secondary">
                  Subtotal: <strong>{{ q.subtotalCents }}</strong> · Discount:
                  <strong>{{ q.discountCents }}</strong> · Total:
                  <strong>{{ q.totalCents }}</strong>
                </p>
              }
            </div>
          </p-panel>

          <p-panel header="Add rule: Item % off" styleClass="mb-4">
            <div class="flex flex-column gap-3" style="max-width: 40rem">
              <label class="text-sm font-medium" for="pct">Percent off</label>
              <input
                id="pct"
                pInputText
                type="number"
                min="1"
                max="100"
                [(ngModel)]="percentOff"
              />

              <label class="text-sm font-medium" for="types"
                >Applies to item types</label
              >
              <p-multiSelect
                id="types"
                [options]="itemTypeOptions"
                [(ngModel)]="selectedItemTypes"
                optionLabel="label"
                optionValue="value"
                placeholder="Select item types"
                styleClass="w-full"
              />

              <app-ui-button
                label="Add rule"
                icon="pi pi-plus"
                [loading]="savingRule()"
                [disabled]="savingRule() || !canAddRule()"
                (clicked)="addRule()"
              />
            </div>
          </p-panel>

          <p-panel header="Create coupon" styleClass="mb-4">
            <div class="flex flex-column gap-3" style="max-width: 40rem">
              <label class="text-sm font-medium" for="code">Code</label>
              <input
                id="code"
                pInputText
                [(ngModel)]="couponCode"
                placeholder="e.g. SPRING20"
              />

              <label class="text-sm font-medium" for="exp"
                >Expires (UTC, optional)</label
              >
              <input
                id="exp"
                pInputText
                type="datetime-local"
                [(ngModel)]="couponExpiresLocal"
              />

              <label class="text-sm font-medium" for="per"
                >Per-buyer max redemptions</label
              >
              <input
                id="per"
                pInputText
                type="number"
                min="1"
                [(ngModel)]="perBuyerMax"
              />

              <app-ui-button
                label="Create coupon"
                icon="pi pi-ticket"
                [loading]="savingCoupon()"
                [disabled]="savingCoupon() || !canCreateCoupon()"
                (clicked)="createCoupon()"
              />
            </div>
          </p-panel>

          <h2 class="text-xl">Rules</h2>
          <app-ui-data-table
            [value]="c.rules"
            [emptyColspan]="3"
            [showPaginator]="false"
            styleClass="mb-4"
          >
            <ng-template uiDataTableHeader>
              <tr>
                <th>Type</th>
                <th>Percent</th>
                <th>Applies to</th>
              </tr>
            </ng-template>
            <ng-template uiDataTableBody let-r>
              <tr>
                <td>{{ r.ruleType }}</td>
                <td>{{ r.percentOff }}%</td>
                <td>{{ r.appliesToItemTypes.join(', ') }}</td>
              </tr>
            </ng-template>
          </app-ui-data-table>

          <h2 class="text-xl">Coupons</h2>
          <app-ui-data-table
            [value]="c.coupons"
            [emptyColspan]="4"
            [showPaginator]="false"
          >
            <ng-template uiDataTableHeader>
              <tr>
                <th>Code</th>
                <th>Status</th>
                <th>Expires</th>
                <th>Per buyer</th>
              </tr>
            </ng-template>
            <ng-template uiDataTableBody let-cp>
              <tr>
                <td class="font-mono text-sm">{{ cp.code }}</td>
                <td>{{ cp.status }}</td>
                <td>
                  {{ cp.expiresUtc ? (cp.expiresUtc | date: 'medium') : '—' }}
                </td>
                <td>{{ cp.perBuyerMaxRedemptions }}</td>
              </tr>
            </ng-template>
          </app-ui-data-table>
        </app-page-shell>
      }
    }
  `,
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
