---
title: ELearning ERD
scope: Full system (B2B + B2C + Hybrid Learning)
status: draft
---

# ELearning ERD

```mermaid
erDiagram
  USER ||--|| LEARNER_PROFILE : has
  USER ||--|| INSTRUCTOR_PROFILE : has
  USER ||--o{ MEMBERSHIP : belongs
  ORGANIZATION ||--o{ MEMBERSHIP : has
  ORGANIZATION ||--|| ORG_SETTINGS : config
  ORGANIZATION ||--o{ DEPARTMENT : has

  ORGANIZATION ||--o{ LICENSE_POOL : owns
  COURSE ||--o{ LICENSE_POOL : for
  LICENSE_POOL ||--o{ LICENSE_ASSIGNMENT : assigns
  USER ||--o{ LICENSE_ASSIGNMENT : receives

  COURSE ||--o{ COURSE_MODULE : has
  COURSE_MODULE ||--o{ LESSON : has
  LESSON ||--o{ CONTENT_ASSET : contains

  COURSE ||--o{ CLASS : delivers
  ORGANIZATION ||--o{ CLASS : hosts
  CLASS ||--o{ SESSION : schedules
  CLASS ||--o{ INSTRUCTOR_ASSIGNMENT : assigns
  INSTRUCTOR_PROFILE ||--o{ INSTRUCTOR_ASSIGNMENT : teaches

  SESSION ||--o{ ATTENDANCE_SHEET : tracks
  ATTENDANCE_SHEET ||--o{ ATTENDANCE_RECORD : records
  USER ||--o{ ATTENDANCE_RECORD : attends

  USER ||--o{ ENROLLMENT : enrolls
  CLASS ||--o{ ENROLLMENT : has

  LEARNING_PATH ||--o{ PATH_ITEM : contains
  LEARNING_PATH ||--o{ PATH_PROGRESS : tracks
  USER ||--o{ PATH_PROGRESS : progresses

  LESSON ||--o{ QUIZ : assesses
  QUIZ ||--o{ QUESTION : includes
  QUIZ ||--o{ ATTEMPT : has
  USER ||--o{ ATTEMPT : submits
  ATTEMPT ||--|| SCORE : results

  ENROLLMENT ||--o{ CERTIFICATE : issues

  PRODUCT ||--o{ PRICE_PLAN : prices
  USER ||--o{ ORDER : places
  ORGANIZATION ||--o{ ORDER : sponsors
  ORDER ||--o{ ORDER_ITEM : contains
  ORDER ||--o{ PAYMENT : paid_by
  ORDER ||--o{ REFUND : refunded_by

  CAMPAIGN ||--o{ PROMOTION_RULE : defines
  CAMPAIGN ||--o{ COUPON : issues
  CAMPAIGN ||--o{ CAMPAIGN_USAGE : tracks
  ORDER ||--o{ CAMPAIGN_USAGE : applied_to

  USER ||--o{ NOTIFICATION : receives
  USER ||--o{ AUDIT_LOG : audits

  USER {
    string Id
    string Email
    string PasswordHash
    string Status
    datetime CreatedAt
  }

  ORGANIZATION {
    string Id
    string Name
    string Status
    datetime CreatedAt
  }

  COURSE {
    string Id
    string Title
    string Status
    datetime PublishedAt
  }

  CLASS {
    string Id
    string CourseId
    string OrganizationId
    datetime StartDate
    datetime EndDate
    int TotalCapacity
    int PublicAvailableSeats
    int B2bReservedSeats
  }

  SESSION {
    string Id
    string ClassId
    string Type
    datetime StartTime
    datetime EndTime
    string ZoomMeetingId
  }

  ENROLLMENT {
    string Id
    string UserId
    string ClassId
    string SourceType
    string ReferenceId
    datetime EnrolledAt
  }

  ORDER {
    string Id
    string UserId
    string OrganizationId
    string Status
    decimal TotalAmount
    datetime CreatedAt
  }

  CAMPAIGN {
    string Id
    string Name
    string Type
    datetime StartDate
    datetime EndDate
    string Status
  }
```
```
