using System;

namespace Maliev.MessagingContracts.Contracts.Pricing;

/// <summary>
/// Event published when a price calculation has been performed and audited.
/// </summary>
public record PriceCalculatedEvent
{
    public Guid PricingAuditId { get; init; }
    public Guid? QuotationId { get; init; }
    public Guid FileId { get; init; }
    public Guid CustomerId { get; init; }
    public Guid MaterialId { get; init; }
    public Guid ProcessId { get; init; }
    public int Quantity { get; init; }

    // Geometry metrics snapshot
    public decimal InputVolumeCm3 { get; init; }
    public decimal InputSupportVolumeCm3 { get; init; }
    public decimal InputSurfaceAreaCm2 { get; init; }

    // Strategy and configuration
    public string Strategy { get; init; } = string.Empty;
    public string? MLModelVersion { get; init; }
    public decimal ConfidenceLevel { get; init; }
    public Guid PricingConfigurationId { get; init; }

    // Financial details
    public PriceBreakdownContract Breakdown { get; init; } = new();
    public decimal TotalUnitPrice { get; init; }
    public decimal TotalPrice { get; init; }
    public string Currency { get; init; } = "THB";

    public DateTime ValidUntil { get; init; }
    public DateTime CalculatedAt { get; init; }
}

public record PriceBreakdownContract
{
    public decimal MaterialCost { get; init; }
    public decimal SupportCost { get; init; }
    public decimal MachineTimeCost { get; init; }
    public decimal SetupCost { get; init; }
    public decimal ComplexitySurcharge { get; init; }
    public decimal SubtotalBeforeMargin { get; init; }
    public decimal MarginAmount { get; init; }
    public decimal TotalPrice { get; init; }
}
