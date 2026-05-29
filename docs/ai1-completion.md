# AI-1 Completion - Quiz Question Generator

## Status

MVP completed.

## Delivered

- Added AI provider abstraction for quiz question generation:
  - `IAiQuizQuestionGenerator`
  - `LocalQuizQuestionGenerator`
  - `AiOptions`
- Added AI audit metadata:
  - `AiRequestLog` aggregate
  - `IAiRequestLogRepository`
  - EF configuration and migration `AiQuestionGeneration`
- Added generation API:
  - `POST /api/v1/ai/quizzes/generate-questions`
  - Permission: `AI.Use`
  - Redis rate-limit policy hook for `/ai/*` POST endpoints
- Added structured draft output:
  - question text, type, points, sort order, difficulty, explanation
  - multiple-choice options with one correct option
- Added Angular quiz detail workflow:
  - Generate with AI action
  - question count, difficulty, and type controls
  - review panel
  - accept draft into existing quiz question API
  - discard draft
- Added tests:
  - AI request log domain tests
  - local generator output tests
  - generation command validator tests

## Design Notes

- The MVP uses a deterministic local provider so the feature is demoable without an external API key.
- Generated questions remain drafts and are not saved until the instructor accepts them.
- The accepted-question flow reuses the existing quiz `AddQuestion` endpoint to avoid duplicating quiz write rules.
- The audit log stores feature, provider, model, prompt version, input hash, token estimate, status, and error message.

## Deferred

- OpenAI/Azure OpenAI provider adapter.
- Prompt quality evaluation dataset.
- Instructor inline editing inside the AI draft panel before acceptance.
- AI cost cache and per-user token budget.
- Admin screen for AI audit logs.

## Verification

- `dotnet build src/ELearning.WebApi/ELearning.WebApi.csproj --no-restore --nologo -v:minimal -m:1 -p:UseSharedCompilation=false -p:BuildInParallel=false`
- `dotnet test tests/ELearning.Domain.UnitTests/ELearning.Domain.UnitTests.csproj --no-restore --nologo -v:minimal -m:1 -p:UseSharedCompilation=false -p:BuildInParallel=false`
- `dotnet test tests/ELearning.Application.UnitTests/ELearning.Application.UnitTests.csproj --no-restore --nologo -v:minimal -m:1 -p:UseSharedCompilation=false -p:BuildInParallel=false`
- `npm run build` in `frontend/web`
