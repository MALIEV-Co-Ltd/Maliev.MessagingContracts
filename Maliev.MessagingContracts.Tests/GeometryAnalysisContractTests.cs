using System.Text.Json;
using System.Text.Json.Serialization;
using Maliev.MessagingContracts.Contracts.Geometry;

namespace Maliev.MessagingContracts.Tests;

/// <summary>
/// Verifies the generated contracts against the complete MassTransit envelopes emitted by GeometryService.
/// </summary>
public class GeometryAnalysisContractTests
{
    /// <summary>
    /// The early metrics envelope preserves both BFF consumers and null body defaults.
    /// </summary>
    [Fact]
    public void FileMetricsReady_DeserializePythonEnvelope_PreservesConsumersAndNullBodyDefaults()
    {
        const string json = """
            {
              "messageId": "7f08d221-65b3-42a8-a394-88b169157612",
              "correlationId": "b88bcb26-a294-4ea2-835d-a78bce21f0db",
              "conversationId": null,
              "sourceAddress": null,
              "destinationAddress": null,
              "messageType": ["urn:message:Maliev.MessagingContracts.Contracts.Geometry:FileMetricsReadyEvent"],
              "headers": {},
              "message": {
                "messageId": "16563020-df3b-4efa-92c8-b3911d831a03",
                "messageName": "FileMetricsReadyEvent",
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
                  "storagePath": "uploads/file-123.step",
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
                  "bodyCount": 1,
                  "bodies": null
                }
              }
            }
            """;

        var envelope = JsonSerializer.Deserialize<MassTransitEnvelope<FileMetricsReadyEvent>>(json);

        Assert.NotNull(envelope);
        AssertEnvelope(
            envelope,
            "urn:message:Maliev.MessagingContracts.Contracts.Geometry:FileMetricsReadyEvent",
            envelope.Message.CorrelationId);
        Assert.Equal(new[] { "IntranetBff", "QuoteEngineBff" }, envelope.Message.ConsumedBy);
        Assert.Equal("file-123", envelope.Message.Payload.FileId);
        Assert.Equal("uploads/file-123.step", envelope.Message.Payload.StoragePath);
        Assert.Equal(12.5, envelope.Message.Payload.Metrics.VolumeCm3);
        Assert.Equal(1, envelope.Message.Payload.BodyCount);
        Assert.Null(envelope.Message.Payload.Bodies);
    }

    /// <summary>
    /// The failure envelope preserves both BFF consumers and safe internal failure details.
    /// </summary>
    [Fact]
    public void FileAnalysisFailed_DeserializePythonEnvelope_PreservesConsumersAndFailureDetails()
    {
        const string json = """
            {
              "messageId": "7f08d221-65b3-42a8-a394-88b169157612",
              "correlationId": "b88bcb26-a294-4ea2-835d-a78bce21f0db",
              "conversationId": null,
              "sourceAddress": null,
              "destinationAddress": null,
              "messageType": ["urn:message:Maliev.MessagingContracts.Contracts.Geometry:FileAnalysisFailedEvent"],
              "headers": {},
              "message": {
                "messageId": "16563020-df3b-4efa-92c8-b3911d831a03",
                "messageName": "FileAnalysisFailedEvent",
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
                  "storagePath": "uploads/file-123.step",
                  "errorCode": "ANALYSIS_FAILED",
                  "details": "Geometry worker could not read the uploaded model."
                }
              }
            }
            """;

        var envelope = JsonSerializer.Deserialize<MassTransitEnvelope<FileAnalysisFailedEvent>>(json);

        Assert.NotNull(envelope);
        AssertEnvelope(
            envelope,
            "urn:message:Maliev.MessagingContracts.Contracts.Geometry:FileAnalysisFailedEvent",
            envelope.Message.CorrelationId);
        Assert.Equal(new[] { "IntranetBff", "QuoteEngineBff" }, envelope.Message.ConsumedBy);
        Assert.Equal("file-123", envelope.Message.Payload.FileId);
        Assert.Equal("ANALYSIS_FAILED", envelope.Message.Payload.ErrorCode);
        Assert.Equal(
            "Geometry worker could not read the uploaded model.",
            envelope.Message.Payload.Details);
    }

    /// <summary>
    /// The DFM envelope preserves both BFF consumers and null defaults emitted on timeout and failure paths.
    /// </summary>
    [Fact]
    public void DfmAnalysisReady_DeserializePythonEnvelope_PreservesConsumersAndNullDefaults()
    {
        const string json = """
            {
              "messageId": "7f08d221-65b3-42a8-a394-88b169157612",
              "correlationId": "b88bcb26-a294-4ea2-835d-a78bce21f0db",
              "conversationId": null,
              "sourceAddress": null,
              "destinationAddress": null,
              "messageType": ["urn:message:Maliev.MessagingContracts.Contracts.Geometry:DfmAnalysisReadyEvent"],
              "headers": {},
              "message": {
                "messageId": "16563020-df3b-4efa-92c8-b3911d831a03",
                "messageName": "DfmAnalysisReadyEvent",
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
                  "storagePath": "uploads/file-123.step",
                  "fdmReport": null,
                  "slaReport": null,
                  "cncReport": null,
                  "analyzedAt": "2026-07-13T12:00:00Z",
                  "overlayPaths": null,
                  "bodyCount": null,
                  "nonManifoldReason": null,
                  "nonManifoldFaceCount": null
                }
              }
            }
            """;

        var envelope = JsonSerializer.Deserialize<MassTransitEnvelope<DfmAnalysisReadyEvent>>(json);

        Assert.NotNull(envelope);
        AssertEnvelope(
            envelope,
            "urn:message:Maliev.MessagingContracts.Contracts.Geometry:DfmAnalysisReadyEvent",
            envelope.Message.CorrelationId);
        Assert.Equal(new[] { "IntranetBff", "QuoteEngineBff" }, envelope.Message.ConsumedBy);
        Assert.Equal("file-123", envelope.Message.Payload.FileId);
        Assert.Equal("uploads/file-123.step", envelope.Message.Payload.StoragePath);
        Assert.Null(envelope.Message.Payload.FdmReport);
        Assert.Null(envelope.Message.Payload.SlaReport);
        Assert.Null(envelope.Message.Payload.CncReport);
        Assert.Null(envelope.Message.Payload.OverlayPaths);
        Assert.Null(envelope.Message.Payload.BodyCount);
        Assert.Null(envelope.Message.Payload.NonManifoldReason);
        Assert.Null(envelope.Message.Payload.NonManifoldFaceCount);
    }

    private static void AssertEnvelope<TMessage>(
        MassTransitEnvelope<TMessage> envelope,
        string expectedMessageType,
        Guid innerCorrelationId)
    {
        Assert.Equal(new Guid("7f08d221-65b3-42a8-a394-88b169157612"), envelope.MessageId);
        Assert.Equal(new Guid("b88bcb26-a294-4ea2-835d-a78bce21f0db"), envelope.CorrelationId);
        Assert.NotEqual(Guid.Empty, envelope.CorrelationId);
        Assert.Equal(envelope.CorrelationId, innerCorrelationId);
        Assert.Null(envelope.ConversationId);
        Assert.Null(envelope.SourceAddress);
        Assert.Null(envelope.DestinationAddress);
        Assert.Equal(new[] { expectedMessageType }, envelope.MessageType);
        Assert.Empty(envelope.Headers);
    }

    private sealed record MassTransitEnvelope<TMessage>(
        [property: JsonPropertyName("messageId")] Guid MessageId,
        [property: JsonPropertyName("correlationId")] Guid CorrelationId,
        [property: JsonPropertyName("conversationId")] Guid? ConversationId,
        [property: JsonPropertyName("sourceAddress")] string? SourceAddress,
        [property: JsonPropertyName("destinationAddress")] string? DestinationAddress,
        [property: JsonPropertyName("messageType")] IReadOnlyList<string> MessageType,
        [property: JsonPropertyName("headers")] IReadOnlyDictionary<string, string> Headers,
        [property: JsonPropertyName("message")] TMessage Message);
}
