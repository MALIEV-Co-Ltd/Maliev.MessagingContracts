# Quickstart: Messaging Contracts

## Overview

This repository uses a **Schema-First** approach. You define messages in JSON Schema, and the build process generates C# models.

## How to Add a New Contract

1.  **Create Schema**: Add a new JSON Schema file in `contracts/schemas/{domain}/`.
    Example `contracts/schemas/orders/new-command.json`:
    ```json
    {
      "$schema": "http://json-schema.org/draft-07/schema#",
      "title": "NewCommand",
      "allOf": [ { "$ref": "../shared/base-message.json" } ],
      "properties": {
        "payload": {
          "type": "object",
          "properties": { "id": { "type": "string", "format": "uuid" } }
        }
      }
    }
    ```
2.  **Update AsyncAPI**: Register the message in `asyncapi/asyncapi.yaml`.
3.  **Validate**: Run `npm test` to validate schemas.
4.  **Generate**: Run `dotnet run --project tools/Generator` to generate C# models into `generated/csharp/`.

## Local Development

1.  **Prerequisites**:
    - Node.js (for AJV validation)
    - .NET 10 SDK
2.  **Scripts**:
    - `npm test`: Runs AJV validation on all schemas.
    - `./scripts/build.ps1`: Runs the custom generator and builds the project.
    - `dotnet test`: Runs C# validation and serialization tests.

## Using the Contracts

The generated contracts are available in the `Maliev.MessagingContracts.Contracts` namespace. All messages inherit from `BaseMessage` and use `record` types for immutability and easy comparison.