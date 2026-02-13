param(
    [Parameter(Mandatory=$true)]
    [string]$Version
)

$ErrorActionPreference = "Stop"

$ProjectName = "Postgirl"
$Runtime = "win-x64"
$Configuration = "Release"

$Root = Resolve-Path "$PSScriptRoot\.."
$ArtifactsPath = Join-Path $Root "Artifacts"
$OutputFolder = "$ProjectName-$Version-$Runtime"
$PublishPath = Join-Path $ArtifactsPath $OutputFolder
$ZipPath = "$PublishPath.zip"

Write-Host "=== Postgirl Release Build ==="
Write-Host "Version: $Version"
Write-Host "Runtime: $Runtime"
Write-Host ""

# Clean artifacts
if (Test-Path $PublishPath) {
    Remove-Item $PublishPath -Recurse -Force
}

if (Test-Path $ZipPath) {
    Remove-Item $ZipPath -Force
}

# Ensure artifacts folder exists
if (!(Test-Path $ArtifactsPath)) {
    New-Item -ItemType Directory -Path $ArtifactsPath | Out-Null
}

# Publish
dotnet publish `
    -c $Configuration `
    -r $Runtime `
    --self-contained true `
    -p:Version=$Version `
    -o $PublishPath

if ($LASTEXITCODE -ne 0) {
    throw "Publish failed."
}

Write-Host "Publish completed."

# Create ZIP
Compress-Archive `
    -Path "$PublishPath\*" `
    -DestinationPath $ZipPath

Write-Host "ZIP created at: $ZipPath"
Write-Host ""
Write-Host "=== Release Build Finished Successfully ==="
