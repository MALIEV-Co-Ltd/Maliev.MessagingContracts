using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Text.Json.Nodes;
using Xunit;

namespace Maliev.MessagingContracts.Tests
{
    /// <summary>
    /// Tests for validating JSON schema contracts.
    /// </summary>
    public class ValidationTests
    {
        private static string FindSchemaRoot()
        {
            var current = Directory.GetCurrentDirectory();
            while (current != null && !Directory.Exists(Path.Combine(current, "contracts", "schemas")))
            {
                current = Path.GetDirectoryName(current);
            }
            return current != null ? Path.Combine(current, "contracts", "schemas") : "";
        }

        private static readonly string SchemaRoot = FindSchemaRoot();

        /// <summary>
        /// Gets all JSON schema files for testing.
        /// </summary>
        public static IEnumerable<object[]> GetSchemaFiles()
        {
            if (string.IsNullOrEmpty(SchemaRoot)) return Enumerable.Empty<object[]>();

            var schemaFiles = Directory.EnumerateFiles(SchemaRoot, "*.json", SearchOption.AllDirectories)
                .Where(f => !f.Contains("base-message.json") && !f.Contains("shared"));

            return schemaFiles.Select(file => new object[] { file });
        }

        /// <summary>
        /// Tests that only events may be declared before their first consumer is implemented.
        /// </summary>
        [Theory]
        [MemberData(nameof(GetSchemaFiles))]
        public void Contracts_May_Have_No_Consumers_Only_For_Events(string schemaPath)
        {
            var schemaContent = File.ReadAllText(schemaPath);
            var jsonNode = JsonNode.Parse(schemaContent);
            Assert.NotNull(jsonNode);

            var allOf = jsonNode["allOf"] as JsonArray;
            if (allOf == null) return; // Skip if not using allOf structure for now

            var consumedByNode = allOf.Select(n => n?["properties"]?["consumedBy"]?["const"]).FirstOrDefault(n => n != null);
            if (consumedByNode == null) return;

            var consumers = consumedByNode as JsonArray;
            Assert.NotNull(consumers);

            var messageType = allOf
                .Select(n => n?["properties"]?["messageType"]?["const"]?.GetValue<string>())
                .FirstOrDefault(value => value != null);
            if (string.Equals(messageType, "Event", StringComparison.Ordinal))
            {
                return;
            }

            Assert.True(
                consumers.Count > 0,
                $"Schema {Path.GetFileName(schemaPath)} is a routed {messageType ?? "contract"} and must have at least one consumer.");
        }
    }
}
