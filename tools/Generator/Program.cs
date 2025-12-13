using NJsonSchema;
using NJsonSchema.CodeGeneration.CSharp;
using System.Text.Json;

var contractsPath = Path.Combine(Directory.GetCurrentDirectory(), "contracts", "schemas");
var outputPath = Path.Combine(Directory.GetCurrentDirectory(), "generated", "csharp", "Contracts");

if (!Directory.Exists(contractsPath))
{
    Console.WriteLine($"Error: Contracts path not found at {contractsPath}");
    return;
}

foreach (var file in Directory.GetFiles(contractsPath, "*.json", SearchOption.AllDirectories))
{
    var schema = await JsonSchema.FromFileAsync(file);
    var schemaName = schema.Title ?? Path.GetFileNameWithoutExtension(file);
    
    // Determine namespace based on folder structure
    var relativePath = Path.GetRelativePath(contractsPath, Path.GetDirectoryName(file));
    var subNamespace = relativePath.Replace(Path.DirectorySeparatorChar, '.').Replace("schemas.", "");
    var fullNamespace = $"Maliev.MessagingContracts.{subNamespace}";

    // Ensure output directory exists
    var outputDir = Path.Combine(outputPath, relativePath);
    Directory.CreateDirectory(outputDir);

    var settings = new CSharpGeneratorSettings
    {
        Namespace = fullNamespace,
        ClassStyle = CSharpClassStyle.Record,
        JsonLibrary = CSharpJsonLibrary.SystemTextJson,
        GenerateOptionalPropertiesAsNullable = true,
        GenerateNullableReferenceTypes = true
    };

    var generator = new CSharpGenerator(schema, settings);
    var code = generator.GenerateFile();

    var outputConfigFile = Path.Combine(outputDir, $"{schemaName}.cs");
    await File.WriteAllTextAsync(outputConfigFile, code);
    
    Console.WriteLine($"Generated {schemaName} -> {outputConfigFile}");
}