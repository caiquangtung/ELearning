# AI-3 Completion - Essay Grading Assistant

## Scope Delivered

- Added local deterministic AI essay/code grading suggestion provider:
  - `IAiEssayGradingService`
  - `LocalEssayGradingService`
- Added grading suggestion API:
  - `POST /api/v1/ai/quizzes/attempts/{attemptId}/grade-suggestions`
  - Permission: `Quizzes.Grade`
- Added structured suggestion output:
  - suggested score
  - confidence
  - reasoning
  - rubric breakdown
- Added guardrail:
  - AI suggestion does not grade or mutate the attempt.
  - Instructor still submits final grade through existing grading API.
- Added AI request audit logging for successful and failed suggestions.
- Enriched quiz attempt result data with question text and points for grading UI.
- Added Angular manual grading AI panel:
  - optional rubric input
  - "AI" action
  - suggested score applied to editable form fields
  - rubric explanation displayed next to learner answer

## Verification

- Docker backend/frontend build passed.
- Angular production build passed.
- Playwright smoke test passed.

## Follow-up

- Add unit tests for scoring boundaries, authorization, invalid provider output, and manual override behavior.
- Persist accepted vs overridden suggestion deltas if detailed model-quality analytics are needed.
- Add external LLM provider adapter behind `IAiEssayGradingService`.
