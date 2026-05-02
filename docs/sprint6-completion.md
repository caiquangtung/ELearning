---
title: Sprint 6 completion — Commerce (Orders + checkout)
status: mvp-done
---

## Goal

Deliver **Commerce checkout**: priced orders, **15-minute checkout window**, optional **seat holds** on training classes during pending payment, **payment intent + completion**, **invoice** row, and a **payment webhook** hook point.

## Delivered (backend MVP)

### Domain & pricing

- **Orders**: `Order` checkout expiry (`CheckoutExpiresAtUtc`), `TryExpireCheckout`, idempotent `MarkPaid` (`src/ELearning.Domain/Aggregates/OrderAggregate/*`).
- **Catalog prices** (server-side; client `unitPriceCents` on create is ignored):
  - `Course.PriceCents` / `Course.Currency`
  - `TrainingClass.PriceCents` / `TrainingClass.Currency`
  - `LicensePool.SeatPriceCents` / `LicensePool.Currency`
- **Commerce entities**: `OrderPayment`, `Invoice`, `CheckoutReservation` (`src/ELearning.Domain/Aggregates/CommerceAggregate/*`).

### Application

- Create order (priced server-side + reservations): `Features/Orders/CreateOrder/*`
- Pay order (NoOp provider completes inline): `Features/Orders/PayOrder/*`
- Complete payment (shared by pay + webhook): `Features/Orders/CompletePayment/*`
- Get / list orders: `Features/Orders/GetOrder/*`, `Features/Orders/ListMyOrders/*`

### API (`api/v1`)

| Method | Route | Permission | Notes |
|--------|-------|------------|--------|
| `POST` | `/orders` | `Commerce.Create` | Creates pending checkout; sets checkout expiry |
| `GET` | `/orders/{id}` | `Commerce.Read` | Includes `checkoutExpiresAtUtc` when applicable |
| `GET` | `/orders/my` | `Commerce.Read` | Buyer history |
| `GET` | `/orders/{id}/invoice` | `Commerce.Read` | Invoice summary when the order is **Paid** |
| `POST` | `/orders/{id}/pay` | `Commerce.Pay` | Creates payment intent; **NoOp** verifies + completes immediately |
| `POST` | `/payments/webhook` | *(anonymous + optional secret)* | Body `{ "transactionId": "..." }`; header `X-Payments-Webhook-Secret` when configured |

### Payments configuration

Bound from `Payments` section (`src/ELearning.Application/Common/Options/PaymentOptions.cs`):

```json
{
  "Payments": {
    "Provider": "NoOp",
    "WebhookSecret": "dev-payments-webhook-secret-change-me"
  }
}
```

Development defaults live in `src/ELearning.WebApi/appsettings.Development.json`.

### Persistence / migrations

- Orders checkout column + commerce tables migration:

```14:67:src/ELearning.Infrastructure/Persistence/Migrations/20260502080808_Sprint6_CommercePaymentsInvoiceReservation.cs
            migrationBuilder.AddColumn<DateTime>(
                name: "checkout_expires_at",
                table: "orders",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "checkout_reservations",
                // ...
            );

            migrationBuilder.CreateTable(
                name: "invoices",
                // ...
            );

            migrationBuilder.CreateTable(
                name: "order_payments",
                // ...
            );
```

- Catalog pricing columns: `20260501094030_Sprint6_PricingFields` (courses / training_classes / license_pools).

### Behaviour notes

- **Checkout timeout**: `CommerceConstants.CheckoutTimeout` = **15 minutes** (`Features/Orders/CommerceConstants.cs`). Expired pending orders are cancelled when `/pay` or the webhook completion runs.
- **Seat reservation**: For `TrainingClass` line items, rows in `checkout_reservations` reserve quantity against class capacity (enrolled count is **0** in MVP — see handler constant). Reservations are removed when payment completes or checkout expires/cancels.
- **Provider**: `NoOpPaymentService` implements `IPaymentService` (`Infrastructure/Payments/NoOpPaymentService.cs`). Swap implementation for Stripe/VNPay without changing handlers.

### Tests

- `tests/ELearning.Domain.UnitTests/OrderAggregateTests.cs` (includes expiry behaviour).

## Angular (`frontend/web`)

- **API client** (`src/app/core/api/lms-api.service.ts`): DTOs include `priceCents` / `currency` on courses and training classes, `seatPriceCents` / `currency` on license pools; order types and methods `createOrder`, `getOrder`, `listMyOrders`, `payOrder`, `getOrderInvoice`.
- **Routes** (`app.routes.ts`): `/checkout` (query `type`, `ref`, optional `qty`), `/orders`, `/orders/:id`.
- **Nav**: “My orders” in `main-layout.component.ts`.
- **Purchase entry points**: “Buy course” on published courses with `priceCents > 0`; “Enroll / checkout” on training classes with price and not cancelled; “Buy seats” on license pools with `seatPriceCents > 0`.
- **Checkout** (`features/checkout/checkout.component.ts`): loads the entity, shows line total, optional discount (cents) and organization id, submits `POST /orders`, then navigates to order detail with `?pay=1` to trigger **Pay now (NoOp)**.
- **Orders** (`features/orders/`): list with link to detail; detail shows status, checkout expiry, line items, pay CTA for `PendingPayment`, and invoice panel when paid.

```bash
cd frontend/web && npm run build
```

## Deferred / follow-ups

- Real **Stripe / VNPay** implementation + signing of webhook payloads (not just shared secret header).
- **Invoice PDF** or downloadable artifact (UI shows metadata only today).
- **Enrollment** tied to paid orders (capacity then subtracts enrolled learners, not only reservations).
- **API integration tests** for `/orders`, `/pay`, `/payments/webhook`.

## Validation

- `dotnet test src/ELearning.sln`
- `cd frontend/web && npm run build`
