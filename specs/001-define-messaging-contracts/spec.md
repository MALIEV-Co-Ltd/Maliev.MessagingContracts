# Feature Specification: Define and Enforce Messaging Contracts

**Feature Branch**: `001-define-messaging-contracts`
**Created**: 2025-12-13
**Status**: Draft
**Input**: User description: "Create a new specification that enforces system-wide discovery, definition, and enforcement of inter-service messaging contracts across the entire Maliev microservices ecosystem. The specification MUST instruct the LLM to move up one directory level from the current repository and recursively analyze all service repositories (e.g. OrderService, QuotationService, AccountingService, PaymentService, UploadService, AuthService, etc.). For each service, the LLM MUST: - Identify the service’s core responsibilities and data ownership. - Analyze existing HTTP APIs, application logic, and any MassTransit or RabbitMQ configuration (even if currently unused). - Detect implicit cross-service workflows, synchronous service-to-service calls, and coordination logic currently implemented via APIs or shared assumptions. Based on this analysis, the LLM MUST: - Identify all missing or implicit inter-service communications that should be explicitly modeled using MassTransit and RabbitMQ. - Decide which interactions must be modeled as Commands, Events, or Request/Response messages. - Explicitly define message ownership (publisher) and consumers for every interaction. - Justify any remaining synchronous HTTP communication. The specification MUST require the LLM to produce concrete, implementable messaging contracts and add them to the Maliev.MessagingContracts repository, including: - Strongly defined message schemas (C# record definitions and/or JSON Schemas) - Clear versioning rules - Correlation and idempotency requirements - Retry and failure semantics suitable for MassTransit The specification MUST enforce that all decisions are final and executable: - No “suggested” or “optional” messaging patterns - No vague or hypothetical consumers - No undocumented message flows The output of this specification MUST represent the authoritative milestone that transitions the Maliev platform from stubbed messaging to a fully defined, event- and command-driven microservices communication architecture using MassTransit and RabbitMQ."

## User Problem

The current microservices architecture relies on implicit, undocumented, and often synchronous communication patterns between services. This leads to tight coupling, poor resilience, and difficulty in understanding and evolving the system as a whole. There is no single source of truth for inter-service communication, making it impossible to guarantee system-wide consistency.

## User Scenarios & Testing *(mandatory)*

### User Story 1 - Discover and Analyze Existing Services (Priority: P1)

As a system architect, I need the agent to automatically scan all microservice repositories to create a complete inventory of service responsibilities, data ownership, and current communication methods.

**Why this priority**: This initial analysis is the foundation for all subsequent work. Without a complete and accurate understanding of the current state, it's impossible to define the correct messaging contracts.

**Independent Test**: The process can be tested by running it against a subset of services and verifying that the output accurately documents their APIs, data models, and any observable interactions. The value is a clear map of the existing system's topology.

**Acceptance Scenarios**:

1.  **Given** a list of service repositories, **When** the analysis process is run, **Then** the system produces a report detailing each service's core functions and public-facing HTTP APIs.
2.  **Given** a service with existing (but unused) MassTransit or RabbitMQ configuration, **When** the analysis is run, **Then** the report includes details of this configuration.

---

### User Story 2 - Define Explicit Messaging Contracts (Priority: P2)

As a system architect, I need the agent to analyze the discovered communication patterns and define explicit, formal messaging contracts (Commands, Events, Request/Response) for all inter-service workflows.

**Why this priority**: This step translates the implicit system behavior into a formal, documented architecture. It is the core of the transition to an event-driven model.

**Independent Test**: This can be tested by providing the output of the analysis phase for a specific workflow (e.g., "order placement") and verifying that the agent produces a logical set of Command and Event contracts to model that workflow.

**Acceptance Scenarios**:

1.  **Given** an implicit workflow where OrderService calls PaymentService via HTTP, **When** the contract definition process is run, **Then** the system defines a `ProcessPaymentCommand` and corresponding `PaymentProcessedEvent` (or `PaymentFailedEvent`).
2.  **Given** a defined contract, **When** it is generated, **Then** it has a clearly identified publisher and at least one intended consumer.

---

### User Story 3 - Generate and Centralize Contracts (Priority: P3)

As a system architect, I need the agent to generate concrete C# code for all defined contracts and commit them to the central `Maliev.MessagingContracts` repository.

**Why this priority**: This makes the defined architecture tangible and consumable by developers. It creates the single source of truth.

**Independent Test**: This can be tested by providing a set of defined contracts and verifying that the agent generates compilable C# records in the correct repository structure.

**Acceptance Scenarios**:

1.  **Given** a set of defined message contracts, **When** the generation process is run, **Then** valid C# record files appear in the `Maliev.MessagingContracts/generated/csharp/Contracts` directory.
2.  **Given** a generated C# contract, **When** it is inspected, **Then** it includes properties for correlation (`CorrelationId`) and follows established versioning patterns.

---

### Edge Cases

-   How are truly synchronous, blocking workflows (which cannot be made asynchronous) handled and documented?

## Requirements *(mandatory)*

### Functional Requirements

-   **FR-001**: The system MUST recursively scan all specified service repositories from a shared parent directory.
-   **FR-002**: For each service, the system MUST identify its core domain responsibilities and primary data ownership.
-   **FR-003**: The system MUST analyze source code to find existing HTTP API endpoints (e.g., ASP.NET Core controllers) and any message bus configurations (MassTransit/RabbitMQ).
-   **FR-004**: The system MUST detect and document implicit cross-service workflows, such as direct synchronous `HttpClient` calls between services.
-   **FR-005**: The system MUST model all identified inter-service interactions as explicit Commands, Events, or Request/Response messages.
-   **FR-006**: The system MUST define a single, authoritative publisher and one or more intended consumers for every message contract.
-   **FR-007**: Any communication that must remain synchronous (HTTP) MUST be explicitly justified with a written rationale.
-   **FR-008**: The system MUST generate strongly-typed message contracts as C# `record` types.
-   **FR-009**: The system MUST add all generated contract files to the `Maliev.MessagingContracts` repository.
-   **FR-010**: All generated contracts MUST include fields for versioning, correlation (`CorrelationId`), and support for idempotency checks (`MessageId`).
-   **FR-011**: The specification for each contract MUST consider retry and failure semantics suitable for implementation in MassTransit (e.g., outbox/inbox patterns, dead-lettering).
-   **FR-012**: The final output MUST be a complete, executable set of contracts, with no "suggested" or "optional" patterns left for interpretation.
-   **FR-013**: When identifying data ownership, the system MUST prioritize the service whose core responsibility aligns with the data entity as the explicit single owner.
-   **FR-014**: The system MUST log informational messages detailing major progress steps, significant findings, and key decisions made during its operation.

### Key Entities *(include if feature involves data)*

-   **Service**: A microservice within the Maliev ecosystem (e.g., OrderService, PaymentService).
-   **Messaging Contract**: A formal, language-agnostic definition of a message exchanged between services. It has a defined owner/publisher.
    -   **Command**: A message directing a specific service to perform an action. Typically has one consumer.
    -   **Event**: A message notifying the system that something of business significance has occurred. Can have multiple consumers.
    -   **Request/Response**: A pair of messages used to query a service for information in an asynchronous-friendly way.
-   **Message Schema**: The concrete data structure of a message, to be implemented as a C# record or JSON Schema.

## Success Criteria *(mandatory)*

### Measurable Outcomes

-   **SC-001**: 100% of inter-service workflows identified during the analysis phase are mapped to an explicit message contract in the `Maliev.MessagingContracts` repository.
-   **SC-002**: The `Maliev.MessagingContracts` repository is established as the single source of truth for all asynchronous inter-service communication schemas.
-   **SC-003**: A developer can determine the complete, authoritative communication flow between any two services by only consulting the `Maliev.MessagingContracts` repository and the services' explicit subscription configurations.
-   **SC-004**: The number of direct, synchronous HTTP `POST`, `PUT`, or `DELETE` calls between services is reduced to zero, except for cases with a documented justification.
-   **SC-005**: All generated C# contracts successfully compile without errors or warnings.

## Assumptions

-   The agent has local filesystem read access to all specified service repositories.
-   The services are co-located in directories at the same level (e.g., `../Maliev.OrderService`, `../Maliev.PaymentService`).
-   The primary goal is to establish an event-driven architecture, favoring asynchronous communication patterns wherever possible.
-   MassTransit over RabbitMQ is the designated technology stack for messaging.

## Clarifications

### Session 2025-12-13

- Q: How should the agent behave if a service repository is temporarily unavailable during its analysis scan? → A: If a service's code repository is inaccessible (e.g., not found on the local filesystem), the agent should report this unavailability and continue with the analysis of other services.
- Q: If two services appear to have conflicting ownership of the same data entity, what should be the agent's default conflict resolution strategy? → A: The service whose core responsibility aligns with the data entity (e.g., 'Customer Service' for 'Customer' data) is the explicit single owner. Other services must interact with this owner for modifications.
- Q: What level of detail should the agent log during its operation? → A: Informational (progress and key decisions).
- Q: What is the assumed access method for the service repositories? → A: Local Filesystem Access.
- Q: Are there any performance targets for the analysis process, such as a maximum time allowed per service? → A: No explicit target; prioritize accuracy and thoroughness over speed as the process is expected to run infrequently.