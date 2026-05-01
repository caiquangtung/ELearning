# Modernize .NET Agent Workflow

This repo uses a lightweight, file-based workflow to plan and execute .NET modernization safely.

## Where the workflow lives

All artifacts are stored under:

- `.github/upgrades/modernize-dotnet/`

Key files:
- `scenario-instructions.md`: preferences, decisions, non-negotiable quality gates
- `assessment.md`: current state inventory + constraints
- `plan.md`: phased plan
- `tasks.md`: task checklist
- `execution-log.md`: append-only timeline

## How to use

1. Create a working branch (recommended):
   - `chore/modernize-dotnet`
2. Fill `assessment.md` (inventory target frameworks, packages, warnings, tests).
3. Write `plan.md` based on the assessment.
4. Execute tasks in `tasks.md` in order.
5. After each task:
   - build + test
   - ensure **no warnings in modified projects**
   - document what changed in the execution log

## Commands (typical)

```bash
dotnet --info
dotnet restore src/ELearning.sln
dotnet build src/ELearning.sln
dotnet test src/ELearning.sln
```

## Notes

- Treat warnings as errors operationally: each completed task should leave touched projects warning-free.
- Keep changes small and reviewable; prefer phased upgrades over big-bang migrations.

