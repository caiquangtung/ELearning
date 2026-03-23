---
title: Identity & Security Design
scope: Authentication · Role-Based Access Control · Permission-Based Authorization
status: implemented
created: 2026-03-22
---

# Identity & Security Design

## 1. Overview

The platform uses **custom JWT authentication** (no ASP.NET Core Identity) combined with a **permission-based authorization** system that derives fine-grained permissions from roles at request time.

```
Request → JWT Middleware (authenticate) → PermissionHandler (authorize) → Controller
```

---

## 2. User Aggregate

**File**: `ELearning.Domain/Aggregates/UserAggregate/User.cs`

```csharp
public sealed class User : AggregateRoot
{
    public string Email { get; private set; }
    public string PasswordHash { get; private set; }
    public string FirstName / LastName { get; private set; }
    public UserStatus Status { get; private set; }      // Pending | Active | Suspended | Deleted
    public IReadOnlyList<string> Roles { get; private set; }
    public string? RefreshTokenHash { get; private set; }  // SHA-256 of raw refresh token
    public DateTime? RefreshTokenExpiresAt { get; private set; }
}
```

**Key behaviours:**
- `User.Create(...)` — factory method; emits `UserRegistered` domain event; default role = `Learner`
- `ChangePassword(hash)` — changes hash, revokes refresh token, emits `PasswordChanged`
- `SetRefreshToken(hash, expiresAt)` — stores hashed refresh token (never stored raw)
- `IsRefreshTokenValid(hash)` — compares hash + expiry check
- `AssignRole / RemoveRole` — enforces minimum 1 role invariant

---

## 3. Roles

**File**: `ELearning.Core/Constants/Roles.cs`

| Role | Description |
|---|---|
| `Admin` | Platform administrator — full access |
| `OrgAdmin` | Organization administrator — manages own org's users, enrollments, reports |
| `Instructor` | Creates and manages courses, views enrollments |
| `Learner` | Browses and enrolls in courses (default for new registrations) |

---

## 4. Permissions

**File**: `ELearning.Core/Constants/Permissions.cs`

| Namespace | Permissions |
|---|---|
| `Users` | `Read`, `Create`, `Update`, `Delete` |
| `Courses` | `Read`, `Create`, `Update`, `Delete`, `Publish` |
| `Organizations` | `Read`, `Manage` |
| `Enrollments` | `Read`, `Create`, `Manage` |
| `Licenses` | `Read`, `Assign` |
| `Reports` | `Read`, `Export` |
| `Admin` | `Access` |

### Role → Permission Mapping

**File**: `ELearning.Core/Constants/PermissionMap.cs`

| Permission | Admin | OrgAdmin | Instructor | Learner |
|---|:---:|:---:|:---:|:---:|
| Users.Read | ✅ | ✅ | | |
| Users.Create/Update | ✅ | ✅ | | |
| Users.Delete | ✅ | | | |
| Courses.Read | ✅ | ✅ | ✅ | ✅ |
| Courses.Create/Update | ✅ | | ✅ | |
| Courses.Delete | ✅ | | | |
| Courses.Publish | ✅ | | ✅ | |
| Organizations.Manage | ✅ | ✅ | | |
| Enrollments.Read | ✅ | ✅ | ✅ | ✅ |
| Enrollments.Create | ✅ | ✅ | | ✅ |
| Enrollments.Manage | ✅ | ✅ | | |
| Licenses.Read/Assign | ✅ | ✅ | | |
| Reports.Read | ✅ | ✅ | ✅ | |
| Reports.Export | ✅ | ✅ | | |
| Admin.Access | ✅ | | | |

---

## 5. JWT Tokens

**File**: `ELearning.Infrastructure/Identity/JwtTokenService.cs`

### Access Token Claims

```json
{
  "sub":         "user-guid",
  "email":       "user@example.com",
  "given_name":  "John",
  "family_name": "Doe",
  "role":        ["Learner"],
  "jti":         "unique-token-id",
  "exp":         1234567890,
  "iss":         "ELearning.API",
  "aud":         "ELearning.Client"
}
```

> **Permissions are NOT embedded in the JWT** — they are resolved at request time from `PermissionMap` based on roles. This keeps tokens small and allows permission changes without re-issuing tokens.

### Token Lifetimes

| Token | Default Lifetime | Storage |
|---|---|---|
| Access Token | 60 minutes (configurable) | Client memory (not localStorage) |
| Refresh Token | 7 days | HttpOnly cookie or secure storage |

### Refresh Token Security
- Raw refresh token is never stored — only its **SHA-256 hash** is persisted in `User.RefreshTokenHash`
- On rotation: old hash is replaced with new hash atomically
- On password change: refresh token is revoked immediately

---

## 6. Authorization Flow

```
[HasPermission("Courses.Create")]  ← attribute on controller action
        │
        ▼
PermissionPolicyProvider           ← IAuthorizationPolicyProvider
  creates policy: RequireAuthenticatedUser + PermissionRequirement("Courses.Create")
        │
        ▼
PermissionAuthorizationHandler     ← IAuthorizationHandler
  reads ClaimTypes.Role from JWT
  calls PermissionMap.GetPermissionsForRoles(roles)
  checks if "Courses.Create" is in the set
  → Succeed / Fail
```

### Usage in Controllers

```csharp
// Require a specific permission
[HasPermission(Permissions.Courses.Create)]
public async Task<IActionResult> CreateCourse(...) { }

// Require a role directly
[Authorize(Roles = Roles.Admin)]
public async Task<IActionResult> AdminDashboard(...) { }

// Public endpoint
[AllowAnonymous]
public async Task<IActionResult> GetCatalog(...) { }
```

---

## 7. API Endpoints

**Base**: `POST /api/v1/identity`

| Method | Endpoint | Auth | Description |
|---|---|---|---|
| `POST` | `/register` | Anonymous | Register new learner account |
| `POST` | `/login` | Anonymous | Authenticate, get token pair |
| `POST` | `/refresh-token` | Anonymous | Rotate token pair |
| `GET` | `/me` | Bearer JWT | Get current user profile |

### Register Request / Response

```json
// POST /api/v1/identity/register
{
  "email": "john@example.com",
  "password": "SecurePass1",
  "firstName": "John",
  "lastName": "Doe"
}

// 201 Created
{
  "accessToken": "eyJ...",
  "refreshToken": "rT...",
  "accessTokenExpiresAt": "2026-03-22T12:00:00Z",
  "user": {
    "id": "uuid",
    "email": "john@example.com",
    "firstName": "John",
    "lastName": "Doe",
    "fullName": "John Doe",
    "roles": ["Learner"]
  }
}
```

### Password Validation Rules
- Minimum 8 characters
- At least one uppercase letter
- At least one lowercase letter
- At least one digit

---

## 8. Middleware Pipeline

```
Request
  → CorrelationIdMiddleware      reads/generates X-Correlation-Id header
  → ExceptionHandlingMiddleware  catches all unhandled exceptions → problem+json
  → UseHttpsRedirection
  → UseCors
  → UseAuthentication            validates JWT → populates HttpContext.User
  → UseAuthorization             evaluates [HasPermission] / [Authorize] attributes
  → MapControllers
```

### Problem Response Format (exceptions)

```json
{
  "title": "Validation Failed",
  "status": 422,
  "correlationId": "abc-123",
  "errors": {
    "Email": ["'Email' is not a valid email address."],
    "Password": ["Password must contain at least one digit."]
  }
}
```

---

## 9. Configuration

**File**: `appsettings.Development.json`

```json
{
  "JwtSettings": {
    "Secret": "dev-secret-key-change-in-production-min-32-chars",
    "Issuer": "ELearning.API",
    "Audience": "ELearning.Client",
    "ExpiryMinutes": 60
  }
}
```

> **Production**: `Secret` must be at least 32 characters, stored in environment variable or secrets manager (Azure Key Vault / AWS Secrets Manager). Never commit to source control.

---

## 10. Database Schema

```sql
CREATE TABLE users (
    id                       UUID PRIMARY KEY,
    email                    VARCHAR(256) NOT NULL UNIQUE,
    password_hash            TEXT NOT NULL,
    first_name               VARCHAR(100) NOT NULL,
    last_name                VARCHAR(100) NOT NULL,
    status                   VARCHAR(20) NOT NULL DEFAULT 'Active',
    roles                    JSONB NOT NULL DEFAULT '["Learner"]',
    refresh_token_hash       TEXT,
    refresh_token_expires_at TIMESTAMPTZ,
    created_at               TIMESTAMPTZ NOT NULL DEFAULT now(),
    updated_at               TIMESTAMPTZ
);
```

> Generate migration: `dotnet ef migrations add InitUsers --project ELearning.Infrastructure --startup-project ELearning.WebApi`

---

## 11. Packages Used

| Package | Version | Layer | Purpose |
|---|---|---|---|
| `Microsoft.AspNetCore.Authentication.JwtBearer` | 10.0.5 | Infrastructure | JWT middleware |
| `System.IdentityModel.Tokens.Jwt` | (transitive) | Infrastructure | JWT generation / validation |
| `BCrypt.Net-Next` | 4.1.0 | Infrastructure | Password hashing (BCrypt enhanced, work factor 12) |
| `MediatR.Contracts` | 2.0.1 | Domain | `INotification` for domain events |

---

## 12. Security Checklist

- [x] Passwords hashed with BCrypt (enhanced, work factor 12)
- [x] Refresh tokens stored as SHA-256 hash only
- [x] JWT signed with HMAC-SHA256; `ClockSkew = 30s`
- [x] Access token short-lived (60 min default)
- [x] Correlation ID on every request for log tracing
- [x] Global exception handler returns `problem+json` (no stack traces in production)
- [x] Password strength enforced in validation (uppercase, lowercase, digit, min 8 chars)
- [x] Unique index on `users.email`
- [x] Refresh token revoked on password change
- [x] Account suspension revokes refresh token
- [ ] Rate limiting on auth endpoints (TODO: Phase 2)
- [ ] Email verification flow (TODO: Phase 2)
- [ ] MFA / TOTP support (TODO: Phase 3)
- [ ] Audit log for auth events (TODO: Phase 2)
