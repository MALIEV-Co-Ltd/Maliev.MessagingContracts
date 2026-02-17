using System;

namespace Maliev.MessagingContracts.Contracts.Delivery;

/// <summary>
/// Event published when a delivery note is created.
/// </summary>
public record DeliveryNoteCreatedEvent
{
    /// <summary>The delivery note identifier.</summary>
    public string DeliveryNoteId { get; init; } = null!;

    /// <summary>The related order identifier.</summary>
    public string? OrderId { get; init; }

    /// <summary>The related purchase order identifier.</summary>
    public int? PurchaseOrderId { get; init; }

    /// <summary>The customer identifier.</summary>
    public Guid CustomerId { get; init; }

    /// <summary>Scheduled delivery date.</summary>
    public DateTime DeliveryDate { get; init; }

    /// <summary>Number of items in the delivery note.</summary>
    public int ItemCount { get; init; }

    /// <summary>Timestamp when the delivery note was created.</summary>
    public DateTime CreatedAt { get; init; } = DateTime.UtcNow;

    /// <summary>User who created the delivery note.</summary>
    public string CreatedBy { get; init; } = null!;
}

/// <summary>
/// Event published when delivery status changes.
/// </summary>
public record DeliveryStatusChangedEvent
{
    /// <summary>The delivery note identifier.</summary>
    public string DeliveryNoteId { get; init; } = null!;

    /// <summary>The related order identifier.</summary>
    public string? OrderId { get; init; }

    /// <summary>Previous delivery status.</summary>
    public string PreviousStatus { get; init; } = null!;

    /// <summary>New delivery status.</summary>
    public string NewStatus { get; init; } = null!;

    /// <summary>Actual delivery timestamp (if delivered).</summary>
    public DateTime? ActualDeliveryTime { get; init; }

    /// <summary>Name of person who received the delivery.</summary>
    public string? ReceivedByName { get; init; }

    /// <summary>Timestamp when status was changed.</summary>
    public DateTime ChangedAt { get; init; } = DateTime.UtcNow;

    /// <summary>User who changed the status.</summary>
    public string ChangedBy { get; init; } = null!;
}

/// <summary>
/// Event published when delivery is completed (all items delivered).
/// </summary>
public record DeliveryCompletedEvent
{
    /// <summary>The delivery note identifier.</summary>
    public string DeliveryNoteId { get; init; } = null!;

    /// <summary>The related order identifier.</summary>
    public string? OrderId { get; init; }

    /// <summary>The related purchase order identifier.</summary>
    public int? PurchaseOrderId { get; init; }

    /// <summary>Timestamp when delivery was completed.</summary>
    public DateTime CompletedAt { get; init; } = DateTime.UtcNow;

    /// <summary>Name of person who received the delivery.</summary>
    public string ReceivedByName { get; init; } = null!;
}

/// <summary>
/// Event published when a delivery note PDF is requested.
/// </summary>
public record DeliveryNotePdfRequestedEvent
{
    /// <summary>The delivery note identifier.</summary>
    public string DeliveryNoteId { get; init; } = null!;

    /// <summary>User who requested the PDF generation.</summary>
    public string RequestedBy { get; init; } = null!;

    /// <summary>Timestamp when PDF was requested.</summary>
    public DateTime RequestedAt { get; init; } = DateTime.UtcNow;
}
