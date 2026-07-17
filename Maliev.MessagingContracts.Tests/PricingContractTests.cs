using System.Text.Json;
using Maliev.MessagingContracts.Contracts.Pricing;
using Maliev.MessagingContracts.Contracts.Shared;

namespace Maliev.MessagingContracts.Tests;

/// <summary>
/// Compatibility and serialization tests for Pricing domain events.
/// </summary>
public class PricingContractTests
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase
    };

    /// <summary>
    /// The additive v2 event preserves fixed-cost allocation details on a round trip.
    /// </summary>
    [Fact]
    public void PriceCalculatedEventV2_RoundTrip_PreservesFixedDfmAllocation()
    {
        var occurredAt = DateTimeOffset.Parse("2026-07-17T01:00:00Z");
        var message = new PriceCalculatedEventV2(
            MessageId: Guid.NewGuid(),
            MessageName: nameof(PriceCalculatedEventV2),
            MessageType: MessageType.Event,
            MessageVersion: "2.0.0",
            PublishedBy: "PricingService",
            ConsumedBy: [],
            CorrelationId: Guid.NewGuid(),
            CausationId: null,
            OccurredAtUtc: occurredAt,
            IsPublic: false,
            Payload: CreateV2Payload(occurredAt));

        var json = JsonSerializer.Serialize(message, JsonOptions);
        var roundTrip = JsonSerializer.Deserialize<PriceCalculatedEventV2>(json, JsonOptions);

        Assert.NotNull(roundTrip);
        Assert.Equal("2.0.0", roundTrip.MessageVersion);
        Assert.Equal(25d, roundTrip.Payload.Breakdown.SetupCost);
        Assert.Equal(1.25d, roundTrip.Payload.Breakdown.FixedDfmSurcharge);
        Assert.Contains("\"fixedDfmSurcharge\":1.25", json, StringComparison.Ordinal);
    }

    /// <summary>
    /// The v1 breakdown retains its original wire fields and does not acquire the v2 allocation field.
    /// </summary>
    [Fact]
    public void PriceCalculatedEventV1_Serialization_RemainsUnchanged()
    {
        var breakdown = new PriceCalculatedEventPayloadBreakdown(
            MaterialCost: 10,
            SupportCost: 2,
            MachineTimeCost: 30,
            SetupCost: 25,
            ComplexitySurcharge: 4,
            SubtotalBeforeMargin: 71,
            MarginAmount: 14.2,
            TotalPrice: 85.2);

        using var document = JsonDocument.Parse(JsonSerializer.Serialize(breakdown, JsonOptions));
        var propertyNames = document.RootElement.EnumerateObject()
            .Select(property => property.Name)
            .Order(StringComparer.Ordinal)
            .ToArray();

        Assert.Equal(
            new[]
            {
                "complexitySurcharge",
                "machineTimeCost",
                "marginAmount",
                "materialCost",
                "setupCost",
                "subtotalBeforeMargin",
                "supportCost",
                "totalPrice"
            },
            propertyNames);
        Assert.DoesNotContain("fixedDfmSurcharge", propertyNames);
    }

    private static PriceCalculatedEventV2Payload CreateV2Payload(DateTimeOffset calculatedAt) =>
        new(
            PricingAuditId: Guid.NewGuid(),
            QuotationId: null,
            FileId: Guid.NewGuid(),
            CustomerId: Guid.NewGuid(),
            MaterialId: Guid.NewGuid(),
            ProcessId: Guid.NewGuid(),
            Quantity: 5,
            InputVolumeCm3: 100,
            InputSupportVolumeCm3: 5,
            InputSurfaceAreaCm2: 60,
            Strategy: "RuleBased",
            MlModelVersion: null,
            ConfidenceLevel: 1,
            PricingConfigurationId: Guid.NewGuid(),
            Breakdown: new PriceCalculatedEventV2PayloadBreakdown(
                MaterialCost: 10,
                SupportCost: 2,
                MachineTimeCost: 30,
                SetupCost: 25,
                FixedDfmSurcharge: 1.25,
                ComplexitySurcharge: 4,
                SubtotalBeforeMargin: 72.25,
                MarginAmount: 14.45,
                TotalPrice: 86.70),
            TotalUnitPrice: 17.34,
            TotalPrice: 86.70,
            Currency: "THB",
            ValidUntil: calculatedAt.AddDays(7),
            CalculatedAt: calculatedAt,
            StoragePath: "quotes/part.stl",
            EstimatedLeadTimeDays: 3);
}
