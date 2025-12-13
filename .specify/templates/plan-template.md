# Implementation Plan: [FEATURE]

**Branch**: `[###-feature-name]` | **Date**: [DATE] | **Spec**: [link]
**Input**: Feature specification from `/specs/[###-feature-name]/spec.md`

**Note**: This template is filled in by the `/speckit.plan` command. See `.specify/templates/commands/plan.md` for the execution workflow.

## Summary

[Extract from feature spec: primary requirement + technical approach from research]

## Technical Context

**Language/Version**: C# / .NET 8.0 (Strictly enforced by Constitution)
**Primary Dependencies**: System.Text.Json ONLY (No external deps like MassTransit/MediatR)
**Packaging**: NuGet (Maliev.MessagingContracts)
**Testing**: Serialization Round-trip Tests (xUnit)
**Target Platform**: netstandard2.1;net8.0

## Constitution Check

*GATE: Must pass before Phase 0 research. Re-check after Phase 1 design.*

*   [ ] **Scope Check**: Does this feature add Domain Logic? (If YES -> STOP. Forbidden.)
*   [ ] **Scope Check**: Does this feature add Infrastructure/DB code? (If YES -> STOP. Forbidden.)
*   [ ] **Compatibility Check**: Is this a Breaking Change? (Renaming/Removing fields?)
    *   If YES: Requires MAJOR version bump + RFC.
    *   If NO: Proceed as MINOR (additive).
*   [ ] **Serialization Check**: Are all new types `record`?
*   [ ] **Serialization Check**: Are all properties PascalCase?

## Project Structure

### Documentation (this feature)

```text
specs/[###-feature]/
├── plan.md              # This file
├── research.md          # Phase 0 output
├── contracts/           # Phase 1 output (Proposed C# files)
└── tasks.md             # Phase 2 output
```

### Source Code (repository root)

**Strictly enforced by Constitution:**

```text
/Contracts
    /Commands          # Action-invoking requests
    /Events            # State changes
    /Dtos              # Shared data objects
    /Errors            # Structured error codes
/Schema                # (Optional) JSON Schema draft 2020-12
/tests                 # Serialization round-trip tests
```

**Structure Decision**: [Confirm adherence to the above structure for this feature]

## Complexity Tracking

> **Fill ONLY if Constitution Check has violations that must be justified**

| Violation | Why Needed | Simpler Alternative Rejected Because |
|-----------|------------|-------------------------------------|
| [e.g. Need Custom Converter] | [Legacy system format] | [Standard JSON attributes insufficient] |