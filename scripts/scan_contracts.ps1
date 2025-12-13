$schemasDir = "contracts/schemas"
$csharpDir = "generated/csharp/Contracts"

if (!(Test-Path $schemasDir)) {
    Write-Error "Schemas directory not found: $schemasDir"
    exit 1
}

$schemas = Get-ChildItem -Path $schemasDir -Recurse -Filter "*.json"

foreach ($schema in $schemas) {
    $relativePath = $schema.FullName.Replace((Get-Item $schemasDir).FullName, "")
    $csharpRelativePath = $relativePath.Replace(".json", ".cs")
    $csharpFile = Join-Path $csharpDir $csharpRelativePath

    if (Test-Path $csharpFile) {
        Write-Host "MATCH: $($schema.Name) -> $($csharpFile)" -ForegroundColor Green
    } else {
        Write-Host "MISSING: $($schema.Name) -> $($csharpFile)" -ForegroundColor Red
    }
}
