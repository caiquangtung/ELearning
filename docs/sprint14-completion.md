# Sprint 14 Completion: Review & Rating

Completed on 2026-05-21.

## Scope

- Added `Review` aggregate with rating, comment, published/rejected moderation state, moderation audit fields, and domain validation.
- Added review repository, EF configuration, and `Sprint14_Reviews` migration.
- Added APIs for:
  - submitting/updating a course review after certificate completion
  - paginated course reviews
  - course rating summary
  - admin review moderation
- Added Angular course detail review experience:
  - average rating stars and review count
  - review submission form
  - review list
  - admin publish/reject controls

## Verification

- `dotnet test src/ELearning.sln --no-restore -m:1 /nr:false` passed.
- `npm run build` passed.

Angular build still reports the existing initial bundle budget warning: 724.24 kB vs 700.00 kB.
