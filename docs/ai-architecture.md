# AI Architecture

This document describes the deployed AI architecture in the ELearning platform: feature boundaries, provider strategy, RAG design, data model, runtime flows, security controls, and operational entry points.

Related documents:

- [AI/RAG Foundation](./ai-rag-foundation.md)
- [AI/RAG Runbook](./ai-rag-runbook.md)
- [AI Quality Evaluation](./ai-quality-evaluation.md)
- [AI-6 Completion - RAG Learning Assistant](./ai6-completion.md)

## Current AI Feature Set

| Code | Feature | Primary user | Current implementation |
| --- | --- | --- | --- |
| AI-1 | Quiz question generation | Instructor | Local deterministic provider or OpenAI-compatible provider |
| AI-2 | Course recommendation | Learner | Local deterministic scoring |
| AI-3 | Essay grading assistant | Instructor | Local deterministic provider or OpenAI-compatible provider |
| AI-4 | Learner risk prediction | Instructor/Admin | Local deterministic scoring |
| AI-5 | Semantic course search | Learner | Local deterministic sparse embeddings |
| AI-5 | Learning path generation | Learner | Local deterministic provider or OpenAI-compatible provider |
| AI-6 | RAG Learning Assistant | Learner | JSON-vector retrieval, local extractive fallback, optional OpenAI-compatible answer synthesis |

## Architectural Principles

- The Application layer owns AI contracts and feature handlers.
- The Infrastructure layer owns local deterministic providers, external provider adapters, RAG retrieval/indexing, and persistence integration.
- AI output is assistive: suggestions, drafts, recommendations, or grounded answers. AI does not directly mutate grades, progress, orders, enrollments, or certificates.
- Local deterministic providers are the default so the app works without provider credentials.
- External provider integration is OpenAI-compatible HTTP first. Azure-specific endpoint mode is a follow-up.
- Prompt versions, provider/model metadata, input hashes, status, and token estimates are logged for auditability.
- RAG answers must be grounded in retrieved course chunks and return citations.

## System Context

```mermaid
flowchart LR
    Learner["Learner"] --> Frontend["Angular Web App"]
    Instructor["Instructor"] --> Frontend
    Admin["Admin / Org Admin"] --> Frontend

    Frontend --> Api["ELearning Web API"]
    Api --> Application["Application Layer<br/>MediatR handlers + AI contracts"]
    Application --> Infrastructure["Infrastructure AI Services"]

    Infrastructure --> Database["Postgres<br/>Courses, AI logs, RAG chunks, chat"]
    Infrastructure --> Cache["Redis<br/>AI/cache/rate-limit primitives"]
    Infrastructure --> LocalProvider["Local deterministic providers"]
    Infrastructure --> OpenAIProvider["OpenAI-compatible provider<br/>optional"]

    OpenAIProvider -. "disabled when Ai:Provider=Local" .-> Infrastructure
```

## Component Architecture

```mermaid
flowchart TB
    subgraph Application["ELearning.Application"]
        AiHandlers["Features/Ai handlers"]
        AiContracts["Common/Interfaces<br/>AI contracts"]
        CourseHandlers["Course mutation handlers"]
    end

    subgraph InfrastructureAI["ELearning.Infrastructure/Ai"]
        Shared["AiOptions<br/>AiRequestLogRepository"]

        subgraph Local["Local"]
            LocalQuiz["LocalQuizQuestionGenerator"]
            LocalEssay["LocalEssayGradingService"]
            LocalPath["LocalLearningPathService"]
            LocalSearch["LocalSemanticSearchService"]
            LocalRisk["LocalLearnerRiskService"]
            LocalReco["LocalCourseRecommendationService"]
            LocalEmbedding["LocalEmbeddingService"]
        end

        subgraph Providers["Providers"]
            ConfigQuiz["ConfigurableAiQuizQuestionGenerator"]
            ConfigEssay["ConfigurableAiEssayGradingService"]
            ConfigPath["ConfigurableAiLearningPathService"]
            ChatClient["OpenAiCompatibleChatClient"]
        end

        subgraph Rag["Rag"]
            Chunker["AiKnowledgeChunker"]
            Indexing["AiKnowledgeIndexingService"]
            Queue["InMemoryAiKnowledgeReindexQueue"]
            Worker["AiKnowledgeReindexWorker"]
            Policy["AiKnowledgeAccessPolicy"]
            Retriever["AiKnowledgeRetriever"]
            Chat["AiRagChatService"]
        end
    end

    AiHandlers --> AiContracts
    CourseHandlers --> Queue
    AiContracts --> Local
    AiContracts --> Providers
    AiContracts --> Rag

    ConfigQuiz --> LocalQuiz
    ConfigQuiz --> ChatClient
    ConfigEssay --> LocalEssay
    ConfigEssay --> ChatClient
    ConfigPath --> LocalPath
    ConfigPath --> ChatClient

    Indexing --> Chunker
    Indexing --> LocalEmbedding
    Retriever --> Policy
    Retriever --> LocalEmbedding
    Chat --> Retriever
    Chat --> ChatClient
    Worker --> Indexing
```

## Application Layer Contracts

AI service contracts live in `src/ELearning.Application/Common/Interfaces`.

Key contracts:

- `IAiQuizQuestionGenerator`
- `IAiEssayGradingService`
- `IAiLearningPathService`
- `IAiSemanticSearchService`
- `IAiLearnerRiskService`
- `IAiCourseRecommendationService`
- `IAiEmbeddingService`
- `IAiRagChatService`
- `IAiKnowledgeIndexingService`
- `IAiKnowledgeRetriever`
- `IAiKnowledgeReindexQueue`
- `IAiRequestLogRepository`

AI feature handlers live under `src/ELearning.Application/Features/Ai/*`.
Handlers validate request shape, resolve current user context, call AI contracts, and persist audit metadata where applicable.

## Infrastructure Layout

| Folder | Responsibility |
| --- | --- |
| `src/ELearning.Infrastructure/Ai` | Shared options and AI request log repository |
| `src/ELearning.Infrastructure/Ai/Local` | Offline deterministic AI providers and local embedding implementation |
| `src/ELearning.Infrastructure/Ai/Providers` | Configurable provider selectors and OpenAI-compatible HTTP adapter |
| `src/ELearning.Infrastructure/Ai/Rag` | Chunking, indexing, queue/worker, access policy, retriever, chat orchestration |

The namespace remains `ELearning.Infrastructure.Ai` to avoid leaking folder layout into consumers.

## Provider Strategy

```mermaid
flowchart LR
    FeatureHandler["AI feature handler"] --> Contract["AI contract"]
    Contract --> Configurable["Configurable provider"]
    Configurable --> Options{"Ai:Provider"}
    Options -->|"Local"| Local["Local deterministic service"]
    Options -->|"OpenAiCompatible + valid key/model"| Remote["OpenAI-compatible chat API"]
    Remote --> Parsed["Structured JSON parsing + validation"]
    Remote -. "failure + FallbackToLocal=true" .-> Local
    Parsed --> Response["Feature response + audit metadata"]
    Local --> Response
```

Current provider rules:

- `Provider=Local` is default.
- `Provider=OpenAiCompatible` requires `ApiKey` and `ChatModel` for remote calls.
- `FallbackToLocal=true` keeps workflows usable if the remote provider fails.
- RAG v1 uses the remote provider only for answer synthesis.
- Embeddings remain local deterministic sparse vectors in this sprint.

## Configuration

AI options are bound from the `Ai` configuration section.

| Option | Default | Purpose |
| --- | --- | --- |
| `Provider` | `Local` | Provider selector: `Local` or `OpenAiCompatible` |
| `Model` | `local-deterministic-v1` | Local/default model metadata |
| `BaseUrl` | `https://api.openai.com/v1` | OpenAI-compatible API base URL |
| `ApiKey` | empty | Provider secret, must come from env/user-secrets |
| `ChatModel` | empty | Remote chat model name |
| `TimeoutSeconds` | `30` | HTTP provider timeout |
| `MaxOutputTokens` | `1200` | Remote response cap |
| `MaxRetries` | `2` | Provider retry count |
| `FallbackToLocal` | `true` | Use local fallback when provider fails |
| `QuizQuestionPromptVersion` | `quiz-question-generator-v1` | Quiz prompt audit version |
| `EssayGradingPromptVersion` | `essay-grading-v1` | Essay prompt audit version |
| `LearningPathPromptVersion` | `learning-path-generator-v1` | Learning path prompt audit version |
| `RagChatPromptVersion` | `rag-learning-assistant-v1` | RAG prompt audit version |
| `RagMaxRetrievedChunks` | `4` | Top retrieved chunks used for RAG |
| `RagMinSimilarity` | `0.05` | Minimum cosine similarity for retrieved chunks |
| `MaxSourceCharacters` | `12000` | Source-content cap for provider prompts |

Secrets must not be committed to `appsettings*.json`. Use environment variables such as `Ai__Provider`, `Ai__ApiKey`, and `Ai__ChatModel`.

## RAG Data Model

```mermaid
erDiagram
    COURSE ||--o{ AI_KNOWLEDGE_CHUNK : "indexed as"
    SECTION ||--o{ AI_KNOWLEDGE_CHUNK : "optional source"
    LESSON ||--o{ AI_KNOWLEDGE_CHUNK : "optional source"
    USER ||--o{ AI_CHAT_SESSION : "owns"
    AI_CHAT_SESSION ||--o{ AI_CHAT_MESSAGE : "contains"

    AI_KNOWLEDGE_CHUNK {
        uuid id
        uuid course_id
        uuid section_id
        uuid lesson_id
        string source_type
        string course_title
        string section_title
        string lesson_title
        int chunk_index
        string content_hash
        text text
        json embedding_json
        json metadata_json
        datetime created_at
        datetime updated_at
    }

    AI_CHAT_SESSION {
        uuid id
        uuid user_id
        uuid course_id
        string title
        datetime created_at
        datetime updated_at
    }

    AI_CHAT_MESSAGE {
        uuid id
        uuid session_id
        string role
        text content
        json citations_json
        string provider
        string model
        string prompt_version
        decimal confidence
        bool used_context
        datetime created_at
    }
```

RAG v1 stores embeddings as JSON-serialized sparse vectors in Postgres and calculates cosine similarity in application code. `IAiKnowledgeRetriever` is the seam for moving to `pgvector`, a vector cache, or an external vector database later.

## Knowledge Indexing Flow

```mermaid
sequenceDiagram
    participant Admin as "Admin / AI.Manage"
    participant API as "AiController"
    participant Handler as "ReindexAiKnowledgeCommandHandler"
    participant Indexer as "AiKnowledgeIndexingService"
    participant Chunker as "AiKnowledgeChunker"
    participant Embedding as "LocalEmbeddingService"
    participant DB as "Postgres"

    Admin->>API: "POST /api/v1/ai/knowledge/reindex"
    API->>Handler: "ReindexAiKnowledgeCommand(courseId?)"
    Handler->>Indexer: "ReindexAsync(courseId?)"
    Indexer->>DB: "Load published course/section/lesson content"
    Indexer->>Chunker: "Build stable chunks"
    Chunker-->>Indexer: "AiKnowledgeChunkSource[]"
    loop "For each desired chunk"
        Indexer->>Embedding: "Embed(chunk.Text)"
        Embedding-->>Indexer: "Sparse vector"
    end
    Indexer->>DB: "Delete stale chunks"
    Indexer->>DB: "Insert new chunks"
    Indexer-->>Handler: "IndexedCourses, IndexedChunks, DeletedStaleChunks"
    Handler-->>API: "ReindexAiKnowledgeDto"
```

Background reindex is also triggered after course mutations:

- publish course
- update published course
- add section to published course
- add lesson to published course
- delete course

```mermaid
flowchart LR
    Mutation["Course mutation handler"] --> Save["Save course changes"]
    Save --> Queue["IAiKnowledgeReindexQueue.EnqueueAsync(courseId)"]
    Queue --> Worker["AiKnowledgeReindexWorker"]
    Worker --> Indexer["AiKnowledgeIndexingService.ReindexAsync(courseId)"]
    Indexer --> Chunks["AiKnowledgeChunks refreshed"]
```

## Learner Chat Flow

```mermaid
sequenceDiagram
    participant Learner as "Learner"
    participant UI as "AI Tutor UI"
    participant API as "AiController"
    participant Handler as "SendAiChatMessageCommandHandler"
    participant Chat as "AiRagChatService"
    participant Retriever as "AiKnowledgeRetriever"
    participant Policy as "AiKnowledgeAccessPolicy"
    participant Provider as "OpenAI-compatible provider"
    participant DB as "Postgres"

    Learner->>UI: "Ask a course question"
    UI->>API: "POST /api/v1/ai/chat/sessions/{id}/messages"
    API->>Handler: "SendAiChatMessageCommand"
    Handler->>Chat: "SendMessageAsync(userId, roles, sessionId, message)"
    Chat->>DB: "Validate session owner and store user message"
    Chat->>Retriever: "RetrieveAsync(userId, roles, question, courseId?)"
    Retriever->>Policy: "Resolve accessible published course ids"
    Policy->>DB: "Check role/course purchase/free scope"
    Retriever->>DB: "Load candidate chunks"
    Retriever-->>Chat: "Ranked citations"
    alt "No citations"
        Chat->>DB: "Store refusal answer"
        Chat-->>Handler: "usedContext=false"
    else "Citations found and provider configured"
        Chat->>Provider: "Grounded prompt with excerpts"
        Provider-->>Chat: "JSON answer + confidence"
        Chat->>DB: "Store assistant answer + citations"
        Chat-->>Handler: "grounded answer"
    else "Provider unavailable or invalid"
        Chat->>DB: "Store local extractive answer + citations"
        Chat-->>Handler: "fallback answer"
    end
    Handler->>DB: "Store AiRequestLog"
    Handler-->>API: "AiChatAnswerDto"
    API-->>UI: "answer, citations, confidence, provider, model"
```

## Access Control and Data Boundaries

| Boundary | Rule |
| --- | --- |
| AI endpoints | Require authentication and permission attributes |
| Chat sessions | Current user can list/read/send only inside own sessions |
| Course-scoped chat session | Course must be published and accessible to the user |
| RAG retrieval for privileged roles | Admin, Instructor, OrgAdmin can retrieve from all published courses |
| RAG retrieval for learner | Learner can retrieve from published free courses and paid course/class purchases |
| Knowledge reindex | Requires `AI.Manage` |
| Provider secrets | Environment/user-secrets only, not committed appsettings |
| Provider prompts | Avoid unnecessary PII; prefer course excerpts and request metadata |

## Guardrails

- RAG refuses when no retrieved context is available.
- RAG citations are generated only from retrieved `AiKnowledgeChunk` rows.
- Provider answer confidence is clamped between `0` and `1`.
- Invalid provider JSON falls back to local extractive answers when fallback is enabled.
- Authoring/grading/path outputs remain drafts or suggestions.
- AI request metadata is logged for review and troubleshooting.

## Public API Surface

`src/ELearning.WebApi/Controllers/v1/AiController.cs` exposes:

- `GET /api/v1/ai/recommendations/courses`
- `GET /api/v1/ai/search/courses`
- `POST /api/v1/ai/learning-paths/generate`
- `POST /api/v1/ai/chat/sessions`
- `GET /api/v1/ai/chat/sessions`
- `GET /api/v1/ai/chat/sessions/{sessionId}/messages`
- `POST /api/v1/ai/chat/sessions/{sessionId}/messages`
- `POST /api/v1/ai/knowledge/reindex`
- `POST /api/v1/ai/quizzes/generate-questions`
- `POST /api/v1/ai/quizzes/attempts/{attemptId}/grade-suggestions`
- `GET /api/v1/ai/learners/{userId}/risk`
- `GET /api/v1/ai/organizations/{organizationId}/risk-report`

## Frontend Integration

Angular API bindings live in:

- `frontend/web/src/app/core/api/lms-api.service.ts`

Learner-facing RAG chat UI lives in:

- `frontend/web/src/app/features/learn/ai-chat.component.ts`

The UI should present AI as a professional assistant:

- show `AI-generated suggestion` or equivalent label where appropriate
- show citations as reference cards/footnotes
- show provider/confidence metadata where useful
- avoid implying the AI answer is authoritative without course evidence

## Observability

`AiRequestLog` stores:

- feature name
- provider
- model
- prompt version
- input hash
- token estimate
- status
- error message when failed

For RAG, `AiChatMessage` also stores answer content, citations JSON, provider/model/prompt metadata, confidence, and whether context was used.

## Known Limits

- RAG vectors are JSON in Postgres, not `pgvector`.
- Retrieval is app-side cosine similarity, not ANN/vector-index accelerated.
- Embeddings are local deterministic sparse vectors.
- External provider integration is OpenAI-compatible chat only.
- No provider-side embedding adapter yet.
- No admin UI for AI logs/RAG reindex status yet.
- Background reindex queue is in-memory and suitable for current app/runtime, not distributed workers.

## Follow-Up Architecture Work

- Replace JSON vector store with `pgvector` or a vector-cache design.
- Add OpenAI-compatible embedding adapter behind `IAiEmbeddingService`.
- Add distributed reindex jobs if the API is horizontally scaled.
- Add admin AI observability UI: request logs, token usage, provider errors, reindex history.
- Add automated RAG quality dataset and regression scoring.
