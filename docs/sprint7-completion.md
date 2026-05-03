---
title: Sprint 7 completion — Campaigns & Coupons (MVP)
status: mvp-done
---

## Goal

Deliver an MVP **campaign + coupon** system with **item-level % off** promotions, a **checkout quote** endpoint, and an **admin UI** to manage campaigns/rules/coupons.

## Delivered

### Domain

- `Campaign` + `PromotionRule` + `Coupon` + `CouponRedemption` + `CouponUsageReservation`:
  - `src/ELearning.Domain/Aggregates/PromotionAggregate/*`
- Campaign supports **Global** and **Organization** scopes, status, and eligibility window.
- Rule type MVP: `ItemPercentOff` targeting `OrderItemType` values.
- Coupon supports code normalization, optional expiry, and per-buyer max redemptions.
 - Usage limits enforced with **checkout-window reservations**.

### Database

- Migration: `20260502154206_Sprint7_Promotions`
  - Tables: `campaigns`, `promotion_rules`, `coupons`, `coupon_redemptions`
  - Unique index: `coupons.code_normalized`
- Migrations:
  - `20260502160839_Sprint7_OrderAppliedCoupon` (`orders.applied_coupon_code`)
  - `Sprint7_CouponUsageReservations` (`coupon_usage_reservations`)

### API (`api/v1`)

#### Checkout quote

- `POST /checkout/quote` (`Commerce.Read`)
  - Applies **best eligible** discounts (global/org + coupon campaign), includes **B2B volume tiers**, and returns totals.
  - Coupon per-buyer limit is validated against redemptions (and enforced atomically at order creation).

#### Campaign admin (MVP)

All endpoints require `Admin.Access`:

- `GET /campaigns?organizationId={guid?}&includeGlobal=true&take=50`
- `GET /campaigns/{id}`
- `GET /campaigns/{id}/analytics`
- `POST /campaigns/{id}/preview` *(admin preview quote for a specific campaign)*
- `POST /campaigns`
- `POST /campaigns/{id}/rules`
- `POST /campaigns/{id}/coupons`

### Angular (`frontend/web`)

- **Checkout coupon input + quote**:
  - `frontend/web/src/app/features/checkout/checkout.component.ts`
  - Calls `POST /checkout/quote` and uses returned `discountCents` when creating the order.
- **Campaign admin UI**:
  - Routes: `/campaigns`, `/campaigns/:id`
  - Components:
    - `frontend/web/src/app/features/campaigns/campaign-list.component.ts`
    - `frontend/web/src/app/features/campaigns/campaign-detail.component.ts`
  - Nav item “Campaigns” is visible for users with role `Admin`.
  - Analytics panel is shown on campaign detail.
  - Preview panel calls `POST /campaigns/{id}/preview` for a single-item quote.

## Tests

- `tests/ELearning.Application.UnitTests/QuoteCheckoutTests.cs`

## Validation

```bash
dotnet test src/ELearning.sln -c Release
cd frontend/web && npm run build
```

