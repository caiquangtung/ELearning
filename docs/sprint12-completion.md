---
title: Sprint 12 completion - Video On Demand & Progress Tracking
status: MVP delivered
date: 2026-05-19
scope: Backend + Frontend MVP
---

# Sprint 12 Completion - Video On Demand & Progress Tracking

## Delivered

- Domain:
  - Added `VideoAsset` aggregate for lesson-level uploaded videos.
  - Added `WatchEvent` aggregate for per-user playback progress and lesson completion.
  - Completion threshold is 80% watched.
- Application:
  - Upload lesson video.
  - Get lesson video.
  - Get video playback URL.
  - Track watch progress heartbeat.
  - Mark lesson complete.
- Web API:
  - `POST /api/v1/videos/courses/{courseId}/sections/{sectionId}/lessons/{lessonId}`
  - `GET /api/v1/videos/lessons/{lessonId}`
  - `GET /api/v1/videos/{id}/playback`
  - `POST /api/v1/videos/{id}/progress`
  - `POST /api/v1/videos/{id}/complete`
- Infrastructure:
  - EF configurations and repositories for `video_assets` and `watch_events`.
  - EF migration `Sprint12_VideoProgress`.
  - Uses existing local `IFileStorage` and `/api/v1/assets/{storageKey}` range-enabled streaming for MVP.
- Frontend:
  - Course detail page can upload a video per lesson.
  - Course detail page renders a native HTML5 video player when a lesson has a video.
  - Playback sends progress heartbeats every 30 seconds and at the 80% completion threshold.
  - Displays watched percentage returned by the API.
- Tests:
  - Domain tests for video content-type validation, progress completion threshold, and idempotent completion.
  - Application validator smoke tests for video upload and progress tracking.

## Deferred

- S3/Azure Blob/Mux storage provider.
- CDN delivery and signed URLs.
- Video transcoding and adaptive streaming.
- Dedicated player library such as Video.js or Plyr.
- Richer learner progress UI across course pages.
- Integration of video completion into certificate completion rules.
- API integration tests for upload/playback/progress endpoints.

## Verification

- `dotnet test src/ELearning.sln` passes:
  - Domain unit tests: 38 passed.
  - Application unit tests: 18 passed.
  - Architecture tests: 1 passed.
- `npm run build` passes for `frontend/web`.

## Notes

- This is a VOD MVP: uploaded videos are stored locally and streamed through the existing asset endpoint with range processing.
- Watch progress currently estimates watched seconds from the current playback position. A production player should use segment-based watch accounting to avoid over-crediting skipped video.
