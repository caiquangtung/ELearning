# Sprint 1 completion notes — Identity & Organization Management

**Status:** Backend and data model for Sprint 1 are implemented; **Angular UI** is deferred until Node/CLI versions match project requirements (see `frontend/README.md`).

## Delivered (backend)

### Domain

- **`User`**: profile fields (`FirstName`, `LastName`, `AvatarUrl`), `PlatformRoles`, `UpdateProfile`, `SetPlatformRoles`.
- **`Organization`**: name, slug, status; **`Department`** (name, code); **`OrganizationMember`** (user, optional department, `OrgRole`, `JoinedAt`).
- **Org roles**: `OrgAdmin`, `Member`, `Instructor` (`OrganizationRoles`).

### Application (MediatR)

| Feature | Handler |
|--------|---------|
| Create organization | `CreateOrganizationCommandHandler` |
| Add member | `AddMemberToOrganizationCommandHandler` |
| List orgs (for current user) | `ListOrganizationsQueryHandler` |
| Org detail + members | `GetOrganizationQueryHandler` |
| Update profile | `UpdateProfileCommandHandler` |
| Assign platform roles (admin) | `AssignRolesToUserCommandHandler` |

- **`SlugHelper`**: ASCII slug from name; collision suffix `-2`, `-3`, …

### Infrastructure

- **EF Core**: `Organization`, `Department`, `OrganizationMember` configurations; indexes (unique slug, unique `(organization_id, user_id)` for members).
- **`IOrganizationRepository`** / **`OrganizationRepository`**: CRUD, slug lookup, membership lookup, list orgs for user.
- **Migration**: `Sprint1_IdentityAndOrganizations`.
- **Seeding**: `DatabaseSeeder` runs migrations on startup; in **Development**, seeds a default **admin** user if the database has no users (`Seed:AdminEmail`, `Seed:AdminPassword` in `appsettings.Development.json`).
- **`NullCurrentUserService`** + **`DesignTimeDbContextFactory`** for design-time migrations without HTTP context.

### Web API

| Method | Route | Permission |
|--------|-------|------------|
| `GET` | `/api/organizations` | Authenticated |
| `GET` | `/api/organizations/{id}` | Member of org or platform admin |
| `POST` | `/api/organizations` | `Admin.Access` |
| `POST` | `/api/organizations/{id}/members` | `Admin.Access` |
| `PUT` | `/api/identity/me` | Authenticated (own profile) |
| `PUT` | `/api/users/{userId}/roles` | `Admin.Access` |

**DTO example — add member**

```json
POST /api/organizations/{id}/members
{
  "userId": "guid",
  "departmentId": null,
  "orgRole": "Member"
}
```

### Tests

- **Domain**: `OrganizationAggregateTests` (create org, add member, duplicate user).
- **Application**: `SlugHelperTests`.

## Deferred

- **Angular 21** screens: login/register (if exposed), profile edit, org list/detail, add member modal; HTTP client + interceptors + route guards.
- **Optional hardening**: FK from `organization_members.user_id` to `users.id` in a follow-up migration; align all EF Core package versions to the same patch (e.g. 10.0.5) to silence binding redirects.

## Configuration snippet (Development seed)

```json
"Seed": {
  "AdminEmail": "admin@local.test",
  "AdminPassword": "ChangeMe123!"
}
```
