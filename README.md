# Maliev Messaging Contracts

[![Build Status](https://img.shields.io/badge/Build-Passing-success)](https://github.com/MALIEV-Co-Ltd/Maliev.MessagingContracts)
[![.NET Version](https://img.shields.io/badge/.NET-10.0-blue)](https://dotnet.microsoft.com/download/dotnet/10.0)
[![NuGet](https://img.shields.io/badge/NuGet-GitHub%20Packages-blue)](https://github.com/orgs/MALIEV-Co-Ltd/packages?repo_name=Maliev.MessagingContracts)

The authoritative, contract-first source of truth for all message types used across the MALIEV microservices ecosystem.

**Role in MALIEV Architecture**: Centralizes all event and command definitions to ensure type-safety and consistency across 22+ microservices. By enforcing a schema-first approach, it prevents runtime integration errors and provides a versioned NuGet package consumed by all services for asynchronous communication.

---

## 🏗️ Architecture & Tech Stack

- **Language**: C# 13 (.NET 10.0)
- **Schema Format**: JSON Schema (Draft-07)
- **Messaging Spec**: AsyncAPI 3.0
- **Generator**: Custom C# Source Generator (Zero-dependency)
- **Message Bus**: RabbitMQ via MassTransit
- **Serialization**: System.Text.Json (Source Generated)
- **Distribution**: NuGet via GitHub Packages

---

## ⚖️ Constitution Rules

This repository strictly adheres to the platform development mandates:

### Banned Libraries
To maintain maximum performance and zero dependency bloat in the generated library:
- ❌ **AutoMapper**: Explicit manual mapping only.
- ❌ **FluentValidation**: Data Annotations or standard logic only.
- ❌ **FluentAssertions**: Standard xUnit `Assert` methods only.
- ❌ **External Generators**: No NJsonSchema or NSwag; uses a proprietary lightweight generator.

### Mandatory Practices
- ✅ **TreatWarningsAsErrors**: Enabled in all `.csproj` files.
- ✅ **XML Documentation**: Required on ALL generated classes and properties.
- ✅ **Schema-First**: Contracts MUST be defined as JSON Schemas before code is generated.
- ✅ **BaseMessage Pattern**: All events and commands MUST inherit from the standardized `BaseMessage`.
- ✅ **Zero Runtime Dependencies**: The generated contracts package should have minimal to no external dependencies.

---

## ✨ Key Features

- **Authoritative Source of Truth**: Single repository managing the lifecycle of all messaging contracts.
- **Automated C# Generation**: JSON Schemas are automatically converted into idiomatic C# 13 `record` types.
- **Type-Safe Messaging**: Provides compile-time safety for event publishing and consumption across services.
- **AsyncAPI Integration**: Messaging topology and schemas are documented using the AsyncAPI 3.0 standard.
- **Semantic Versioning**: Supports versioned message payloads to allow for graceful schema evolution.

---

## 📦 Consumption in Other Services

To use these contracts in your .NET service:

1. **Configure NuGet**: Ensure your `nuget.config` points to GitHub Packages.
2. **Install Package**:
   ```bash
   dotnet add package Maliev.MessagingContracts
   ```
3. **Implementation**:
   ```csharp
   using Maliev.MessagingContracts.Generated;

   // Example: Consuming an event
   public class EmployeeCreatedConsumer : IConsumer<EmployeeCreatedEvent>
   {
       public async Task Consume(ConsumeContext<EmployeeCreatedEvent> context)
       {
           var @event = context.Message;
           var employeeId = @event.Payload.EmployeeId;
           // Process business logic...
       }
   }
   ```

---

## 🛠️ How to Add a New Contract

1. **Define Schema**: Add a new `.json` file in `contracts/schemas/{domain}/`.
   - Use `allOf` to reference `../shared/base-message.json`.
2. **Register Channel**: Add the new message and its RabbitMQ address to `asyncapi/asyncapi.yaml`.
3. **Generate Code**:
   ```powershell
   # Validate schemas and generate C# models
   ./scripts/build.ps1
   ```
4. **Verify**: Run `dotnet test` to ensure serialization works as expected.
5. **Publish**: Merge to `develop` (alpha) or `main` (stable) to trigger automatic NuGet publishing.

---

## 📂 Project Structure

- `contracts/schemas/`: Source JSON Schemas grouped by domain (employee, leave, order, etc.)
- `asyncapi/`: AsyncAPI 3.0 definitions for messaging topology.
- `tools/Generator/`: Proprietary C# model generator from JSON Schemas.
- `generated/csharp/`: The produced C# class library project.
- `tests/`: Serialization and schema validation tests.

---

## 🧪 Testing

We ensure contract integrity through automated validation:
- **Schema Validation**: Ensures all JSON Schemas are syntactically correct.
- **Serialization Tests**: Verifies that generated C# models correctly round-trip via `System.Text.Json`.
- **Inheritance Checks**: Confirms all messages adhere to the `BaseMessage` structure.

---

## 📦 Deployment & Versioning

Versioning follows the platform's semantic release strategy:
- **Develop branch**: Publishes `1.0.X-alpha` (Development)
- **Staging branch**: Publishes `1.0.X-beta` (UAT)
- **Main branch**: Publishes `1.0.X` (Production)

---

## 📄 License

Proprietary - © 2025 MALIEV Co., Ltd. All rights reserved.
