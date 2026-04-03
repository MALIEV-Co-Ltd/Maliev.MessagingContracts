# Maliev.MessagingContracts Agent Guidelines

This document provides instructions for AI agents working in this repository.
The repository is the **authoritative, contract-first source of truth** for message types in the MALIEV ecosystem.

---

## 1. Core Principles & "Constitution" Rules

**CRITICAL: Strictly adhere to these mandates. Violations will break the build or architecture.**

### Schema-First Mandates
- **Schema-First**: NEVER edit generated C# code directly. Edit JSON Schemas in `contracts/schemas/` and regenerate.
- **BaseMessage Pattern**: All events/commands MUST inherit from `BaseMessage` (reference `../shared/base-message.json`).
- **XML Documentation**: Required on ALL generated classes/properties (handled by generator, but ensure schema descriptions are present).
- **Zero Dependencies**: Generated contracts must have minimal/no external dependencies (System.Text.Json only).

### Banned Libraries (Build Will Fail)

| Banned | Use Instead |
|--------|-------------|
| AutoMapper | Manual mapping extensions |
| FluentValidation | DataAnnotations or manual validation |
| FluentAssertions | Standard xUnit `Assert.*` |
| Swashbuckle/Swagger | Scalar (at `/{service}/scalar`) |
| InMemoryDatabase (EF Core) | Testcontainers with real PostgreSQL |
| External Generators (NJsonSchema/NSwag) | Proprietary generator in `tools/Generator/` |

---

## 2. Environment & Tech Stack

- **Language**: C# 13
- **Framework**: .NET 10.0
- **Schemas**: JSON Schema (Draft-07), AsyncAPI 3.0
- **Serialization**: `System.Text.Json` (Source Generated)
- **OS**: Windows (`win32`)

---

## 3. Workflow: Adding/Modifying Contracts

To add or modify a message contract, follow this exact sequence:

1. **Define Schema**: Create/Edit `.json` file in `contracts/schemas/{domain}/`.
   - File naming: `kebab-case.json` (e.g., `employee-created.json`).
   - Use `allOf` to inherit from `shared/base-message.json`.
2. **Register Topology**: Update `asyncapi/asyncapi.yaml`.
   - Add the message definition.
   - Define the channel and RabbitMQ address.
3. **Validate Schemas**: Run `npm test` to validate JSON and AsyncAPI syntax.
4. **Generate Code**: Run `./scripts/build.ps1` to generate C# models.
5. **Verify C#**: Run `dotnet test` to ensure serialization works.

---

## 4. Build, Test & Lint Commands

All commands run from `B:\maliev\Maliev.MessagingContracts`.

```powershell
# Build (treats warnings as errors — all must be fixed)
dotnet build Maliev.MessagingContracts.slnx

# Run all tests
dotnet test Maliev.MessagingContracts.slnx --verbosity normal

# Run a single test method
dotnet test --filter "FullyQualifiedName~Maliev.MessagingContracts.Tests.ValidationTests.Schema_ShouldBeValid"

# Run all tests in a class
dotnet test --filter "FullyQualifiedName~Maliev.MessagingContracts.Tests.SerializationTests"

# Run with code coverage
dotnet test Maliev.MessagingContracts.slnx --collect:"XPlat Code Coverage"

# Format check
dotnet format Maliev.MessagingContracts.slnx
```

### Schema-Specific Commands

```bash
# Validate JSON Schemas & AsyncAPI (Fast)
npm test

# Generate C# Models (validates schemas + runs custom C# generator)
./scripts/build.ps1
```

---

## 5. Directory Structure

- `contracts/schemas/`: **Source of Truth**. JSON Schemas grouped by domain (e.g., `employee/`, `orders/`).
- `asyncapi/`: AsyncAPI definitions (`asyncapi.yaml`).
- `tools/Generator/`: Custom C# source generator code.
- `generated/csharp/`: **Read-Only**. Output directory for generated C# code.
- `tests/`: Unit tests for serialization and validation.

---

## 6. Code Style & Conventions

### JSON Schemas
- **Format**: JSON Schema Draft-07.
- **Naming**: Files use `kebab-case.json`. Properties use `camelCase`.
- **Inheritance**: Always use `allOf` with `$ref` to `shared/base-message.json`.
- **Descriptions**: Logical `description` fields are required for all properties (becomes XML docs).

### C# Naming & Formatting
- **Namespaces**: File-scoped (`namespace Maliev.MessagingContracts.Generator;`)
- **Classes/Methods/Properties**: `PascalCase`
- **Private fields**: `_camelCase` (underscore prefix)
- **Parameters/locals**: `camelCase`
- **Async methods**: Suffix with `Async` (e.g., `GenerateAsync`)
- **Interfaces**: Prefix with `I` (e.g., `ISchemaParser`)
- **XML docs**: Required on ALL public methods and properties
- **Nullable**: Enabled (`<Nullable>enable</Nullable>`). Use `?` explicitly
- **Imports**: System first, then third-party, then local. Alphabetize within groups. Remove unused `using`
- **Braces**: Allman style (new line) for methods and control structures. Expression-bodied for properties/accessors
- **Indentation**: 4 spaces, LF line endings, UTF-8, trim trailing whitespace

### C# Patterns
- **DI**: Constructor injection with `private readonly` fields
- **Logging**: `ILogger<T>` with structured placeholders (never interpolate): `_logger.LogInformation("Processing {SchemaName}", schemaName)`
- **Manual mapping**: Static extension methods (`ToDto()`, `ToEntity()`). AutoMapper is banned
- **Validation**: `System.ComponentModel.DataAnnotations` on DTOs. FluentValidation is banned

---

## 7. Testing Rules

- **Framework**: xUnit with standard `Assert` (`Assert.Equal`, `Assert.NotNull`, etc.)
- **Naming**: `MethodName_StateUnderTest_ExpectedBehavior` or `HTTP_METHOD_Path_Scenario_ExpectedStatus`
- **Coverage**: Minimum 80% per service
- **Integration tests**: `BaseIntegrationTestFactory<TProgram, TDbContext>` with Testcontainers (PostgreSQL, Redis, RabbitMQ). Never InMemoryDatabase
- **System tests** (Tier 3): `AspireTestFixture` with `[Collection("AspireDomainTests")]` — shared AppHost, never one per class
- **Eventual consistency**: Use `TestHelpers.WaitForAsync`. Never `Task.Delay`
- **MassTransit consumers**: Must have consumer tests using `AddMassTransitTestHarness()`

---

## 8. Mandatory Rules

- **`TreatWarningsAsErrors = true`**: Zero warnings allowed. No suppression
- **`[RequirePermission("domain.resources.action")]`**: On all endpoints, not plain `[Authorize]`
- **API versioning**: All routes versioned (`v1/`)
- **Service prefix**: Routes prefixed with service domain (e.g., `/auth`, `/customer`, `/job`)
- **Scalar docs**: Configured at `/{service}/scalar`
- **Secrets**: Never hardcoded. Use GCP Secret Manager or environment variables
- **Async/await**: All the way down. Pass `CancellationToken`
- **EF Core Design package**: Only in Infrastructure project, never in Api
- **PostgreSQL xmin**: Shadow property only — `entity.Property<uint>("xmin").HasColumnType("xid").IsRowVersion()`. Never add entity property
- **Temporary files**: Generate in `/temp` folder, clean up afterwards

---

## 9. Database & EF Core — Mandatory Rules

### EF Core Design Package
- `Microsoft.EntityFrameworkCore.Design` MUST NOT be in Api projects
- It belongs ONLY in the Infrastructure (or Data) project where migrations live
- Migration commands must target Infrastructure as both project and startup-project (since EF Core Design package is in Infrastructure):
  ```
  dotnet ef migrations add <Name> --project Maliev.<Domain>Service.Infrastructure --startup-project Maliev.<Domain>Service.Infrastructure
  ```

### PostgreSQL xmin Concurrency — Mandatory Pattern
Use shadow property ONLY. Never add a Xmin/xmin property to domain entities.
```csharp
entity.Property<uint>("xmin").HasColumnType("xid").IsRowVersion();
```
- Never use `UseXminAsConcurrencyToken()` (removed in Npgsql EF v7)
- Never use entity property `public uint Xmin { get; set; }` or `public uint xmin { get; set; }`
- Never use `.Ignore(e => e.Xmin)` — remove the entity property instead

---

## 10. Git Rules

- Each `Maliev.*` folder is an independent git repo. `cd` into it before git commands
- **Commit early and often** after every meaningful unit of work. Do not accumulate changes
- **Never use `git checkout` to restore files** — commit first, then `git revert` or `git reset --soft`
- Feature branches merged to `develop` via PR. Do not push without being asked

---

## 11. Troubleshooting

- **Build fails on warnings**: Fix the warning; do not disable `TreatWarningsAsErrors`.
- **Serialization fails**: Check if `System.Text.Json` attributes are correctly generated. Ensure the schema types map correctly to C# types.
- **Generator issues**: If the generator fails, check `tools/Generator` logic. It is a custom proprietary tool.

---

## 12. Development Checklist

Before asking the user to commit:
1. [ ] Are schemas valid? (`npm test`)
2. [ ] Is C# code generated? (`./scripts/build.ps1`)
3. [ ] Do all tests pass? (`dotnet test Maliev.MessagingContracts.slnx --verbosity normal`)
4. [ ] Are there any warnings? (Must be zero)
