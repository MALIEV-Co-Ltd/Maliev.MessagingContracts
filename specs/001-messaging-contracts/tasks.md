---
description: "Task list template for feature implementation"
---

# Tasks: Messaging Contracts Repository Setup

**Input**: Design documents from `/specs/001-messaging-contracts/`
**Prerequisites**: plan.md (required), spec.md (required for user stories), research.md, data-model.md

**Organization**: Tasks are grouped by User Story to enable independent implementation and testing.

## Format: `[ID] [P?] [Story] Description`

- **[P]**: Can run in parallel (different files, no dependencies)
- **[Story]**: [US1], [US2], [US3]
- Include exact file paths in descriptions

## Phase 1: Setup (Shared Infrastructure)

**Purpose**: Project initialization and basic structure.

- [x] T001 Create repository structure with folders: `contracts/schemas/{events,commands,shared}`, `asyncapi`, `topology`, `generated/csharp`, `tests`
- [x] T002 Initialize .NET 10 Class Library project `Maliev.MessagingContracts` in `generated/csharp/Maliev.MessagingContracts.csproj`
- [x] T003 Install `System.Text.Json` and `JsonSchema.Net` dependencies in `generated/csharp/Maliev.MessagingContracts.csproj`
- [x] T004 [P] Initialize Node.js project for tooling in `package.json` (root) and install `@asyncapi/cli`

## Phase 2: Foundational (Blocking Prerequisites)

**Purpose**: Core schemas and generation tooling.

- [x] T005 Create `contracts/schemas/shared/envelope.json` schema (from data-model.md)
- [x] T006 [P] Create base schemas: `contracts/schemas/shared/domain-event.json`, `integration-event.json`, `command.json`
- [x] T007 Configure NJsonSchema generation script/config (e.g. `njsonschema.json`) to target `System.Text.Json`, `record` types, `PascalCase`, and base interfaces (`ICommand`, `IEvent`, etc.)
- [x] T008 [P] Create `asyncapi/asyncapi.yaml` with basic info block

## Phase 3: User Story 1 - Define and Distribute Contracts (Priority: P1)

**Goal**: Define contracts in Schema, generate C# models, and package them.
**Independent Test**: Create a dummy schema, generate code, and verify NuGet package creation.

### Tests for User Story 1
- [x] T009 [US1] Create xUnit test project `tests/Maliev.MessagingContracts.Tests.csproj`
- [x] T010 [US1] Add `SerializationTests.cs` to verify round-trip serialization of generated models

### Implementation for User Story 1
- [x] T011 [US1] Create sample schema `contracts/schemas/commands/create-user-command.json`
- [x] T012 [US1] Run NJsonSchema generation to produce `generated/csharp/Contracts/Commands/CreateUserCommand.cs`
- [x] T013 [US1] Configure `generated/csharp/Maliev.MessagingContracts.csproj` for NuGet packaging (Metadata, License, etc.)
- [x] T014 [US1] Verify build and package creation (`dotnet pack`)

## Phase 4: User Story 2 - Automated Documentation & Topology (Priority: P2)

**Goal**: Generate AsyncAPI docs and define RabbitMQ topology.
**Independent Test**: Generate HTML docs from AsyncAPI file.

### Tests for User Story 2
- [x] T015 [US2] Add test/script to verify `asyncapi.yaml` is valid using `@asyncapi/cli`

### Implementation for User Story 2
- [x] T016 [US2] Update `asyncapi/asyncapi.yaml` to reference `create-user-command.json`
- [x] T017 [US2] Create npm script `gen-docs` to run `asyncapi generate fromTemplate ...`
- [x] T018 [US2] Create `topology/definitions.yaml` with sample exchange/queue definitions (custom format)

## Phase 5: User Story 3 - LLM-Driven Evolution Support (Priority: P3)

**Goal**: programmatic access and clear structure.

### Implementation for User Story 3
- [x] T019 [US3] Create `scripts/scan_contracts.ps1` to recursively list all JSON schemas and map them to expected C# files
- [x] T020 [US3] Verify folder structure aligns with Constitution rules (Validation)

## Phase 6: Polish & Cross-Cutting Concerns

- [x] T021 [P] Setup GitHub Actions workflow `.github/workflows/validate-contracts.yaml` (Schema/AsyncAPI validation)
- [x] T022 [P] Setup GitHub Actions workflow `.github/workflows/publish.yaml` (NuGet Pack & Push)
- [x] T023 Update `README.md` with "How to add a new contract" guide (referencing Quickstart)

## Dependencies & Execution Order

- **Setup (Phase 1)**: Must run first.
- **Foundational (Phase 2)**: Depends on Phase 1. Blocks all User Stories.
- **US1 (Phase 3)**: Depends on Phase 2.
- **US2 (Phase 4)**: Depends on Phase 2 (and US1 for sample data).
- **US3 (Phase 5)**: Depends on Phase 2.

## Parallel Opportunities

- T004 (Node setup) can run with T002/T003 (.NET setup).
- T006 (Base schemas) can run with T005 (Envelope).
- T015 (AsyncAPI test) can run with T016 (AsyncAPI update).
- T021/T022 (CI/CD) can run anytime after Phase 2.
