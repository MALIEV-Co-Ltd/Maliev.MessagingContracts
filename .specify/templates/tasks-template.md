---
description: "Task list template for feature implementation"
---

# Tasks: [FEATURE NAME]

**Input**: Design documents from `/specs/[###-feature-name]/`
**Prerequisites**: plan.md (required), spec.md (required for user stories), contracts/

**Organization**: Tasks are grouped by Contract Category (Commands, Events, DTOs) or User Story if applicable.

## Format: `[ID] [P?] [Category] Description`

- **[P]**: Can run in parallel (different files, no dependencies)
- **[Category]**: Command, Event, DTO, Test, Release
- Include exact file paths in descriptions

## Phase 1: Setup & Definition (Proposed Contracts)

**Purpose**: Define the C# records and DTOs.

- [ ] T001 [P] [DTO] Create/Update DTOs in `Contracts/Dtos/`
- [ ] T002 [P] [Command] Create new Commands in `Contracts/Commands/`
- [ ] T003 [P] [Event] Create new Events in `Contracts/Events/`
- [ ] T004 [P] [Error] Define Error Codes in `Contracts/Errors/`

## Phase 2: Serialization Verification (Mandatory)

**Purpose**: Ensure strict JSON compatibility and round-tripping.

- [ ] T005 [P] [Test] Add Round-trip test for new DTOs in `tests/Dtos/`
- [ ] T006 [P] [Test] Add Round-trip test for new Commands in `tests/Commands/`
- [ ] T007 [P] [Test] Add Round-trip test for new Events in `tests/Events/`
- [ ] T008 [Test] specific: Verify `PascalCase` C# props serialize to `camelCase` JSON

## Phase 3: Schema & Documentation (Optional)

- [ ] T009 [Schema] Generate/Update JSON Schema in `/Schema`
- [ ] T010 [Docs] Update README or inline documentation if logic is complex

## Phase 4: Release Prep

**Purpose**: Versioning and Packaging.

- [ ] T011 [Version] Bump package version in `.csproj` (Check Constitution for Major/Minor rules)
- [ ] T012 [CI] Verify `dotnet pack` produces correct artifacts locally
- [ ] T013 [Audit] Check for forbidden dependencies (MassTransit, MediatR, etc.)

## Implementation Strategy

1.  **Define DTOs first**: They are the building blocks.
2.  **Define Commands/Events**: Use DTOs.
3.  **Write Tests**: Verify serialization *immediately* after defining.
4.  **Review**: Ensure no "Logic" crept in.

## Notes

- All types must be `record`.
- No `Newtonsoft.Json`. Use `System.Text.Json`.
- No Integration Tests required.