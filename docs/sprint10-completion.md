---
title: Sprint 10 completion — Notifications & Messaging
status: Backend MVP delivered
date: 2026-05-19
scope: Backend MVP
---

# Sprint 10 Completion — Notifications & Messaging

## Delivered

- Notification/message domain model:
  - `Notification` supports recipient, title/body, type, optional action URL, optional source `MessageId`, read state, and idempotent mark-read behavior.
  - `Message` records announcement sender, scope, optional org/course/class target IDs, recipient count, and sent timestamp.
- Application features:
  - Send in-app notification to a user.
  - List current user's notifications with pagination and unread filter.
  - Get unread notification count.
  - Mark current user's notification as read.
  - Send announcement to explicit recipients and fan out in-app notifications linked to the message.
  - Send email through `IEmailService` with a `NoOpEmailService` development implementation.
- Web API:
  - `GET /api/v1/notifications`
  - `GET /api/v1/notifications/unread-count`
  - `POST /api/v1/notifications`
  - `POST /api/v1/notifications/{id}/read`
  - `POST /api/v1/notifications/announcements`
  - `POST /api/v1/notifications/email`
- Infrastructure:
  - EF configurations and repositories for `messages` and `notifications`.
  - DI registrations for repositories and no-op email service.
  - EF migration `Sprint10_NotificationsMessaging`.
  - Permission constants and role mapping for notification read/send access.
- Tests:
  - Domain tests for notification creation, mark-read idempotency, announcement creation, and domain validation.
  - Application validator smoke tests for notification, announcement, email, and notification list paging.

## Deferred

- Notification and email template engine.
- Background delivery queue/scheduler.
- Redis-backed unread count cache using `notifications:unread:{userId}`.
- Real email provider integration such as SendGrid or AWS SES.
- SignalR or Redis Pub/Sub real-time notification delivery.
- Angular notification bell, notification list page, announcement UI, and email preferences.
- API integration tests for notification endpoints.

## Verification

- `dotnet test src/ELearning.sln` passes:
  - Domain unit tests: 34 passed.
  - Application unit tests: 16 passed.
  - Architecture tests: 1 passed.

## Notes

- This is a backend MVP. The unread count endpoint currently queries the database directly; the Redis cache requirement remains a follow-up and should align with Sprint 15a shared cache abstractions.
- `Sprint10_NotificationsMessaging` intentionally creates only `messages` and `notifications`. The EF model snapshot now includes Sprint 9 certificates plus Sprint 10 notifications so future generated migrations do not repeat either model.
