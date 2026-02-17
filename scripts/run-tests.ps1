param(
    [Parameter(Mandatory = $true, Position = 0)]
    [ValidateSet("unit", "integration", "all")]
    [string]$Suite
)

$ErrorActionPreference = "Stop"
$root = Split-Path -Parent $PSScriptRoot

# Map suite to project(s)
$projects = switch ($Suite) {
    "unit"        { @("ECommerce.Tests") }
    "integration" { @("ECommerce.IntegrationTests") }
    "all"         { @("ECommerce.Tests", "ECommerce.IntegrationTests") }
}

$coverageDir  = Join-Path $root "coverage"
$reportDir    = Join-Path $coverageDir "report"
$settingsFile = Join-Path $root "coverlet.runsettings"

# Clean previous results
if (Test-Path $coverageDir) { Remove-Item $coverageDir -Recurse -Force }

Write-Host "`n=== Running $Suite tests ===" -ForegroundColor Cyan

foreach ($proj in $projects) {
    $projPath = Join-Path $root $proj
    Write-Host "`n--- $proj ---" -ForegroundColor Yellow

    dotnet test $projPath `
        --collect:"XPlat Code Coverage" `
        --results-directory $coverageDir `
        --settings $settingsFile

    if ($LASTEXITCODE -ne 0) {
        Write-Host "`n$proj failed." -ForegroundColor Red
        exit $LASTEXITCODE
    }
}

# Find all coverage files
$coverageFiles = Get-ChildItem -Path $coverageDir -Filter "coverage.cobertura.xml" -Recurse
if ($coverageFiles.Count -eq 0) {
    Write-Host "`nNo coverage files generated." -ForegroundColor Red
    exit 1
}

$reports = ($coverageFiles | ForEach-Object { $_.FullName }) -join ";"

Write-Host "`n=== Generating HTML report ===" -ForegroundColor Cyan

reportgenerator `
    -reports:$reports `
    -targetdir:$reportDir `
    -reporttypes:Html

Write-Host "`n=== Report ready ===" -ForegroundColor Green
Write-Host "$reportDir\index.htm"

Start-Process "$reportDir\index.htm"
