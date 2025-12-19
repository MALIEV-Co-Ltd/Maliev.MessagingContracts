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
        private static readonly string SchemaRoot = Path.Combine("..", "..", "..", "contracts", "schemas");

        public static IEnumerable<object[]> GetSchemaFiles()
        {
            var schemaFiles = Directory.EnumerateFiles(SchemaRoot, "*.json", SearchOption.AllDirectories)
                .Where(f => !f.Contains("base-message.json") && !f.Contains("shared"));

            foreach (var file in schemaFiles)
            {
                yield return new object[] { file };
            }
        }

        [Theory]
        [MemberData(nameof(GetSchemaFiles))]
        public void All_Contracts_Must_Have_Consumers(string schemaPath)
        {
            var schemaContent = File.ReadAllText(schemaPath);
            var jsonNode = JsonNode.Parse(schemaContent);
            
            var allOf = jsonNode["allOf"] as JsonArray;
            Assert.NotNull(allOf);

            var consumedByNode = allOf.Select(n => n["properties"]?["consumedBy"]?["const"]).FirstOrDefault(n => n != null);
            Assert.NotNull(consumedByNode);

            var consumers = consumedByNode as JsonArray;
            Assert.NotNull(consumers);
            Assert.True(consumers.Count > 0, $"Schema {Path.GetFileName(schemaPath)} must have at least one consumer.");
        }
    }
}
