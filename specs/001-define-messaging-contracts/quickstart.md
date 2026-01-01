# Quickstart: Consuming Messaging Contracts

This guide provides the essential steps for a Maliev microservice to consume the centralized messaging contracts.

## 1. Add Package Reference

All services MUST reference the official `Maliev.MessagingContracts` NuGet package. Add the following to your service's `.csproj` file:

```xml
<ItemGroup>
  <PackageReference Include="Maliev.MessagingContracts" Version="[VERSION]" />
</ItemGroup>
```

Replace `[VERSION]` with the latest stable version of the contracts package.

## 2. Configure MassTransit Consumer

In your service's startup configuration (e.g., `Program.cs`), define a MassTransit consumer for the specific message you want to handle.

**Example**: A `NotificationService` consuming an `OrderCreatedEvent`.

```csharp
using MassTransit;
using Maliev.MessagingContracts.Contracts.orders; // Correct namespace

public class OrderCreatedConsumer : IConsumer<OrderCreatedEvent>
{
    private readonly ILogger<OrderCreatedConsumer> _logger;

    public OrderCreatedConsumer(ILogger<OrderCreatedConsumer> logger)
    {
        _logger = logger;
    }

    public async Task Consume(ConsumeContext<OrderCreatedEvent> context)
    {
        var payload = context.Message.Payload;
        _logger.LogInformation(
            "Received OrderCreatedEvent for Order ID: {OrderId}, Correlation ID: {CorrelationId}",
            payload.OrderId,
            context.CorrelationId);

        // Your business logic here...
        // e.g., send an email to the customer
        await Task.CompletedTask;
    }
}
```

## 3. Register the Consumer

In your MassTransit registration, configure the endpoint and register the consumer.

```csharp
// In your Program.cs or service extension method for MassTransit
services.AddMassTransit(x =>
{
    // Add your consumer
    x.AddConsumer<OrderCreatedConsumer>();

    x.UsingRabbitMq((context, cfg) =>
    {
        cfg.Host("rabbitmq", "/", h =>
        {
            h.Username("guest");
            h.Password("guest");
        });

        // Configure the receive endpoint for this service
        cfg.ReceiveEndpoint("notification-service:order-created-event", e =>
        {
            // Bind the consumer to the endpoint
            e.ConfigureConsumer<OrderCreatedConsumer>(context);

            // Apply standard retry policies
            e.UseMessageRetry(r => r.Interval(5, TimeSpan.FromSeconds(5)));
        });
    });
});
```

### Key Configuration Rules

-   **Endpoint Naming**: Queues MUST be named according to the convention: `{service-name}:{message-name}` (e.g., `notification-service:order-created-event`).
-   **Explicit Binding**: Consumers must be explicitly configured for their respective receive endpoints.
-   **Idempotency**: Your consumer logic should be idempotent. Use the `messageId` from the contract and a persistent store (e.g., an inbox table) to ensure a message is not processed more than once.

## 4. Publishing a Message

When a service needs to publish a message, it should use the `IPublishEndpoint` and the concrete C# record from the contracts package.

**Example**: An `OrderService` publishing an `OrderCreatedEvent`.

```csharp
using MassTransit;
using Maliev.MessagingContracts.Contracts.orders; // Correct namespace

public class OrderCreationService
{
    private readonly IPublishEndpoint _publishEndpoint;

    public OrderCreationService(IPublishEndpoint publishEndpoint)
    {
        _publishEndpoint = publishEndpoint;
    }

    public async Task CreateOrder(Guid orderId, Guid customerId, decimal amount)
    {
        // ... your logic to save the order to the database ...

        // Publish the event using the contract
        await _publishEndpoint.Publish(new OrderCreatedEvent
        {
            Payload = new Payload
            {
                OrderId = orderId,
                CustomerId = customerId,
                Amount = amount
            }
            // The MassTransit envelope will handle the metadata like CorrelationId, etc.
        });
    }
}

```
