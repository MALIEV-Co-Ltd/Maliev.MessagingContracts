# Data Model: Messaging Contracts

This document outlines the key entities involved in the messaging contract system. The source of truth for the structure of these entities is the set of JSON Schemas defined in the `contracts/schemas/` directory.

## Core Entities

### 1. Service

-   **Description**: A deployable, autonomous microservice within the Maliev ecosystem. Each service has defined domain responsibilities and is the authoritative owner of a specific set of data.
-   **Attributes**:
    -   `ServiceName` (string): A unique, machine-readable identifier for the service (e.g., "OrderService", "PaymentService").
-   **Relationships**:
    -   Publishes zero or more **Messaging Contracts**.
    -   Consumes zero or more **Messaging Contracts**.

### 2. Messaging Contract

-   **Description**: A formal definition of a message exchanged between services. It represents a single, well-defined unit of communication. Every message circulating in the system MUST conform to a defined contract.
-   **Sub-Types**:
    -   **Command**: Directs a specific service to perform a state-changing action.
    -   **Event**: Notifies the system that a significant state change has occurred.
    -   **Request/Response**: A pair of contracts for query-based interactions.
-   **Relationships**:
    -   Is defined by exactly one **Message Schema**.
    -   Has exactly one publishing **Service**.
    -   Has one or more consuming **Services**.

### 3. Message Schema

-   **Description**: The concrete data structure and validation rules for a **Messaging Contract**. This is the non-negotiable "hard schema" that defines the shape of the data on the wire.
-   **Implementation**: Defined as a JSON Schema (Draft-07) file.
-   **Attributes (Mandatory Metadata)**:
    -   `messageId` (GUID): A unique identifier for the specific message instance.
    -   `messageName` (string): The canonical, PascalCase name of the contract (e.g., "CreateOrderCommand").
    -   `messageType` (enum): The type of contract (`Command`, `Event`, `Request`, `Response`).
    -   `messageVersion` (string): The semantic version of the schema (e.g., "1.0").
    -   `publishedBy` (string): The `ServiceName` of the publishing service.
    -   `consumedBy` (array of string): An explicit list of `ServiceName`s for the intended consumers.
    -   `correlationId` (GUID): A GUID used to trace a flow or conversation across multiple messages.
    -   `causationId` (GUID): The `messageId` of the message that triggered this message, if applicable.
    -   `occurredAtUtc` (string): The ISO-8601 timestamp of when the message was created.
    -   `isPublic` (boolean): A flag indicating if the message is part of the public, cross-service contract surface.
-   **Attributes (Payload)**:
    -   The payload is a flat, explicit DTO defined within the schema. It contains the business-specific data for the message.
    -   **Rules**:
        -   No inheritance or polymorphism.
        -   No nullable primitive types unless explicitly justified.
        -   All identifiers MUST be GUIDs.
        -   All monetary values MUST be objects containing `amount` and `currency` fields.
        -   All dates MUST be in UTC format.
