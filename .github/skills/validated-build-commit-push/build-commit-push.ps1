[CmdletBinding()]
param(
    [Parameter(Mandatory = $true)]
    [ValidateNotNullOrEmpty()]
    [string]$CommitMessage
)

Set-StrictMode -Version Latest
$ErrorActionPreference = 'Stop'

function Invoke-NativeCommand {
    param(
        [Parameter(Mandatory = $true)]
        [scriptblock]$Command,

        [Parameter(Mandatory = $true)]
        [string]$ErrorMessage
    )

    & $Command

    if ($LASTEXITCODE -ne 0) {
        throw $ErrorMessage
    }
}

Write-Host 'Building project...'

try {
    Invoke-NativeCommand -Command { dotnet build .\Postgirl.csproj --nologo } -ErrorMessage 'Build failed. Review the compiler errors above for file and line details.'

    Write-Host 'Staging all changes...'
    Invoke-NativeCommand -Command { git add --all } -ErrorMessage 'Failed to stage repository changes.'

    & git diff --cached --quiet

    switch ($LASTEXITCODE) {
        0 {
            throw 'There are no changes to commit.'
        }
        1 {
        }
        default {
            throw 'Failed to inspect staged changes.'
        }
    }

    Write-Host 'Creating commit...'
    Invoke-NativeCommand -Command { git commit -m $CommitMessage } -ErrorMessage 'Commit failed.'

    $currentBranch = (git branch --show-current).Trim()

    if ($LASTEXITCODE -ne 0 -or [string]::IsNullOrWhiteSpace($currentBranch)) {
        throw 'Failed to determine the current branch.'
    }

    & git rev-parse --abbrev-ref --symbolic-full-name '@{u}' *> $null

    switch ($LASTEXITCODE) {
        0 {
            Write-Host "Pushing branch '$currentBranch'..."
            Invoke-NativeCommand -Command { git push } -ErrorMessage 'Push failed.'
        }
        128 {
            Write-Host "Pushing branch '$currentBranch' and setting upstream on origin..."
            Invoke-NativeCommand -Command { git push --set-upstream origin $currentBranch } -ErrorMessage 'Push failed while setting upstream.'
        }
        default {
            throw 'Failed to inspect upstream branch configuration.'
        }
    }

    Write-Host 'Done.'
}
catch {
    Write-Error $_
    exit 1
}
