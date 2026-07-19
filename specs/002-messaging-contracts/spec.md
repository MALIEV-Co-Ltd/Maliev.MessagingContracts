# Feature Specification: New Messaging Contracts

**Feature Branch**: `002-messaging-contracts`  
**Created**: 2026-02-21  
**Status**: Draft  
**Input**: User description: "Three new or updated contracts: Extend FileAnalyzedEvent with DFM report, add JobStartedEvent, add MaterialLowStockEvent"

## User Scenarios & Testing *(mandatory)*

### User Story 1 - Geometry Service Publishes DFM Analysis (Priority: P1)

As a geometry service, when I complete file analysis, I can optionally include DFM (Design for Manufacturing) analysis results so downstream services can make informed decisions about manufacturability.

**Why this priority**: This extends an existing contract with optional data, allowing backward compatibility while providing enhanced manufacturing intelligence.

**Independent Test**: Can be tested by publishing a FileAnalyzedEvent with and without dfmReport data. Consumers must handle both cases correctly.

**Acceptance Scenarios**:

1. **Given** a 3D file analysis completes successfully, **When** DFM analysis runs successfully, **Then** the event includes dfmReport with thin wall and overhang data
2. **Given** a 3D file analysis completes, **When** DFM analysis fails or is not applicable, **Then** dfmReport is null
3. **Given** a consumer receives FileAnalyzedEvent, **When** dfmReport is present, **Then** thinWallRegions contains valid [x,y,z] coordinate arrays

---

### User Story 2 - Job Service Publishes Job Started Event (Priority: P1)

As a job service, when a production job transitions to InProgress status, I publish an event so the inventory service can automatically deduct material stock.

**Why this priority**: This enables real-time inventory tracking and material consumption, which is critical for accurate stock management.

**Independent Test**: Can be tested by creating a job that transitions to InProgress and verifying the event is published with correct job and material details.

**Acceptance Scenarios**:

1. **Given** a job transitions to InProgress, **When** the event is published, **Then** it includes jobId, orderId, materialId, volumeCm3, technology, assignedMachineId, and startedAt
2. **Given** the inventory service receives JobStartedEvent, **When** processing the event, **Then** material stock is deducted based on volumeCm3

---

### User Story 3 - Inventory Service Alerts on Low Stock (Priority: P2)

As an inventory service, when remaining stock for a batch drops below threshold, I publish an event so the notification service can alert employees to restock.

**Why this priority**: This enables proactive inventory management, preventing production delays due to material shortages.

**Independent Test**: Can be tested by consuming material until threshold is crossed and verifying the alert event is published.

**Acceptance Scenarios**:

1. **Given** material stock is consumed, **When** remaining weight drops below threshold, **Then** MaterialLowStockEvent is published with materialId, batchId, remainingWeightGrams, lowStockThresholdGrams, and occurredAt
2. **Given** the notification service receives MaterialLowStockEvent, **When** processing the event, **Then** employees are notified to restock

---

### Edge Cases

- What happens when DFM analysis partially succeeds (some data available)? → dfmReport is still null; all-or-nothing approach for data integrity
- What happens when a job is cancelled mid-production? → Not in scope for this contract; a separate JobCancelledEvent would handle this
- What happens when multiple batches of the same material trigger low stock simultaneously? → Each batch publishes its own event; consumers must deduplicate if needed

## Requirements *(mandatory)*

### Functional Requirements

- **FR-001**: FileAnalyzedEvent MUST include an optional dfmReport property that can be null
- **FR-002**: DfmReport MUST include thinWallCount (integer), thinWallRegions (array of [x,y,z] coordinates), overhangFaceCount (integer), and overhangAreaCm2 (decimal)
- **FR-003**: JobStartedEvent MUST be created with jobId, orderId, materialId, volumeCm3, technology, assignedMachineId, and startedAt fields
- **FR-004**: MaterialLowStockEvent MUST be created with materialId, batchId, remainingWeightGrams, lowStockThresholdGrams, and occurredAt fields
- **FR-005**: All contracts MUST follow the existing JSON Schema Draft-07 format and generate corresponding C# records
- **FR-006**: All new contracts MUST be registered in the AsyncAPI topology

### Key Entities

- **DfmReport**: Contains thin wall detection results (count and region centroids) and overhang analysis (face count and projected area). All measurements use millimeters for coordinates and cm² for area.
- **JobStartedEvent**: Represents a production job starting, linking to order, material, and assigned machine. Technology is limited to: Fdm, Sla, Cnc, Scanning, Design.
- **MaterialLowStockEvent**: Represents an inventory alert triggered when remaining material weight falls below configured threshold for a specific batch.

## Success Criteria *(mandatory)*

### Measurable Outcomes

- **SC-001**: JSON schemas validate successfully with `npm test`
- **SC-002**: C# code generates successfully with `./scripts/build.ps1`
- **SC-003**: All C# tests pass with `dotnet test`
- **SC-004**: No build warnings (TreatWarningsAsErrors enforced)
- **SC-005**: Generated C# records serialize and deserialize correctly with System.Text.Json

## Assumptions

- Technology values are limited to the five specified options (Fdm, Sla, Cnc, Scanning, Design)
- All UUIDs are valid GUIDs in standard format
- All timestamps are UTC and in ISO-8601 format
- DFM thresholds (0.8mm thin wall, 45° overhang) are enforced in GeometryService, not in the contract
- Version bump is patch-level only (backward-compatible additions)
