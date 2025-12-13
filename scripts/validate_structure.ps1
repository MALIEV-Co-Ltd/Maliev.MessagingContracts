$requiredDirs = @(
    "contracts/schemas/events",
    "contracts/schemas/commands",
    "contracts/schemas/shared",
    "asyncapi",
    "topology",
    "generated/csharp",
    "tests"
)

$missing = @()
foreach ($dir in $requiredDirs) {
    if (!(Test-Path $dir)) {
        $missing += $dir
    }
}

if ($missing.Count -gt 0) {
    Write-Error "Missing directories: $($missing -join ', ')"
    exit 1
} else {
    Write-Host "Folder structure validation PASSED" -ForegroundColor Green
}
