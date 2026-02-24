using System;
using System.Text.Json.Serialization;

namespace Maliev.MessagingContracts.Contracts.Jobs;

/// <summary>
/// Published when a production job status changes, for real-time Kanban updates.
/// </summary>
public record JobStatusChangedEvent
{
    [JsonPropertyName("jobId")]
    public Guid JobId { get; init; }

    [JsonPropertyName("orderId")]
    public Guid OrderId { get; init; }

    [JsonPropertyName("previousStatus")]
    public string PreviousStatus { get; init; } = string.Empty;

    [JsonPropertyName("newStatus")]
    public string NewStatus { get; init; } = string.Empty;

    [JsonPropertyName("technology")]
    public string Technology { get; init; } = string.Empty;

    [JsonPropertyName("assignedMachineId")]
    public string? AssignedMachineId { get; init; }

    [JsonPropertyName("changedAt")]
    public DateTime ChangedAt { get; init; }

    [JsonPropertyName("changedBy")]
    public string ChangedBy { get; init; } = string.Empty;

    public JobStatusChangedEvent() { }

    public JobStatusChangedEvent(
        Guid jobId,
        Guid orderId,
        string previousStatus,
        string newStatus,
        string technology,
        string? assignedMachineId,
        DateTime changedAt,
        string changedBy)
    {
        JobId = jobId;
        OrderId = orderId;
        PreviousStatus = previousStatus;
        NewStatus = newStatus;
        Technology = technology;
        AssignedMachineId = assignedMachineId;
        ChangedAt = changedAt;
        ChangedBy = changedBy;
    }
}
