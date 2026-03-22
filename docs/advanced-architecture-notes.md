---
title: Advanced Architecture Notes
scope: B2B + B2C Hybrid Learning (Zoom + VOD)
status: draft
---

# Advanced Architecture Notes

This document rewrites and consolidates the production-grade suggestions for a B2B-first
LMS that also supports B2C and hybrid learning (Zoom + VOD).

## 1) Pricing Engine (Service + Rules)

**Goal:** deterministic pricing across B2B/B2C, campaigns, coupons, and taxes.

### Core flow
1. Identify context: B2C or B2B (Organization)
2. Lookup base price: Product/PricePlan
3. Apply campaign rules (global → eligible → stackable)
4. Apply coupon (if stacking is allowed)
5. Apply B2B volume discount (tier pricing)
6. Apply VAT/Tax by Organization region
7. Produce final price + audit trace of rules applied

### Suggested entities
- `PricingRule`
- `DiscountTier`
- `TaxProfile`
- `PriceSnapshot` (persist final price, source rules, and timestamps)

### Example rule order (pseudo)
```text
FinalPrice = BasePrice
  - GlobalDiscount
  - BestEligibleCampaignDiscount (or stack if allowed)
  - CouponDiscount (if stackable)
  - VolumeDiscount (B2B)
  + Tax
```

## 2) Reservation Pattern & Concurrency

**Problem:** avoid overbooking when many users buy limited seats or limited campaigns.

### Reservation flow (recommended)
1. User clicks checkout
2. Create `PendingOrder` (status = RESERVED)
3. Reserve seat in Class AND reserve campaign usage (if any)
4. Payment succeeds → mark order COMPLETED, create Enrollment
5. Payment fails/timeout → release seat + campaign usage

### Storage fields
- `Class.AvailableSeats`
- `Campaign.CurrentUsage`
- `PendingOrder.ExpiresAt`

### Atomic update examples (DB)
```sql
UPDATE classes
SET available_seats = available_seats - 1
WHERE id = 'CLASS_123' AND available_seats > 0;
```

```sql
UPDATE campaigns
SET current_usage = current_usage + 1
WHERE id = 'FLASHSALE_ID' AND current_usage < max_usage;
```

### Scaling options
- DB atomic updates (default)
- Reservation TTL (15–30 minutes)
- Redis lock / Redlock for very high contention

### Capacity split (B2B vs B2C)
Maintain:
- `total_capacity`
- `b2b_reserved_seats`
- `public_available_seats`

B2C competes only inside `public_available_seats`. B2B allocation adjusts reserved seats.

## 3) Zoom + VOD Automation via Domain Events

### Zoom (Live Sessions)
- `SessionCreated` → call Zoom API → store `MeetingId`
- `SessionStarted` → notify learners with Zoom link
- `ZoomWebhook.MeetingEnded` → fetch participants → auto-mark attendance

### VOD (Video Learning)
- Client sends heartbeat every 30–60s
- Server aggregates watch time in `Progress`
- When watch >= threshold (e.g., 80%) → raise `LessonCompleted`

### Infrastructure boundaries
- Webhooks handled in Infrastructure
- Mapping/validation in Application
- State changes in Domain aggregates

## 4) Seat Management (B2B)

### Core tables
| Table | Key Columns | Purpose |
|---|---|---|
| `LicensePool` | `OrgId, CourseId, TotalSlots, UsedSlots, ExpiryDate` | Seat inventory per org |
| `LicenseAssignment` | `PoolId, LearnerId, AssignedAt, Status` | Track seat distribution |
| `Enrollment` | `UserId, ClassId, SourceType, ReferenceId` | Final learning access |

### Rule
If `UsedSlots >= TotalSlots`, block new assignments.

## 5) Campaign Concurrency

**Flash sales** and **limited campaigns** must enforce usage limits.

Recommended:
- `CampaignUsage` table
- Atomic update with usage cap
- Reservation window to prevent oversubscription

## 6) Suggested Aggregates (recap)

- `OrganizationContext` (Org, LicensePool, Membership)
- `ClassManagement` (Class, Session, InstructorAssignment)
- `AttendanceControl` (AttendanceSheet, AttendanceRecord)
- `Assessment` (Quiz, Attempt, Score)
- `LearningPath` (Path, PathProgress)
- `Commerce` (Order, Payment, Refund)
- `Campaign` (Campaign, PromotionRule, Usage)
- `Identity/Profile` (User, LearnerProfile, InstructorProfile)
