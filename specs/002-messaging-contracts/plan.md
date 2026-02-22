# Implementation Plan: New Messaging Contracts

**Branch**: `002-messaging-contracts` | **Date**: 2026-02-21 | **Spec**: [spec.md](./spec.md)
**Input**: Feature specification from `/specs/002-messaging-contracts/spec.md`

## Summary

Extend `FileAnalyzedEvent` with optional `DfmReport` (DFM analysis data), create new `JobStartedEvent` (Jobs domain), and create new `MaterialLowStockEvent` (Inventory domain). All changes follow schema-first approach with JSON Schema Draft-07 definitions and corresponding C# record generation.

## Technical Context

**Language/Version**: C# 13 / .NET 10.0
**Primary Dependencies**: System.Text.Json (source-generated serialization)
**Storage**: N/A (contracts only, no persistence)
**Testing**: xUnit
**Target Platform**: NuGet package (cross-platform)
**Project Type**: Library (shared contracts)
**Performance Goals**: N/A (data contracts)
**Constraints**: TreatWarningsAsErrors enforced, zero external runtime dependencies
**Scale/Scope**: 3 contract changes (1 extension, 2 new events)

## Constitution Check

*GATE: Must pass before Phase 0 research. Re-check after Phase 1 design.*

| Rule | Status | Notes |
|------|--------|-------|
| Schema-First (Section III) | ✅ PASS | JSON Schema is source of truth |
| No Domain Logic (Section I) | ✅ PASS | Contracts only, no behavior |
| System.Text.Json Serialization (Section III) | ✅ PASS | Generated records use STJ |
| camelCase (schema) / PascalCase (C#) (Section III) | ✅ PASS | Convention followed |
| No Forbidden Dependencies (Section IV) | ✅ PASS | No MassTransit, EF Core, etc. |
| Backward Compatibility (Section II) | ✅ PASS | `dfmReport` is optional (additive change) |
| AsyncAPI Registration (Section VI) | ✅ PASS | New events will be registered |

**Gate Status**: ✅ ALL PASSED

## Project Structure

### Documentation (this feature)

```text
specs/002-messaging-contracts/
├── plan.md              # This file
├── research.md          # Phase 0 output
├── data-model.md        # Phase 1 output
├── quickstart.md        # Phase 1 output
├── contracts/           # Phase 1 output - contract definitions
│   ├── dfm-report.md
│   ├── job-started-event.md
│   └── material-low-stock-event.md
└── tasks.md             # Phase 2 output (created by /speckit.tasks)
```

### Source Code (repository root)

```text
contracts/schemas/
├── geometry/
│   └── geometry-events.json      # MODIFY: Add dfmReport to FileAnalyzedEvent
├── jobs/                          # CREATE: New domain
│   └── job-events.json
└── inventory/                     # CREATE: New domain
    └── inventory-events.json

generated/csharp/
├── Maliev.MessagingContracts.csproj
├── Contracts/
│   ├── FileAnalyzedEvent.cs      # MODIFY: Add DfmReport property
│   ├── Jobs/                      # CREATE: New folder
│   │   └── JobStartedEvent.cs
│   └── Inventory/                 # CREATE: New folder
│       └── MaterialLowStockEvent.cs
└── ...

asyncapi/
└── asyncapi.yaml                  # MODIFY: Add new channels/messages
```

**Structure Decision**: Schema-first approach. JSON Schemas in `contracts/schemas/` are the source of truth. C# records in `generated/csharp/` must match schemas exactly.

## Complexity Tracking

No violations to justify. All changes are additive and follow existing patterns.

## Implementation Phases

### Phase 1: Extend FileAnalyzedEvent (Geometry Domain)

**Files to modify:**
- `contracts/schemas/geometry/geometry-events.json`
- `generated/csharp/Contracts/FileAnalyzedEvent.cs`

**Changes:**
1. Add `dfmReport` property (type: `["object", "null"]`) to `FileAnalyzedEvent`
2. Define nested `DfmReport` object with: `thinWallCount`, `thinWallRegions`, `overhangFaceCount`, `overhangAreaCm2`
3. Do NOT add `dfmReport` to `required` array (optional field)

### Phase 2: Add JobStartedEvent (Jobs Domain)

**Files to create:**
- `contracts/schemas/jobs/job-events.json` (new domain folder)
- `generated/csharp/Contracts/Jobs/JobStartedEvent.cs` (new folder)
- Update `asyncapi/asyncapi.yaml` with new channel

**Fields:** `jobId`, `orderId`, `materialId`, `volumeCm3`, `technology`, `assignedMachineId`, `startedAt`

### Phase 3: Add MaterialLowStockEvent (Inventory Domain)

**Files to create:**
- `contracts/schemas/inventory/inventory-events.json` (new domain folder)
- `generated/csharp/Contracts/Inventory/MaterialLowStockEvent.cs` (new folder)
- Update `asyncapi/asyncapi.yaml` with new channel

**Fields:** `materialId`, `batchId`, `remainingWeightGrams`, `lowStockThresholdGrams`, `occurredAt`

### Phase 4: Verify & Version

1. Run `npm test` - validate JSON schemas
2. Run `dotnet build` - must compile with zero warnings
3. Run `dotnet test` - all serialization tests must pass
4. Bump patch version in `.csproj`
