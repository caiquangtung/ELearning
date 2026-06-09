# AI Quality Evaluation

This document defines how to evaluate AI/RAG behavior in the ELearning platform. It is intended for developers, QA, product owners, and reviewers.

## Evaluation Goals

AI features should be:

- useful for the target workflow
- grounded in available data
- traceable through metadata and citations
- safe on out-of-scope or low-confidence inputs
- resilient when the provider fails
- deterministic enough for regression tests where local providers are used

## Global AI Quality Checklist

For every AI feature:

- output is marked as suggestion/draft/assistant response
- output does not directly mutate final business state unless reviewed through existing workflow
- provider/model/prompt version are recorded where applicable
- invalid provider output is rejected or safely falls back
- errors are recoverable in the UI/API
- no unnecessary PII is sent to provider prompts
- tests cover success, invalid input, provider failure, and fallback

## RAG Quality Checklist

For each RAG answer:

- in-scope course question returns `usedContext=true`
- answer includes at least one citation
- cited snippets support the answer
- citation course/lesson metadata is correct
- out-of-scope question returns refusal
- no answer-specific citations are invented by the provider
- confidence is within `0..1`
- provider/model/prompt version are present on assistant messages
- learner cannot retrieve inaccessible paid/unpublished content

## Golden Dataset

Maintain a small dataset of repeatable RAG test cases after seed/demo content is stable.

Recommended case types:

| Case type | Example | Expected result |
| --- | --- | --- |
| Direct lesson fact | "What does JWT validation check?" | Answer with lesson citation |
| Course overview | "What topics are covered in this course?" | Answer with course/lesson citation |
| Concept explanation | "Why does audience validation matter?" | Answer grounded in relevant lesson if present |
| Out of scope | "Who won the World Cup?" | Refusal, no citations |
| Access boundary | Learner asks about paid course without purchase | Refusal or no accessible context |
| Unpublished content | Question about draft course | No retrieval for learner |
| Provider failure | Simulate provider error | Local extractive fallback |
| Bad provider JSON | Provider returns invalid object | Local extractive fallback |

## RAG Evaluation Rubric

Score each answer on a 0-2 scale per dimension.

| Dimension | 0 | 1 | 2 |
| --- | --- | --- | --- |
| Grounding | unsupported | partially supported | fully supported by citations |
| Citation quality | missing/wrong | relevant but incomplete | exact useful references |
| Completeness | misses key point | answers basic point | answers with sufficient detail |
| Concision | rambling/noisy | acceptable | direct and focused |
| Refusal behavior | guesses | uncertain or partial refusal | clear refusal when context is weak |
| Safety/privacy | exposes/uses wrong data | minor concern | appropriate boundary |

Suggested acceptance:

- in-scope cases average at least `1.5`
- out-of-scope cases must score `2` for refusal behavior
- access-boundary cases must score `2` for safety/privacy

## Manual RAG Review Template

Use this format when reviewing answers:

```markdown
## Question

...

## Expected source

- Course:
- Section:
- Lesson:

## Actual answer

...

## Citations returned

1. ...
2. ...

## Scores

- Grounding:
- Citation quality:
- Completeness:
- Concision:
- Refusal behavior:
- Safety/privacy:

## Verdict

Pass / Needs fix

## Notes

...
```

## Automated Test Targets

Backend unit tests should cover:

- chunking is stable and respects max chunk size
- reindex is idempotent by content hash
- local dense embedding is deterministic, 384-dimensional, and normalized
- reindex writes pgvector data plus debug `embedding_json`
- retriever ranks relevant chunks above unrelated chunks
- chat refuses when no context is retrieved
- local extractive answer returns only retrieved citations
- provider confidence is clamped within `0..1`
- invalid provider output falls back or fails safely
- session access is limited to owner
- retrieval access is limited to published and entitled course content

Integration/smoke tests should cover:

- seed demo data
- reindex knowledge
- ask lesson-specific question
- verify answer has citation
- ask out-of-scope question
- verify refusal
- verify anonymous users cannot use protected AI chat endpoints
- verify learner cannot retrieve another user's chat session

## Feature-Specific Evaluation

### Quiz Question Generation

Check:

- generated question count matches request
- question types are allowed
- difficulty is respected
- options have exactly one correct answer for single-choice questions
- explanation is meaningful
- instructor must still accept/edit generated questions

Failure cases:

- provider returns malformed JSON
- provider returns too many/few questions
- provider returns unsupported question type
- generated points are invalid

### Essay Grading Suggestions

Check:

- score suggestion stays within valid bounds
- feedback references rubric where provided
- suggestion does not submit final grade
- low confidence is visible
- instructor can override

Failure cases:

- no essay/code answers
- invalid rubric
- provider returns out-of-range score
- provider returns empty feedback

### Learning Path Generation

Check:

- returned courses exist in catalog
- no hallucinated course ids
- course order is reasonable for goal/current skills
- output is draft metadata, not automatic assignment
- provider metadata is accurate

Failure cases:

- provider invents course ids
- no available courses match goal
- overly broad goal

### Course Recommendations

Check:

- recommendations are explainable
- already purchased/enrolled courses are deprioritized where expected
- popular/relevant courses rank above unrelated courses
- empty data state is handled

### Learner Risk

Check:

- score is explainable through signals
- risk level maps to score consistently
- missing activity data does not crash
- recommendations are actions for instructor/admin, not automatic penalties

### Semantic Search

Check:

- keyword-like query returns matching published courses
- natural-language query returns relevant courses
- unpublished/deleted courses are not returned
- empty query is rejected or handled

## Regression Process

Before changing prompts, retrieval thresholds, chunking, or provider parsing:

1. Run unit tests.
2. Run the RAG golden dataset manually or through smoke tests.
3. Compare answer/citation quality against previous behavior.
4. Update prompt version for meaningful prompt changes.
5. Document expected behavior changes.

## Metrics to Track Later

Recommended future metrics:

- answer success rate
- refusal rate
- citation click-through rate
- provider failure rate
- fallback rate
- average retrieved chunk score
- token usage by feature
- cost by provider/model
- thumbs up/down feedback from learners
- manual grade override delta for AI grading

## Release Gate

For a demo/internal release:

- local provider path works without credentials
- `dotnet test` passes
- `npm run build` passes
- manual reindex succeeds
- one in-scope RAG question returns citation
- one out-of-scope RAG question refuses

For a public/enterprise pilot:

- provider credentials are configured through secrets
- AI request logs are monitored
- RAG golden dataset is passing
- security/access-boundary cases are tested
- support team has the runbook
- users see citations and AI-generated labels clearly
