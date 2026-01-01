# Research: Messaging Contracts Repository Setup

**Feature**: Messaging Contracts Repository (Schema-First)
**Date**: 2024-12-12

## 1. Schema Validation and C# Generation Tooling

**Decision**:
- Schema Validation & Parsing: **JsonSchema.Net**
- C# Class Generation: **NJsonSchema**

**Rationale**:
- **JsonSchema.Net**: Provides robust, high-performance JSON Schema validation and parsing directly within the .NET ecosystem. It's fully compliant with Draft-07 (and later) specifications. This will be used for runtime validation and potentially as a parsing layer for NJsonSchema.
- **NJsonSchema**: Mature and widely adopted for generating C# `record` types from JSON Schemas. It offers fine-grained control over generation options (e.g., `System.Text.Json` attributes) crucial for this project.

**Alternatives Considered**: (No changes, previously documented)
- JsonSchema.Net.CodeGeneration (for C# generation, but NJsonSchema is preferred for its maturity in this specific context)

## 2. AsyncAPI 3.0 Tooling

**Decision**: Use **AsyncAPI CLI (@asyncapi/cli)** for validation and documentation generation.
**Rationale**: (No changes, previously documented)
- Official tool, supports 3.0 spec, can bundle/validate.
## 3. Topology Definition Format

**Decision**: Use a custom **YAML format compatible with RabbitMQ Definition Upload**.
**Rationale**:
- RabbitMQ supports importing definitions via JSON/YAML (users, vhosts, queues, exchanges, bindings).
- We can structure the `topology/` directory to map to these definitions.
- Format:
  ```yaml
  exchanges:
    - name: maliev.orders
      type: topic
      durable: true
  queues:
    - name: maliev.orders.created
      durable: true
  bindings:
    - source: maliev.orders
      destination: maliev.orders.created
      routing_key: order.created
  ```

## 4. GitHub Actions Workflow Strategy

**Decision**:
1. **Validation Job**: Runs on PR. Validates JSON Schemas (using `ajv-cli` or similar) and AsyncAPI specs. Checks for breaking changes using `json-schema-diff` or similar.
2. **Generation Job**: Runs on Merge to Main. Uses `NJsonSchema` to build C# files into `generated/csharp/`. Packs and Pushes NuGet.
3. **Documentation Job**: Runs on Merge to Main. Generates HTML docs and publishes to GitHub Pages (or central doc portal).

## 5. Constitution Amendment

**Action**: The Constitution MUST be amended to align with the new direction.
- **Change**: "Code-First" -> "Schema-First".
- **Change**: ".NET 8" -> ".NET 10".
- **Change**: "Draft 2020-12" -> "Draft-07" (User requirement).
- **Change**: Directory structure rules.

## 6. .NET 10 Support

**Note**: .NET 10 is likely a future target (as of 2025 context). We will configure the `.csproj` to target `net10.0`. If the SDK is not available in the environment, we might need to fallback to `net9.0` or `net8.0` temporarily or use a specific container.
**Assumption**: The environment supports the requested SDK.
