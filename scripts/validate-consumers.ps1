# scripts/validate-consumers.ps1
# Validates that every consumer service ID declared in any schema's consumedBy[] field
# corresponds to a real Maliev service directory in the monorepo.
#
# Discovers services dynamically from the parent directory — no pre-generated report file needed.

$ErrorActionPreference = "Stop"

Write-Host "Validating consumers against discovered services..."

# ── 1. Locate the monorepo root (parent of this repository) ─────────────────
$scriptDir   = Split-Path -Parent $MyInvocation.MyCommand.Path
$contractsDir = Resolve-Path (Join-Path $scriptDir "..")      # Maliev.MessagingContracts root
$monorepoRoot = Resolve-Path (Join-Path $contractsDir "..")   # b:\maliev

# ── 2. Discover all Maliev service identifiers dynamically ───────────────────
# A service directory is any sibling directory that contains at least one .csproj file.
# Service ID = directory name                  e.g. "Maliev.JobService"
# Short ID   = without "Maliev." prefix        e.g. "JobService"
$discoveredServiceIds = [System.Collections.Generic.HashSet[string]]::new([StringComparer]::OrdinalIgnoreCase)

Get-ChildItem -Path $monorepoRoot -Directory | ForEach-Object {
    $hasProjects = (Get-ChildItem -Path $_.FullName -Filter "*.csproj" -Recurse -ErrorAction SilentlyContinue | Select-Object -First 1)
    if ($hasProjects) {
        $fullName  = $_.Name                            # Maliev.JobService
        $shortName = $fullName -replace '^Maliev\.', '' # JobService
        [void]$discoveredServiceIds.Add($fullName)
        [void]$discoveredServiceIds.Add($shortName)
    }
}

Write-Host "Discovered $($discoveredServiceIds.Count / 2) services in monorepo."

# ── 3. Schemas exempt from consumedBy validation ─────────────────────────────
$exemptSchemas = @(
    "base-message.json", "envelope.json", "command.json",
    "domain-event.json", "integration-event.json", "notification-event.json"
)

# ── 4. Validate each schema's consumedBy entries ─────────────────────────────
$schemaFiles = Get-ChildItem -Path (Join-Path $contractsDir "contracts/schemas") -Recurse -Filter "*.json" |
    Where-Object { $exemptSchemas -notcontains $_.Name }

$allValid      = $true
$warningCount  = 0
$errorCount    = 0

foreach ($file in $schemaFiles) {
    $schemaContent = Get-Content -Raw -Path $file.FullName | ConvertFrom-Json -ErrorAction SilentlyContinue
    if ($null -eq $schemaContent) { continue }

    $consumers = $null

    # Pattern A: allOf[].properties.consumedBy.const (e.g. commands/create-user-command.json)
    if ($null -ne $schemaContent.allOf) {
        $consumers = $schemaContent.allOf |
            Where-Object { $_.properties.consumedBy } |
            Select-Object -ExpandProperty properties |
            Select-Object -ExpandProperty consumedBy |
            Select-Object -ExpandProperty const
    }
    # Pattern B: definitions.{EventName}.properties.consumedBy.const (e.g. geometry-events.json)
    elseif ($null -ne $schemaContent.definitions) {
        $consumerList = @()
        foreach ($def in $schemaContent.definitions.PSObject.Properties) {
            $props = $def.Value.properties
            if ($null -ne $props -and $null -ne $props.consumedBy) {
                $constVal = $props.consumedBy.const
                if ($null -ne $constVal) {
                    $consumerList += ,@($constVal) | ForEach-Object { $_ }
                }
            }
        }
        if ($consumerList.Count -gt 0) { $consumers = $consumerList }
    }

    if ($null -eq $consumers -or $consumers.Count -eq 0) {
        Write-Warning "[WARN] $($file.Name): consumedBy is empty. No consumers registered yet."
        $warningCount++
        continue
    }

    foreach ($consumer in $consumers) {
        if (-not $discoveredServiceIds.Contains($consumer)) {
            Write-Error "[ERROR] Consumer '$consumer' declared in '$($file.Name)' does not match any known Maliev service. Valid IDs are short names like 'JobService' or full names like 'Maliev.JobService'."
            $allValid    = $false
            $errorCount++
        }
    }
}

# ── 5. Report ─────────────────────────────────────────────────────────────────
Write-Host ""
Write-Host "Consumer validation complete."
Write-Host "  Warnings : $warningCount (empty consumedBy - advisory only)"
Write-Host "  Errors   : $errorCount"

if (-not $allValid) {
    Write-Host "[FAIL] One or more consumer IDs are invalid. See errors above." -ForegroundColor Red
    exit 1
}

Write-Host "[PASS] All declared consumer IDs are valid." -ForegroundColor Green
