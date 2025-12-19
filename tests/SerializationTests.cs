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
}
