using System.Reflection;
using System.Text.Json;
using Maliev.MessagingContracts.Contracts.Orders;
using Maliev.MessagingContracts.Contracts.Payments;

namespace Maliev.MessagingContracts.Tests;

/// <summary>
/// Protects the v1 ProviderName schema, constructor, and JSON compatibility contract.
/// </summary>
public class ProviderNameContractTests
{
    private static readonly string SchemaRoot = FindSchemaRoot();

    /// <summary>
    /// Payloads whose v1 schema makes ProviderName optional support both constructor shapes.
    /// </summary>
    [Theory]
    [MemberData(nameof(OptionalProviderNamePayloadTypes))]
    public void OptionalProviderNamePayload_ExposesConstructorsWithAndWithoutProviderName(Type payloadType)
    {
        var constructors = payloadType
            .GetConstructors(BindingFlags.Instance | BindingFlags.Public)
            .Where(constructor => constructor.GetParameters().Length > 0)
            .ToArray();

        Assert.Contains(constructors, HasProviderNameParameter);
        Assert.Contains(constructors, constructor => !HasProviderNameParameter(constructor));
    }

    /// <summary>
    /// Payloads whose v1 schema requires ProviderName never expose a non-default constructor that omits it.
    /// </summary>
    [Theory]
    [MemberData(nameof(RequiredProviderNamePayloadTypes))]
    public void RequiredProviderNamePayload_NonDefaultConstructorsRequireProviderName(Type payloadType)
    {
        var constructors = payloadType
            .GetConstructors(BindingFlags.Instance | BindingFlags.Public)
            .Where(constructor => constructor.GetParameters().Length > 0)
            .ToArray();

        Assert.NotEmpty(constructors);
        Assert.All(constructors, constructor => Assert.True(
            HasProviderNameParameter(constructor),
            $"{payloadType.Name} exposes a non-default constructor that omits ProviderName."));
    }

    /// <summary>
    /// Optional ProviderName schemas explicitly document their v1 default and required schemas remain strict.
    /// </summary>
    [Theory]
    [MemberData(nameof(ProviderNameSchemaCases))]
    public void ProviderNameSchema_DeclaresCanonicalV1Rule(
        string relativePath,
        string definitionName,
        bool expectedRequired)
    {
        using var document = JsonDocument.Parse(File.ReadAllText(Path.Combine(SchemaRoot, relativePath)));
        var payload = string.IsNullOrEmpty(definitionName)
            ? document.RootElement.GetProperty("properties").GetProperty("payload")
            : document.RootElement.GetProperty("definitions").GetProperty(definitionName);
        var providerName = payload.GetProperty("properties").GetProperty("providerName");
        var required = payload.GetProperty("required")
            .EnumerateArray()
            .Any(item => string.Equals(item.GetString(), "providerName", StringComparison.Ordinal));

        Assert.Equal(expectedRequired, required);
        Assert.True(
            providerName.TryGetProperty("description", out var descriptionElement),
            $"{relativePath} must document the ProviderName v1 compatibility rule.");
        var description = descriptionElement.GetString();
        Assert.Contains(expectedRequired ? "Required" : "Optional", description, StringComparison.OrdinalIgnoreCase);

        if (expectedRequired)
        {
            Assert.False(providerName.TryGetProperty("default", out _));
        }
        else
        {
            Assert.True(providerName.TryGetProperty("default", out var defaultValue));
            Assert.Equal(string.Empty, defaultValue.GetString());
        }
    }

    /// <summary>
    /// Existing v1 JSON without ProviderName still deserializes, while the field keeps its canonical wire name.
    /// </summary>
    [Fact]
    public void OptionalProviderNamePayload_PreservesV1JsonCompatibility()
    {
        var withoutProviderName = $$"""
            {
              "orderId": "{{Guid.NewGuid():D}}",
              "orderNumber": "ORD-1001",
              "customerId": "customer-1001",
              "paymentId": "{{Guid.NewGuid():D}}",
              "amount": 1500.0,
              "currency": "THB"
            }
            """;

        var deserialized = JsonSerializer.Deserialize<PaymentCompletedEventPayload>(withoutProviderName);
        Assert.NotNull(deserialized);
        Assert.Equal(string.Empty, deserialized.ProviderName);

        var json = JsonSerializer.Serialize(deserialized with { ProviderName = "omise" });
        using var document = JsonDocument.Parse(json);
        Assert.Equal("omise", document.RootElement.GetProperty("providerName").GetString());
    }

    /// <summary>
    /// Gets payloads where ProviderName is optional for v1 compatibility.
    /// </summary>
    public static IEnumerable<object[]> OptionalProviderNamePayloadTypes()
    {
        yield return [typeof(PaymentCreatedEventPayload)];
        yield return [typeof(PaymentCompletedEventPayload)];
        yield return [typeof(PaymentFailedEventPayload)];
        yield return [typeof(OrderPaidEventPayload)];
    }

    /// <summary>
    /// Gets payloads where ProviderName is required by the original v1 schema.
    /// </summary>
    public static IEnumerable<object[]> RequiredProviderNamePayloadTypes()
    {
        yield return [typeof(PaymentCancelledEventPayload)];
        yield return [typeof(PaymentExpiredEventPayload)];
        yield return [typeof(PaymentPendingEventPayload)];
    }

    /// <summary>
    /// Gets the canonical ProviderName schema decisions.
    /// </summary>
    public static IEnumerable<object[]> ProviderNameSchemaCases()
    {
        yield return ["payments/payment-created-event.json", string.Empty, false];
        yield return ["payments/payment-completed-event.json", string.Empty, false];
        yield return ["payments/payment-failed-event.json", string.Empty, false];
        yield return ["orders/order-events.json", "OrderPaidEvent", false];
        yield return ["payments/payment-cancelled-event.json", string.Empty, true];
        yield return ["payments/payment-expired-event.json", string.Empty, true];
        yield return ["payments/payment-pending-event.json", string.Empty, true];
    }

    private static bool HasProviderNameParameter(ConstructorInfo constructor) =>
        constructor.GetParameters().Any(parameter =>
            string.Equals(parameter.Name, "ProviderName", StringComparison.OrdinalIgnoreCase));

    private static string FindSchemaRoot()
    {
        var current = Directory.GetCurrentDirectory();
        while (current is not null && !Directory.Exists(Path.Combine(current, "contracts", "schemas")))
        {
            current = Path.GetDirectoryName(current);
        }

        return current is not null
            ? Path.Combine(current, "contracts", "schemas")
            : throw new DirectoryNotFoundException("Could not locate contracts/schemas.");
    }
}
