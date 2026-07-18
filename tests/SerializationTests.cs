using System.Text.Json;
using Xunit;
using Maliev.MessagingContracts.Generated;

namespace Maliev.MessagingContracts.Tests;

public class SerializationTests
{
    private readonly JsonSerializerOptions _options = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        WriteIndented = true
    };

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

    [Fact]
    public void CanRoundTrip_EquipmentStatusChangedEvent()
    {
        var message = new EquipmentStatusChangedEvent(
            MessageId: Guid.NewGuid(),
            MessageName: "EquipmentStatusChangedEvent",
            MessageType: MessageType.Event,
            MessageVersion: "1.0",
            PublishedBy: "FacilityService",
            ConsumedBy: new[] { "NotificationService" },
            CorrelationId: Guid.NewGuid(),
            CausationId: null,
            OccurredAtUtc: DateTimeOffset.UtcNow,
            IsPublic: false,
            Payload: new EquipmentStatusChangedEventPayload(
                EquipmentId: Guid.NewGuid(),
                AssetCode: "MAL-CNC-0001",
                Name: "CNC Mill",
                Category: "CNC",
                PreviousStatus: "Available",
                NewStatus: "Maintenance"));

        var json = JsonSerializer.Serialize(message, _options);
        var deserialized = JsonSerializer.Deserialize<EquipmentStatusChangedEvent>(json, _options);

        Assert.Contains("\"equipmentId\"", json, StringComparison.Ordinal);
        Assert.Contains("\"newStatus\"", json, StringComparison.Ordinal);
        Assert.NotNull(deserialized);
        Assert.Equal(message.Payload, deserialized.Payload);
    }

    [Fact]
    public void CanRoundTrip_LoanDocumentRequestedEvent()
    {
        var message = new LoanDocumentRequestedEvent(
            MessageId: Guid.NewGuid(),
            MessageName: "LoanDocumentRequestedEvent",
            MessageType: MessageType.Event,
            MessageVersion: "1.0",
            PublishedBy: "FacilityService",
            ConsumedBy: new[] { "PdfService" },
            CorrelationId: Guid.NewGuid(),
            CausationId: null,
            OccurredAtUtc: DateTimeOffset.UtcNow,
            IsPublic: false,
            Payload: new LoanDocumentRequestedEventPayload(
                LoanId: Guid.NewGuid(),
                EquipmentId: Guid.NewGuid(),
                AssetCode: "MAL-CNC-0001",
                EquipmentName: "CNC Mill",
                Brand: "MALIEV",
                ModelName: "Mill-01",
                ManufacturerSerial: "SN-001",
                BorrowerId: Guid.NewGuid(),
                BorrowerName: "Example Borrower",
                BorrowerType: "Employee",
                ApprovedByEmployeeId: Guid.NewGuid(),
                LoanStartDate: "2026-07-18",
                ExpectedReturnDate: "2026-07-25",
                Purpose: "Training",
                DocumentLanguage: "th"));

        var json = JsonSerializer.Serialize(message, _options);
        var deserialized = JsonSerializer.Deserialize<LoanDocumentRequestedEvent>(json, _options);

        Assert.Contains("\"loanId\"", json, StringComparison.Ordinal);
        Assert.Contains("\"documentLanguage\"", json, StringComparison.Ordinal);
        Assert.NotNull(deserialized);
        Assert.Equal(message.Payload, deserialized.Payload);
    }
}
