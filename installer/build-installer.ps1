param(
    [string]$Configuration = "Release",
    [string]$RuntimeIdentifier = "win-x64",
    [string]$ProjectPath = "Postgirl.csproj",
    [string]$OutputRoot = "Artifacts",
    [switch]$SkipPublish
)

$ErrorActionPreference = "Stop"

function Get-ProjectPropertyValue {
    param(
        [xml]$ProjectXml,
        [string]$PropertyName
    )

    foreach ($propertyGroup in $ProjectXml.Project.PropertyGroup) {
        $property = $propertyGroup.$PropertyName
        if (-not [string]::IsNullOrWhiteSpace($property)) {
            return $property.Trim()
        }
    }

    return $null
}

function Resolve-InnoSetupCompilerPath {
    $command = Get-Command "ISCC.exe" -ErrorAction SilentlyContinue
    if ($null -ne $command) {
        return $command.Source
    }

    $candidatePaths = @(
        (Join-Path ${env:ProgramFiles(x86)} "Inno Setup 7\ISCC.exe"),
        (Join-Path $env:ProgramFiles "Inno Setup 7\ISCC.exe"),
        (Join-Path ${env:ProgramFiles(x86)} "Inno Setup 6\ISCC.exe"),
        (Join-Path $env:ProgramFiles "Inno Setup 6\ISCC.exe")
    )

    foreach ($candidatePath in $candidatePaths) {
        if (-not [string]::IsNullOrWhiteSpace($candidatePath) -and (Test-Path $candidatePath)) {
            return $candidatePath
        }
    }

    throw "Inno Setup compiler not found. Install Inno Setup 7 or 6, or make ISCC.exe available on PATH."
}

function Get-NextInstallerRevision {
    param(
        [string]$RevisionFilePath
    )

    $currentRevision = 0
    if (Test-Path $RevisionFilePath) {
        $content = (Get-Content -Path $RevisionFilePath -Raw).Trim()
        if (-not [string]::IsNullOrWhiteSpace($content)) {
            if (-not [int]::TryParse($content, [ref]$currentRevision)) {
                throw "Invalid installer revision value in $RevisionFilePath"
            }
        }
    }

    $nextRevision = $currentRevision + 1
    Set-Content -Path $RevisionFilePath -Value $nextRevision -NoNewline
    return $nextRevision
}

function Get-InstallerFullVersion {
    param(
        [string]$BaseVersion,
        [int]$Revision
    )

    return "{0}-rev{1:D4}" -f $BaseVersion, $Revision
}

function Get-InstallerOutputBaseFileName {
    param(
        [string]$AppName,
        [string]$FullVersion
    )

    return "{0}-Setup-{1}" -f $AppName, $FullVersion
}

$repoRoot = Split-Path -Path $PSScriptRoot -Parent
$projectFullPath = Join-Path $repoRoot $ProjectPath

if (-not (Test-Path $projectFullPath)) {
    throw "Project file not found: $projectFullPath"
}

[xml]$projectXml = Get-Content -Path $projectFullPath
$appName = Get-ProjectPropertyValue -ProjectXml $projectXml -PropertyName "Product"
if ([string]::IsNullOrWhiteSpace($appName)) {
    $appName = [System.IO.Path]::GetFileNameWithoutExtension($projectFullPath)
}

$appVersion = Get-ProjectPropertyValue -ProjectXml $projectXml -PropertyName "Version"
if ([string]::IsNullOrWhiteSpace($appVersion)) {
    throw "The project does not define a Version property."
}

$appPublisher = Get-ProjectPropertyValue -ProjectXml $projectXml -PropertyName "Company"
if ([string]::IsNullOrWhiteSpace($appPublisher)) {
    $appPublisher = $appName
}

$outputRootFullPath = Join-Path $repoRoot $OutputRoot
$publishDirectory = Join-Path $outputRootFullPath (Join-Path "Publish" $RuntimeIdentifier)
$installerOutputDirectory = Join-Path $outputRootFullPath "Installer"
$installerScriptPath = Join-Path $PSScriptRoot "Postgirl.iss"
$revisionFilePath = Join-Path $installerOutputDirectory "installer-revision.txt"

New-Item -ItemType Directory -Path $publishDirectory -Force | Out-Null
New-Item -ItemType Directory -Path $installerOutputDirectory -Force | Out-Null

$installerRevision = Get-NextInstallerRevision -RevisionFilePath $revisionFilePath
$installerFullVersion = Get-InstallerFullVersion -BaseVersion $appVersion -Revision $installerRevision
$installerOutputBaseFileName = Get-InstallerOutputBaseFileName -AppName $appName -FullVersion $installerFullVersion

if (-not $SkipPublish) {
    & dotnet publish $projectFullPath -c $Configuration -r $RuntimeIdentifier --self-contained true -o $publishDirectory
    if ($LASTEXITCODE -ne 0) {
        throw "dotnet publish failed."
    }
}
elseif (-not (Test-Path (Join-Path $publishDirectory "Postgirl.exe"))) {
    throw "SkipPublish was used, but the publish output was not found at $publishDirectory."
}

$isccPath = Resolve-InnoSetupCompilerPath

& $isccPath $installerScriptPath "/DAppName=$appName" "/DAppVersion=$appVersion" "/DAppFullVersion=$installerFullVersion" "/DAppPublisher=$appPublisher" "/DOutputBaseFileName=$installerOutputBaseFileName" "/DSourceDir=$publishDirectory" "/DOutputDir=$installerOutputDirectory"
if ($LASTEXITCODE -ne 0) {
    throw "Inno Setup compilation failed."
}

$installerPath = Join-Path $installerOutputDirectory ("{0}.exe" -f $installerOutputBaseFileName)
if (-not (Test-Path $installerPath)) {
    throw "Installer was not created at the expected path: $installerPath"
}

Write-Host "Installer created: $installerPath"
Write-Host "Installer revision: $installerRevision"
Write-Host "Installer version: $installerFullVersion"
