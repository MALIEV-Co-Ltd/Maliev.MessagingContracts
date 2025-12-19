# Maliev.MessagingContracts

Maintain a central 'Messaging Contract' repository for the MALIEV ecosystem. This repository serves as the **authoritative, contract-first source of truth** for all message types used across Maliev's RabbitMQ-based microservices.

## Overview
This repository uses a **Schema-First** approach. Contracts are defined as JSON Schemas (Draft-07), and C# models are automatically generated from them using a custom generator.

## Consumption in Other Services

To use these contracts in your .NET service:

1. **Add NuGet Source**: Ensure your `nuget.config` includes the GitHub Packages source for `MALIEV-Co-Ltd`.
2. **Install Package**:
   ```bash
   dotnet add package Maliev.MessagingContracts
   ```
3. **Usage**:
   ```csharp
   using Maliev.MessagingContracts.Contracts;

   // Example: Handling a command
   public class CreateOrderConsumer : IConsumer<CreateOrderCommand>
   {
       public async Task Consume(ConsumeContext<CreateOrderCommand> context)
       {
           var command = context.Message;
           // Access type-safe payload
           var orderId = command.Payload.OrderId;
       }
   }
   ```

## How to Add a New Contract

1.  **Create Schema**: Add a new JSON Schema file in `contracts/schemas/{domain}/`.
    - Commands: `contracts/schemas/orders/create-order-command.json`
    - Events: `contracts/schemas/payments/payment-completed-event.json`
2.  **Inherit from Base**: Ensure your schema uses `allOf` to reference `../shared/base-message.json`.
3.  **Update AsyncAPI**: Register the message and its channel in `asyncapi/asyncapi.yaml`.
4.  **Local Validation & Generation**:
    ```bash
    # Validate schemas
    npm test
    
    # Generate C# code
    dotnet run --project tools/Generator/Generator.csproj
    ```
5.  **Verify**: Run `dotnet test` to ensure serialization and structure are correct.
6.  **Publish**: Merging to `main` or `develop` will automatically publish a new version to GitHub Packages.

## Project Structure
- `contracts/schemas/`: Source JSON Schemas.
- `asyncapi/`: AsyncAPI 3.0 definitions for messaging topology.
- `tools/Generator/`: Custom C# code generator.
- `generated/csharp/`: The generated C# class library.
- `tests/`: Integration and validation tests.

## Why Custom Generator?
We use a custom generator instead of standard tools (like NJsonSchema) to:
- Enforce strict adherence to our `BaseMessage` inheritance pattern.
- Generate clean, idiomatic C# 10 `record` types.
- Zero external dependencies in the generator tool itself (using `System.Text.Json`).
- Minimize the footprint of the generated library.