namespace Maliev.MessagingContracts.Contracts.Orders;

using System;

/// <summary>
/// Event published when an order has been completed.
/// Extended to support pricing training data collection.
/// </summary>
public record OrderCompletedEvent
{
    /// <summary>Unique identifier for this message.</summary>
    public Guid MessageId { get; init; } = Guid.NewGuid();

    /// <summary>When the event was created.</summary>
    public DateTime Timestamp { get; init; } = DateTime.UtcNow;

    /// <summary>Source service name.</summary>
    public string Source { get; init; } = "OrderService";

    /// <summary>Order ID that was completed.</summary>
    public required Guid OrderId { get; init; }

    /// <summary>Quotation ID the order was created from.</summary>
    public required Guid QuotationId { get; init; }

    /// <summary>When the order was created.</summary>
    public required DateTime OrderCreatedAt { get; init; }

    /// <summary>When the order was completed.</summary>
    public required DateTime CompletedAt { get; init; }

    /// <summary>Whether the manufacturing job succeeded.</summary>
    public required bool JobSucceeded { get; init; }

    /// <summary>Actual material used in cubic centimeters.</summary>
    public decimal? ActualMaterialUsedCm3 { get; init; }

    /// <summary>Actual print time in hours.</summary>
    public decimal? ActualPrintTimeHours { get; init; }

    /// <summary>Actual labor hours spent.</summary>
    public decimal? ActualLaborHours { get; init; }

    /// <summary>Actual total cost of the job.</summary>
    public decimal? ActualTotalCost { get; init; }
}
