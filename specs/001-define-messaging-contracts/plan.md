# Implementation Plan: Define and Enforce Messaging Contracts

**Feature Spec**: [spec.md](./spec.md)
**Branch**: `001-define-messaging-contracts`
**Status**: In Progress

## Technical Context

-   **Objective**: To transition the Maliev microservices ecosystem from an implicit, API-driven communication model to an explicit, event-driven architecture using MassTransit and RabbitMQ. This plan outlines the steps for a full-system analysis and the subsequent definition and implementation of formal messaging contracts.
-   **Core Technologies**: .NET, C#, MassTransit, RabbitMQ, JSON Schema.
-   **Repositories Involved**: All Maliev service repositories located one level above the `Maliev.MessagingContracts` repository.
-   **Key Artifacts**: A central `Maliev.MessagingContracts` NuGet package containing all C# message contract definitions.

## Constitution Check

The project constitution has been reviewed, and this plan is in full compliance. Key alignments include:
- Adherence to the schema-first approach (JSON Schema as the source of truth).
- Use of .NET 10.
- Generation of C# models from schemas.
- Centralization of contracts in the `Maliev.MessagingContracts` repository.

## Execution Plan

### PHASE 1 — Repository & Scope Discovery
-   [ ] **Confirm Directory Layout**: Script a process to scan the `../` directory to identify all subdirectories assumed to be Maliev service repositories.
-   [ ] **Enumerate Services**: For each discovered directory, confirm it is a service repository (e.g., by presence of a `.csproj` or `sln` file). Create a definitive list of all service names.
-   [ ] **Identify Exclusions**: Manually review the enumerated list to identify and document any repositories that are infrastructure-only (e.g., build scripts, logging infrastructure) and should be excluded from the messaging analysis.
-   [ ] **Produce Scope Report**: Generate a `discovery-report.md` file listing all included and excluded services with a brief justification for each exclusion.

### PHASE 2 — Communication Inventory
-   [ ] **Analyze Each Service**: For each service in the scope report, perform the following:
    -   [ ] **Scan for Incoming APIs**: Parse source code (e.g., ASP.NET Core Controllers) to identify all `[HttpPost]`, `[HttpPut]`, and `[HttpDelete]` endpoints that imply state changes or cross-service workflows.
    -   [ ] **Scan for Outgoing Calls**: Parse source code to find all instances of `HttpClient` calls targeting other services within the ecosystem.
    -   [ ] **Infer Domain Actions**: Analyze service logic to identify domain state changes that should trigger notifications to other services (e.g., a customer's address is updated).
-   [ ] **Classify Interactions**: For each identified interaction, classify it as a **Command**, **Event**, or **Request/Response** pair. Document the source, destination, and trigger for each.
-   [ ] **Document Implicit Paths**: Create an "implicit-communications.md" report detailing all discovered communication paths that currently rely on synchronous calls or undocumented assumptions.

### PHASE 3 — Contract Definition
-   [ ] **Define Canonical Contracts**: For every interaction in the inventory, define a canonical message contract.
-   [ ] **Specify Contract Details**: For each contract, specify:
    -   **Message Name**: e.g., `CreateOrderCommand`, `OrderCreatedEvent`.
    -   **Intent**: A brief description of its purpose.
    -   **Publisher**: The single service that owns and sends the message.
    -   **Consumers**: An explicit list of services that will subscribe to the message.
    -   **Idempotency Strategy**: Define how consumers will handle duplicate messages (e.g., using a MessageId check).
-   [ ] **Define Message Scope**: Classify each message as `public` (available for any service to consume) or `internal` (scoped to a specific service or bounded context).

### PHASE 4 — HARD MESSAGING CONTRACT SCHEMA (NON-NEGOTIABLE)
-   [ ] **Implement a Base Schema Template**: Create a JSON Schema file that codifies the mandatory message structure.
-   [ ] **Enforce Metadata**: All contracts MUST include the following metadata fields:
    -   `messageId`: GUID
    -   `messageName`: string
    -   `messageType`: enum { Command, Event, Request, Response }
    -   `messageVersion`: semver string (e.g., "1.0")
    -   `publishedBy`: string (service identifier)
    -   `consumedBy`: string[]
    -   `correlationId`: GUID
    -   `causationId`: GUID or null
    -   `occurredAtUtc`: ISO-8601 timestamp
    -   `isPublic`: boolean
-   [ ] **Enforce Payload Rules**:
    -   Payloads MUST be flat DTOs.
    -   IDs MUST be GUIDs.
    -   Monetary values MUST be represented as objects with amount and currency.
    -   Dates MUST be in UTC ISO-8601 format.
-   [ ] **Enforce Behavioral Constraints in CI**:
    -   A Command MUST have exactly one consumer defined in its `consumedBy` array.
    -   An Event MUST have at least one consumer.
    -   A Request MUST have a corresponding Response contract defined.

### PHASE 5 — Repository Structure & Artifacts
-   [ ] **Define Folder Structure**: Implement the following folder structure within `Maliev.MessagingContracts`:
    -   `contracts/schemas/{domain}/{message-name}.json`
    -   `generated/csharp/Contracts/{Domain}/{MessageName}.cs`
-   [ ] **Establish Naming Conventions**:
    -   Commands: Must end with `Command`.
    -   Events: Must end with `Event`.
    -   Requests: Must end with `Request`.
    -   Responses: Must end with `Response`.
-   [ ] **Implement Schema Generation Tooling**: Set up NJsonSchema or a similar tool to generate C# records from the JSON Schema files.
-   [ ] **Implement Anti-Corruption Guards**: Add a CI step to fail if any circular dependencies between contracts are detected.

### PHASE 6 — Integration Rules
-   [ ] **Document Consumption Pattern**: Provide a `quickstart.md` guide demonstrating how a service should reference the `Maliev.MessagingContracts` NuGet package.
-   [ ] **Specify MassTransit Configuration**:
    -   Exchange naming convention MUST be the full message name (e.g., `Maliev.MessagingContracts.Orders.OrderCreatedEvent`).
    -   Queue naming convention MUST be `{service-name}:{message-name}`.
-   [ ] **Define Standard Policies**: Document standard, reusable policies for retry, timeout, and dead-lettering that all services must adopt.

### PHASE 7 — Migration Strategy
-   [ ] **Develop Migration Path**: For each synchronous HTTP call identified in Phase 2, define a step-by-step migration plan.
    1.  Introduce the new message contract.
    2.  The consumer deploys a handler for the new message.
    3.  The publisher is updated to publish the message *in addition* to the old HTTP call (dual-write).
    4.  The publisher is updated to remove the HTTP call.
-   [ ] **Prioritize Migrations**: Classify migrations as low-risk (e.g., informational events) or high-risk (e.g., core financial transactions) to guide the rollout sequence.
-   [ ] **Define Rollback Procedures**: For each migration, document a clear procedure for rolling back in case of failure (e.g., re-enabling the HTTP call via a feature flag).

### PHASE 8 — Validation & Enforcement
-   [ ] **Implement CI Checks**:
    -   [ ] Add a CI step to validate all `.json` files in `contracts/schemas/` against the base schema.
    -   [ ] Add a CI step to ensure every `publishedBy` service exists in the scope report.
    -   [ ] Add a CI step to ensure every message has at least one consumer listed in `consumedBy`.
    -   [ ] Implement a schema diff tool to detect breaking changes and enforce MAJOR version increments.
-   [ ] **Create Verification Tests**:
    -   [ ] A test to ensure no service project in the ecosystem defines its own message contracts.
    -   [ ] A test to verify that for every published message, there is a corresponding consumer configuration in at least one consuming service.
