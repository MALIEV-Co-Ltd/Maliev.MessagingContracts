using System;

namespace Maliev.MessagingContracts.Contracts.Geometry;

/// <summary>
/// Event published when a file has been analyzed for geometric properties.
/// </summary>
public record FileAnalyzedEvent
{
    public Guid FileId { get; init; }
    public Guid CustomerId { get; init; }

    // Geometry metrics in decimal for precision (matching PricingAuditRecord)
    public decimal VolumeCm3 { get; init; }
    public decimal SupportVolumeCm3 { get; init; }
    public decimal SurfaceAreaCm2 { get; init; }
    public decimal BoundingBoxX { get; init; }
    public decimal BoundingBoxY { get; init; }
    public decimal BoundingBoxZ { get; init; }
    public bool IsManifold { get; init; }
    public int TriangleCount { get; init; }

    public DateTime AnalyzedAt { get; init; } = DateTime.UtcNow;
}
