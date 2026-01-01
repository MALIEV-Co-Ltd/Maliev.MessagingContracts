<!--
SYNC IMPACT REPORT
Version: 1.1.0 (Major Update - Schema-First Pivot)
- AMENDED: Constitution to support Schema-First workflow (User Requirement).
- AMENDED: .NET Version allowed (added .NET 10).
- AMENDED: JSON Schema version (Draft-07).
- AMENDED: Directory structure (contracts/ subfolders).
- STATUS: Constitution now aligns with Feature 001.
-->

# **MALIEV MessagingContracts Constitution**

**Version:** 1.1.0
**Purpose:** Define strict governance for shared messaging contracts across all MALIEV microservices.

---

# **I. Scope and Purpose (NON-NEGOTIABLE)**

This repository exists **solely** to define:

* **Message Contracts** defined via **JSON Schema (Draft-07)** and **AsyncAPI 3.0**.
* **Generated C# Models** derived strictly from these schemas.
* **Enumerations, error codes, and shared primitives** defined in schema.

This repository is **not** a microservice and must never contain:

* Domain logic
* Infrastructure code (except for schema generation tooling)
* Databases or migrations
* Controllers, APIs, DI, or hosting logic (except for validation tools)
* Business workflows

**Rationale:** Ensures MessagingContracts remains a pure, interoperable contract surface.

---

# **II. Contract Stability and Backward Compatibility (NON-NEGOTIABLE)**

* Contracts must be designed for **long-term stability**.
* **Breaking changes require a new MAJOR version**.
* **Additive changes only** allowed under MINOR version bumps.
* **Schema Validation** must run in CI to prevent breaking changes.
* Contracts must support **side-by-side versioning**.

**Rationale:** Breaks in contract shape cause cascading failures.

---

# **III. Serialization & Schema Rules (NON-NEGOTIABLE)**

* **Source of Truth**: JSON Schema (Draft-07) and AsyncAPI 3.0.
* **Generated Code**: C# models must be generated from schemas.
* **Serialization**: Generated C# models must use `System.Text.Json` attributes.
* **Naming**: Schemas use `camelCase`. Generated C# properties use `PascalCase`.

**Rationale:** Schema-First ensures language-agnostic compatibility while maintaining C# type safety.

---

# **IV. NuGet Packaging Rules (NON-NEGOTIABLE)**

This repository must produce a **single NuGet package**:

`Maliev.MessagingContracts`

Rules:

* Package must include **exact version numbers**.
* Package must target:
  * `net10.0` (Primary)
  * `net8.0` (Secondary/LTS support if possible)
  * `netstandard2.1` (Legacy support if possible)

Explicitly forbidden dependencies in the *runtime* package:

* MassTransit
* Rebus
* MediatR
* EF Core
* Any cloud SDKs

(Build-time tools like NJsonSchema are allowed in the build process but not as runtime dependencies).

**Rationale:** Consumers must remain lightweight.

---

# **V. Contract Categories and Naming Standards**

Contracts must be organized as follows:

* `contracts/commands/`
* `contracts/events/`
* `contracts/requests/`
* `contracts/responses/`
* `contracts/shared/`

Naming:
* Schema Files: `kebab-case.json` (e.g., `create-order.json`)
* Message Types: `PascalCase` (e.g., `CreateOrderCommand`)

---

# **VI. Topology & Documentation**

* **AsyncAPI**: Defined in `asyncapi/asyncapi.yaml`.
* **Topology**: Exchange/Queue definitions in `topology/`.
* **Documentation**: Generated HTML/Markdown from schemas.

---

# **VII. Contract Evolution Rules (NON-NEGOTIABLE)**

* **Schema Evolution**: Must be additive.
* **Breaking Changes**: Changing types, removing required fields, or renaming fields is forbidden without a Major version bump.

---

# **VIII. Testing Requirements**

* **Schema Validation Tests**: Ensure schemas are valid Draft-07.
* **Round-trip Tests**: Ensure generated C# models serialize/deserialize correctly matching the schema.

---

# **IX. Repository Structure Rules (NON-NEGOTIABLE)**

Root structure:

```
/MessagingContracts.sln
/contracts
    /commands
    /events
    /requests
    /responses
    /shared
/asyncapi
/topology
/generated
    /csharp
/tests
README.md
CONSTITUTION.md
nuget.config
```

---

# **X. CI/CD Standards**

* CI must validate:
  * Schema validity
  * Breaking changes (Schema Diff)
  * C# Compilation
* CI must publish NuGet packages on Main.

---

# **XI. Governance**

* This Constitution supersedes developer preference.
* All PRs must comply before merge.
