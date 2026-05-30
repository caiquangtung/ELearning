# AI-5 Completion - Semantic Search + Learning Path Generator

## Scope Delivered

- Added AI semantic search abstractions:
  - `IAiEmbeddingService`
  - `IAiSemanticSearchService`
  - `IAiLearningPathService`
- Added local deterministic providers:
  - token-frequency embedding generation
  - cosine similarity ranking
  - keyword fallback boost
  - draft learning path ranking from learner goal, skills, and target role
- Added APIs:
  - `GET /api/v1/ai/search/courses?q=&limit=`
  - `POST /api/v1/ai/learning-paths/generate`
- Added Redis caching:
  - semantic search by query hash and limit
  - learning path draft by input hash
- Added AI request audit logging for semantic search and learning path generation.
- Added Angular course catalog AI UX:
  - `Keyword` / `Semantic AI` search mode
  - semantic result cards with score, matched concepts, and reasons
  - "AI path" entry point
  - learning path draft dialog with goal, current skills, target role, max courses
  - ordered course path with reasons, estimated effort, confidence, and missing skill notes

## Verification

- Angular production build passed.
- Docker API build passed.
- Full Docker API/frontend rebuild and e2e smoke should be run after this doc update.

## Follow-up

- Add unit tests for embedding similarity, ranking, empty query validation, cache behavior, and audit logging.
- Add persistent learning path aggregate if the product needs review/publish/assignment workflows.
- Add OpenAI/Azure OpenAI embedding provider behind `IAiEmbeddingService`.
- Add pgvector or Elasticsearch only when catalog size makes local ranking insufficient.
