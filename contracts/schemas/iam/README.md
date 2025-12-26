# IAM Service Events

Event contracts for the Maliev Identity and Access Management (IAM) Service.

## Events

### RoleUpdatedEvent
**Channel**: `maliev.iam.events.role-updated`
**Published When**: A role's permissions are modified (permissions added or removed)
**Consumed By**: Services that cache permission information

**Use Cases**:
- Invalidate permission caches when role permissions change
- Trigger policy re-evaluation in dependent services
- Audit trail for compliance

**Payload**:
```json
{
  "roleId": "storage-service/admin",
  "serviceName": "storage-service",
  "updatedAt": "2025-12-20T10:30:00Z"
}
```

---

### PrincipalRoleGrantedEvent
**Channel**: `maliev.iam.events.principal-role-granted`
**Published When**: A role is granted to a principal (service account)
**Consumed By**: Services that maintain permission caches, audit services

**Use Cases**:
- Update principal permission caches
- Trigger access provisioning in downstream services
- Audit logging and compliance tracking
- Send notifications about permission changes

**Payload**:
```json
{
  "bindingId": "550e8400-e29b-41d4-a716-446655440000",
  "principalId": "123e4567-e89b-12d3-a456-426614174000",
  "roleId": "storage-service/reader",
  "resourceType": "bucket",
  "resourceId": "production-data",
  "grantedAt": "2025-12-20T10:30:00Z",
  "expiresAt": "2026-12-20T10:30:00Z"
}
```

**Notes**:
- `resourceType` and `resourceId` are optional - used for resource-scoped permissions
- `expiresAt` is optional - if present, the role binding will expire at that time

---

### PrincipalRoleRevokedEvent
**Channel**: `maliev.iam.events.principal-role-revoked`
**Published When**: A role is revoked from a principal (service account)
**Consumed By**: Services that maintain permission caches, audit services

**Use Cases**:
- Invalidate principal permission caches
- Trigger access revocation in downstream services
- Audit logging and compliance tracking
- Send notifications about permission changes

**Payload**:
```json
{
  "bindingId": "550e8400-e29b-41d4-a716-446655440000",
  "principalId": "123e4567-e89b-12d3-a456-426614174000",
  "roleId": "storage-service/reader",
  "revokedAt": "2025-12-20T10:30:00Z"
}
```

---

### PermissionRegisteredEvent
**Channel**: `maliev.iam.events.permission-registered`
**Published When**: A new permission is registered by a service
**Consumed By**: Admin dashboards, permission catalog services

**Use Cases**:
- Update permission catalogs
- Notify administrators of new permissions
- Track permission expansion across services

**Payload**:
```json
{
  "permissionId": "storage-service/bucket.read",
  "serviceName": "storage-service",
  "resourceType": "bucket",
  "action": "read",
  "registeredAt": "2025-12-20T10:30:00Z"
}
```

---

## Integration Examples

### Cache Invalidation on Role Update

```csharp
using Maliev.MessagingContracts.Contracts;
using MassTransit;

public class RoleUpdatedConsumer : IConsumer<RoleUpdatedEvent>
{
    private readonly ICacheService _cache;

    public async Task Consume(ConsumeContext<RoleUpdatedEvent> context)
    {
        var evt = context.Message;

        // Invalidate all permission caches affected by this role
        await _cache.RemoveByPrefixAsync($"permissions:role:{evt.Payload.RoleId}");
        await _cache.RemoveByPrefixAsync($"principals:role:{evt.Payload.RoleId}");

        _logger.LogInformation(
            "Invalidated caches for role {RoleId} updated at {UpdatedAt}",
            evt.Payload.RoleId,
            evt.Payload.UpdatedAt);
    }
}
```

### Permission Cache Update on Grant/Revoke

```csharp
public class PrincipalRoleGrantedConsumer : IConsumer<PrincipalRoleGrantedEvent>
{
    private readonly ICacheService _cache;

    public async Task Consume(ConsumeContext<PrincipalRoleGrantedEvent> context)
    {
        var evt = context.Message;

        // Invalidate principal's permission cache
        await _cache.RemoveAsync($"permissions:principal:{evt.Payload.PrincipalId}");

        // If resource-scoped, invalidate specific resource permissions
        if (!string.IsNullOrEmpty(evt.Payload.ResourceType))
        {
            await _cache.RemoveAsync(
                $"permissions:principal:{evt.Payload.PrincipalId}:" +
                $"{evt.Payload.ResourceType}:{evt.Payload.ResourceId}");
        }
    }
}
```

## Schema Validation

All schemas inherit from `base-message.json` and include standard metadata fields:
- `messageId`: Unique message instance identifier
- `messageName`: Contract name (e.g., "RoleUpdatedEvent")
- `messageType`: Always "Event"
- `messageVersion`: Schema version (e.g., "1.0")
- `publishedBy`: "iam-service"
- `consumedBy`: Array of consuming service identifiers
- `correlationId`: For tracing
- `occurredAtUtc`: Event timestamp
- `isPublic`: Whether event is part of public contract surface

## Publishing Service

- **Service**: `iam-service`
- **Repository**: [Maliev.IAMService](https://github.com/MALIEV-Co-Ltd/Maliev.IAMService)
- **Publisher**: IAM Service publishes these events via RabbitMQ/MassTransit

## Version History

| Version | Date       | Changes                           |
|---------|------------|-----------------------------------|
| 1.0     | 2025-12-20 | Initial IAM event contracts added |
