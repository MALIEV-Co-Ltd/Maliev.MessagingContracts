using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace Maliev.MessagingContracts.Contracts.Geometry;

/// <summary>
/// DFM (Design for Manufacturing) analysis results.
/// </summary>
public record DfmReport
{
    [JsonPropertyName("thinWallCount")]
    public int ThinWallCount { get; init; }

    [JsonPropertyName("thinWallRegions")]
    public List<decimal[]> ThinWallRegions { get; init; } = new();

    [JsonPropertyName("overhangFaceCount")]
    public int OverhangFaceCount { get; init; }

    [JsonPropertyName("overhangAreaCm2")]
    public decimal OverhangAreaCm2 { get; init; }

    public DfmReport() { }

    public DfmReport(int thinWallCount, List<decimal[]> thinWallRegions, int overhangFaceCount, decimal overhangAreaCm2)
    {
        ThinWallCount = thinWallCount;
        ThinWallRegions = thinWallRegions;
        OverhangFaceCount = overhangFaceCount;
        OverhangAreaCm2 = overhangAreaCm2;
    }
}

/// <summary>
/// Event published when a file has been analyzed for geometric properties.
/// </summary>
public record FileAnalyzedEvent
{
    public Guid FileId { get; init; }
    public Guid CustomerId { get; init; }

    public decimal VolumeCm3 { get; init; }
    public decimal SupportVolumeCm3 { get; init; }
    public decimal SurfaceAreaCm2 { get; init; }
    public decimal BoundingBoxX { get; init; }
    public decimal BoundingBoxY { get; init; }
    public decimal BoundingBoxZ { get; init; }
    public bool IsManifold { get; init; }
    public int TriangleCount { get; init; }

    public string? GlbStoragePath { get; init; }
    public string? ThumbnailStoragePath { get; init; }

    [JsonPropertyName("dfmReport")]
    public DfmReport? DfmReport { get; init; }

    public DateTime AnalyzedAt { get; init; } = DateTime.UtcNow;
}

