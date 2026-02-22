using System;
using System.Text.Json.Serialization;

namespace Maliev.MessagingContracts.Contracts.Inventory;

/// <summary>
/// Published when remaining stock for a batch drops below threshold.
/// </summary>
public record MaterialLowStockEvent
{
    [JsonPropertyName("materialId")]
    public Guid MaterialId { get; init; }

    [JsonPropertyName("batchId")]
    public Guid BatchId { get; init; }

    [JsonPropertyName("remainingWeightGrams")]
    public decimal RemainingWeightGrams { get; init; }

    [JsonPropertyName("lowStockThresholdGrams")]
    public decimal LowStockThresholdGrams { get; init; }

    [JsonPropertyName("occurredAt")]
    public DateTime OccurredAt { get; init; }

    public MaterialLowStockEvent() { }

    public MaterialLowStockEvent(
        Guid materialId,
        Guid batchId,
        decimal remainingWeightGrams,
        decimal lowStockThresholdGrams,
        DateTime occurredAt)
    {
        MaterialId = materialId;
        BatchId = batchId;
        RemainingWeightGrams = remainingWeightGrams;
        LowStockThresholdGrams = lowStockThresholdGrams;
        OccurredAt = occurredAt;
    }
}
