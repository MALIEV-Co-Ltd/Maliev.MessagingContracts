# Feature Specification: Messaging Contracts Repository Setup

**Feature Branch**: `001-messaging-contracts`
**Created**: 2024-12-12
**Status**: Draft
**Input**: User description: "Create a new repository named 'Maliev.MessagingContracts' that serves as the authoritative, contract-first source of truth for all message types used across Maliev’s RabbitMQ-based .NET 10 microservices..."

## Clarifications

### Session 2024-12-12
- Q: Source of Truth (Code-First vs Schema-First)? → A: **Schema-First (JSON/YAML is Source)**. Developers write JSON Schemas/AsyncAPI; C# models are generated.
- Q: NuGet Package Registry Target? → A: **GitHub Packages**. Packages are published to the repository's feed using `GITOPS_PAT`.
- Q: Envelope Tracing Standard? → A: **W3C TraceContext**. Use standard `traceparent` headers.

## User Scenarios & Testing *(mandatory)*

### User Story 1 - Define and Distribute Contracts (Priority: P1)

As a Backend Developer, I want to define message contracts (Commands, Events, DTOs) **using JSON Schema (Draft-07)** and have C# models **generated automatically**, so that the schema is the single source of truth and .NET clients stay in sync.

**Why this priority**: This is the core purpose of the repository; without this, no sharing is possible.

**Independent Test**: Create a dummy `test-command.json`, run the generator, and verify a `TestCommand` record is generated in `MessagingContracts.cs`.

**Acceptance Scenarios**:

1. **Given** a new JSON Schema `create-user-command.json` in the `schemas/commands` folder, **When** the generator runs, **Then** a C# record `CreateUserCommand` is generated in `MessagingContracts.cs`.
2. **Given** the generated C# model, **When** it is serialized to JSON, **Then** it matches the source schema structure exactly (using `camelCase` naming policy).
3. **Given** a schema change that removes a required field, **When** CI runs, **Then** the build fails (Future requirement: Breaking change detection).

---

### User Story 2 - Automated Documentation & Topology (Priority: P2)

As a System Architect, I want to define the messaging topology using **AsyncAPI** (referencing the JSON Schemas) so that human-readable documentation and routing configurations are standardized and version-controlled.

**Why this priority**: Ensures interoperability and "living documentation" driven by the source files.

**Independent Test**: Update `asyncapi.yaml` with a new channel, run the doc generator (`npm run gen-docs`), and verify the HTML output includes the new channel.

---

### User Story 3 - LLM-Driven Evolution Support (Priority: P3)

As an AI Assistant (or Dev using AI), I want a clear, consistent file structure (separate JSON/YAML files) so that I can reliably parse and modify contracts.

**Why this priority**: Supports the long-term goal of "LLM-driven contract evolution."

## Requirements *(mandatory)*

### Functional Requirements

- **FR-001**: System MUST provide base `record` types or interfaces for `BaseMessage` in the generated C# code.
- **FR-002**: System MUST define a standard `BaseMessage` containing Metadata (`MessageId`, `CorrelationId`, `OccurredAtUtc`, etc.).
- **FR-003**: Repository MUST target .NET 10 and enforce strict type-safety in generated models.
- **FR-004**: System SHOULD use consistent naming conventions that map JSON `camelCase` to C# `PascalCase`.
- **FR-005**: Build process MUST generate C# `record` models from the source JSON Schemas (Draft-07) using a custom generator.
- **FR-006**: Repository MUST contain AsyncAPI specifications serving as the source of truth for the RabbitMQ topology, referencing the payload schemas.
- **FR-007**: CI pipeline SHOULD detect breaking changes in the JSON Schemas.
- **FR-008**: Repository MUST NOT contain any domain logic, database code, or infrastructure dependencies (pure contracts only).
- **FR-009**: The CI pipeline MUST publish the generated NuGet package to **GitHub Packages**.

### Key Entities

- **Schema**: A JSON file defining the structure of a message (Command/Event).
- **AsyncAPI Spec**: A YAML document defining the messaging topology and referencing Schemas.
- **Contract (Generated)**: A C# `record` generated from a Schema.
- **Envelope**: A standard wrapper schema defined once and referenced by all messages.
- **Metadata**: Contextual data including W3C TraceContext `traceparent`, `tracestate`, `CorrelationId`, CausedById, CreatedAt.

## Success Criteria *(mandatory)*

### Measurable Outcomes

- **SC-001**: 100% of generated C# models have serialization round-trip parity with their source JSON Schema.
- **SC-002**: CI pipeline detects a breaking schema change (field removal) and fails the build in under 5 minutes.
- **SC-003**: NuGet package is successfully generated and published to **GitHub Packages** containing the C# models.
- **SC-004**: Generated documentation covers 100% of defined messages and channels.