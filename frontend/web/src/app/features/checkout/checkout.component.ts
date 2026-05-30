import { Component, inject, OnInit, signal } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { ActivatedRoute, Router } from '@angular/router';
import { InputTextModule } from 'primeng/inputtext';
import { Panel } from 'primeng/panel';
import {
  CreateOrderRequest,
  LmsApiService,
  CourseDetailDto,
  TrainingClassDetailDto,
  LicensePoolDetailDto,
  PromotionQuoteDto,
  QuoteCheckoutRequest,
} from '../../core/api/lms-api.service';
import { AuthService } from '../../core/auth/auth.service';
import { GlobalErrorService } from '../../core/error/global-error.service';
import { PageShellComponent } from '../../shared/ui';
import { UiButtonComponent } from '../../shared/ui/ui-button/ui-button.component';

const uuidRe =
  /^[0-9a-f]{8}-[0-9a-f]{4}-[1-5][0-9a-f]{3}-[89ab][0-9a-f]{3}-[0-9a-f]{12}$/i;

type CheckoutType = 'Course' | 'TrainingClass' | 'LicensePool';

function formatMoney(cents: number, currency: string): string {
  return `${(cents / 100).toFixed(2)} ${currency}`;
}

@Component({
  selector: 'app-checkout',
  standalone: true,
  imports: [
    FormsModule,
    InputTextModule,
    Panel,
    PageShellComponent,
    UiButtonComponent,
  ],
  templateUrl: './checkout.component.html',
})
export class CheckoutComponent implements OnInit {
  private readonly api = inject(LmsApiService);
  private readonly route = inject(ActivatedRoute);
  private readonly router = inject(Router);
  private readonly auth = inject(AuthService);
  private readonly errors = inject(GlobalErrorService);

  readonly loading = signal(true);
  readonly invalidParams = signal(false);
  readonly submitting = signal(false);
  readonly title = signal('');
  readonly unitPriceCents = signal(0);
  readonly currency = signal('USD');
  readonly quantity = signal(1);
  readonly lineTotalCents = signal(0);
  readonly quote = signal<PromotionQuoteDto | null>(null);
  readonly formatMoney = formatMoney;

  qtyModel = 1;
  couponModel = '';
  orgModel = '';

  private checkoutType: CheckoutType | null = null;
  private referenceId = '';

  ngOnInit(): void {
    const q = this.route.snapshot.queryParamMap;
    const type = q.get('type') as CheckoutType | null;
    const ref = q.get('ref') ?? '';
    const qtyRaw = q.get('qty');

    if (!type || !ref || !uuidRe.test(ref)) {
      this.invalidParams.set(true);
      this.loading.set(false);
      return;
    }
    if (
      type !== 'Course' &&
      type !== 'TrainingClass' &&
      type !== 'LicensePool'
    ) {
      this.invalidParams.set(true);
      this.loading.set(false);
      return;
    }

    this.checkoutType = type;
    this.referenceId = ref;
    const qty = Math.max(1, Number.parseInt(qtyRaw ?? '1', 10) || 1);
    this.quantity.set(qty);
    this.qtyModel = qty;

    this.loadEntity();
  }

  typeLabel(): string {
    switch (this.checkoutType) {
      case 'Course':
        return 'Course purchase';
      case 'TrainingClass':
        return 'Training class enrollment';
      case 'LicensePool':
        return 'License pool seats';
      default:
        return '';
    }
  }

  onQtyChange(v: string | number): void {
    const n = typeof v === 'number' ? v : Number.parseInt(String(v), 10);
    const q = Number.isFinite(n) && n >= 1 ? Math.floor(n) : 1;
    this.quantity.set(q);
    this.recalcLine();
    this.refreshQuote();
  }

  canSubmit(): boolean {
    return !!this.auth.user() && this.unitPriceCents() > 0;
  }

  private loadEntity(): void {
    const type = this.checkoutType;
    const id = this.referenceId;
    if (!type) return;

    this.errors.clear();
    this.loading.set(true);
    this.quote.set(null);

    if (type === 'Course') {
      this.api.getCourse(id).subscribe({
        next: (c: CourseDetailDto) => this.applyCourse(c),
        error: () => this.failLoad(),
      });
      return;
    }
    if (type === 'TrainingClass') {
      this.api.getTrainingClass(id).subscribe({
        next: (tc: TrainingClassDetailDto) => this.applyTrainingClass(tc),
        error: () => this.failLoad(),
      });
      return;
    }
    this.api.getLicensePool(id).subscribe({
      next: (p: LicensePoolDetailDto) => this.applyPool(p),
      error: () => this.failLoad(),
    });
  }

  private applyCourse(c: CourseDetailDto): void {
    this.title.set(c.title);
    this.currency.set(c.currency || 'USD');
    this.unitPriceCents.set(c.priceCents ?? 0);
    this.recalcLine();
    this.loading.set(false);
    this.refreshQuote();
  }

  private applyTrainingClass(tc: TrainingClassDetailDto): void {
    this.title.set(tc.title);
    this.currency.set(tc.currency || 'USD');
    this.unitPriceCents.set(tc.priceCents ?? 0);
    this.recalcLine();
    this.loading.set(false);
    this.refreshQuote();
  }

  private applyPool(p: LicensePoolDetailDto): void {
    this.title.set(p.name);
    this.currency.set(p.currency || 'USD');
    this.unitPriceCents.set(p.seatPriceCents ?? 0);
    this.recalcLine();
    this.loading.set(false);
    this.refreshQuote();
  }

  private failLoad(): void {
    this.loading.set(false);
    this.invalidParams.set(true);
  }

  private recalcLine(): void {
    this.lineTotalCents.set(this.unitPriceCents() * this.quantity());
  }

  applyCoupon(): void {
    this.refreshQuote();
  }

  private refreshQuote(): void {
    const user = this.auth.user();
    const type = this.checkoutType;
    if (!user || !type || !this.referenceId || this.unitPriceCents() <= 0)
      return;

    const orgTrim = this.orgModel.trim();
    const orgId = orgTrim && uuidRe.test(orgTrim) ? orgTrim : null;
    const coupon = this.couponModel.trim();

    const body: QuoteCheckoutRequest = {
      buyerUserId: user.id,
      organizationId: orgId,
      currency: this.currency(),
      couponCode: coupon ? coupon : null,
      items: [
        {
          itemType: type,
          referenceId: this.referenceId,
          quantity: this.quantity(),
        },
      ],
    };

    this.errors.clear();
    this.api.quoteCheckout(body).subscribe({
      next: (q) => this.quote.set(q),
      error: () => this.quote.set(null),
    });
  }

  submit(): void {
    const user = this.auth.user();
    const type = this.checkoutType;
    if (!user || !type) return;

    const orgTrim = this.orgModel.trim();
    const orgId = orgTrim && uuidRe.test(orgTrim) ? orgTrim : null;
    const qty = this.quantity();
    const discount = this.quote()?.discountCents ?? 0;

    const body: CreateOrderRequest = {
      buyerUserId: user.id,
      organizationId: orgId,
      currency: this.currency(),
      discountCents: discount > 0 ? discount : undefined,
      items: [
        {
          itemType: type,
          referenceId: this.referenceId,
          quantity: qty,
          unitPriceCents: this.unitPriceCents(),
        },
      ],
    };

    this.errors.clear();
    this.submitting.set(true);
    this.api.createOrder(body).subscribe({
      next: (o) => {
        this.submitting.set(false);
        void this.router.navigate(['/orders', o.id], {
          queryParams: { pay: '1' },
        });
      },
      error: () => this.submitting.set(false),
    });
  }
}
