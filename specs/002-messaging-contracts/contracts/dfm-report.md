# Contract: DfmReport (FileAnalyzedEvent Extension)

**Domain**: Geometry
**Type**: Nested Object (embedded in FileAnalyzedEvent)
**Status**: Draft

## Overview

DfmReport contains Design for Manufacturing (DFM) analysis results embedded within FileAnalyzedEvent. This is an optional field - when DFM analysis fails or is not applicable, the field is null.

## JSON Schema Definition

```json
"dfmReport": {
  "type": ["object", "null"],
  "description": "Optional DFM analysis results. Null if analysis failed or not applicable.",
  "properties": {
    "thinWallCount": {
      "type": "integer",
      "description": "Number of detected thin-wall regions"
    },
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
    "overhangFaceCount": {
      "type": "integer",
      "description": "Number of mesh faces with overhang angle > threshold"
    },
    "overhangAreaCm2": {
      "type": "number",
      "description": "Total projected area (cm²) of overhanging faces"
    }
  },
  "required": ["thinWallCount", "thinWallRegions", "overhangFaceCount", "overhangAreaCm2"]
}
```

## C# Record

```csharp
public record DfmReport
{
    public int ThinWallCount { get; init; }
    public List<decimal[]> ThinWallRegions { get; init; } = new();
    public int OverhangFaceCount { get; init; }
    public decimal OverhangAreaCm2 { get; init; }
}
```

## Usage in FileAnalyzedEvent

```csharp
public DfmReport? DfmReport { get; init; }
```

## Thresholds (Informational)

Enforced by GeometryService, not in contract:
- **Thin Wall**: Faces where local thickness < 0.8 mm
- **Overhang**: Faces where angle from Z-axis > 45°

## Consumer Responsibilities

Consumers MUST handle `null` dfmReport gracefully:
- Do not assume DFM data is always present
- Default to safe behavior when DFM analysis unavailable
- Log warning if DFM data expected but missing
