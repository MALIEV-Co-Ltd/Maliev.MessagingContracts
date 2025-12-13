# Maliev.MessagingContracts
Maintain a central 'Messaging Contract' repository for the MALIEV ecosystem.

## Overview
This repository uses a **Schema-First** approach. Contracts are defined as JSON Schemas, and C# models are generated from them.

## How to Add a New Contract

1.  **Create Schema**: Add a new JSON Schema file in `contracts/schemas/events/` or `contracts/schemas/commands/`.
    ```json
    {
      "$schema": "http://json-schema.org/draft-07/schema#",
      "title": "MyNewEvent",
      "type": "object",
      "properties": { ... }
    }
    ```
2.  **Update AsyncAPI**: Register the message in `asyncapi/asyncapi.yaml` if it's part of a public channel.
3.  **Validate**: Run `npm test` locally to validate schemas.
4.  **Generate & Publish**: The CI/CD pipeline will automatically generate the C# models, package the library, and publish it to GitHub Packages on merge to `main`.
