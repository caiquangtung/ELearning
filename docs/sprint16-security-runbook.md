# Sprint 16 Security Runbook

## Production API Settings

Required environment variables or secret-manager entries:

- `ConnectionStrings__DefaultConnection`
- `JwtSettings__Secret` with at least 32 characters
- `JwtSettings__Issuer`
- `JwtSettings__Audience`
- `Redis__ConnectionString`
- `Cors__AllowedOrigins` as comma-separated explicit `https://` origins
- `Payments__WebhookSecret`
- `Seed__AdminPassword` only for initial provisioning, never as a long-lived default

Do not use wildcard CORS origins. The API rejects wildcard or empty CORS outside Development.

## Browser Security Headers

The API now emits:

- `Content-Security-Policy`
- `Strict-Transport-Security` outside Development
- `X-Content-Type-Options`
- `X-Frame-Options`
- `Referrer-Policy`
- `Permissions-Policy`

For CDN/static hosting, mirror the same headers at the edge. Keep `sw.js` and `index.html` on short TTL/no-cache.

## WAF Baseline

Recommended managed WAF rules:

- OWASP Core Rule Set in detection mode first, then prevention mode after false-positive tuning.
- Block common SQL injection, XSS, path traversal, and malicious file upload signatures.
- Rate-limit `/api/v1/identity/*`, `/api/v1/payments/webhook`, checkout/order creation, and upload endpoints.
- Allow webhook provider IP ranges only when the payment provider publishes stable ranges.

## TLS

- Require TLS 1.2+.
- Redirect HTTP to HTTPS at the edge and keep API `UseHttpsRedirection`.
- Enable HSTS after the production domain and certificate renewal path are verified.
- Use automated certificate renewal and alert on expiry within 30 days.

## Security Scan

Run locally or in CI:

```bash
bash scripts/security-scan.sh
```

The default npm audit checks production dependencies. To include Angular/webpack dev tooling advisories:

```bash
INCLUDE_DEV_AUDIT=1 bash scripts/security-scan.sh
```

Run OWASP ZAP baseline when the API/frontend are reachable:

```bash
ZAP_TARGET=https://staging.example.com bash scripts/security-scan.sh
```

The ZAP step uses Docker and is skipped when `ZAP_TARGET` is not set.

## Current Dependency Audit Notes

- NuGet vulnerability scan returns no vulnerable packages with the current advisory source.
- Frontend production dependency audit returns no vulnerabilities.
- Full npm dev-tooling audit still reports Angular CLI/build-chain advisories in transitive packages that npm marks as requiring a breaking Angular CLI 21 upgrade or having no non-breaking fix. These packages are development tooling and are not shipped in the production browser bundle.
