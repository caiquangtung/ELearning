---
title: Project Structure
scope: Backend (.NET Clean Architecture) + Frontend (Angular)
status: draft
---

# Project Structure

This document describes the recommended folder structure for the ELearning system.

## Root

```
.
├── backend/
├── frontend/
├── docs/
├── scripts/
└── README.md
```

## Backend (Vertical Slice Architecture + Clean Architecture)

This structure uses **Feature-based organization** (Vertical Slices) for the Application layer, with clear bounded contexts and separation of concerns.

```
backend/
├── src/
│   ├── Domain/                          # Pure domain (no dependencies)
│   │   ├── Aggregates/
│   │   │   ├── CourseAggregate/
│   │   │   │   ├── Course.cs
│   │   │   │   ├── Section.cs
│   │   │   │   └── Lesson.cs
│   │   │   ├── ClassAggregate/
│   │   │   │   ├── Class.cs
│   │   │   │   ├── Session.cs
│   │   │   │   └── Attendance.cs
│   │   │   ├── OrganizationAggregate/
│   │   │   │   ├── Organization.cs
│   │   │   │   ├── LicensePool.cs
│   │   │   │   └── Member.cs
│   │   │   ├── CommerceAggregate/
│   │   │   │   ├── Order.cs
│   │   │   │   ├── Payment.cs
│   │   │   │   └── Invoice.cs
│   │   │   └── CampaignAggregate/
│   │   │       ├── Campaign.cs
│   │   │       └── PromotionRule.cs
│   │   ├── Shared/                      # Shared domain primitives
│   │   │   ├── Entity.cs
│   │   │   ├── AggregateRoot.cs
│   │   │   ├── ValueObject.cs
│   │   │   ├── DomainEvent.cs
│   │   │   └── Result.cs
│   │   ├── ValueObjects/
│   │   │   ├── Money.cs
│   │   │   ├── Email.cs
│   │   │   ├── Duration.cs
│   │   │   └── QuizScore.cs
│   │   ├── Enums/
│   │   │   ├── CourseStatus.cs
│   │   │   ├── EnrollmentStatus.cs
│   │   │   └── PaymentStatus.cs
│   │   ├── Events/
│   │   │   ├── CoursePublished.cs
│   │   │   ├── SessionCompleted.cs
│   │   │   └── PaymentProcessed.cs
│   │   └── Exceptions/
│   │       ├── DomainException.cs
│   │       └── BusinessRuleException.cs
│   │
│   ├── Core/                            # Shared Kernel
│   │   ├── Abstractions/
│   │   │   ├── IRepository.cs
│   │   │   ├── IUnitOfWork.cs
│   │   │   ├── IDomainEventHandler.cs
│   │   │   ├── ICurrentUserService.cs
│   │   │   └── IAuditableEntity.cs
│   │   ├── Common/
│   │   │   ├── Result.cs
│   │   │   ├── PagedList.cs
│   │   │   ├── Error.cs
│   │   │   └── Constants.cs
│   │   └── Exceptions/
│   │       ├── NotFoundException.cs
│   │       ├── ValidationException.cs
│   │       ├── UnauthorizedException.cs
│   │       └── ConflictException.cs
│   │
│   ├── Application/                     # Use Cases (Vertical Slices)
│   │   ├── Features/
│   │   │   ├── Courses/
│   │   │   │   ├── CreateCourse/
│   │   │   │   │   ├── CreateCourseCommand.cs
│   │   │   │   │   ├── CreateCourseHandler.cs
│   │   │   │   │   ├── CreateCourseValidator.cs
│   │   │   │   │   └── CreateCourseDto.cs
│   │   │   │   ├── GetCourseDetails/
│   │   │   │   │   ├── GetCourseDetailsQuery.cs
│   │   │   │   │   ├── GetCourseDetailsHandler.cs
│   │   │   │   │   └── CourseDetailsDto.cs
│   │   │   │   ├── UpdateCourse/
│   │   │   │   ├── DeleteCourse/
│   │   │   │   └── PublishCourse/
│   │   │   ├── Classes/
│   │   │   │   ├── CreateClass/
│   │   │   │   ├── ScheduleSession/
│   │   │   │   ├── RecordAttendance/
│   │   │   │   └── GetClassSchedule/
│   │   │   ├── Commerce/
│   │   │   │   ├── CreateOrder/
│   │   │   │   ├── ProcessPayment/
│   │   │   │   ├── ApplyCampaign/
│   │   │   │   └── CalculatePrice/
│   │   │   ├── Organizations/
│   │   │   │   ├── CreateOrganization/
│   │   │   │   ├── AssignLicense/
│   │   │   │   ├── ManageMembers/
│   │   │   │   └── GetOrganizationDashboard/
│   │   │   ├── Identity/
│   │   │   │   ├── Login/
│   │   │   │   ├── Register/
│   │   │   │   └── RefreshToken/
│   │   │   └── Enrollments/
│   │   │       ├── EnrollStudent/
│   │   │       ├── TrackProgress/
│   │   │       └── IssueCertificate/
│   │   ├── Common/                      # Shared application logic
│   │   │   ├── Behaviors/
│   │   │   │   ├── ValidationBehavior.cs
│   │   │   │   ├── LoggingBehavior.cs
│   │   │   │   ├── TransactionBehavior.cs
│   │   │   │   ├── PerformanceBehavior.cs
│   │   │   │   └── CachingBehavior.cs
│   │   │   ├── Mappings/
│   │   │   │   └── MappingProfile.cs
│   │   │   ├── Interfaces/
│   │   │   │   ├── IEmailService.cs
│   │   │   │   ├── IZoomService.cs
│   │   │   │   ├── IPaymentService.cs
│   │   │   │   ├── IVideoService.cs
│   │   │   │   └── IPricingEngine.cs
│   │   │   └── Models/
│   │   │       ├── PaginationRequest.cs
│   │   │       └── SortRequest.cs
│   │   └── DependencyInjection.cs
│   │
│   ├── Infrastructure/
│   │   ├── Persistence/
│   │   │   ├── Configurations/          # EF entity configurations
│   │   │   │   ├── CourseConfiguration.cs
│   │   │   │   ├── ClassConfiguration.cs
│   │   │   │   └── OrderConfiguration.cs
│   │   │   ├── Migrations/
│   │   │   ├── Repositories/
│   │   │   │   ├── GenericRepository.cs
│   │   │   │   └── CourseRepository.cs
│   │   │   ├── Interceptors/
│   │   │   │   ├── AuditInterceptor.cs
│   │   │   │   └── SoftDeleteInterceptor.cs
│   │   │   ├── ApplicationDbContext.cs
│   │   │   └── UnitOfWork.cs
│   │   ├── ExternalServices/
│   │   │   ├── Zoom/
│   │   │   │   ├── ZoomService.cs
│   │   │   │   ├── ZoomConfiguration.cs
│   │   │   │   └── Models/
│   │   │   ├── Payment/
│   │   │   │   ├── StripeService.cs
│   │   │   │   ├── VNPayService.cs
│   │   │   │   └── PaymentFactory.cs
│   │   │   ├── Email/
│   │   │   │   ├── EmailService.cs
│   │   │   │   └── Templates/
│   │   │   └── Video/
│   │   │       ├── VideoService.cs
│   │   │       └── VideoConfiguration.cs
│   │   ├── Identity/
│   │   │   ├── IdentityService.cs
│   │   │   ├── JwtTokenService.cs
│   │   │   ├── CurrentUserService.cs
│   │   │   └── PermissionService.cs
│   │   ├── Messaging/
│   │   │   ├── EventBus.cs
│   │   │   ├── DomainEventDispatcher.cs
│   │   │   ├── OutboxPattern/
│   │   │   └── BackgroundJobs/
│   │   │       ├── ProcessOutboxJob.cs
│   │   │       └── CleanupExpiredReservationsJob.cs
│   │   ├── Logging/
│   │   │   ├── SerilogConfiguration.cs
│   │   │   └── Enrichers/
│   │   │       ├── UserEnricher.cs
│   │   │       └── CorrelationIdEnricher.cs
│   │   ├── Caching/
│   │   │   ├── RedisCacheService.cs
│   │   │   └── CacheConfiguration.cs
│   │   └── DependencyInjection.cs
│   │
│   └── WebApi/
│       ├── Controllers/
│       │   ├── v1/
│       │   │   ├── CoursesController.cs
│       │   │   ├── ClassesController.cs
│       │   │   ├── CommerceController.cs
│       │   │   ├── OrganizationsController.cs
│       │   │   └── IdentityController.cs
│       │   └── v2/
│       ├── Middlewares/
│       │   ├── ExceptionHandlingMiddleware.cs
│       │   ├── CorrelationIdMiddleware.cs
│       │   ├── AuthenticationMiddleware.cs
│       │   └── RequestLoggingMiddleware.cs
│       ├── Filters/
│       │   ├── ValidateModelStateFilter.cs
│       │   └── ApiKeyAuthorizationFilter.cs
│       ├── Extensions/
│       │   ├── ServiceCollectionExtensions.cs
│       │   └── ApplicationBuilderExtensions.cs
│       ├── Contracts/                   # API DTOs (versioned)
│       │   ├── v1/
│       │   │   ├── Requests/
│       │   │   └── Responses/
│       │   └── v2/
│       ├── Webhooks/
│       │   ├── ZoomWebhookController.cs
│       │   └── PaymentWebhookController.cs
│       ├── Program.cs
│       ├── appsettings.json
│       └── appsettings.Development.json
│
├── tests/
│   ├── Domain.UnitTests/
│   │   ├── Aggregates/
│   │   └── ValueObjects/
│   ├── Application.UnitTests/
│   │   └── Features/
│   │       ├── Courses/
│   │       │   ├── CreateCourseHandlerTests.cs
│   │       │   └── GetCourseDetailsHandlerTests.cs
│   │       └── Commerce/
│   │           └── ProcessPaymentHandlerTests.cs
│   ├── Infrastructure.IntegrationTests/
│   │   ├── Persistence/
│   │   └── ExternalServices/
│   ├── WebApi.IntegrationTests/
│   │   └── Controllers/
│   └── ArchitectureTests/
│       └── DependencyTests.cs           # Enforce layer rules
│
└── ELearning.sln
```

### Architecture Principles

#### 1. Vertical Slice Architecture (VSA)
- Each feature is self-contained with command/query/handler/validator/DTO in one folder
- Reduces cross-layer navigation and coupling
- Enables team ownership per feature

#### 2. Clean Architecture Layers
- **Domain**: Pure business logic, no dependencies
- **Core**: Shared kernel (interfaces, base types, common exceptions)
- **Application**: Use cases orchestration (depends on Domain + Core)
- **Infrastructure**: External concerns (depends on Application, Domain, Core)
- **WebApi**: HTTP endpoints (depends on all layers)

#### 3. Dependency Rules
```
Domain ← Core
  ↑      ↑
  └──────┴─── Application
             ↑
             Infrastructure
             ↑
             WebApi
```

### Folder Responsibilities

#### Domain Layer
- **Aggregates/**: Cluster of entities with aggregate root
  - Each aggregate folder contains related entities
  - Business rules enforced at aggregate boundaries
- **Shared/**: Base domain types (Entity, AggregateRoot, Result)
- **ValueObjects/**: Immutable types representing domain concepts
- **Enums/**: Domain-specific enumerations
- **Events/**: Domain events for cross-aggregate communication
- **Exceptions/**: Domain-specific exceptions

**Key Rules**:
- No framework dependencies (no EF, no ASP.NET)
- Pure C# business logic
- All business rules live here

#### Core Layer (Shared Kernel)
- **Abstractions/**: Common interfaces (IRepository, IUnitOfWork, ICurrentUserService)
- **Common/**: Shared types (Result, PagedList, Error)
- **Exceptions/**: Technical exceptions (NotFoundException, ValidationException)

**Key Rules**:
- Minimal dependencies
- Only interfaces and common types
- Used by all layers

#### Application Layer
- **Features/**: Vertical slices organized by domain area
  - Each feature folder contains:
    - `Command.cs` / `Query.cs`: request object
    - `Handler.cs`: MediatR handler
    - `Validator.cs`: FluentValidation rules
    - `Dto.cs`: response objects
- **Common/**: Cross-cutting application concerns
  - **Behaviors/**: MediatR pipeline behaviors (validation, logging, transactions)
  - **Mappings/**: AutoMapper profiles
  - **Interfaces/**: Application service contracts
- **DependencyInjection.cs**: Register MediatR, AutoMapper, FluentValidation

**Key Rules**:
- No direct DB access (use repositories)
- No HTTP concerns
- Orchestrates domain logic
- Returns DTOs, not entities

#### Infrastructure Layer
- **Persistence/**: EF Core implementation
  - **Configurations/**: Fluent API entity configs
  - **Migrations/**: EF migrations
  - **Repositories/**: Repository implementations
  - **Interceptors/**: Audit, soft delete interceptors
- **ExternalServices/**: Third-party integrations (Zoom, payment, email, video)
- **Identity/**: Authentication/authorization implementation
- **Messaging/**: Event bus, outbox pattern, background jobs
- **Logging/**: Serilog configuration and enrichers
- **Caching/**: Redis or in-memory cache
- **DependencyInjection.cs**: Register all infrastructure services

**Key Rules**:
- Implements interfaces from Application/Core
- Contains all external dependencies
- Handles persistence, external APIs, logging

#### WebApi Layer
- **Controllers/**: REST endpoints (versioned by folder)
- **Middlewares/**: Exception handling, logging, correlation ID
- **Filters/**: Action/result filters
- **Extensions/**: DI and middleware registration helpers
- **Contracts/**: API-specific DTOs (versioned)
- **Webhooks/**: External webhook handlers (Zoom, payment providers)
- **Program.cs**: Application entry point

**Key Rules**:
- Thin controllers (delegate to MediatR)
- Version APIs (v1/, v2/)
- Handle HTTP concerns only
- Map API contracts to commands/queries

### NuGet Packages

**Core packages**:
- MediatR (CQRS, vertical slices)
- FluentValidation.AspNetCore
- AutoMapper.Extensions.Microsoft.DependencyInjection

**Infrastructure packages**:
- Microsoft.EntityFrameworkCore.SqlServer / Npgsql.EntityFrameworkCore.PostgreSQL
- Serilog.AspNetCore
- StackExchange.Redis
- MassTransit (optional, for messaging)

**WebApi packages**:
- Swashbuckle.AspNetCore (Swagger)
- Microsoft.AspNetCore.Authentication.JwtBearer
- Microsoft.AspNetCore.Mvc.Versioning

### Testing Strategy

- **Domain.UnitTests**: Test aggregates, value objects, business rules (no mocks)
- **Application.UnitTests**: Test handlers with mocked repositories
- **Infrastructure.IntegrationTests**: Test DB, external services (real or test containers)
- **WebApi.IntegrationTests**: Test full HTTP request flow
- **ArchitectureTests**: NetArchTest to enforce dependency rules

## Frontend (Angular)

```
frontend/
├── src/
│   ├── app/
│   │   ├── core/
│   │   ├── shared/
│   │   └── features/
│   ├── assets/
│   └── environments/
├── e2e/
└── README.md
```

## Docs

```
docs/
├── project-management-plan.md
├── advanced-architecture-notes.md
├── erd.md
└── project-structure.md
```

## Notes

- Keep domain logic in `Domain` only.
- Use `Application` for use cases, orchestration, and validation.
- Keep infrastructure concerns (DB, integrations, logging) in `Infrastructure`.
- Expose HTTP endpoints only in `WebApi`.
