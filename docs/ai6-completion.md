# AI-6 Completion - RAG Learning Assistant

## Status

MVP completed.

## Scope Delivered

- Added retrieval-augmented generation (RAG) chat assistant for course learning.
- Added persistent chat session and message storage:
  - `POST /api/v1/ai/chat/sessions`
  - `GET /api/v1/ai/chat/sessions`
  - `GET /api/v1/ai/chat/sessions/{sessionId}/messages`
  - `POST /api/v1/ai/chat/sessions/{sessionId}/messages`
- Added knowledge indexing and reindexing:
  - `POST /api/v1/ai/knowledge/reindex`
- Added course knowledge ingestion services:
  - `AiKnowledgeChunker` for course/section/lesson chunking
  - `AiKnowledgeIndexingService` for embedding creation, deduplication, and chunk persistence
- Added RAG retrieval pipeline:
  - question embedding via `IAiEmbeddingService`
  - access-scoped citation retrieval from `AiKnowledgeChunks` through `IAiKnowledgeRetriever`
  - chat orchestration and provider fallback in `AiRagChatService`
- Added OpenAI-compatible provider integration with extractive fallback.
- Added Angular learner chat UI and API bindings in `frontend/web/src/app/features/learn/ai-chat.component.ts`.
- Added AI/RAG architecture, foundation, runbook, and quality evaluation docs:
  - `docs/ai-architecture.md`
  - `docs/ai-rag-foundation.md`
  - `docs/ai-rag-runbook.md`
  - `docs/ai-quality-evaluation.md`

## Design Notes

- The assistant answers only from published course content excerpts and returns source citations.
- Knowledge chunks are generated from course overview, section, and lesson text and are deduplicated by content hash.
- The RAG flow supports external model calls when configured, but falls back to local extractive answers if the provider fails.
- AI chat messages and citations are persisted for traceability, replay, and audit.
- The RAG assistant is decision-support only and does not automatically mutate learner progress, grades, or enrollment state.

## Verification

- Backend build passed.
- `tests/ELearning.Application.UnitTests/RagLearningAssistantTests.cs` covers RAG helper behavior.
- Angular chat route and API integration were validated.

## Follow-up

- Add frontend acceptance tests for chat session creation, message send, and citation rendering.
- Add provider-quality checks for external OpenAI-compatible chat responses.
- Replace in-memory background reindex queue with durable distributed jobs before horizontal scaling.
- Add an admin screen for RAG usage, citations, and audit logs.
