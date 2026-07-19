# Contract: JobStartedEvent

**Domain**: Jobs
**Type**: Event
**Publisher**: Maliev.JobService
**Consumers**: Maliev.InventoryService
**Status**: Draft

## Overview

Published when a production job transitions to `InProgress` status. Enables InventoryService to automatically deduct material stock based on estimated consumption.

## JSON Schema Definition

```json
{
  "$schema": "http://json-schema.org/draft-07/schema#",
  "$id": "maliev/job-events",
  "title": "Jobs Domain Events",
  "description": "Events published by the Job Service",
  "definitions": {
    "JobStartedEvent": {
      "type": "object",
      "description": "Published when a production job transitions to InProgress status",
      "properties": {
        "jobId": {
          "type": "string",
          "format": "uuid",
          "description": "Unique job identifier"
        },
        "orderId": {
          "type": "string",
          "format": "uuid",
          "description": "Parent order identifier"
        },
        "materialId": {
          "type": "string",
          "format": "uuid",
          "description": "Material being used (FK to MaterialService)"
        },
        "volumeCm3": {
          "type": "number",
          "description": "Part volume in cm³ (used for material consumption estimate)"
        },
        "technology": {
          "type": "string",
          "description": "Manufacturing technology: Fdm, Sla, Cnc, Scanning, or Design"
        },
        "assignedMachineId": {
          "type": "string",
          "description": "Machine assigned (e.g., PRUSA-01, HAAS-VF2)"
        },
        "startedAt": {
          "type": "string",
          "format": "date-time",
          "description": "UTC timestamp when job started"
        }
      },
      "required": [
        "jobId",
        "orderId",
        "materialId",
        "volumeCm3",
        "technology",
        "assignedMachineId",
        "startedAt"
      ]
    }
  }
}
```

## C# Record

```csharp
namespace Maliev.MessagingContracts.Contracts.Jobs;

public record JobStartedEvent
{
    public Guid JobId { get; init; }
    public Guid OrderId { get; init; }
    public Guid MaterialId { get; init; }
    public decimal VolumeCm3 { get; init; }
    public string Technology { get; init; } = string.Empty;
    public string AssignedMachineId { get; init; } = string.Empty;
    public DateTime StartedAt { get; init; }
}
```

## Technology Values

| Value | Description |
|-------|-------------|
| Fdm | Fused Deposition Modeling (3D printing) |
| Sla | Stereolithography (3D printing) |
| Cnc | Computer Numerical Control (machining) |
| Scanning | 3D scanning services |
| Design | Design-only work (no material consumption) |

## AsyncAPI Channel

```yaml
job/started:
  address: maliev.jobs.events.job-started
  messages:
    JobStartedEvent:
      $ref: '#/components/messages/JobStartedEvent'
```

## Consumer Responsibilities

- InventoryService: Deduct material stock based on `volumeCm3`
- Handle unknown technology values gracefully (extendable enum)
- Correlate with `orderId` for order tracking
