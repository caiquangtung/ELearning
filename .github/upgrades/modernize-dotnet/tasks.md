# Tasks — modernize-dotnet

This is the high-level task list for the `modernize-dotnet` workflow.

Status legend:
- [ ] pending
- [~] in progress
- [x] completed
- [!] blocked

## Stage 0 — Source control + baseline

- [ ] T000: Create working branch and capture baseline build/test

## Stage 1 — Assessment

- [ ] T010: Inventory projects, target frameworks, SDK constraints
- [ ] T011: Inventory NuGet packages and vulnerable/outdated dependencies
- [ ] T012: Capture baseline warnings/analyzers and test coverage state

## Stage 2 — Plan

- [ ] T020: Draft phased upgrade plan (with risk and rollback notes)

## Stage 3 — Execution (phased)

- [ ] T030: Upgrade SDK + global.json (if used) and build tooling
- [ ] T031: Upgrade project TargetFramework(s)
- [ ] T032: Upgrade core NuGet dependencies (EF Core, MediatR, Serilog, etc.)
- [ ] T033: Fix breaking changes + remove obsolete APIs
- [ ] T034: Enable/adjust nullable + analyzers (optional / staged)
- [ ] T035: Update CI pipeline to match new SDK + run tests

## Stage 4 — Stabilization

- [ ] T040: Clean warnings to zero in modified projects
- [ ] T041: Regression test pass + smoke test key APIs
- [ ] T042: Documentation update (what changed, how to run, gotchas)

