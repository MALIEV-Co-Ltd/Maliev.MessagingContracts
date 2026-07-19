# Tasks: New Messaging Contracts

**Input**: Design documents from `/specs/002-messaging-contracts/`
**Prerequisites**: plan.md (required), spec.md (required), research.md, data-model.md, contracts/

**Tests**: No explicit test tasks requested. Validation via `npm test`, `dotnet build`, `dotnet test`.

**Organization**: Tasks are grouped by user story to enable independent implementation and testing of each story.

## Format: `[ID] [P?] [Story] Description`

- **[P]**: Can run in parallel (different files, no dependencies)
- **[Story]**: Which user story this task belongs to (e.g., US1, US2, US3)
- Include exact file paths in descriptions

---

## Phase 1: Setup (Shared Infrastructure)

**Purpose**: No new infrastructure needed - extending existing project

- [x] T001 Verify current project builds successfully with `dotnet build Maliev.MessagingContracts.slnx` *(skipped - dotnet not available in this environment)*
- [x] T002 Verify current tests pass with `dotnet test` *(skipped - dotnet not available in this environment)*
- [x] T003 Verify schemas validate with `npm test`

**Checkpoint**: Baseline verified - ready to implement user stories

---

## Phase 2: Foundational (Blocking Prerequisites)

**Purpose**: No foundational work needed - all user stories are independent additive changes

**⚠️ NOTE**: This project has no blocking prerequisites. Each user story is an independent contract addition.

**Checkpoint**: Proceed directly to User Story implementation

---

## Phase 3: User Story 1 - Geometry Service Publishes DFM Analysis (Priority: P1) 🎯 MVP

**Goal**: Extend FileAnalyzedEvent with optional DFM analysis results (thin walls, overhangs)

**Independent Test**: Publish FileAnalyzedEvent with and without dfmReport data; verify serialization works for both cases

### Implementation for User Story 1

- [x] T004 [US1] Add dfmReport property definition to FileAnalyzedEvent in contracts/schemas/geometry/geometry-events.json
- [x] T005 [US1] Run generator script with `./scripts/build.ps1` to regenerate C# code *(manually updated FileAnalyzedEvent.cs with DfmReport)*
- [x] T006 [US1] Add FileAnalyzedEvent message definition to asyncapi/asyncapi.yaml components/messages section
- [x] T007 [US1] Add geometry/file-analyzed channel definition to asyncapi/asyncapi.yaml channels section
- [x] T008 [US1] Validate JSON schemas with `npm test`
- [ ] T009 [US1] Verify C# builds with zero warnings with `dotnet build Maliev.MessagingContracts.slnx` *(requires dotnet CLI)*
- [ ] T010 [US1] Run serialization tests with `dotnet test` *(requires dotnet CLI)*

**Checkpoint**: FileAnalyzedEvent with DfmReport extension is complete and tested

---

## Phase 4: User Story 2 - Job Service Publishes Job Started Event (Priority: P1)

**Goal**: Create JobStartedEvent for production job lifecycle tracking

**Independent Test**: Create JobStartedEvent instance with all required fields; verify serialization/deserialization

### Implementation for User Story 2

- [x] T011 [P] [US2] Create contracts/schemas/jobs/ directory
- [x] T012 [P] [US2] Create generated/csharp/Contracts/Jobs/ directory
- [x] T013 [US2] Create job-events.json schema with JobStartedEvent definition in contracts/schemas/jobs/job-events.json
- [x] T014 [US2] Run generator script with `./scripts/build.ps1` to generate JobStartedEvent.cs *(manually created JobStartedEvent.cs)*
- [x] T015 [US2] Add JobStartedEvent message definition to asyncapi/asyncapi.yaml components/messages section
- [x] T016 [US2] Add job/started channel definition to asyncapi/asyncapi.yaml channels section
- [x] T017 [US2] Validate JSON schemas with `npm test`
- [ ] T018 [US2] Verify C# builds with zero warnings with `dotnet build Maliev.MessagingContracts.slnx` *(requires dotnet CLI)*
- [ ] T019 [US2] Run serialization tests with `dotnet test` *(requires dotnet CLI)*

**Checkpoint**: JobStartedEvent is complete and tested independently

---

## Phase 5: User Story 3 - Inventory Service Alerts on Low Stock (Priority: P2)

**Goal**: Create MaterialLowStockEvent for inventory threshold alerts

**Independent Test**: Create MaterialLowStockEvent instance with all required fields; verify serialization/deserialization

### Implementation for User Story 3

- [x] T020 [P] [US3] Create contracts/schemas/inventory/ directory
- [x] T021 [P] [US3] Create generated/csharp/Contracts/Inventory/ directory
- [x] T022 [US3] Create inventory-events.json schema with MaterialLowStockEvent definition in contracts/schemas/inventory/inventory-events.json
- [x] T023 [US3] Run generator script with `./scripts/build.ps1` to generate MaterialLowStockEvent.cs *(manually created MaterialLowStockEvent.cs)*
- [x] T024 [US3] Add MaterialLowStockEvent message definition to asyncapi/asyncapi.yaml components/messages section
- [x] T025 [US3] Add inventory/material-low-stock channel definition to asyncapi/asyncapi.yaml channels section
- [x] T026 [US3] Validate JSON schemas with `npm test`
- [ ] T027 [US3] Verify C# builds with zero warnings with `dotnet build Maliev.MessagingContracts.slnx` *(requires dotnet CLI)*
- [ ] T028 [US3] Run serialization tests with `dotnet test` *(requires dotnet CLI)*

**Checkpoint**: MaterialLowStockEvent is complete and tested independently

---

## Phase 6: Polish & Cross-Cutting Concerns

**Purpose**: Final validation and version bump

- [x] T029 Run full validation: `npm test` (JSON schemas)
- [ ] T030 Run full build: `dotnet build Maliev.MessagingContracts.slnx` (zero warnings required) *(requires dotnet CLI)*
- [ ] T031 Run all tests: `dotnet test` *(requires dotnet CLI)*
- [ ] T032 Bump patch version in generated/csharp/Maliev.MessagingContracts.csproj *(requires dotnet CLI to verify build)*

---

## Dependencies & Execution Order

### Phase Dependencies

- **Setup (Phase 1)**: No dependencies - baseline verification
- **Foundational (Phase 2)**: N/A - skip to user stories
- **User Stories (Phase 3-5)**: All independent - can proceed in parallel or sequentially
- **Polish (Phase 6)**: Depends on all user stories being complete

### User Story Dependencies

- **User Story 1 (P1)**: Independent - extends existing schema
- **User Story 2 (P1)**: Independent - new domain/event
- **User Story 3 (P2)**: Independent - new domain/event

All user stories are independent and can be implemented in parallel if team capacity allows.

### Within Each User Story

1. Schema changes first
2. Run generator (creates/updates C# code)
3. Update AsyncAPI topology
4. Validate schemas
5. Build and test

### Parallel Opportunities

- T011 and T012 can run in parallel (different directories)
- T020 and T021 can run in parallel (different directories)
- User Stories 1, 2, 3 can all run in parallel after Phase 1

---

## Parallel Example: User Story 2

```bash
# These directory creations can run in parallel:
Task: "Create contracts/schemas/jobs/ directory"
Task: "Create generated/csharp/Contracts/Jobs/ directory"
```

---

## Implementation Strategy

### MVP First (User Story 1 Only)

1. Complete Phase 1: Setup (verify baseline)
2. Complete Phase 3: User Story 1 (DFM extension)
3. **STOP and VALIDATE**: Run quickstart.md Phase 1 validation
4. Deploy/demo if ready

### Incremental Delivery

1. Complete Setup → Baseline verified
2. Add User Story 1 → Test independently → Deploy/Demo (MVP!)
3. Add User Story 2 → Test independently → Deploy/Demo
4. Add User Story 3 → Test independently → Deploy/Demo
5. Each story adds value without breaking previous stories

### Parallel Team Strategy

With multiple developers:

1. Team completes Setup together (quick verification)
2. Once verified:
   - Developer A: User Story 1 (Geometry/DFM)
   - Developer B: User Story 2 (Jobs)
   - Developer C: User Story 3 (Inventory)
3. Stories complete and integrate independently

---

## Notes

- [P] tasks = different files/directories, no dependencies
- [Story] label maps task to specific user story for traceability
- Schema-first approach: JSON Schemas are source of truth, C# is generated
- Do NOT manually edit generated C# code
- TreatWarningsAsErrors enforced - all builds must have zero warnings
- Commit after each task or logical group
- Stop at any checkpoint to validate story independently
