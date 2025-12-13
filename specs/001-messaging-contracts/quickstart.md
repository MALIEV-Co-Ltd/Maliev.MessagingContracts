# Quickstart: Messaging Contracts

## Overview

This repository uses a **Schema-First** approach. You define messages in JSON Schema, and the build process generates C# models.

## How to Add a New Contract

1.  **Create Schema**: Add a new JSON Schema file in `contracts/events/` or `contracts/commands/`.
    ```json
    {
      "$schema": "http://json-schema.org/draft-07/schema#",
      "title": "MyNewEvent",
      "type": "object",
      "properties": { ... }
    }
    ```
2.  **Update AsyncAPI**: Register the message in `asyncapi/asyncapi.yaml`.
3.  **Validate**: Run `npm test` (or equivalent script) to validate schemas.
4.  **Generate**: The CI/CD pipeline will generate C# models into `generated/csharp/` and publish the NuGet package.

## Local Development

1.  **Install Tools**:
    - Node.js (for AsyncAPI/AJV)
    - .NET 10 SDK (for NJsonSchema)
2.  **Run Validation**:
    ```bash
    # (Example)
    asyncapi validate asyncapi/asyncapi.yaml
    ```
3.  **Generate Code**:
    ```bash
    # (Example)
    dotnet run --project tools/Generator
    ```
