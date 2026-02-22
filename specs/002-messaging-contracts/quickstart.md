# Quickstart: New Messaging Contracts

**Feature**: 002-messaging-contracts
**Date**: 2026-02-21

## Prerequisites

- .NET 10 SDK installed
- Node.js 18+ (for npm test)
- PowerShell 7+ (for build scripts)

## Implementation Order

1. **Phase 1**: Extend FileAnalyzedEvent (Geometry)
2. **Phase 2**: Create JobStartedEvent (Jobs)
3. **Phase 3**: Create MaterialLowStockEvent (Inventory)
4. **Phase 4**: Verify and version bump

---

## Phase 1: Extend FileAnalyzedEvent

### Step 1.1: Update JSON Schema

Edit `contracts/schemas/geometry/geometry-events.json`:

Add `dfmReport` property to `FileAnalyzedEvent`:

```json
"dfmReport": {
  "type": ["object", "null"],
  "description": "Optional DFM analysis results. Null if analysis failed.",
  "properties": {
    "thinWallCount": { "type": "integer", "description": "Number of detected thin-wall regions" },
    "thinWallRegions": {
      "type": "array",
      "description": "Centroid coordinates (mm) of each thin-wall region",
      "items": {
        "type": "array",
        "items": { "type": "number" },
        "minItems": 3,
        "maxItems": 3
      }
    },
    "overhangFaceCount": { "type": "integer", "description": "Number of overhanging mesh faces" },
    "overhangAreaCm2": { "type": "number", "description": "Total projected overhang area in cm²" }
  },
  "required": ["thinWallCount", "thinWallRegions", "overhangFaceCount", "overhangAreaCm2"]
}
```

### Step 1.2: Update C# Record

Edit `generated/csharp/Contracts/FileAnalyzedEvent.cs`:

```csharp
public record DfmReport
{
    public int ThinWallCount { get; init; }
    public List<decimal[]> ThinWallRegions { get; init; } = new();
    public int OverhangFaceCount { get; init; }
    public decimal OverhangAreaCm2 { get; init; }
}

public DfmReport? DfmReport { get; init; }
```

---

## Phase 2: Create JobStartedEvent

### Step 2.1: Create Schema Directory

```powershell
mkdir contracts/schemas/jobs
```

### Step 2.2: Create Schema File

Create `contracts/schemas/jobs/job-events.json` with JobStartedEvent definition.

### Step 2.3: Create C# Directory

```powershell
mkdir generated/csharp/Contracts/Jobs
```

### Step 2.4: Create C# Record

Create `generated/csharp/Contracts/Jobs/JobStartedEvent.cs`.

### Step 2.5: Update AsyncAPI

Add to `asyncapi/asyncapi.yaml`:
- Channel: `job/started`
- Message: `JobStartedEvent`

---

## Phase 3: Create MaterialLowStockEvent

### Step 3.1: Create Schema Directory

```powershell
mkdir contracts/schemas/inventory
```

### Step 3.2: Create Schema File

Create `contracts/schemas/inventory/inventory-events.json` with MaterialLowStockEvent definition.

### Step 3.3: Create C# Directory

```powershell
mkdir generated/csharp/Contracts/Inventory
```

### Step 3.4: Create C# Record

Create `generated/csharp/Contracts/Inventory/MaterialLowStockEvent.cs`.

### Step 3.5: Update AsyncAPI

Add to `asyncapi/asyncapi.yaml`:
- Channel: `inventory/material-low-stock`
- Message: `MaterialLowStockEvent`

---

## Phase 4: Verify & Version

### Step 4.1: Validate Schemas

```bash
npm test
```

Expected: All tests pass

### Step 4.2: Build

```bash
dotnet build Maliev.MessagingContracts.slnx
```

Expected: 0 errors, 0 warnings

### Step 4.3: Run Tests

```bash
dotnet test
```

Expected: All tests pass

### Step 4.4: Bump Version

Edit `generated/csharp/Maliev.MessagingContracts.csproj`:
- Increment patch version (e.g., 1.2.0 → 1.2.1)

---

## Validation Checklist

- [ ] JSON schemas valid (`npm test`)
- [ ] C# compiles with zero warnings
- [ ] All tests pass (`dotnet test`)
- [ ] AsyncAPI updated with new channels
- [ ] Version bumped in .csproj
