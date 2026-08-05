# AI Prompt and Output Optimization - Implementation Log

Completed on 2026-07-28.

## Implemented So Far

- Added versioned prompt templates for the first prompt set used by AI workflows:
  - `quiz-question-generator-v1`
  - `essay-grading-v1`
  - `learning-path-generator-v1`
  - `rag-learning-assistant-v1`
  - `rag-learning-assistant-no-context-v1`
- Added a shared prompt template loader:
  - `src/ELearning.Infrastructure/Ai/PromptTemplateStore.cs`
  - loads from `Ai/Prompts` with file and embedded-resource fallback
  - keeps runtime safe if a template file is missing
- Updated the AI providers to use versioned prompt templates instead of hard-coded system prompts:
  - quiz question generation
  - essay grading suggestions
  - learning path generation
  - RAG chat assistant
- Updated the infrastructure project file so prompt templates are copied and embedded with the build output.
- Expanded the RAG golden dataset from 3 cases to a broader seed-based baseline covering:
  - Secure Coding
  - Data Analytics
  - AI
  - Cloud Engineering
  - Backend Architecture
  - DevOps
  - Product Management
  - UX Research
  - Sales Enablement
  - Leadership
  - plus an out-of-scope refusal case

## Verification

- `ELearning.Infrastructure` build passed after the loader fix.
- The RAG evaluation service already reads `Ai/Rag/rag-golden-dataset.json`, so the expanded dataset is now available to the existing baseline runner.

## Next Step

- Run the RAG evaluation baseline against the expanded golden dataset and record the resulting scores.
- Add a lightweight evaluation report doc once the baseline has been executed.
