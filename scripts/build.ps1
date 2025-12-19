# Fail fast on any error
$ErrorActionPreference = "Stop"

# This script now delegates all generation logic to the C# Generator tool.

Write-Host "Running C# code generator..."
dotnet run --project tools/Generator/Generator.csproj