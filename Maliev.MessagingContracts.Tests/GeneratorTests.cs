using Generator;
using Microsoft.Extensions.Logging.Abstractions;

namespace Maliev.MessagingContracts.Tests;

/// <summary>
/// Regression tests for the schema-to-C# generator.
/// </summary>
public class GeneratorTests
{
    /// <summary>
    /// Definitions-based payloads expose a compatibility constructor when ProviderName is schema-optional.
    /// </summary>
    [Fact]
    public async Task GenerateAsync_DefinitionPayloadWithOptionalProviderName_EmitsCompatibilityConstructor()
    {
        const string schema = """
            {
              "definitions": {
                "OptionalProviderEvent": {
                  "type": "object",
                  "properties": {
                    "requiredId": { "type": "string", "format": "uuid" },
                    "providerName": { "type": "string", "default": "" }
                  },
                  "required": ["requiredId"]
                },
                "RequiredProviderEvent": {
                  "type": "object",
                  "properties": {
                    "requiredEventId": { "type": "string", "format": "uuid" },
                    "providerName": { "type": "string" }
                  },
                  "required": ["requiredEventId", "providerName"]
                }
              }
            }
            """;

        var source = await GenerateDomainAsync("orders", "provider-events.json", schema);

        Assert.Contains(
            "public OptionalProviderEventPayload(System.Guid RequiredId) : this(RequiredId, string.Empty) { }",
            source,
            StringComparison.Ordinal);
        Assert.DoesNotContain(
            "public RequiredProviderEventPayload(System.Guid RequiredEventId) :",
            source,
            StringComparison.Ordinal);
        Assert.Contains(
            "public void Deconstruct(out System.Guid RequiredId)",
            source,
            StringComparison.Ordinal);
        Assert.Contains(
            "RequiredId = this.RequiredId;",
            source,
            StringComparison.Ordinal);
        Assert.DoesNotContain(
            "public void Deconstruct(out System.Guid RequiredEventId)",
            source,
            StringComparison.Ordinal);
    }

    /// <summary>
    /// Nested payloads on BaseMessage-derived schemas honor the same optional ProviderName rule.
    /// </summary>
    [Fact]
    public async Task GenerateAsync_DerivedPayloadWithOptionalProviderName_EmitsCompatibilityConstructor()
    {
        const string schema = """
            {
              "$schema": "http://json-schema.org/draft-07/schema#",
              "title": "OptionalProviderEvent",
              "allOf": [{ "$ref": "../shared/base-message.json" }],
              "type": "object",
              "properties": {
                "payload": {
                  "type": "object",
                  "properties": {
                    "requiredId": { "type": "string", "format": "uuid" },
                    "providerName": { "type": "string", "default": "" }
                  },
                  "required": ["requiredId"]
                }
              },
              "required": ["payload"]
            }
            """;

        var source = await GenerateDomainAsync("payments", "optional-provider-event.json", schema);

        Assert.Contains(
            "public OptionalProviderEventPayload(System.Guid RequiredId) : this(RequiredId, string.Empty) { }",
            source,
            StringComparison.Ordinal);
        Assert.Contains(
            "public void Deconstruct(out System.Guid RequiredId)",
            source,
            StringComparison.Ordinal);
    }

    /// <summary>
    /// A present required keyword must be an array; wrong container kinds fail closed.
    /// </summary>
    [Theory]
    [InlineData("null")]
    [InlineData("{ }")]
    [InlineData("\"requiredId\"")]
    public async Task GenerateAsync_RequiredUsesWrongContainerKind_ThrowsClearSchemaError(string requiredJson)
    {
        var schema = $$"""
            {
              "definitions": {
                "MalformedRequiredEvent": {
                  "type": "object",
                  "properties": {
                    "requiredId": { "type": "string", "format": "uuid" },
                    "providerName": { "type": "string", "default": "" }
                  },
                  "required": {{requiredJson}}
                }
              }
            }
            """;

        var exception = await Assert.ThrowsAsync<InvalidDataException>(
            () => GenerateDomainAsync("orders", "malformed-required-event.json", schema));

        Assert.Contains(
            "Schema 'required' must be an array when present.",
            exception.Message,
            StringComparison.Ordinal);
    }

    /// <summary>
    /// Malformed non-string required entries fail closed before optional compatibility members are emitted.
    /// </summary>
    [Fact]
    public async Task GenerateAsync_RequiredArrayContainsNonStringEntry_ThrowsClearSchemaError()
    {
        const string schema = """
            {
              "definitions": {
                "MalformedRequiredEvent": {
                  "type": "object",
                  "properties": {
                    "requiredId": { "type": "string", "format": "uuid" },
                    "providerName": { "type": "string", "default": "" }
                  },
                  "required": ["requiredId", 42]
                }
              }
            }
            """;

        var exception = await Assert.ThrowsAsync<InvalidDataException>(
            () => GenerateDomainAsync("orders", "malformed-required-event.json", schema));

        Assert.Contains(
            "Schema 'required' entries must be non-empty strings.",
            exception.Message,
            StringComparison.Ordinal);
    }

    private static async Task<string> GenerateDomainAsync(string domain, string fileName, string schema)
    {
        var tempRoot = Path.Combine(Path.GetTempPath(), $"messaging-contract-generator-{Guid.NewGuid():N}");
        var schemaRoot = Path.Combine(tempRoot, "contracts", "schemas");
        var domainRoot = Path.Combine(schemaRoot, domain);
        Directory.CreateDirectory(domainRoot);

        try
        {
            await File.WriteAllTextAsync(Path.Combine(domainRoot, fileName), schema);
            var generator = new ManualCSharpGenerator(
                schemaRoot,
                NullLogger<ManualCSharpGenerator>.Instance);
            var generated = await generator.GenerateAsync();
            var pascalDomain = char.ToUpperInvariant(domain[0]) + domain[1..];

            return generated[Path.Combine(pascalDomain, $"{pascalDomain}Events.cs")];
        }
        finally
        {
            Directory.Delete(tempRoot, recursive: true);
        }
    }
}
