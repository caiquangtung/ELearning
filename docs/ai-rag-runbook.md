# AI/RAG Runbook

This runbook covers setup, reindexing, operational checks, and troubleshooting for the AI/RAG features.

## Quick Status

| Area | Current state |
| --- | --- |
| Default provider | `Local` |
| External provider | OpenAI-compatible chat and optional embedding HTTP |
| RAG embeddings | Local deterministic dense vectors by default; optional OpenAI-compatible or Google AI Studio embeddings, 768 dimensions |
| RAG vector store | Postgres pgvector `embedding_vector vector(768)` |
| RAG reindex endpoint | `POST /api/v1/ai/knowledge/reindex` |
| RAG background queue | Persisted job rows + Postgres polling/claiming hosted worker |
| RAG evaluation | Golden dataset runner via admin API |
| RAG fallback | Local extractive answer from retrieved snippets |
| Required RAG manage permission | `AI.Manage` |

## Configuration

Use environment variables or user-secrets. Do not commit provider secrets to appsettings.

### Local Provider

No secret is required.

```bash
Ai__Provider=Local
Ai__FallbackToLocal=true
```

### OpenAI-Compatible Provider

```bash
Ai__Provider=OpenAiCompatible
Ai__BaseUrl=https://api.openai.com/v1
Ai__ApiKey=<secret>
Ai__ChatModel=<chat-model>
Ai__TimeoutSeconds=30
Ai__MaxOutputTokens=1200
Ai__MaxRetries=2
Ai__FallbackToLocal=true
```

For local OpenAI-compatible gateways, set `Ai__BaseUrl` to the gateway's `/v1` base URL and set `Ai__ChatModel` to the served model name.

### OpenAI-Compatible RAG Embeddings

RAG embeddings are local by default. To use a real embedding provider:

```bash
Ai__RagEmbeddingProvider=OpenAiCompatible
Ai__RagEmbeddingBaseUrl=https://api.openai.com/v1
Ai__RagEmbeddingApiKey=<secret>
Ai__RagEmbeddingModel=<embedding-model>
Ai__RagEmbeddingDimensions=768
Ai__RagEmbeddingTimeoutSeconds=30
Ai__RagEmbeddingMaxRetries=2
Ai__FallbackToLocal=true
```

The provider response must return exactly `768` dimensions. The app normalizes the vector before storing it in `embedding_vector`. If the OpenAI-compatible provider fails and `FallbackToLocal=true`, the pipeline falls back to the local dense embedding model.

### Google AI Studio RAG Embeddings

Use native Gemini `embedContent` for Google AI Studio embeddings so the app can send `taskType`, `title`, and `outputDimensionality`.

```bash
Ai__RagEmbeddingProvider=GoogleAiStudio
Ai__RagEmbeddingBaseUrl=https://generativelanguage.googleapis.com/v1beta
Ai__RagEmbeddingApiKey=<secret>
Ai__RagEmbeddingModel=gemini-embedding-2
Ai__RagEmbeddingDimensions=768
Ai__RagEmbeddingFailureMode=FullTextFallback
Ai__RagQueryEmbeddingCacheTtlDays=30
Ai__RagAutoReindexEnabled=false
Ai__RagEmbeddingTimeoutSeconds=30
Ai__RagEmbeddingMaxRetries=2
```

Google document chunks are sent as `taskType=RETRIEVAL_DOCUMENT` with a separate `title`; user questions are sent as `taskType=RETRIEVAL_QUERY`. Do not mix Google vectors with local dense vectors. When Google embedding fails and `RagEmbeddingFailureMode=FullTextFallback`, retrieval falls back to PostgreSQL full-text search. Use `FailFast` to return an error instead.

Free API quotas can be exhausted quickly during reindex. Keep `RagAutoReindexEnabled=false` in CI/CD and quota-limited environments, then run manual reindex during a low-traffic window.

## RAG Tuning Options

```bash
Ai__RagChatPromptVersion=rag-learning-assistant-v1
Ai__RagEmbeddingDimensions=768
Ai__RagMaxRetrievedChunks=4
Ai__RagMinSimilarity=0.05
Ai__RagMaxContextCharacters=2400
Ai__RagCandidateMultiplier=8
Ai__RagReindexPollSeconds=5
Ai__MaxSourceCharacters=12000
Ai__RagEmbeddingFailureMode=FullTextFallback
Ai__RagQueryEmbeddingCacheTtlDays=30
Ai__RagAutoReindexEnabled=true
```

Guidance:

- Increase `RagMaxRetrievedChunks` if answers miss context.
- Decrease `RagMaxRetrievedChunks` if answers include noisy references.
- Increase `RagMinSimilarity` if irrelevant citations appear.
- Decrease `RagMinSimilarity` if the assistant refuses too often.
- Increase `RagCandidateMultiplier` if relevant chunks are being missed before ranking.
- Decrease `RagMaxContextCharacters` if provider prompts get too large.
- `RagEmbeddingDimensions` must stay `768` while the database column is `vector(768)`.
- Query embeddings are cached by normalized exact query, provider, model, and dimension for the configured TTL.

## pgvector Setup

Local Docker uses a pgvector-enabled Postgres image:

```yaml
postgres:
  image: pgvector/pgvector:pg17
```

The EF migration creates the extension and vector column:

```sql
CREATE EXTENSION IF NOT EXISTS vector;
ALTER TABLE ai_knowledge_chunks ADD COLUMN IF NOT EXISTS embedding_vector vector(768);
```

`embedding_json` remains as debug/backward compatibility data. Runtime retrieval uses `embedding_vector`.

Changing provider, model, or dimensions requires a full knowledge reindex. The Google migration clears old vectors before switching to `vector(768)` so stale `vector(384)` values are not queried accidentally.

## Reindex Knowledge

Reindex all published course content:

```http
POST /api/v1/ai/knowledge/reindex
Authorization: Bearer <admin-token>
Content-Type: application/json

{ "courseId": null }
```

Reindex one course:

```http
POST /api/v1/ai/knowledge/reindex
Authorization: Bearer <admin-token>
Content-Type: application/json

{ "courseId": "00000000-0000-0000-0000-000000000000" }
```

Expected response:

```json
{
  "jobId": "00000000-0000-0000-0000-000000000000",
  "indexedCourses": 1,
  "indexedChunks": 12,
  "deletedStaleChunks": 2
}
```

## Automatic Reindex Triggers

The app enqueues background reindex after:

- course publish
- update of a published course
- add section to a published course
- add lesson to a published course
- course delete

Each queued reindex creates an `AiKnowledgeReindexJob` row. `AiKnowledgeReindexWorker` polls queued rows, claims one with Postgres locking, and runs the indexer. Stale `InProgress` jobs older than 30 minutes are requeued by the worker.

## RAG Evaluation

Run the golden dataset:

```http
POST /api/v1/ai/rag/evaluations/run
Authorization: Bearer <admin-token>
```

List recent evaluation summaries:

```http
GET /api/v1/ai/rag/evaluations
Authorization: Bearer <admin-token>
```

Metrics:

- `retrievalHitRate`: in-scope cases retrieved expected course/snippet terms.
- `citationValidityRate`: returned citations point to existing knowledge chunks.
- `refusalAccuracyRate`: out-of-scope cases returned no context.
- `groundednessRate`: in-scope retrieved snippets contain expected grounding terms.

The dataset file is copied from `src/ELearning.Infrastructure/Ai/Rag/rag-golden-dataset.json`.

## Learner Chat Smoke Test

Prerequisites:

- published course with lesson content
- knowledge reindexed
- learner has access to the course, or the course is free

Flow:

1. Create session:

```http
POST /api/v1/ai/chat/sessions
Authorization: Bearer <learner-token>
Content-Type: application/json

{ "courseId": null, "title": "AI Tutor" }
```

2. Send question:

```http
POST /api/v1/ai/chat/sessions/{sessionId}/messages
Authorization: Bearer <learner-token>
Content-Type: application/json

{ "message": "What does this course say about JWT validation?" }
```

3. Check response:

```json
{
  "answer": "Based on the course material: ...",
  "citations": [
    {
      "courseTitle": "Secure API Development",
      "lessonTitle": "JWT validation",
      "snippet": "...",
      "score": 0.91
    }
  ],
  "confidence": 0.85,
  "usedContext": true,
  "provider": "Local",
  "model": "extractive-rag-v1"
}
```

## Expected Refusal

For out-of-scope questions, the assistant should refuse:

```json
{
  "answer": "I don't have enough course material to answer that.",
  "citations": [],
  "confidence": 0,
  "usedContext": false
}
```

## Operational Checks

### Check Knowledge Chunks

Use DB inspection to verify `AiKnowledgeChunks` has rows for published courses.

Minimum fields to inspect:

- `course_id`
- `source_type`
- `course_title`
- `lesson_title`
- `chunk_index`
- `content_hash`
- `text`
- `embedding_json`
- `embedding_vector`

### Check Chat Persistence

For a sent message, verify:

- one user `AiChatMessage`
- one assistant `AiChatMessage`
- assistant `citations_json`
- assistant `provider`
- assistant `model`
- assistant `prompt_version`
- assistant `confidence`
- assistant `used_context`

### Check AI Request Log

For RAG chat, inspect `AiRequestLog`:

- `feature=RagLearningAssistant`
- provider/model
- prompt version
- status
- token estimate
- error message if failed

## Troubleshooting

### Assistant Always Refuses

Likely causes:

- knowledge was not reindexed
- course is not published
- learner does not have access to the course
- question does not overlap with local dense hash embeddings
- `RagMinSimilarity` is too high
- lesson content is empty or not included in chunks

Actions:

1. Reindex knowledge manually.
2. Confirm published course has lessons/content.
3. Confirm learner course access.
4. Lower `Ai__RagMinSimilarity`.
5. Ask a more course-specific question using terms from the lesson.

### Citations Are Irrelevant

Likely causes:

- local dense hash embeddings are too shallow for semantic matching
- `RagMinSimilarity` is too low
- chunks are too broad
- course content has repeated generic terms

Actions:

1. Increase `Ai__RagMinSimilarity`.
2. Reduce `Ai__RagMaxRetrievedChunks`.
3. Improve course/lesson text quality.
4. Consider a real embedding provider behind `IAiTextEmbeddingService`.

### Provider Fails But Local Works

Likely causes:

- missing `Ai__ApiKey`
- missing `Ai__ChatModel`
- invalid `Ai__BaseUrl`
- provider timeout
- provider response is not valid JSON

Actions:

1. Confirm env/user-secret values.
2. Check provider logs.
3. Keep `Ai__FallbackToLocal=true` for demo/internal usage.
4. Review `AiRequestLog` failed records.

### Provider Answers Without Enough Grounding

Expected behavior should prevent this because citations come from retrieval, not from the provider.

Actions:

1. Inspect `citations_json` on assistant message.
2. Confirm answer text is supported by cited snippets.
3. If unsupported answer text appears, tighten the RAG prompt and increment `RagChatPromptVersion`.
4. Add evaluation cases for this scenario.

### Reindex Does Not Delete Stale Chunks

Check:

- Was reindex scoped to the right `courseId`?
- Was the course soft-deleted and full reindex not run?
- Did background queue run before process shutdown?

Actions:

1. Run manual full reindex.
2. Inspect `deletedStaleChunks`.
3. Confirm old content hashes are no longer desired.

## Deployment Notes

- Keep `Provider=Local` as default for local/dev environments.
- Use user-secrets or environment variables for provider credentials.
- Run manual full reindex after seed/demo data changes.
- Run manual full reindex after bulk import/migration of course content.
- For heavy multi-instance deployments, consider Hangfire/Quartz or a dedicated queue; current Postgres polling is sufficient for v1 operational tooling.

## Verification Commands

Backend:

```bash
dotnet test src/ELearning.sln --no-restore -m:1 /nr:false
```

Frontend:

```bash
cd frontend/web
npm run build
```

Playwright smoke requires the API to be running on the configured base URL before RAG tests can pass.
