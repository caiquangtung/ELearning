# Modernize .NET — Scenario Instructions (ELearning)

This folder contains a lightweight, repo-local modernization workflow for upgrading and modernizing the .NET solution in this repository.

It is designed to be:
- **Repeatable**: artifacts record decisions and changes.
- **Reviewable**: tasks are small, verifiable, and build/test-gated.
- **Safe**: source control + validation gates prevent half-migrations.

## Scope

- **Solution**: `src/ELearning.sln`
- **Primary goals** (fill/adjust per upgrade):
  - Upgrade Target Framework(s) across projects
  - Modernize build/test pipeline and SDK tooling
  - Remove obsolete packages/APIs, fix analyzers/warnings
  - Keep behavior intact (no breaking functional changes unless planned)

## Flow Mode

- **Automatic (default)**: proceed end-to-end, only pause when blocked.
- **Guided**: pause after Assessment and Plan for review.

Record the chosen mode below.

## User Preferences

### Technical Preferences

- **Target .NET version**: _TBD_
- **Nullable**: _TBD_ (enable / keep as-is / staged enable)
- **Treat warnings as errors**: **Yes** (project must build warning-free on completion of each task)

### Execution Style

- **Flow mode**: **Automatic**
- **Branching**: use a dedicated working branch (e.g. `chore/modernize-dotnet`)

## Decisions

Record decisions with short rationale, for example:
- Why a package was upgraded or pinned
- Any breaking changes and how they were mitigated
- Whether multi-targeting was used during transition

## Artifacts

- `assessment.md`: current state and upgrade constraints
- `plan.md`: phased plan and task sequencing
- `tasks.md`: derived task view (status + ordering)
- `tasks/<taskId>/task.md`: per-task working memory + research findings
- `tasks/<taskId>/progress-details.md`: what changed and how to validate
- `execution-log.md`: chronological log

## Quality Gates (non-negotiable)

For each completed task:
- Solution builds successfully
- Tests run and pass (unit/integration as applicable)
- **No build warnings in modified projects**
- Progress-details document updated with:
  - files changed
  - commands executed
  - validation results

