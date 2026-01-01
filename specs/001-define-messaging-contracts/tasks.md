# Tasks for Define and Enforce Messaging Contracts

This document breaks down the implementation plan into a series of actionable, ordered, and measurable tasks.

## Phase 1: Setup

-   [x] T001 Create a PowerShell script for service discovery in `scripts/discover-services.ps1`.
-   [x] T002 Create a markdown file for the discovery report at `specs/001-define-messaging-contracts/discovery-report.md`.
-   [x] T003 Create a markdown file for the communication inventory at `specs/001-define-messaging-contracts/implicit-communications.md`.

## Phase 2: Foundational - Discovery & Inventory (US1)

This phase corresponds to User Story 1: Discover and Analyze Existing Services.

**Independent Test Criteria**: The `discovery-report.md` and `implicit-communications.md` files must be populated with a complete and accurate analysis of all in-scope microservices.

-   [x] T004 [US1] Implement the service discovery script (`scripts/discover-services.ps1`) to scan the `../` directory and populate `discovery-report.md` with all included and excluded services.
-   [x] T005 [P] [US1] For each service in `discovery-report.md`, analyze its source code for incoming HTTP endpoints and document them in `implicit-communications.md`.
-   [x] T006 [P] [US1] For each service in `discovery-report.md`, analyze its source code for outgoing `HttpClient` calls and document them in `implicit-communications.md`.
-   [x] T007 [P] [US1] For each service in `discovery-report.md`, analyze its business logic to infer and document domain events that should be published in `implicit-communications.md`.
-   [x] T008 [US1] Review the completed `implicit-communications.md` and classify each documented interaction as `Command`, `Event`, or `Request/Response`.
-   [x] T009 [US1] Create a `synchronous-justification.md` report documenting the justification for any remaining synchronous HTTP calls.

## Phase 3: Contract Definition (US2)

This phase corresponds to User Story 2: Define Explicit Messaging Contracts.

**Independent Test Criteria**: The `contracts/schemas/` directory must contain a valid JSON Schema file for each interaction identified in the previous phase.

-   [x] T010 [US2] Create the base directory structure `contracts/schemas/`.
-   [x] T011 [US2] Define a base JSON Schema template in `contracts/schemas/shared/base-message.json` that enforces the hard-contract schema from the plan (messageId, messageType, etc.).
-   [x] T012 [P] [US2] For each `Command` identified, create a corresponding JSON schema file in `contracts/schemas/{domain}/{command-name}.json`.
-   [x] T013 [P] [US2] For each `Event` identified, create a corresponding JSON schema file in `contracts/schemas/{domain}/{event-name}.json`.
-   [x] T014 [P] [US2] For each `Request/Response` pair identified, create the corresponding JSON schema files in `contracts/schemas/{domain}/`.
-   [x] T015 [US2] For every schema, explicitly define `publishedBy` (single service) and `consumedBy` (array of services).

## Phase 4: Implementation & Tooling (US3)

This phase corresponds to User Story 3: Generate and Centralize Contracts.

**Independent Test Criteria**: The `Maliev.MessagingContracts` NuGet package can be built successfully, and it contains generated C# records for all defined JSON schemas.

-   [x] T016 [US3] Set up a schema generation tool (e.g., NJsonSchema) in the project, configured to read from `contracts/schemas/` and output to `generated/csharp/Contracts/`.
-   [x] T017 [US3] Implement the generation process, ensuring C# records are created in the correct namespaces corresponding to their domain.
-   [x] T018 [US3] Configure the `Maliev.MessagingContracts.csproj` file to include the generated files and produce a NuGet package.
-   [x] T019 [US3] Update the `quickstart.md` with a complete, working example of consuming a generated contract.

## Phase 5: Validation & Enforcement (US3)

This phase enhances User Story 3 by adding the crucial enforcement and validation mechanisms.

**Independent Test Criteria**: The CI pipeline must fail if any of the defined validation rules are violated.

-   [x] T020 [US3] Implement a CI step in `.github/workflows/validate-contracts.yaml` to validate all schema files against the `base-message.json` template.
-   [x] T021 [US3] Implement a CI step in `.github/workflows/validate-contracts.yaml` that fails if any message in `consumedBy` does not have a corresponding consumer implementation (placeholder check).
-   [x] T022 [US3] Implement a schema diff tool in the CI pipeline in `.github/workflows/validate-contracts.yaml` to detect breaking changes and enforce versioning rules.
-   [x] T023 [US3] Write a verification test in `tests/ValidationTests.cs` that ensures no contract has an empty `consumedBy` array.

## Phase 6: Integration Rules (US3)

This phase addresses the documentation of integration rules.

-   [x] T024 [US3] Document standard policies for retry, timeout, poison message, and dead-lettering in `specs/001-define-messaging-contracts/standard-policies.md`.

## Dependencies

-   **US1 (Discovery)** must be completed before **US2 (Definition)**.
-   **US2 (Definition)** must be completed before **US3 (Implementation & Validation)**.
-   The phases within each User Story should be completed in the order presented.

## Parallel Execution

-   Within **Phase 2 (US1)**, the analysis of each microservice (T005, T006, T007) can be performed in parallel.
-   Within **Phase 3 (US2)**, the creation of individual schema files (T011, T012, T013) can be performed in parallel once the base template is complete.

## Implementation Strategy

The implementation will follow the user stories in priority order, ensuring that a solid foundation of discovery and analysis is complete before contracts are defined and generated. This constitutes an MVP-first approach where the "product" is the set of contracts, delivered incrementally based on the analysis.
1.  **MVP**: Complete User Story 1 to have a full picture of the system.
2.  **V2**: Complete User Story 2 to have the full set of contracts defined as schemas.
3.  **V3**: Complete User Story 3 (Phases 4 & 5) to have a consumable, versioned, and validated NuGet package.