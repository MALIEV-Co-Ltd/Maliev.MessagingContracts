# Contract: MaterialLowStockEvent

**Domain**: Inventory
**Type**: Event
**Publisher**: Maliev.InventoryService
**Consumers**: Maliev.NotificationService
**Status**: Draft

## Overview

Published when remaining stock for a specific batch drops below the configured threshold. Triggers employee notifications to restock material.

## JSON Schema Definition

```json
{
  "$schema": "http://json-schema.org/draft-07/schema#",
  "$id": "maliev/inventory-events",
  "title": "Inventory Domain Events",
  "description": "Events published by the Inventory Service",
  "definitions": {
    "MaterialLowStockEvent": {
      "type": "object",
      "description": "Published when remaining stock for a batch drops below threshold",
      "properties": {
        "materialId": {
          "type": "string",
          "format": "uuid",
          "description": "Material ID (FK to MaterialService)"
        },
        "batchId": {
          "type": "string",
          "format": "uuid",
          "description": "The specific batch that triggered the alert"
        },
        "remainingWeightGrams": {
          "type": "number",
          "description": "Current remaining weight in grams"
        },
        "lowStockThresholdGrams": {
          "type": "number",
          "description": "Configured alert threshold in grams"
        },
        "occurredAt": {
          "type": "string",
          "format": "date-time",
          "description": "UTC timestamp of the alert"
        }
      },
      "required": [
        "materialId",
        "batchId",
        "remainingWeightGrams",
        "lowStockThresholdGrams",
        "occurredAt"
      ]
    }
  }
}
```

## C# Record

```csharp
namespace Maliev.MessagingContracts.Contracts.Inventory;

public record MaterialLowStockEvent
{
    public Guid MaterialId { get; init; }
    public Guid BatchId { get; init; }
    public decimal RemainingWeightGrams { get; init; }
    public decimal LowStockThresholdGrams { get; init; }
    public DateTime OccurredAt { get; init; }
}
```

## Trigger Condition

Event is published when:
```
remainingWeightGrams < lowStockThresholdGrams
```

## AsyncAPI Channel

```yaml
inventory/material-low-stock:
  address: maliev.inventory.events.material-low-stock
  messages:
    MaterialLowStockEvent:
      $ref: '#/components/messages/MaterialLowStockEvent'
```

## Consumer Responsibilities

- NotificationService: Alert employees to restock the material batch
- Deduplicate alerts if multiple batches of same material trigger simultaneously
- Include material name and location in notification (requires lookup via materialId)

## Edge Cases

| Scenario | Behavior |
|----------|----------|
| Multiple batches low stock | Each batch publishes separate event |
| Stock replenished then drops again | New event published |
| Threshold configuration changed | Re-evaluated on next stock change |
