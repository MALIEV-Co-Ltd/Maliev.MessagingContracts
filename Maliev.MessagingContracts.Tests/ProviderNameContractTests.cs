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
    /// Optional ProviderName payload constructors exactly match their full and compatibility API shapes.
    /// </summary>
    [Theory]
    [MemberData(nameof(OptionalProviderNamePayloadTypes))]
    public void OptionalProviderNamePayload_ConstructorsMatchExactDeconstructSignatures(Type payloadType)
    {
        var constructors = GetNonDefaultConstructors(payloadType);
        var deconstructMethods = GetDeclaredDeconstructMethods(payloadType);

        Assert.Equal(2, constructors.Length);
        Assert.Equal(2, deconstructMethods.Length);
        Assert.All(constructors, constructor => Assert.Contains(
            deconstructMethods,
            method => HasMatchingParameterSignature(constructor, method)));
    }

    /// <summary>
    /// Required ProviderName payload constructors exactly match their sole full API shape.
    /// </summary>
    [Theory]
    [MemberData(nameof(RequiredProviderNamePayloadTypes))]
    public void RequiredProviderNamePayload_ConstructorMatchesExactFullDeconstructSignature(Type payloadType)
    {
        var constructor = Assert.Single(GetNonDefaultConstructors(payloadType));
        var deconstructMethod = Assert.Single(GetDeclaredDeconstructMethods(payloadType));

        Assert.True(
            HasMatchingParameterSignature(constructor, deconstructMethod),
            $"{payloadType.Name} constructor does not match its exact full Deconstruct signature.");
    }

    /// <summary>
    /// Optional ProviderName payloads retain the full package Deconstruct and the exact provider-omitting shape.
    /// </summary>
    [Fact]
    public void OptionalProviderNamePayload_DeconstructSignaturesPreserveBothApiShapes()
    {
        AssertDeconstructSignatures(
            typeof(PaymentCreatedEventPayload),
            [
                new("TransactionId", typeof(Guid)),
                new("IdempotencyKey", typeof(string)),
                new("Amount", typeof(double)),
                new("Currency", typeof(string)),
                new("CustomerId", typeof(string)),
                new("OrderId", typeof(string)),
                new("ProviderName", typeof(string))
            ],
            [
                new("TransactionId", typeof(Guid)),
                new("IdempotencyKey", typeof(string)),
                new("Amount", typeof(double)),
                new("Currency", typeof(string)),
                new("CustomerId", typeof(string)),
                new("OrderId", typeof(string))
            ]);
        AssertDeconstructSignatures(
            typeof(PaymentCompletedEventPayload),
            [
                new("OrderId", typeof(Guid)),
                new("OrderNumber", typeof(string)),
                new("CustomerId", typeof(string)),
                new("PaymentId", typeof(Guid)),
                new("Amount", typeof(double)),
                new("Currency", typeof(string)),
                new("ProviderName", typeof(string))
            ],
            [
                new("OrderId", typeof(Guid)),
                new("OrderNumber", typeof(string)),
                new("CustomerId", typeof(string)),
                new("PaymentId", typeof(Guid)),
                new("Amount", typeof(double)),
                new("Currency", typeof(string))
            ]);
        AssertDeconstructSignatures(
            typeof(PaymentFailedEventPayload),
            [
                new("TransactionId", typeof(Guid)),
                new("IdempotencyKey", typeof(string)),
                new("Amount", typeof(double)),
                new("Currency", typeof(string)),
                new("CustomerId", typeof(string)),
                new("OrderId", typeof(string)),
                new("ProviderName", typeof(string)),
                new("ErrorMessage", typeof(string)),
                new("ProviderErrorCode", typeof(string)),
                new("FailedAt", typeof(DateTimeOffset))
            ],
            [
                new("TransactionId", typeof(Guid)),
                new("IdempotencyKey", typeof(string)),
                new("Amount", typeof(double)),
                new("Currency", typeof(string)),
                new("CustomerId", typeof(string)),
                new("OrderId", typeof(string)),
                new("ErrorMessage", typeof(string)),
                new("ProviderErrorCode", typeof(string)),
                new("FailedAt", typeof(DateTimeOffset))
            ]);
        AssertDeconstructSignatures(
            typeof(OrderPaidEventPayload),
            [
                new("OrderId", typeof(Guid)),
                new("OrderNumber", typeof(string)),
                new("PaymentId", typeof(Guid)),
                new("PaidAmount", typeof(double)),
                new("Currency", typeof(string)),
                new("PaidAt", typeof(DateTimeOffset)),
                new("ProviderName", typeof(string))
            ],
            [
                new("OrderId", typeof(Guid)),
                new("OrderNumber", typeof(string)),
                new("PaymentId", typeof(Guid)),
                new("PaidAmount", typeof(double)),
                new("Currency", typeof(string)),
                new("PaidAt", typeof(DateTimeOffset))
            ]);
    }

    /// <summary>
    /// Required ProviderName payloads expose only their full compiler-generated Deconstruct signature.
    /// </summary>
    [Fact]
    public void RequiredProviderNamePayload_DeconstructSignaturesRemainFullOnly()
    {
        AssertSingleDeconstructSignature(
            typeof(PaymentCancelledEventPayload),
            [
                new("TransactionId", typeof(Guid)),
                new("IdempotencyKey", typeof(string)),
                new("Amount", typeof(double)),
                new("Currency", typeof(string)),
                new("CustomerId", typeof(string)),
                new("OrderId", typeof(string)),
                new("ProviderName", typeof(string)),
                new("Reason", typeof(string)),
                new("ProviderEventCode", typeof(string)),
                new("CancelledAt", typeof(DateTimeOffset))
            ]);
        AssertSingleDeconstructSignature(
            typeof(PaymentExpiredEventPayload),
            [
                new("TransactionId", typeof(Guid)),
                new("IdempotencyKey", typeof(string)),
                new("Amount", typeof(double)),
                new("Currency", typeof(string)),
                new("CustomerId", typeof(string)),
                new("OrderId", typeof(string)),
                new("ProviderName", typeof(string)),
                new("Reason", typeof(string)),
                new("ProviderEventCode", typeof(string)),
                new("ExpiredAt", typeof(DateTimeOffset))
            ]);
        AssertSingleDeconstructSignature(
            typeof(PaymentPendingEventPayload),
            [
                new("TransactionId", typeof(Guid)),
                new("IdempotencyKey", typeof(string)),
                new("Amount", typeof(double)),
                new("Currency", typeof(string)),
                new("CustomerId", typeof(string)),
                new("OrderId", typeof(string)),
                new("ProviderName", typeof(string)),
                new("ProviderEventCode", typeof(string)),
                new("PendingAt", typeof(DateTimeOffset))
            ]);
    }

    /// <summary>
    /// The legacy PaymentCompleted deconstruction remains source-compatible and ordered.
    /// </summary>
    [Fact]
    public void PaymentCompletedEventPayload_LegacyDeconstruction_CompilesInOriginalOrder()
    {
        var expectedOrderId = Guid.NewGuid();
        var expectedPaymentId = Guid.NewGuid();
        var payload = new PaymentCompletedEventPayload(
            expectedOrderId,
            "ORD-1001",
            "customer-1001",
            expectedPaymentId,
            1500,
            "THB",
            "omise");

        var (orderId, orderNumber, customerId, paymentId, amount, currency) = payload;

        Assert.Equal(expectedOrderId, orderId);
        Assert.Equal("ORD-1001", orderNumber);
        Assert.Equal("customer-1001", customerId);
        Assert.Equal(expectedPaymentId, paymentId);
        Assert.Equal(1500, amount);
        Assert.Equal("THB", currency);
    }

    /// <summary>
    /// The legacy OrderPaid deconstruction remains source-compatible and ordered.
    /// </summary>
    [Fact]
    public void OrderPaidEventPayload_LegacyDeconstruction_CompilesInOriginalOrder()
    {
        var expectedOrderId = Guid.NewGuid();
        var expectedPaymentId = Guid.NewGuid();
        var expectedPaidAt = DateTimeOffset.UtcNow;
        var payload = new OrderPaidEventPayload(
            expectedOrderId,
            "ORD-1001",
            expectedPaymentId,
            1500,
            "THB",
            expectedPaidAt,
            "omise");

        var (orderId, orderNumber, paymentId, paidAmount, currency, paidAt) = payload;

        Assert.Equal(expectedOrderId, orderId);
        Assert.Equal("ORD-1001", orderNumber);
        Assert.Equal(expectedPaymentId, paymentId);
        Assert.Equal(1500, paidAmount);
        Assert.Equal("THB", currency);
        Assert.Equal(expectedPaidAt, paidAt);
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
    /// Existing OrderPaid v1 JSON without ProviderName still deserializes with the canonical default.
    /// </summary>
    [Fact]
    public void OrderPaidEventPayload_OmittedProviderName_PreservesV1JsonCompatibility()
    {
        var withoutProviderName = $$"""
            {
              "orderId": "{{Guid.NewGuid():D}}",
              "orderNumber": "ORD-1001",
              "paymentId": "{{Guid.NewGuid():D}}",
              "paidAmount": 1500.0,
              "currency": "THB",
              "paidAt": "{{DateTimeOffset.UtcNow:O}}"
            }
            """;

        var deserialized = JsonSerializer.Deserialize<OrderPaidEventPayload>(withoutProviderName);
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

    private static ConstructorInfo[] GetNonDefaultConstructors(Type payloadType) =>
        payloadType
            .GetConstructors(BindingFlags.Instance | BindingFlags.Public)
            .Where(constructor => constructor.GetParameters().Length > 0)
            .ToArray();

    private static bool HasMatchingParameterSignature(ConstructorInfo constructor, MethodInfo deconstructMethod)
    {
        var constructorParameters = constructor.GetParameters();
        var deconstructParameters = deconstructMethod.GetParameters();
        if (constructorParameters.Length != deconstructParameters.Length)
        {
            return false;
        }

        for (var index = 0; index < constructorParameters.Length; index++)
        {
            if (!deconstructParameters[index].IsOut ||
                constructorParameters[index].ParameterType != deconstructParameters[index].ParameterType.GetElementType() ||
                !string.Equals(
                    constructorParameters[index].Name,
                    deconstructParameters[index].Name,
                    StringComparison.Ordinal))
            {
                return false;
            }
        }

        return true;
    }

    private static void AssertDeconstructSignatures(
        Type payloadType,
        ParameterSignature[] fullSignature,
        ParameterSignature[] compatibilitySignature)
    {
        var methods = GetDeclaredDeconstructMethods(payloadType);

        Assert.Equal(2, methods.Length);
        Assert.Contains(methods, method => HasExactSignature(method, fullSignature));
        Assert.Contains(methods, method => HasExactSignature(method, compatibilitySignature));
    }

    private static void AssertSingleDeconstructSignature(Type payloadType, ParameterSignature[] expectedSignature)
    {
        var method = Assert.Single(GetDeclaredDeconstructMethods(payloadType));

        Assert.True(
            HasExactSignature(method, expectedSignature),
            $"{payloadType.Name}.Deconstruct does not match the expected full signature.");
    }

    private static MethodInfo[] GetDeclaredDeconstructMethods(Type payloadType) =>
        payloadType
            .GetMethods(BindingFlags.Instance | BindingFlags.Public | BindingFlags.DeclaredOnly)
            .Where(method => string.Equals(method.Name, "Deconstruct", StringComparison.Ordinal))
            .ToArray();

    private static bool HasExactSignature(MethodInfo method, ParameterSignature[] expectedSignature)
    {
        var parameters = method.GetParameters();
        if (method.ReturnType != typeof(void) || parameters.Length != expectedSignature.Length)
        {
            return false;
        }

        for (var index = 0; index < parameters.Length; index++)
        {
            var actualType = parameters[index].ParameterType.GetElementType();
            if (!parameters[index].IsOut ||
                actualType != expectedSignature[index].Type ||
                !string.Equals(parameters[index].Name, expectedSignature[index].Name, StringComparison.Ordinal))
            {
                return false;
            }
        }

        return true;
    }

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

    private readonly record struct ParameterSignature(string Name, Type Type);
}
