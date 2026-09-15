param(
    [string]$ExpectedIssue = ""
)

$ErrorActionPreference = "Stop"

$repoRoot = Resolve-Path (Join-Path $PSScriptRoot "..\..")
$multiRepoRoot = Split-Path $repoRoot -Parent

$repos = @(
    "DocuArchi.Api",
    "DocuArchiCore",
    "DocuArchiCore.Abstractions",
    "DocuArchiCore.Web",
    "MiApp.DTOs",
    "MiApp.Services",
    "MiApp.Repository",
    "MiApp.Models"
)

function Get-RepoStatus {
    param(
        [string]$RepoPath,
        [string]$RepoName,
        [string]$ExpectedIssue
    )

    $gitDir = Join-Path $RepoPath ".git"
    if (-not (Test-Path $gitDir)) {
        return [pscustomobject]@{
            Repo = $RepoName
            Branch = "NO_GIT"
            Dirty = "n/a"
            Pattern = "n/a"
            ExpectedIssue = "n/a"
            Status = "skip"
        }
    }

    $branch = (& git -C $RepoPath rev-parse --abbrev-ref HEAD 2>$null)
    if ($LASTEXITCODE -ne 0 -or [string]::IsNullOrWhiteSpace($branch)) {
        $branch = "unknown"
    }

    $dirtyOutput = (& git -C $RepoPath status --porcelain 2>$null)
    $dirtyLines = @()
    if (-not [string]::IsNullOrWhiteSpace($dirtyOutput)) {
        $dirtyLines = @($dirtyOutput -split "`r?`n" | Where-Object { -not [string]::IsNullOrWhiteSpace($_) })
    }
    $effectiveDirty = @(
        $dirtyLines | Where-Object {
            $line = $_
            $path = if ($line.Length -ge 4) { $line.Substring(3) } else { $line }
            $normalized = $path.Replace('\', '/')
            -not ($normalized -match "^bin/")
        } | Where-Object {
            $line = $_
            $path = if ($line.Length -ge 4) { $line.Substring(3) } else { $line }
            $normalized = $path.Replace('\', '/')
            -not ($normalized -match "^obj/")
        } | Where-Object {
            $line = $_
            $path = if ($line.Length -ge 4) { $line.Substring(3) } else { $line }
            $normalized = $path.Replace('\', '/')
            -not ($normalized -match "^openspec/logs/")
        }
    )
    $dirty = if ($effectiveDirty.Count -eq 0) { "clean" } else { "dirty" }

    $matchesPattern = ($branch -match "^(main|master|[A-Za-z]+-\d+-.+)$")
    $expectedOk = $true
    if (-not [string]::IsNullOrWhiteSpace($ExpectedIssue)) {
        $expectedPrefix = ($ExpectedIssue.ToLowerInvariant() + "-")
        $expectedOk = ($branch.ToLowerInvariant().StartsWith($expectedPrefix) -or $branch -in @("main", "master"))
    }

    $status = if ($matchesPattern -and $expectedOk) { "ok" } else { "warn" }
    return [pscustomobject]@{
        Repo = $RepoName
        Branch = $branch
        Dirty = $dirty
        Pattern = if ($matchesPattern) { "ok" } else { "bad" }
        ExpectedIssue = if ($expectedOk) { "ok" } else { "mismatch" }
        Status = $status
    }
}

$results = foreach ($repo in $repos) {
    $path = Join-Path $multiRepoRoot $repo
    Get-RepoStatus -RepoPath $path -RepoName $repo -ExpectedIssue $ExpectedIssue
}

$results | Format-Table -AutoSize

$warnCount = @($results | Where-Object { $_.Status -eq "warn" }).Count
if ($warnCount -gt 0) {
    Write-Output "Branch audit completed with warnings: $warnCount"
    exit 1
}

Write-Output "Branch audit completed successfully."
