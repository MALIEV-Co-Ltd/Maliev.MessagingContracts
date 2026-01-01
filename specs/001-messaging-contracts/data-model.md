# Data Model: Messaging Contracts

## Overview

The system uses a **Schema-First** approach. The primary entities are JSON Schemas and AsyncAPI definitions. C# models are derivative.

## Key Entities

### 1. Message Envelope (`shared/envelope.json`)

Wraps all payloads to ensure standard metadata tracking.

| Field | Type | Required | Description |
|---|---|---|---|
| `messageId` | UUID | Yes | Unique ID of the message. |
| `correlationId` | UUID | Yes | ID for tracing workflows across services. |
| `timestamp` | Date-Time | Yes | ISO 8601 creation time. |
| `source` | String | Yes | Name of the publishing service (e.g., "Maliev.OrderService"). |
| `type` | String | Yes | The fully qualified message type (e.g., "Maliev.OrderService.Events.OrderCreated"). |
| `version` | String | Yes | Version of the schema (e.g., "1.0.0"). |
| `data` | Object | Yes | The actual payload (Command/Event/DTO). |

### 2. Domain Event (`events/domain-event.schema.json`)

Base schema for domain events.

| Field | Type | Description |
|---|---|---|
| `aggregateId` | String | ID of the aggregate root. |
| `occurredOn` | Date-Time | When the business event happened. |

### 3. Integration Event (`events/integration-event.schema.json`)

Base schema for cross-boundary integration events.

| Field | Type | Description |
|---|---|---|
| `eventData` | Object | Arbitrary payload specific to the event. |

### 4. Command (`commands/command.schema.json`)

Base schema for RPC or Fire-and-Forget commands.

| Field | Type | Description |
|---|---|---|
| `replyTo` | String (Queue) | Optional queue for responses. |

## Directory Structure Strategy

```text
contracts/
  schemas/
    events/
      order-created.v1.json
    commands/
      create-order.v1.json
    shared/
      envelope.v1.json
      address.v1.json
asyncapi/
  asyncapi.yaml
topology/
  definitions.yaml
```
