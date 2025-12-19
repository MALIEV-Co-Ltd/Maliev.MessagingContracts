using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Text.Json.Nodes;
using Xunit;

namespace Maliev.MessagingContracts.Tests
{
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

        public static IEnumerable<object[]> GetSchemaFiles()
        {
            if (string.IsNullOrEmpty(SchemaRoot)) return Enumerable.Empty<object[]>();

            var schemaFiles = Directory.EnumerateFiles(SchemaRoot, "*.json", SearchOption.AllDirectories)
                .Where(f => !f.Contains("base-message.json") && !f.Contains("shared"));

            return schemaFiles.Select(file => new object[] { file });
        }

        [Theory]
        [MemberData(nameof(GetSchemaFiles))]
        public void All_Contracts_Must_Have_Consumers_If_Specified(string schemaPath)
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
            Assert.True(consumers.Count > 0, $"Schema {Path.GetFileName(schemaPath)} must have at least one consumer.");
        }
    }
}
