# AI RAG Baseline Report

Date: 2026-08-05

## Scope

This report records the current RAG Tutor baseline after adding hybrid retrieval, lightweight rerank/fusion, provider output validation, and retrieval quality gating.

## Implemented Baseline

- Retrieval now uses dense vector candidates plus sparse PostgreSQL full-text candidates when available.
- Candidate fusion uses a lightweight RRF-style rank boost plus sparse score boost before the existing lexical rerank and context budget trimming.
- Provider answers with retrieved context must parse as JSON with a non-empty `answer` and valid `confidence`; invalid output falls back safely.
- Retrieval quality gate refuses generation when citations do not meet the accepted similarity threshold.
- Chat responses preserve `promptVersion` through the frontend contract and AI Tutor UI metadata.

## Automated Verification

- `dotnet test src/ELearning.sln --no-restore`
  - Passed: 149 tests (96 Application unit tests, 45 Domain unit tests, 8 Architecture tests).
  - Full test coverage for hybrid fusion, provider JSON validation, confidence clamping, configured no-context responses, `promptVersion` DTO contract preservation, and CRAG retrieval quality gate behavior.
- `npm --prefix frontend/web run build`
  - Passed.
  - Angular app built successfully with updated AI Tutor UI metadata (`Provider`, `Model`, `Prompt Version`, `Confidence`, `Used Context`).

## Runtime Evaluation

The application-level RAG evaluation endpoint is available through:

- `POST /api/v1/ai/rag/evaluations/run`
- Admin UI: AI Knowledge -> Run evaluation

Run this after the target environment has published courses indexed into `ai_knowledge_chunks`. Record the resulting retrieval hit rate, citation validity, refusal accuracy, and groundedness in the admin screen before changing prompt versions, model versions, retrieval thresholds, or chunking behavior.

## Current Release Gate

- Backend and frontend builds must pass.
- RAG evaluation should be run on the target indexed dataset before release.
- Regression is indicated by lower retrieval hit rate, citation validity, refusal accuracy, or groundedness compared with the last accepted evaluation run.
