# Implementation Plan: [FEATURE]

**Branch**: `[###-feature-name]` | **Date**: [DATE] | **Spec**: [link]
**Input**: Feature specification from `/specs/[###-feature-name]/spec.md`

**Note**: This template is filled in by the `/speckit.plan` command. See `.specify/templates/commands/plan.md` for the execution workflow.

## Summary

Establish a Schema-First repository ("Maliev.MessagingContracts") serving as the authoritative source of truth. Contracts are defined in JSON Schema (Draft-07) and AsyncAPI 3.0, from which .NET 10 C# models are generated. This pivots from the previous Code-First approach. The repo includes automated validation, versioning enforcement, and documentation generation.

## Technical Context

**Language/Version**: C# / .NET 10
**Primary Dependencies**:
- Validation & Schema Parsing: JsonSchema.Net
- C# Code Generation: NJsonSchema
- AsyncAPI Tooling: AsyncAPI CLI
- Serialization: System.Text.Json (for generated models)
**Packaging**: NuGet (`Maliev.MessagingContracts`) containing generated code.
**Testing**: Serialization Round-trip Tests on generated models.
**Target Platform**: .NET 10
**Project Type**: Library (Schema-First)

## Constitution Check

*GATE: Must pass before Phase 0 research. Re-check after Phase 1 design.*

*   [x] **Scope Check**: Pure contracts? Yes.
*   [x] **Compatibility Check**: Breaking Changes? N/A (New Repo).
*   [x] **Structure Check**: `/contracts/events/` vs `/Contracts/Events`? **PASSED**: Constitution Amended (v1.1.0).
*   [x] **Tech Stack Check**: .NET 10 vs .NET 8? **PASSED**: Constitution Amended (v1.1.0).
*   [x] **Serialization Check**: Schema-First vs Code-First? **PASSED**: Constitution Amended (v1.1.0).
*   [x] **Schema Version**: Draft-07 vs 2020-12? **PASSED**: Constitution Amended (v1.1.0).


## Project Structure

### Documentation (this feature)

```text
specs/001-messaging-contracts/
├── plan.md
├── research.md
├── contracts/           # Proposed Schema definitions
└── tasks.md
```

### Source Code (repository root)

**Proposed Structure (User Input):**

```text
/contracts
    /events
    /commands
    /requests
    /responses
    /shared
/asyncapi
/topology
/generated
    /csharp
/tests
README.md
CONSTITUTION.md
nuget.config
```

**Structure Decision**: Adopting the User's requested structure (Schema-First).

## Complexity Tracking

| Violation | Why Needed | Simpler Alternative Rejected Because |
|-----------|------------|-------------------------------------|
| Schema-First Approach | User Requirement ("contract-first source of truth", "Generate ... models") | Code-First doesn't support the requested strictly decoupled contract evolution or diverse client generation as easily. |
| .NET 10 | User Requirement | .NET 8 is LTS but user specifically requested .NET 10. |
| JSON Schema Draft-07 | User Requirement | Constitution mandates 2020-12, but user explicitly asked for Draft-07 (likely for tool compatibility). |