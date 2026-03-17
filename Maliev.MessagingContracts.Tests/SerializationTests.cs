using System.Text.Json;
using Xunit;
using Maliev.MessagingContracts.Contracts.Shared;
using Maliev.MessagingContracts.Contracts.Orders;
using Maliev.MessagingContracts.Contracts.Jobs;

namespace Maliev.MessagingContracts.Tests;

/// <summary>
/// Tests for serialization roundtrip of message contracts.
/// </summary>
public class SerializationTests
{
    private readonly JsonSerializerOptions _options = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        WriteIndented = true
    };

    /// <summary>
    /// Tests that CreateOrderCommand can be serialized and deserialized correctly.
    /// </summary>
    [Fact]
    public void CanRoundTrip_CreateOrderCommand()
    {
        var command = new CreateOrderCommand(
            MessageId: System.Guid.NewGuid(),
            MessageName: "CreateOrderCommand",
            MessageType: MessageType.Command,
            MessageVersion: "1.0.0",
            PublishedBy: "OrderService",
            ConsumedBy: new[] { "OrderService" },
            CorrelationId: System.Guid.NewGuid(),
            CausationId: null,
            OccurredAtUtc: System.DateTimeOffset.UtcNow,
            IsPublic: true,
            Payload: new CreateOrderCommandPayload(
                OrderId: System.Guid.NewGuid(),
                CustomerId: System.Guid.NewGuid(),
                Amount: 100.50,
                Currency: "USD"
            )
        );

        var json = JsonSerializer.Serialize(command, _options);
        var deserialized = JsonSerializer.Deserialize<CreateOrderCommand>(json, _options);

        Assert.NotNull(deserialized);
        Assert.Equal(command.MessageId, deserialized.MessageId);
        Assert.Equal(command.Payload.Amount, deserialized.Payload.Amount);
    }

    /// <summary>
    /// Tests that JobCreatedEvent can be serialized and deserialized correctly.
    /// </summary>
    [Fact]
    public void CanRoundTrip_JobCreatedEvent()
    {
        var jobId = System.Guid.NewGuid();
        var orderId = System.Guid.NewGuid();
        var orderItemId = System.Guid.NewGuid();

        var evt = new JobCreatedEvent(
            MessageId: System.Guid.NewGuid(),
            MessageName: "JobCreatedEvent",
            MessageType: MessageType.Event,
            MessageVersion: "1.0.0",
            PublishedBy: "JobService",
            ConsumedBy: new[] { "ProjectService" },
            CorrelationId: System.Guid.NewGuid(),
            CausationId: null,
            OccurredAtUtc: System.DateTimeOffset.UtcNow,
            IsPublic: false,
            Payload: new JobCreatedEventPayload(
                JobId: jobId,
                OrderId: orderId,
                OrderItemId: orderItemId,
                ProcessType: "FDM",
                JobNumber: "JOB-2026-0001",
                CreatedAt: System.DateTimeOffset.UtcNow
            )
        );

        var json = JsonSerializer.Serialize(evt, _options);
        var deserialized = JsonSerializer.Deserialize<JobCreatedEvent>(json, _options);

        Assert.NotNull(deserialized);
        Assert.Equal(evt.MessageId, deserialized.MessageId);
        Assert.Equal(jobId, deserialized.Payload.JobId);
        Assert.Equal(orderId, deserialized.Payload.OrderId);
        Assert.Equal(orderItemId, deserialized.Payload.OrderItemId);
        Assert.Equal("FDM", deserialized.Payload.ProcessType);
        Assert.Equal("JOB-2026-0001", deserialized.Payload.JobNumber);
    }
}
