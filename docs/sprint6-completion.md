---
title: Sprint 6 completion — Commerce (Orders MVP)
status: in-progress
---

## Goal

Deliver a first **Commerce** slice: orders + order history, with totals calculated and persisted.

## Delivered (MVP)

### Backend

- **Domain**: `Order` + `OrderItem` (`src/ELearning.Domain/Aggregates/OrderAggregate/*`)
  - Draft → PendingPayment flow (`SubmitForPayment`)
  - Manual discount support
  - Totals computed as \( \text{subtotal} - \text{discount} \)
- **Application**:
  - Create order: `ELearning.Application/Features/Orders/CreateOrder/*`
  - Get order: `ELearning.Application/Features/Orders/GetOrder/*`
  - List buyer orders: `ELearning.Application/Features/Orders/ListMyOrders/*`
- **API** (`api/v1`):
  - `POST /orders`
  - `GET /orders/{id}`
  - `GET /orders/my?buyerUserId={guid}&take=50`
- **Permissions**
  - Added `Commerce.Read`, `Commerce.Create`, `Commerce.Pay` (`src/ELearning.Core/Constants/Permissions.cs`)
  - Role mapping updated (`src/ELearning.Core/Constants/PermissionMap.cs`)

### Persistence / DB

- **EF configurations**
  - `src/ELearning.Infrastructure/Persistence/Configurations/OrderConfiguration.cs`
  - `src/ELearning.Infrastructure/Persistence/Configurations/OrderItemConfiguration.cs`
- **Migration**
  - `20260430163239_Sprint6_CommerceOrders` (`src/ELearning.Infrastructure/Persistence/Migrations/`)

### Tests

- `tests/ELearning.Domain.UnitTests/OrderAggregateTests.cs`

## Deferred / follow-ups

- Catalog-based pricing engine (derive prices from Course/Class/LicensePool price tables)
- Payment integration (Stripe/VNPay) + webhook handling
- Invoice generation + storage
- Reservation/timeout logic for seat holds during checkout
- Integration tests for Orders API

## Validation

- `dotnet test src/ELearning.sln`

