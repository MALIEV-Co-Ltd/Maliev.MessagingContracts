---
name: maliev-messaging-contracts
description: Contract-first source of truth for all MALIEV message types
license: MIT
compatibility: opencode
metadata:
  audience: developers
  workflow: messaging-contracts
---

## What I do

- Maintain JSON Schemas (Draft-07) for all cross-service messages
- Define RabbitMQ topology and channel addresses (AsyncAPI 3.0)
- Generate C# record types and nested payloads
- Enforce semantic versioning and consumer declarations

## When to use me

- Adding a new cross-service event, command, or request/response
- Modifying fields in an existing contract
- Updating publisher/consumer mappings
- Troubleshooting messaging serialization or topology issues

---

## 1. Core Principles & "Constitution" Rules

**CRITICAL: Strictly adhere to these mandates. Violations will break the build or architecture.**

### Banned Libraries
*   ❌ **AutoMapper**: Do NOT use. Use explicit manual mapping.
*   ❌ **FluentValidation**: Do NOT use. Use Data Annotations or standard logic.
*   ❌ **FluentAssertions**: Do NOT use. Use standard xUnit `Assert` methods.
*   ❌ **External Generators**: Do NOT use NJsonSchema or NSwag. Use the proprietary generator in `tools/Generator/`.

### Mandatory Practices
*   ✅ **Schema-First**: NEVER edit generated C# code directly. Edit JSON Schemas in `contracts/schemas/` and regenerate.
*   ✅ **BaseMessage Pattern**: All events/commands MUST inherit from `BaseMessage` (reference `../shared/base-message.json`).
*   ✅ **XML Documentation**: Required on ALL generated classes/properties (handled by generator, but ensure schema descriptions are present).
*   ✅ **TreatWarningsAsErrors**: The build will fail on any warning.
*   ✅ **Zero Dependencies**: Generated contracts must have minimal/no external dependencies (System.Text.Json only).

## 2. Environment & Tech Stack

*   **Language**: C# 13
*   **Framework**: .NET 10.0
*   **Schemas**: JSON Schema (Draft-07), AsyncAPI 3.0
*   **Serialization**: `System.Text.Json` (Source Generated)
*   **OS**: Linux / Windows

## 3. Workflow: Adding/Modifying Contracts

To add or modify a message contract, follow this exact sequence:

1.  **Define Schema**: Create/Edit `.json` file in `contracts/schemas/{domain}/`.
    *   File naming: `kebab-case.json` (e.g., `employee-created.json`).
    *   Use `allOf` to inherit from `shared/base-message.json`.
2.  **Register Topology**: Update `asyncapi/asyncapi.yaml`.
    *   Add the message definition.
    *   Define the channel and RabbitMQ address.
3.  **Validate Schemas**: Run `npm test` to validate JSON and AsyncAPI syntax.
4.  **Generate Code**: Run the generator.
    ```bash
    dotnet run --project tools/Generator/Generator.csproj
    ```
5.  **Verify C#**: Run `dotnet test` to ensure serialization works.

## 4. Commands

### Build & Generate
*   **Generate C# Models**:
    ```bash
    dotnet run --project tools/Generator/Generator.csproj
    ```
    *This script validates schemas and runs the custom C# generator.*

*   **Build Solution**:
    ```bash
    dotnet build Maliev.MessagingContracts.slnx
    ```

### Testing & Validation
*   **Validate JSON Schemas & AsyncAPI (Fast)**:
    ```bash
    npm test
    ```
    *Runs `npm run validate:asyncapi` and `npm run validate:schemas`.*

*   **Run All C# Tests**:
    ```bash
    dotnet test
    ```

## 5. Directory Structure

*   `contracts/schemas/`: **Source of Truth**. JSON Schemas grouped by domain (e.g., `employee/`, `orders/`).
*   `asyncapi/`: AsyncAPI definitions (`asyncapi.yaml`).
*   `tools/Generator/`: Custom C# source generator code.
*   `generated/csharp/`: **Read-Only**. Output directory for generated C# code.
*   `tests/`: Unit tests for serialization and validation.
*   `topology/`: RabbitMQ exchange, queue, and binding declarations (`rabbitmq.yaml`).

## 6. Code Style Guidelines

### JSON Schemas
*   **Format**: JSON Schema Draft-07.
*   **Naming**: Files use `kebab-case.json`. Properties use `camelCase`.
*   **Inheritance**: Always use `allOf` with `$ref` to `shared/base-message.json`.
*   **Descriptions**: logical `description` fields are required for all properties (becomes XML docs).

### C# (Tests & Generator)
*   **Naming**: `PascalCase` for classes, methods, properties. `camelCase` for local variables.
*   **Formatting**: Standard Allman style.
*   **Tests**: Use `xUnit`. Naming convention: `MethodName_StateUnderTesting_ExpectedBehavior`.

## 7. Development Checklist

Before asking the user to commit:
1.  [ ] Are schemas valid? (`npm test`)
2.  [ ] Is C# code generated? (`dotnet run --project tools/Generator`)
3.  [ ] Do all tests pass? (`dotnet test`)
4.  [ ] Are there any warnings? (Must be zero)

