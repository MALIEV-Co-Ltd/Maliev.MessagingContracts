using System.Text.Json;
using System.Text.Json.Serialization;
using Maliev.MessagingContracts.Contracts.Geometry;

namespace Maliev.MessagingContracts.Tests;

/// <summary>
/// Verifies compatibility with the MassTransit envelope emitted by GeometryService.
/// </summary>
public class FileAnalyzedContractTests
{
    /// <summary>
    /// The generated contract accepts the Python publisher shape while keeping legacy context defaults non-authoritative.
    /// </summary>
    [Fact]
    public void Deserialize_PythonMassTransitEnvelopeWithoutLegacyContext_PreservesAuthoritativeGeometry()
    {
        const string json = """
            {
              "messageId": "7f08d221-65b3-42a8-a394-88b169157612",
              "correlationId": "b88bcb26-a294-4ea2-835d-a78bce21f0db",
              "messageType": [
                "urn:message:Maliev.MessagingContracts.Contracts.Geometry:FileAnalyzedEvent"
              ],
              "headers": {},
              "message": {
                "messageId": "16563020-df3b-4efa-92c8-b3911d831a03",
                "messageName": "FileAnalyzedEvent",
                "messageType": "Event",
                "messageVersion": "1.0.0",
                "publishedBy": "GeometryService",
                "consumedBy": ["IntranetBff", "QuoteEngineBff"],
                "correlationId": "b88bcb26-a294-4ea2-835d-a78bce21f0db",
                "causationId": null,
                "occurredAtUtc": "2026-07-13T12:00:00Z",
                "isPublic": false,
                "payload": {
                  "fileId": "file-123",
                  "metrics": {
                    "volumeCm3": 12.5,
                    "supportVolumeCm3": 1.25,
                    "surfaceAreaCm2": 42.75,
                    "boundingBox": { "x": 10, "y": 20, "z": 30 },
                    "isManifold": true,
                    "triangleCount": 120,
                    "eulerNumber": 2,
                    "nonManifoldReason": null,
                    "nonManifoldFaceCount": null
                  },
                  "processedAt": "2026-07-13T12:00:00Z",
                  "glbStoragePath": "processed/file-123.glb",
                  "viewerStoragePath": "processed/file-123.glb",
                  "viewerFileExtension": ".glb",
                  "thumbnailStoragePath": null,
                  "storagePath": "uploads/file-123.stl",
                  "dfmReport": null,
                  "bodyCount": 1,
                  "bodies": []
                }
              }
            }
            """;

        var envelope = JsonSerializer.Deserialize<MassTransitEnvelope<FileAnalyzedEvent>>(json);

        Assert.NotNull(envelope);
        Assert.Equal(
            new[] { "IntranetBff", "QuoteEngineBff" },
            envelope.Message.ConsumedBy);
        Assert.Equal("file-123", envelope.Message.Payload.FileId);
        Assert.Equal(12.5, envelope.Message.Payload.Metrics.VolumeCm3);
        Assert.Equal("uploads/file-123.stl", envelope.Message.Payload.StoragePath);
        Assert.Equal(Guid.Empty, envelope.Message.Payload.CustomerId);
        Assert.Equal(Guid.Empty, envelope.Message.Payload.MaterialId);
        Assert.Equal(string.Empty, envelope.Message.Payload.MaterialCode);
        Assert.Equal(Guid.Empty, envelope.Message.Payload.ManufacturingProcessId);
        Assert.Equal(string.Empty, envelope.Message.Payload.ManufacturingProcessName);
    }

    private sealed record MassTransitEnvelope<TMessage>(
        [property: JsonPropertyName("message")] TMessage Message);
}
