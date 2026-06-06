# AI Architecture

This document describes the deployed AI architecture in the ELearning platform, including the current AI feature set, application layers, provider strategy, and the RAG learning assistant design.

## AI feature set

- AI-1: Quiz question generation
- AI-2: Course recommendation
- AI-3: Essay grading assistant
- AI-4: Learner risk prediction
- AI-5: Semantic course search and learning path generation
- AI-6: RAG Learning Assistant

## Architectural principles

- The Application layer defines AI contracts and command/query handlers, keeping AI logic decoupled from external provider details.
- The Infrastructure layer implements provider adapters and local deterministic defaults.
- All AI features are permissioned and audited.
- Local deterministic providers are the default demo path, so AI features work without an external API key.
- External OpenAI-compatible providers are supported via adapter classes.

## Application layer

### Interfaces and DTOs

AI service contracts live under:

- `src/ELearning.Application/Common/Interfaces`

Key interfaces:

- `IAiQuizQuestionGenerator`
- `IAiEssayGradingService`
- `IAiLearningPathService`
- `IAiSemanticSearchService`
- `IAiLearnerRiskService`
- `IAiCourseRecommendationService`
- `IAiRagChatService`
- `IAiEmbeddingService`
- `IAiKnowledgeIndexingService`

### Mediator handlers

AI requests are exposed through command/query handlers in:

- `src/ELearning.Application/Features/Ai/*`

The handlers perform validation, authorization, and service orchestration.
They also persist AI audit logs via `IAiRequestLogRepository` when a request is sent.

## Infrastructure layer

### Dependency injection

`src/ELearning.Infrastructure/DependencyInjection.cs` registers the AI services.

Local and configurable services include:

- `LocalQuizQuestionGenerator`
- `LocalEssayGradingService`
- `LocalLearningPathService`
- `LocalSemanticSearchService`
- `LocalLearnerRiskService`
- `LocalCourseRecommendationService`
- `LocalEmbeddingService`
- `AiRagChatService`
- `AiKnowledgeIndexingService`
- `OpenAiCompatibleQuizQuestionGenerator`
- `OpenAiCompatibleEssayGradingService`
- `OpenAiCompatibleLearningPathService`
- `OpenAiCompatibleChatClient`

### Provider strategy

- Local deterministic providers are used by default for demos and offline validation.
- Configurable adapters allow switching to an OpenAI-compatible provider without changing business logic.
- `AiOptions` controls provider selection, prompt version, API key, and RAG tuning values.

## RAG Learning Assistant architecture

### Data ingestion

- `AiKnowledgeChunker` splits course text into normalized chunks using sentence boundaries.
- Each chunk is hashed and stored as an `AiKnowledgeChunk` record.
- `AiKnowledgeIndexingService` embeds each chunk and stores embeddings as JSON.

### Retrieval pipeline

`AiRagChatService.SendMessageAsync` executes the RAG flow:

1. Persist user message as `AiChatMessage.User`.
2. Embed the incoming question via `IAiEmbeddingService`.
3. Retrieve published course chunks with cosine similarity and configurable thresholds.
4. Build a structured prompt containing excerpts and send it to the provider.
5. If external provider fails or is unavailable, fallback to a local extractive answer.
6. Persist the assistant response and citations as `AiChatMessage.Assistant`.

### Answer structure

The RAG answer includes:

- `Answer` text
- `Citations` with course/section/lesson metadata
- `Confidence`
- `Provider`, `Model`, `PromptVersion`
- `UsedContext` flag
- Token estimate

## AI data and audit

### AI logs

- `AiRequestLog` stores request metadata for every AI feature.
- Logged fields include feature, provider, model, prompt version, input hash, token estimate, status, and error.

### Knowledge storage

- `AiKnowledgeChunks` stores chunk text, embedding JSON, metadata JSON, and content hash.
- Reindexing removes stale chunks and adds new chunks when course content changes.

## API surface

`src/ELearning.WebApi/Controllers/v1/AiController.cs` exposes AI endpoints with permission guards:

- `GET /api/v1/ai/recommendations/courses`
- `GET /api/v1/ai/search/courses`
- `POST /api/v1/ai/learning-paths/generate`
- `POST /api/v1/ai/quizzes/generate-questions`
- `POST /api/v1/ai/quizzes/attempts/{attemptId}/grade-suggestions`
- `GET /api/v1/ai/learners/{userId}/risk`
- `GET /api/v1/ai/organizations/{organizationId}/risk-report`
- `POST /api/v1/ai/chat/sessions`
- `GET /api/v1/ai/chat/sessions`
- `GET /api/v1/ai/chat/sessions/{sessionId}/messages`
- `POST /api/v1/ai/chat/sessions/{sessionId}/messages`
- `POST /api/v1/ai/knowledge/reindex`

## Frontend integration

Angular API bindings are defined in `frontend/web/src/app/core/api/lms-api.service.ts`.
RAG chat UI is implemented in `frontend/web/src/app/features/learn/ai-chat.component.ts`.

## Deployment notes

- The RAG assistant works in demo mode without external API credentials.
- Enabling an external provider requires setting provider credentials and model selection in configuration.
- `RagChatPromptVersion` is versioned to ensure repeatability and auditability.

## Observability

- AI request logs enable analysis of provider success/failure, token usage, and model behavior.
- RAG citations provide traceability for course-based answers.
