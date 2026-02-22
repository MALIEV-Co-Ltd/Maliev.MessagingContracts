# Data Model: New Messaging Contracts

**Feature**: 002-messaging-contracts
**Date**: 2026-02-21

## Entity Overview

| Entity | Type | Domain | Description |
|--------|------|--------|-------------|
| DfmReport | Nested Object | Geometry | DFM analysis results embedded in FileAnalyzedEvent |
| JobStartedEvent | Event | Jobs | Published when production job starts |
| MaterialLowStockEvent | Event | Inventory | Published when material stock drops below threshold |

---

## Entity: DfmReport

**Parent**: FileAnalyzedEvent (embedded, optional)

### Fields

| Field | JSON Type | C# Type | Required | Description |
|-------|-----------|---------|----------|-------------|
| thinWallCount | integer | int | Yes | Number of detected thin-wall regions |
| thinWallRegions | array | List&lt;decimal[]&gt; | Yes | Centroid coordinates [x,y,z] in mm for each region |
| overhangFaceCount | integer | int | Yes | Number of mesh faces with overhang angle > 45° |
| overhangAreaCm2 | number | decimal | Yes | Total projected area of overhanging faces in cm² |

### Validation Rules

- `thinWallCount` >= 0
- `thinWallRegions` items must be exactly 3-element arrays [x, y, z]
- `overhangFaceCount` >= 0
- `overhangAreaCm2` >= 0

### State Transitions

N/A - Immutable data object

---

## Entity: JobStartedEvent

**Domain**: Jobs
**Publisher**: Maliev.JobService
**Consumers**: Maliev.InventoryService

### Fields

| Field | JSON Type | C# Type | Required | Description |
|-------|-----------|---------|----------|-------------|
| jobId | string (uuid) | Guid | Yes | Unique job identifier |
| orderId | string (uuid) | Guid | Yes | Parent order identifier |
| materialId | string (uuid) | Guid | Yes | Material being consumed |
| volumeCm3 | number | decimal | Yes | Part volume for material consumption estimate |
| technology | string | string | Yes | One of: Fdm, Sla, Cnc, Scanning, Design |
| assignedMachineId | string | string | Yes | Machine identifier (e.g., "PRUSA-01") |
| startedAt | string (date-time) | DateTime | Yes | UTC timestamp when job started |

### Validation Rules

- All UUID fields must be valid GUIDs
- `volumeCm3` > 0
- `technology` must be one of: Fdm, Sla, Cnc, Scanning, Design (enforced by service)
- `startedAt` must be valid ISO-8601 datetime
- `assignedMachineId` non-empty string

### Relationships

- `orderId` → Order (in OrderService)
- `materialId` → Material (in MaterialService)

---

## Entity: MaterialLowStockEvent

**Domain**: Inventory
**Publisher**: Maliev.InventoryService
**Consumers**: Maliev.NotificationService

### Fields

| Field | JSON Type | C# Type | Required | Description |
|-------|-----------|---------|----------|-------------|
| materialId | string (uuid) | Guid | Yes | Material identifier |
| batchId | string (uuid) | Guid | Yes | Specific batch triggering alert |
| remainingWeightGrams | number | decimal | Yes | Current remaining weight in grams |
| lowStockThresholdGrams | number | decimal | Yes | Configured alert threshold in grams |
| occurredAt | string (date-time) | DateTime | Yes | UTC timestamp of the alert |

### Validation Rules

- All UUID fields must be valid GUIDs
- `remainingWeightGrams` >= 0
- `lowStockThresholdGrams` > 0
- `remainingWeightGrams` < `lowStockThresholdGrams` (trigger condition)
- `occurredAt` must be valid ISO-8601 datetime

### Relationships

- `materialId` → Material (in MaterialService)
- `batchId` → MaterialBatch (in InventoryService)

---

## Schema-to-C# Type Mapping

| JSON Schema Type | C# Type | Notes |
|------------------|---------|-------|
| string (uuid) | Guid | Parsed from string representation |
| string (date-time) | DateTime | ISO-8601 format |
| number | decimal | High precision for measurements |
| integer | int | Standard 32-bit integer |
| string | string | UTF-8 encoded |
| array of arrays | List&lt;decimal[]&gt; | Jagged array for coordinates |
