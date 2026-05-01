# Plan — modernize-dotnet

This plan should be written after `assessment.md` is complete.

## Goals

- Upgrade runtime/framework safely
- Keep the solution building and tests passing throughout
- Remove warnings in modified projects at each phase boundary

## Phases (template)

### Phase 1 — Tooling + SDK alignment

- Decide SDK pinning strategy (`global.json` or CI-defined)
- Ensure local dev + CI uses the same SDK

### Phase 2 — TargetFramework upgrade

- Upgrade projects in dependency order (leaf libs → infrastructure → web API)
- Fix compile breaks, update analyzers

### Phase 3 — Dependency modernization

- Upgrade major packages (EF Core, logging, auth, testing)
- Address breaking changes

### Phase 4 — Stabilization

- Drive warnings to zero (modified projects)
- Run full test suite + smoke tests
- Update docs

## Validation checklist

- `dotnet --info`
- `dotnet restore`
- `dotnet build`
- `dotnet test`

