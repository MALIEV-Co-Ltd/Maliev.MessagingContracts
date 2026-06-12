using System.Text.Json;
using Xunit;
using Maliev.MessagingContracts.Contracts.Shared;
using Maliev.MessagingContracts.Contracts.Geometry;
using Maliev.MessagingContracts.Contracts.Orders;
using Maliev.MessagingContracts.Contracts.Jobs;
using Maliev.MessagingContracts.Contracts.Payments;
using Maliev.MessagingContracts.Contracts.Search;

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

    /// <summary>
    /// Tests that CNC turning reports preserve the model-space axis point.
    /// </summary>
    [Fact]
    public void CanRoundTrip_CncDfmReportPayload_AxisPoint()
    {
        var payload = new CncDfmReportPayload(
            ReportType: "CNC_TURN",
            SharpCornerCount: 0,
            SharpCornerRegions: [],
            HasUndercuts: false,
            UndercutRegions: [],
            HasDrillHoles: false,
            DrillHoleCount: 0,
            RequiresEdm: false,
            RequiresGrinding: false,
            MinimumFeatureSizeMm: 0,
            IsTurnable: true,
            PrimaryAxis: "Z",
            AxisVector: [0, 0, 1],
            AxisPoint: [12.5, -4.25, 0],
            LengthDiameterRatio: 2.4,
            SymmetryDeviation: 0.01,
            Issues: []);

        var json = JsonSerializer.Serialize(payload, _options);
        var deserialized = JsonSerializer.Deserialize<CncDfmReportPayload>(json, _options);

        Assert.NotNull(deserialized);
        Assert.Equal([12.5, -4.25, 0], deserialized.AxisPoint);
    }

    /// <summary>
    /// Tests that search document upsert events preserve the indexable fields.
    /// </summary>
    [Fact]
    public void CanRoundTrip_SearchDocumentUpsertedEvent()
    {
        var resourceId = System.Guid.NewGuid().ToString();
        var evt = new SearchDocumentUpsertedEvent(
            MessageId: System.Guid.NewGuid(),
            MessageName: "SearchDocumentUpsertedEvent",
            MessageType: MessageType.Event,
            MessageVersion: "1.0.0",
            PublishedBy: "ProjectService",
            ConsumedBy: new[] { "SearchService" },
            CorrelationId: System.Guid.NewGuid(),
            CausationId: null,
            OccurredAtUtc: System.DateTimeOffset.UtcNow,
            IsPublic: false,
            Payload: new SearchDocumentUpsertedEventPayload(
                SourceService: "ProjectService",
                ResourceType: "project",
                ResourceId: resourceId,
                Title: "PRJ-2026-0001",
                Subtitle: "Acme prototype",
                Summary: "Prototype quote for Acme",
                Keywords: ["PRJ-2026-0001", "Acme", "prototype"],
                Status: "Draft",
                RequiredPermission: "project.projects.read",
                OccurredAtUtc: System.DateTimeOffset.UtcNow
            )
        );

        var json = JsonSerializer.Serialize(evt, _options);
        var deserialized = JsonSerializer.Deserialize<SearchDocumentUpsertedEvent>(json, _options);

        Assert.NotNull(deserialized);
        Assert.Equal(resourceId, deserialized.Payload.ResourceId);
        Assert.Equal("project.projects.read", deserialized.Payload.RequiredPermission);
        Assert.Contains("Acme", deserialized.Payload.Keywords);
    }

    /// <summary>
    /// Tests that search reindex commands preserve the requested source scope.
    /// </summary>
    [Fact]
    public void CanRoundTrip_SearchReindexRequestedCommand()
    {
        var command = new SearchReindexRequestedCommand(
            MessageId: System.Guid.NewGuid(),
            MessageName: "SearchReindexRequestedCommand",
            MessageType: MessageType.Command,
            MessageVersion: "1.0.0",
            PublishedBy: "SearchService",
            ConsumedBy: new[] { "CustomerService", "ProjectService" },
            CorrelationId: System.Guid.NewGuid(),
            CausationId: null,
            OccurredAtUtc: System.DateTimeOffset.UtcNow,
            IsPublic: false,
            Payload: new SearchReindexRequestedCommandPayload(
                SourceService: null,
                RequestedBy: "system",
                RequestedAtUtc: System.DateTimeOffset.UtcNow
            )
        );

        var json = JsonSerializer.Serialize(command, _options);
        var deserialized = JsonSerializer.Deserialize<SearchReindexRequestedCommand>(json, _options);

        Assert.NotNull(deserialized);
        Assert.Null(deserialized.Payload.SourceService);
        Assert.Equal("system", deserialized.Payload.RequestedBy);
    }

    /// <summary>
    /// Tests that provider-neutral non-paid payment events preserve their transaction and status context.
    /// </summary>
    [Fact]
    public void CanRoundTrip_PaymentTerminalAndPendingEvents()
    {
        var transactionId = System.Guid.NewGuid();
        var now = System.DateTimeOffset.UtcNow;

        var cancelled = new PaymentCancelledEvent(
            MessageId: System.Guid.NewGuid(),
            MessageName: nameof(PaymentCancelledEvent),
            MessageType: MessageType.Event,
            MessageVersion: "1.0.0",
            PublishedBy: "PaymentService",
            ConsumedBy: new[] { "NotificationService" },
            CorrelationId: System.Guid.NewGuid(),
            CausationId: null,
            OccurredAtUtc: now,
            IsPublic: true,
            Payload: new PaymentCancelledEventPayload(
                TransactionId: transactionId,
                IdempotencyKey: "idem-123",
                Amount: 1200,
                Currency: "THB",
                CustomerId: "customer-1",
                OrderId: "order-1",
                ProviderName: "omise",
                Reason: "Customer cancelled checkout",
                ProviderEventCode: "payment.cancelled",
                CancelledAt: now
            )
        );
        var expired = new PaymentExpiredEvent(
            MessageId: System.Guid.NewGuid(),
            MessageName: nameof(PaymentExpiredEvent),
            MessageType: MessageType.Event,
            MessageVersion: "1.0.0",
            PublishedBy: "PaymentService",
            ConsumedBy: new[] { "NotificationService" },
            CorrelationId: System.Guid.NewGuid(),
            CausationId: null,
            OccurredAtUtc: now,
            IsPublic: true,
            Payload: new PaymentExpiredEventPayload(
                TransactionId: transactionId,
                IdempotencyKey: "idem-123",
                Amount: 1200,
                Currency: "THB",
                CustomerId: "customer-1",
                OrderId: "order-1",
                ProviderName: "omise",
                Reason: "Hosted payment session expired",
                ProviderEventCode: "checkout.session.expired",
                ExpiredAt: now
            )
        );
        var pending = new PaymentPendingEvent(
            MessageId: System.Guid.NewGuid(),
            MessageName: nameof(PaymentPendingEvent),
            MessageType: MessageType.Event,
            MessageVersion: "1.0.0",
            PublishedBy: "PaymentService",
            ConsumedBy: new[] { "NotificationService" },
            CorrelationId: System.Guid.NewGuid(),
            CausationId: null,
            OccurredAtUtc: now,
            IsPublic: true,
            Payload: new PaymentPendingEventPayload(
                TransactionId: transactionId,
                IdempotencyKey: "idem-123",
                Amount: 1200,
                Currency: "THB",
                CustomerId: "customer-1",
                OrderId: "order-1",
                ProviderName: "omise",
                ProviderEventCode: "payment.pending",
                PendingAt: now
            )
        );

        Assert.Equal("payment.cancelled", RoundTrip(cancelled).Payload.ProviderEventCode);
        Assert.Equal("checkout.session.expired", RoundTrip(expired).Payload.ProviderEventCode);
        Assert.Equal("payment.pending", RoundTrip(pending).Payload.ProviderEventCode);
    }

    private T RoundTrip<T>(T message)
    {
        var json = JsonSerializer.Serialize(message, _options);
        var deserialized = JsonSerializer.Deserialize<T>(json, _options);

        Assert.NotNull(deserialized);
        return deserialized;
    }
}
