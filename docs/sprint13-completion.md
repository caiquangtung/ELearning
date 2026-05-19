---
title: Sprint 13 completion - Search & Filtering
status: MVP delivered
date: 2026-05-19
scope: Backend + Frontend MVP
---

# Sprint 13 Completion - Search & Filtering

## Delivered

- Backend:
  - Extended `ListCoursesQuery` with price range filters and sort option.
  - Added `CourseSortOption`: newest, oldest, title A-Z/Z-A, price low-high/high-low.
  - Course catalog search now checks course title, course description, lesson title, and lesson content.
  - Course list results now include price and currency.
  - Existing `/api/v1/courses` endpoint accepts:
    - `search`
    - `status`
    - `minPriceCents`
    - `maxPriceCents`
    - `sort`
- Frontend:
  - Added global course search in the main layout.
  - Courses page reads search/filter/sort from query params.
  - Courses page includes status, min price, max price, and sort controls.
  - Course list displays price.
  - `/courses?search=...` acts as the MVP search results page.
- Tests:
  - Added validator coverage for invalid course price range.

## Deferred

- Course category, level, instructor metadata and matching filters.
- Popularity/rating sort, pending review/rating domain.
- Redis-backed catalog/detail/search-result cache.
- Cache invalidation hooks for catalog updates.
- Full-text indexes or Elasticsearch.
- Search suggestions/autocomplete.
- API integration tests for query combinations.

## Verification

- `dotnet test src/ELearning.sln` passes:
  - Domain unit tests: 38 passed.
  - Application unit tests: 19 passed.
  - Architecture tests: 1 passed.
- `npm run build` passes for `frontend/web`.

## Notes

- This MVP intentionally uses database-side filtering and sorting because category/level/instructor metadata and Redis cache abstractions are not in place yet.
- Angular build currently passes with a budget warning: initial bundle is about 18 KB over the 700 KB warning budget.
