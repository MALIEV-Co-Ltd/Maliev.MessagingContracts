using System;

namespace Maliev.MessagingContracts.Contracts.Customers;

/// <summary>
/// Event published when an NDA is approaching expiration.
/// </summary>
public record NdaExpiringEvent
{
    public Guid NdaId { get; init; }
    public Guid CustomerId { get; init; }
    public DateTime ExpiresAt { get; init; }
    public int DaysUntilExpiration { get; init; }
    public DateTime WarningGeneratedAt { get; init; } = DateTime.UtcNow;
}

/// <summary>
/// Event published when an NDA has expired.
/// </summary>
public record NdaExpiredEvent
{
    public Guid NdaId { get; init; }
    public Guid CustomerId { get; init; }
    public DateTime ExpiredAt { get; init; }
    public DateTime ProcessedAt { get; init; } = DateTime.UtcNow;
}
