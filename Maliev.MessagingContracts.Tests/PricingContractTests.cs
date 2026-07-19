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
    /// The additive v2 event preserves enough commercial factors to reconstruct a multi-unit total.
    /// </summary>
    [Fact]
    public void PriceCalculatedEventV2_RoundTrip_PreservesVariableAndFixedDfmAllocation()
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
        var breakdown = roundTrip.Payload.Breakdown;
        Assert.Equal(50d, breakdown.SetupCost);
        Assert.Equal(10d, breakdown.VariableDfmSurcharge);
        Assert.Equal(5d, breakdown.FixedDfmSurcharge);
        Assert.Equal(
            breakdown.SubtotalBeforeMargin,
            (breakdown.MaterialCost
            + breakdown.SupportCost
            + breakdown.MachineTimeCost
            + breakdown.VariableDfmSurcharge
            + breakdown.ComplexitySurcharge) * roundTrip.Payload.Quantity
            + breakdown.SetupCost
            + breakdown.FixedDfmSurcharge);
        Assert.Equal(
            breakdown.SubtotalBeforeMargin * (breakdown.MarginMultiplier - 1d),
            breakdown.MarginAmount,
            precision: 8);

        var variableUnitCost = breakdown.MaterialCost
            + breakdown.SupportCost
            + breakdown.MachineTimeCost
            + breakdown.VariableDfmSurcharge
            + breakdown.ComplexitySurcharge;
        var fixedLineCost = breakdown.SetupCost + breakdown.FixedDfmSurcharge;
        var discountedVariableUnitPrice = variableUnitCost
            * breakdown.MarginMultiplier
            * (1d - breakdown.VolumeDiscountPercent / 100d);
        var marginedFixedLineCost = fixedLineCost * breakdown.MarginMultiplier;
        var rawBaseCurrencyLineTotal = (discountedVariableUnitPrice * roundTrip.Payload.Quantity
            + marginedFixedLineCost)
            * breakdown.LeadTimeMultiplier
            * breakdown.ToleranceMultiplier;
        var flooredBaseCurrencyLineTotal = Math.Max(
            rawBaseCurrencyLineTotal,
            breakdown.MinimumOrderPriceFloorThb);
        var reconstructedTotal = flooredBaseCurrencyLineTotal * breakdown.ExchangeRate;

        Assert.Equal(899.514d, rawBaseCurrencyLineTotal, precision: 8);
        Assert.Equal(30d, reconstructedTotal, precision: 8);
        Assert.Equal(reconstructedTotal, roundTrip.Payload.TotalPrice, precision: 8);
        Assert.Equal(roundTrip.Payload.TotalPrice, breakdown.TotalPrice, precision: 8);
        Assert.Equal(reconstructedTotal / roundTrip.Payload.Quantity, roundTrip.Payload.TotalUnitPrice, precision: 8);
        Assert.Equal("THB", breakdown.BaseCurrency);
        Assert.Contains("\"variableDfmSurcharge\":10", json, StringComparison.Ordinal);
        Assert.Contains("\"fixedDfmSurcharge\":5", json, StringComparison.Ordinal);
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
            Quantity: 3,
            InputVolumeCm3: 100,
            InputSupportVolumeCm3: 5,
            InputSurfaceAreaCm2: 60,
            Strategy: "RuleBased",
            MlModelVersion: null,
            ConfidenceLevel: 1,
            PricingConfigurationId: Guid.NewGuid(),
            Breakdown: new PriceCalculatedEventV2PayloadBreakdown(
                MaterialCost: 100,
                SupportCost: 20,
                MachineTimeCost: 80,
                SetupCost: 50,
                VariableDfmSurcharge: 10,
                FixedDfmSurcharge: 5,
                ComplexitySurcharge: 10,
                SubtotalBeforeMargin: 715,
                MarginAmount: 143,
                MarginMultiplier: 1.2,
                VolumeDiscountPercent: 10,
                LeadTimeMultiplier: 1.1,
                ToleranceMultiplier: 1.05,
                MinimumOrderPriceFloorThb: 1000,
                ExchangeRate: 0.03,
                BaseCurrency: "THB",
                TotalPrice: 30),
            TotalUnitPrice: 10,
            TotalPrice: 30,
            Currency: "USD",
            ValidUntil: calculatedAt.AddDays(7),
            CalculatedAt: calculatedAt,
            StoragePath: "quotes/part.stl",
            EstimatedLeadTimeDays: 3);
}
