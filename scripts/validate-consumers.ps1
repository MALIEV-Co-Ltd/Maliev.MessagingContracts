# scripts/validate-consumers.ps1

$ErrorActionPreference = "Stop"

Write-Host "Validating that all consumers exist..."

$schemaFiles = Get-ChildItem -Path "contracts/schemas" -Recurse -Filter "*.json" | Where-Object {
    $_.Name -ne "base-message.json"
}

$discoveredServices = (Get-Content "specs/001-define-messaging-contracts/discovery-report.md" | Where-Object { $_ -match 'Maliev\..*Service' } | ForEach-Object { 
    $fullName = ($_ -split '\|')[1].Trim()
    $shortName = $fullName -replace '^Maliev\.', ''
    $fullName, $shortName
}) | Select-Object -Unique

$allConsumersExist = $true

foreach ($file in $schemaFiles) {
    $schemaContent = Get-Content -Raw -Path $file.FullName | ConvertFrom-Json
    $consumers = $schemaContent.allOf | Where-Object { $_.properties.consumedBy } | Select-Object -ExpandProperty properties | Select-Object -ExpandProperty consumedBy | Select-Object -ExpandProperty const

    if ($null -ne $consumers) {
        foreach ($consumer in $consumers) {
            if (-not ($discoveredServices -contains $consumer)) {
                Write-Error "Validation failed: Consumer '$consumer' defined in $($file.Name) does not exist in the discovery report."
                $allConsumersExist = $false
            }
        }
    }
}

if ($allConsumersExist) {
    Write-Host "All consumer validations passed."
} else {
    exit 1
}
