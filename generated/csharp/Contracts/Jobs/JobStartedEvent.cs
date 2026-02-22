using System;
using System.Text.Json.Serialization;

namespace Maliev.MessagingContracts.Contracts.Jobs;

/// <summary>
/// Published when a production job transitions to InProgress status.
/// </summary>
public record JobStartedEvent
{
    [JsonPropertyName("jobId")]
    public Guid JobId { get; init; }

    [JsonPropertyName("orderId")]
    public Guid OrderId { get; init; }

    [JsonPropertyName("materialId")]
    public Guid MaterialId { get; init; }

    [JsonPropertyName("volumeCm3")]
    public decimal VolumeCm3 { get; init; }

    [JsonPropertyName("technology")]
    public string Technology { get; init; } = string.Empty;

    [JsonPropertyName("assignedMachineId")]
    public string AssignedMachineId { get; init; } = string.Empty;

    [JsonPropertyName("startedAt")]
    public DateTime StartedAt { get; init; }

    public JobStartedEvent() { }

    public JobStartedEvent(
        Guid jobId,
        Guid orderId,
        Guid materialId,
        decimal volumeCm3,
        string technology,
        string assignedMachineId,
        DateTime startedAt)
    {
        JobId = jobId;
        OrderId = orderId;
        MaterialId = materialId;
        VolumeCm3 = volumeCm3;
        Technology = technology;
        AssignedMachineId = assignedMachineId;
        StartedAt = startedAt;
    }
}
