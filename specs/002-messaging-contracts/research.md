# Research: New Messaging Contracts

**Feature**: 002-messaging-contracts
**Date**: 2026-02-21

## Research Summary

No unresolved technical questions. All decisions follow existing repository patterns.

## Decisions

### D1: Schema Structure for DfmReport

**Decision**: Define `DfmReport` as an inline object property within `FileAnalyzedEvent` schema definition

**Rationale**: 
- Follows existing pattern in `geometry-events.json` where `metrics` is an inline nested object
- No reuse across other events, so no need for shared definition
- Keeps schema self-contained

**Alternatives Considered**:
- Separate schema file: Rejected - DfmReport is specific to FileAnalyzedEvent
- Shared definitions section: Rejected - adds complexity without benefit

### D2: Technology Field Validation

**Decision**: String field without JSON Schema enum validation; validation enforced by JobService

**Rationale**:
- Existing contracts in this repo use strings for similar fields (e.g., `assignedMachineId`)
- Enum values may evolve; service-layer validation provides flexibility
- C# code will use string type matching existing patterns

**Alternatives Considered**:
- JSON Schema `enum`: Rejected - would require schema version bump for new technology types

### D3: Coordinate Array Representation

**Decision**: Use `{"type": "array", "items": {"type": "array", "items": {"type": "number"}, "minItems": 3, "maxItems": 3}}` for thinWallRegions

**Rationale**:
- Matches spec requirement for [x,y,z] coordinate tuples
- C# will use `List<decimal[]>` as specified
- Consistent with 3D coordinate representation in geometry domain

### D4: Namespace Structure for New Domains

**Decision**: Create new namespace folders under `generated/csharp/Contracts/` for Jobs and Inventory

**Rationale**:
- Follows existing pattern (Geometry, Orders, Materials, etc.)
- Domain separation improves discoverability
- Namespace format: `Maliev.MessagingContracts.Contracts.{Domain}`

### D5: AsyncAPI Channel Naming

**Decision**: Follow existing pattern `maliev.{domain}.events.{event-name}`

**Rationale**:
- Consistent with existing 111 events in asyncapi.yaml
- Examples: `maliev.jobs.events.job-started`, `maliev.inventory.events.material-low-stock`

## Validation Commands

| Command | Purpose | Expected Result |
|---------|---------|-----------------|
| `npm test` | Validate JSON Schema syntax | Pass |
| `dotnet build` | Compile C# with TreatWarningsAsErrors | 0 errors, 0 warnings |
| `dotnet test` | Run serialization tests | All pass |

## No Open Questions

All technical decisions resolved based on:
1. Existing repository patterns
2. Constitution rules (schema-first, System.Text.Json)
3. User specification requirements
