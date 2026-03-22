---
title: Challenges & Solutions - ELearning Platform
scope: Business Logic, Concurrency, Data Consistency, Scalability
status: draft
---

# Challenges & Solutions - ELearning Platform

This document outlines the key challenges we'll face building a B2B+B2C hybrid LMS and architectural solutions (no code, conceptual only).

---

## 1. Concurrency & Race Conditions

### Challenge 1.1: Seat Reservation (Flash Sale / Limited Capacity)
**Scenario**: 1000 users try to enroll in a Zoom class with only 100 seats at the same time.

**Problems**:
- Over-booking: Multiple users see "100 seats available", all click "Enroll", system accepts 1000 enrollments
- Under-utilization: System locks seats but users don't complete payment, seats stay locked
- Fairness: Who gets priority? First click? First payment?

**Solutions**:
- **Optimistic locking**: Use database row versioning (EF Core concurrency tokens) to detect conflicts
- **Pessimistic locking**: Lock rows during checkout (careful with deadlocks)
- **Atomic operations**: Use database-level atomic updates (UPDATE SET seats = seats - 1 WHERE seats > 0)
- **Reservation pattern**: Hold seat for 15-30 minutes during checkout, release if payment fails
- **Queue system**: Put high-demand enrollments in queue, process FIFO
- **Distributed lock**: Use Redis for high-traffic scenarios (Redlock pattern)

**Trade-offs**:
- Atomic DB updates: Simple but may bottleneck at very high scale
- Reservation: Fair but complex (need cleanup jobs for expired reservations)
- Queue: Most fair but adds latency

---

### Challenge 1.2: Campaign Usage Limits
**Scenario**: "First 100 users get 50% off" - but 1000 users apply the coupon simultaneously.

**Problems**:
- Over-redemption: 200 users get the discount instead of 100
- Unfair distribution: Same user uses multiple accounts

**Solutions**:
- **Atomic counter**: Database atomic increment on campaign usage
- **Distributed counter**: Redis INCR for high throughput
- **Idempotency**: Ensure same user can't use same campaign twice (userId + campaignId unique constraint)
- **Two-phase apply**: Reserve campaign slot, confirm after payment

---

### Challenge 1.3: License Pool Exhaustion
**Scenario**: Organization has 100 licenses, admins try to assign 120 employees simultaneously.

**Problems**:
- Over-assignment: More licenses assigned than purchased
- License leakage: User leaves company, license not reclaimed

**Solutions**:
- **Database constraint**: CHECK constraint (assigned_count <= total_count)
- **Aggregate root validation**: LicensePool aggregate validates before assignment
- **Saga pattern**: Long-running transaction with compensation (revoke if validation fails)
- **License reclamation job**: Background job to reclaim inactive licenses

---

## 2. Business Logic Complexity

### Challenge 2.1: Pricing Engine
**Scenario**: Calculate final price with multiple overlapping rules.

**Problems**:
- Stacking rules: Can user apply coupon + campaign + volume discount + early bird?
- Conflicting rules: Campaign A says "50% off", Campaign B says "$100 off" - which wins?
- Regional pricing: Different tax rates, currency conversion
- B2B vs B2C: Different pricing logic entirely

**Solutions**:
- **Rule engine pattern**: Define priority, stacking rules, and evaluation order
- **Strategy pattern**: Different pricing strategies for B2B/B2C
- **Immutable price snapshot**: Save pricing calculation result with order (for audit)
- **Price preview**: Calculate and show final price before checkout
- **Clear documentation**: Document stacking rules publicly to avoid disputes

**Considerations**:
- Performance: Pricing calculation may involve multiple DB queries (user, org, campaigns)
- Caching: Cache campaign rules, but invalidate when campaigns change
- Testing: Pricing logic needs extensive unit tests for all combinations

---

### Challenge 2.2: Course Completion Rules
**Scenario**: When should a user be considered "complete" and eligible for certificate?

**Problems**:
- Multiple criteria: Attendance ≥ 80%, all lessons watched ≥ 80%, quiz score ≥ 70%
- Partial completion: User completes 95% but missing one lesson
- Retroactive changes: Instructor changes completion rules mid-course

**Solutions**:
- **Composite specification pattern**: Combine multiple rules (Attendance AND Progress AND Quiz)
- **Version completion rules**: Lock rules when course starts, don't change mid-flight
- **Grace period**: Allow slight deviations (79% attendance = pass)
- **Manual override**: Instructor can manually mark complete with reason

**Considerations**:
- Fairness: Rules should be clear and communicated upfront
- Flexibility: Some organizations may have different standards
- Audit: Log all completion decisions for dispute resolution

---

### Challenge 2.3: Enrollment Eligibility
**Scenario**: Can this user enroll in this class?

**Problems**:
- Prerequisites: Course A requires Course B completion
- Capacity: Class is full
- Time conflicts: User already enrolled in overlapping class
- Payment: B2C requires payment, B2B requires license
- Organization restrictions: Private class only for Company X employees

**Solutions**:
- **Chain of responsibility pattern**: Multiple eligibility validators
- **Domain service**: EnrollmentEligibilityService checks all rules
- **Clear error messages**: Tell user exactly why they can't enroll
- **Waitlist**: If class full, allow waitlist enrollment

---

## 3. Data Consistency

### Challenge 3.1: Distributed Transactions
**Scenario**: User pays for course → Payment succeeds → Enrollment creation fails.

**Problems**:
- User charged but not enrolled
- Partial state: Payment recorded, but no enrollment
- Rollback complexity: Hard to roll back payment to gateway

**Solutions**:
- **Saga pattern**: Orchestrate multi-step transaction with compensation
- **Outbox pattern**: Save events in DB, process asynchronously with retry
- **Idempotency**: Ensure operations can be retried safely (use idempotency keys)
- **Event sourcing**: Store all events, rebuild state from events
- **Manual reconciliation**: Admin tools to fix inconsistencies

**Trade-offs**:
- Saga: Complex but reliable
- Outbox: Eventual consistency (slight delay)
- Manual: Last resort for edge cases

---

### Challenge 3.2: Attendance vs Progress Sync
**Scenario**: Zoom webhook says user attended, but progress tracking shows no lesson completion.

**Problems**:
- Multiple sources of truth: Zoom, VOD watch events, manual attendance
- Timing: Webhook arrives 10 minutes after session ends
- Conflicts: Manual attendance says "Absent", Zoom says "Present"

**Solutions**:
- **Single source of truth**: Decide which source has priority
- **Conflict resolution rules**: Manual > Zoom > Automatic
- **Reconciliation job**: Periodically sync Zoom data with attendance records
- **Audit trail**: Keep all raw data (Zoom events) plus calculated attendance

---

### Challenge 3.3: License Expiry
**Scenario**: Organization's license pool expires, but users are mid-course.

**Problems**:
- Hard cutoff: User kicked out of course immediately
- Grace period: How long to allow access after expiry?
- Notifications: Did user get warned before expiry?

**Solutions**:
- **Grace period**: 7-30 days after expiry for in-progress courses
- **Warning notifications**: 30, 14, 7, 1 day before expiry
- **Organization freeze**: Block new enrollments but allow existing to finish
- **Proactive renewal**: Auto-charge or send renewal reminder 30 days before expiry

---

## 4. Integration Challenges

### Challenge 4.1: Zoom API Reliability
**Scenario**: Zoom API is down during peak hours.

**Problems**:
- Can't create sessions: Instructors can't schedule new Zoom meetings
- Can't fetch attendees: Attendance not recorded automatically
- Webhooks missed: Zoom webhook fails, attendance data lost

**Solutions**:
- **Circuit breaker pattern**: Stop calling Zoom after N failures, retry later
- **Fallback to manual**: If Zoom fails, allow instructors to enter meeting link manually
- **Webhook retry**: Store all webhook payloads, retry processing if failed
- **Alternative provider**: Have backup video conferencing (Google Meet, Teams)
- **Queue webhooks**: Don't process webhooks synchronously, queue for later

---

### Challenge 4.1.1: Zoom Webhook Development/Testing (No Dev Environment)
**Scenario**: Zoom doesn't provide development/staging webhook endpoints - how to test locally?

**Problems**:
- No dev webhooks: Can't test webhook handlers locally
- Local testing: Localhost not accessible from internet (Zoom can't send webhooks)
- Webhook payloads unknown: Don't know exact payload structure until production
- Testing attendance flow: Can't test automatic attendance recording in dev

**Solutions**:

**1. Webhook Testing Tools (Development)**
- **ngrok**: Expose localhost to internet, Zoom sends webhooks to ngrok URL → tunnels to localhost
  - Free tier: Limited, URL changes each restart
  - Paid tier: Fixed domain, stable URL
  - Workflow: Start ngrok → Configure Zoom webhook URL → Test locally
- **webhook.site**: Temporary webhook endpoint to inspect payloads
  - Copy webhook.site URL → Configure in Zoom → View all webhook payloads
  - Useful for understanding payload structure
  - Not for production, testing only
- **LocalTunnel / Cloudflared**: Similar to ngrok, alternatives for tunneling

**2. Mock Webhook Server (Testing)**
- **Custom mock API**: Build internal endpoint that simulates Zoom webhooks
- **Stored payloads**: Save real Zoom webhook payloads, replay them in tests
- **Webhook payload library**: Create test fixtures with sample payloads
- **Integration tests**: Use stored payloads to test webhook handlers

**3. Polling as Fallback (Production + Testing)**
- **Zoom Reports API**: Poll `/meetings/{meetingId}/participants` after meeting ends
  - Schedule background job to run 5-10 minutes after session end time
  - Fetch participant list → Create attendance records
  - Less real-time, but reliable backup
- **Zoom Operation Logs**: Poll `/operationlogs` for meeting events
  - Admin permission required
  - Can track meeting start/end events
- **Trade-off**: Lag (5-15 minutes), but works when webhooks fail

**4. Zoom SDK Events (In-Session)**
- **Meeting SDK**: If you embed Zoom in your app, use SDK events
  - Real-time participant join/leave events during meeting
  - Can track attendance in real-time (no webhook needed)
  - Requires client-side integration (Zoom SDK)
- **Use case**: For future enhancement, not for current architecture

**5. Hybrid Approach (Recommended)**
- **Development**:
  - Use ngrok for local testing
  - Store webhook payloads for test fixtures
  - Create mock webhook endpoint for unit tests
- **Staging**:
  - Use ngrok paid tier or staging server with public URL
  - Or use webhook.site temporarily to capture payloads
- **Production**:
  - Use Zoom webhooks (primary)
  - Poll Zoom Reports API as backup (fallback job runs after meeting ends)
  - Manual attendance entry as last resort

**6. Webhook Payload Storage & Replay**
- **Store all webhooks**: Save all webhook payloads to database/file (even in dev)
- **Replay mechanism**: Admin tool to replay webhook payloads for testing
- **Test fixtures**: Use stored payloads in integration tests
- **Forensics**: Keep payloads for debugging production issues

**7. Manual Simulation (Testing)**
- **Admin tool**: Create UI to manually trigger "fake" webhooks
  - Input: Meeting ID, event type, participant data
  - Output: Calls same webhook handler (without Zoom)
- **Use case**: Test webhook handlers without needing Zoom at all
- **Benefit**: Fast, repeatable testing

**Architecture Pattern**:
```
┌─────────────┐
│   Zoom      │ (Production webhooks)
└──────┬──────┘
       │
       ├──→ Webhook Handler (Production)
       │
┌──────┴──────┐
│  ngrok      │ (Dev: Tunnels to localhost)
└──────┬──────┘
       │
       └──→ Local Webhook Handler (Development)
       
┌─────────────┐
│ Background  │
│ Job (Poll)  │ (Fallback: Poll Reports API)
└──────┬──────┘
       │
       └──→ Attendance Sync Service
```

**Development Workflow**:
1. **Local Development**:
   - Start ngrok → Get public URL
   - Configure Zoom webhook URL (point to ngrok URL)
   - Test webhook handlers locally
   - Save webhook payloads for test fixtures
2. **Integration Testing**:
   - Use stored webhook payloads
   - Mock webhook endpoint for tests
   - Test webhook handlers with known payloads
3. **Staging**:
   - Use staging server with public URL
   - Or use ngrok paid tier (fixed domain)
   - Test full flow end-to-end
4. **Production**:
   - Configure Zoom webhook URL (production URL)
   - Enable polling fallback job
   - Monitor webhook delivery in logs

**Testing Strategy**:
- **Unit Tests**: Mock Zoom service, test webhook handler logic
- **Integration Tests**: Replay stored webhook payloads, verify attendance creation
- **E2E Tests**: Use ngrok in CI/CD, or skip webhook tests (test polling instead)

**Trade-offs**:
- **ngrok**: Free but URL changes, paid is stable
- **Polling**: Reliable but delayed (5-15 min lag)
- **Webhook.site**: Great for understanding payloads, not for automation
- **Manual simulation**: Fast testing, but doesn't test actual Zoom integration

---

### Challenge 4.2: Payment Gateway Failures
**Scenario**: Payment gateway returns "timeout" - did payment succeed or fail?

**Problems**:
- Unknown state: Can't confirm if payment processed
- Double charge: Retry payment, user charged twice
- Lost revenue: Assume failed, user actually paid

**Solutions**:
- **Idempotency keys**: Send unique key with payment request, gateway dedupes
- **Webhook reconciliation**: Wait for payment gateway webhook to confirm
- **Manual verification**: Admin checks payment gateway dashboard
- **Status polling**: Periodically poll payment gateway for transaction status
- **Timeout handling**: Define clear timeout policy (wait 5 min, then mark pending)

**Trade-offs**:
- Idempotency: Requires gateway support
- Webhooks: Eventual consistency (delay)
- Manual: Operational overhead

---

### Challenge 4.3: Email Delivery
**Scenario**: User registered but didn't receive confirmation email.

**Problems**:
- Spam folder: Email marked as spam
- Email service down: SendGrid/AWS SES outage
- Wrong email: User typo in email address
- Rate limits: Too many emails sent, provider throttles

**Solutions**:
- **Queue with retry**: Queue emails, retry on failure
- **Multiple providers**: Fallback to secondary email provider
- **Email verification**: Send verification code, user must confirm
- **In-app notification**: Show important notifications in-app too (not just email)
- **Status tracking**: Track email delivery status (sent, delivered, opened, bounced)

---

## 5. Scalability Concerns

### Challenge 5.1: Database Query Performance
**Scenario**: Dashboard query takes 30 seconds to load enrollments for large organization.

**Problems**:
- N+1 queries: Loading 1000 enrollments triggers 1000 additional queries for user details
- Missing indexes: Queries scan entire table
- Complex joins: Joining 10+ tables for dashboard
- Large datasets: Organization has 50,000+ members

**Solutions**:
- **Eager loading**: Use EF Core `.Include()` to load related data
- **Projection**: Use AutoMapper `ProjectTo` to query only needed fields
- **Database indexes**: Index foreign keys, frequently filtered columns
- **Pagination**: Load 20-50 records at a time
- **Caching**: Cache dashboard data for 5-15 minutes
- **Read replicas**: Route read queries to replica database
- **Materialized views**: Pre-calculate complex aggregations
- **CQRS**: Separate read models optimized for queries

---

### Challenge 5.2: File Upload & Storage
**Scenario**: Instructor uploads 2GB video, server runs out of memory.

**Problems**:
- Memory exhaustion: Entire file loaded into server memory
- Slow uploads: Large files take 30+ minutes
- Bandwidth costs: Serving video from web server is expensive
- Concurrent uploads: 100 users uploading simultaneously

**Solutions**:
- **Streaming uploads**: Process file in chunks, don't load entire file
- **Direct upload to storage**: Generate pre-signed URL, user uploads directly to S3/Azure
- **CDN delivery**: Serve videos via CDN (CloudFront, Azure CDN)
- **Transcoding**: Use service (AWS MediaConvert, Mux) to transcode videos
- **Upload limits**: Restrict file size (e.g., 500MB max per file)
- **Background processing**: Queue video processing jobs

---

### Challenge 5.3: Real-time Notifications
**Scenario**: 10,000 users online, system needs to send real-time notifications.

**Problems**:
- Connection limits: Server can only handle 1000 concurrent WebSocket connections
- Broadcasting: Sending notification to 10,000 users takes too long
- Reconnection storms: If server restarts, all 10,000 reconnect at once

**Solutions**:
- **SignalR with backplane**: Use Redis backplane to scale across multiple servers
- **Pub/sub pattern**: Publish to channel, users subscribe to relevant channels
- **Notification batching**: Group notifications, send in batches
- **Fallback to polling**: If WebSocket fails, poll for notifications every 30s
- **Push notifications**: Use mobile push for critical notifications
- **Don't notify everything**: Only notify critical events (session starting, payment success)

---

## 6. Security Challenges

### Challenge 6.1: Authorization (Row-Level Security)
**Scenario**: Student A tries to access Student B's progress data.

**Problems**:
- Missing checks: API endpoint doesn't verify ownership
- Leaked IDs: Sequential IDs make it easy to guess other users' data
- Broken access control: OWASP Top 10 vulnerability

**Solutions**:
- **Authorization policies**: Check user owns resource before returning data
- **Query filters**: EF Core global query filters for multi-tenancy
- **GUIDs instead of IDs**: Use GUIDs for public-facing IDs
- **Resource-based authorization**: ASP.NET Core IAuthorizationHandler
- **API key for webhooks**: Verify webhook requests are from Zoom/payment provider

---

### Challenge 6.2: Payment Fraud
**Scenario**: User uses stolen credit card to purchase courses.

**Problems**:
- Chargebacks: User disputes charge, platform loses money + chargeback fee
- Stolen accounts: Attacker uses stolen credentials to purchase courses
- Bulk purchases: Attacker buys 100 licenses with stolen cards

**Solutions**:
- **Payment gateway fraud detection**: Stripe Radar, risk scores
- **Rate limiting**: Limit purchases per user/IP per hour
- **Manual review**: Flag large purchases for manual approval
- **Identity verification**: Require phone verification for first purchase
- **Refund policy**: Clear policy to reduce disputes

---

### Challenge 6.3: Multi-Tenant Data Isolation
**Scenario**: Bug causes Organization A to see Organization B's data.

**Problems**:
- Data leak: Massive privacy violation, legal liability
- Query missing filter: Forgot to filter by OrganizationId
- Shared database: All orgs in same DB, easy to leak

**Solutions**:
- **Global query filters**: EF Core automatically adds OrganizationId filter
- **Separate schemas**: Each org gets own schema (PostgreSQL) or database
- **Architecture tests**: Automated tests enforce multi-tenant rules
- **Code review**: Always check filters in queries
- **Audit logging**: Log all data access for forensics

---

## 7. Operational Challenges

### Challenge 7.1: Zero-Downtime Deployments
**Scenario**: Deploy new version without kicking users out mid-session.

**Problems**:
- User watching video: Deployment interrupts video
- User taking quiz: Deployment loses quiz progress
- Active WebSocket connections: Deployment drops connections

**Solutions**:
- **Blue-green deployment**: Deploy to new servers, switch traffic gradually
- **Rolling deployment**: Deploy to servers one-by-one
- **Database migrations**: Use backward-compatible migrations
- **Graceful shutdown**: Wait for active requests to finish before shutdown
- **WebSocket reconnection**: Client automatically reconnects on disconnect

---

### Challenge 7.2: Data Migrations
**Scenario**: Need to change License model schema mid-production.

**Problems**:
- Downtime: Taking system offline for migration
- Data loss: Migration script has bug, data corrupted
- Rollback: Can't easily roll back migration

**Solutions**:
- **Expand-contract pattern**: Add new column, migrate data, remove old column (3 deployments)
- **Feature flags**: Deploy code for both old/new schema, switch via flag
- **Backup first**: Always backup DB before migration
- **Test on staging**: Run migration on staging with production-like data
- **Rollback plan**: Have script to undo migration if needed

---

### Challenge 7.3: Monitoring & Alerting
**Scenario**: Payment API is down for 2 hours before anyone notices.

**Problems**:
- No visibility: Don't know system is down
- User complaints: Users report issues before monitoring detects
- Lost revenue: Can't process payments during downtime

**Solutions**:
- **Health checks**: /health endpoint, check DB, Redis, external services
- **Uptime monitoring**: Pingdom, UptimeRobot, synthetic monitoring
- **Error tracking**: Sentry, Application Insights for exceptions
- **APM**: Application Performance Monitoring (New Relic, Datadog)
- **Business metrics**: Track enrollments, payments, logins per minute
- **Alerting**: PagerDuty, Slack alerts for critical issues

---

## 8. User Experience Challenges

### Challenge 8.1: Long-Running Operations
**Scenario**: User uploads 500MB video, sees loading spinner for 10 minutes.

**Problems**:
- User closes browser: Upload lost, must restart
- No progress feedback: User doesn't know if it's working
- Timeout: Browser or server times out

**Solutions**:
- **Background jobs**: Upload file, return immediately, process in background
- **Progress tracking**: Show progress bar (% uploaded, % processed)
- **Job status polling**: Frontend polls /jobs/{id} for status
- **Notifications**: Notify user when processing complete (email, in-app)
- **Resumable uploads**: If connection lost, resume from last chunk

---

### Challenge 8.2: Conflicting Updates
**Scenario**: Instructor and assistant both edit course at same time.

**Problems**:
- Last write wins: Assistant's changes overwrite instructor's changes
- Lost work: One person's edits are lost
- No warning: Users not aware of conflict

**Solutions**:
- **Optimistic concurrency**: EF Core concurrency token, reject second save with warning
- **Pessimistic locking**: Lock record when editing (discouraged for long edits)
- **Auto-save draft**: Periodically save draft, merge on final save
- **Conflict resolution UI**: Show diff, let user choose which changes to keep
- **Real-time collaboration**: Use WebSocket to show "X is editing" indicator

---

## 9. Compliance & Legal

### Challenge 9.1: GDPR / Data Privacy
**Scenario**: European user requests data deletion.

**Problems**:
- Data spread across tables: User data in 20+ tables
- Foreign key constraints: Can't delete user without deleting related data
- Audit trail: Need to keep some data for legal reasons
- Backups: Data exists in backups for 90 days

**Solutions**:
- **Soft delete**: Mark user deleted, anonymize PII (name, email)
- **Cascade rules**: Define what to delete vs anonymize
- **Data export**: Provide user data export in machine-readable format
- **Data retention policy**: Define how long to keep data
- **Consent management**: Track user consent for data processing

---

### Challenge 9.2: Accessibility (WCAG)
**Scenario**: Blind user can't navigate course content.

**Problems**:
- Screen reader support: Missing ARIA labels, semantic HTML
- Keyboard navigation: Can't access features without mouse
- Color contrast: Text hard to read for visually impaired
- Video captions: No captions for deaf users

**Solutions**:
- **WCAG 2.1 AA compliance**: Follow accessibility guidelines
- **Automated testing**: axe-core, Lighthouse accessibility audits
- **Manual testing**: Test with screen readers (NVDA, JAWS)
- **Captions**: Require/generate captions for all videos
- **Alt text**: Require alt text for images

---

## 10. Testing Challenges

### Challenge 10.1: Integration Testing
**Scenario**: Need to test Zoom webhook, but Zoom doesn't send webhooks in test environment.

**Problems**:
- External dependencies: Can't control Zoom, payment gateway in tests
- Non-deterministic: Tests fail randomly due to external service issues
- Slow: Integration tests take 10+ minutes

**Solutions**:
- **Test doubles**: Mock external services in unit tests
- **Contract testing**: Use Pact for API contract testing
- **Stubbed responses**: WireMock to stub HTTP responses
- **Test containers**: Use Testcontainers for real DB in tests
- **E2E tests**: Separate E2E tests that call real services (run less frequently)

---

### Challenge 10.2: Test Data Management
**Scenario**: Tests fail because they depend on specific seed data.

**Problems**:
- Shared state: Tests interfere with each other
- Data drift: Production has different data than tests expect
- Cleanup: Tests don't clean up data, pollute DB

**Solutions**:
- **Isolated tests**: Each test creates own data
- **Test fixtures**: Reusable test data builders
- **Database per test**: Use in-memory DB or container per test
- **Cleanup**: Always clean up in teardown or use transactions
- **Avoid seed data dependencies**: Don't assume specific IDs exist

---

## Summary of Top 10 Critical Challenges

| Challenge | Impact | Mitigation Priority |
|-----------|--------|---------------------|
| Seat reservation concurrency | High | Critical - implement reservation pattern |
| Pricing calculation complexity | High | Critical - rule engine + extensive tests |
| Payment transaction consistency | High | Critical - outbox pattern + idempotency |
| Zoom API reliability | Medium | High - circuit breaker + fallback |
| Database query performance | Medium | High - indexes + caching + pagination |
| Multi-tenant data isolation | High | Critical - global filters + tests |
| Authorization/access control | High | Critical - resource-based auth |
| Zero-downtime deployments | Medium | High - blue-green + migrations |
| Real-time notifications scale | Low | Medium - SignalR backplane |
| GDPR compliance | High | High - soft delete + data export |

---

## Recommended Architecture Patterns

1. **Reservation Pattern**: For limited resources (seats, campaign usage)
2. **Saga Pattern**: For distributed transactions (payment + enrollment)
3. **Outbox Pattern**: For reliable event publishing
4. **CQRS**: For read-heavy operations (dashboards, reports)
5. **Circuit Breaker**: For external service calls (Zoom, payment)
6. **Event Sourcing**: For audit trail and compliance (optional)
7. **Cache-Aside**: For frequently accessed data (campaigns, courses)
8. **Strangler Fig**: For migrating from monolith to microservices (future)

---

## Monitoring & Observability Needs

- **Metrics**: Enrollments/min, payments/min, API latency, error rate
- **Logging**: Structured logs with correlation ID
- **Tracing**: Distributed tracing (OpenTelemetry) for request flow
- **Alerting**: PagerDuty/Slack for critical issues
- **Dashboards**: Grafana for real-time system health

---

**Conclusion**: This is a complex system with many edge cases. Success requires:
- Clear business rules and documentation
- Comprehensive testing (unit, integration, E2E)
- Robust error handling and monitoring
- Iterative approach (start simple, add complexity as needed)
