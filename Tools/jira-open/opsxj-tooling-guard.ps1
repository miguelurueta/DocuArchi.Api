$ErrorActionPreference = "Stop"

param(
    [string]$RequiredKey = "Maria20230126*",
    [string]$RequiredModeMarker = "opsx:explore"
)

function Get-ChangedFiles {
    $eventName = [string]$env:GITHUB_EVENT_NAME
    $eventPath = [string]$env:GITHUB_EVENT_PATH

    if ($eventName -eq "pull_request" -and -not [string]::IsNullOrWhiteSpace([string]$env:GITHUB_BASE_REF)) {
        $baseRef = [string]$env:GITHUB_BASE_REF
        & git fetch origin $baseRef --depth=1 | Out-Null
        $lines = & git diff --name-only "origin/$baseRef...HEAD"
        return @($lines | Where-Object { -not [string]::IsNullOrWhiteSpace($_) })
    }

    if ($eventName -eq "push" -and -not [string]::IsNullOrWhiteSpace($eventPath) -and (Test-Path $eventPath)) {
        $payload = Get-Content -Raw -Path $eventPath | ConvertFrom-Json
        $before = [string]$payload.before
        $after = [string]$payload.after
        if (-not [string]::IsNullOrWhiteSpace($before) -and $before -ne "0000000000000000000000000000000000000000") {
            $lines = & git diff --name-only "$before..$after"
            return @($lines | Where-Object { -not [string]::IsNullOrWhiteSpace($_) })
        }
    }

    $fallback = & git diff --name-only "HEAD~1..HEAD" 2>$null
    return @($fallback | Where-Object { -not [string]::IsNullOrWhiteSpace($_) })
}

function Get-EvidenceText {
    $parts = New-Object System.Collections.Generic.List[string]

    $eventPath = [string]$env:GITHUB_EVENT_PATH
    $eventName = [string]$env:GITHUB_EVENT_NAME
    if (-not [string]::IsNullOrWhiteSpace($eventPath) -and (Test-Path $eventPath)) {
        $payload = Get-Content -Raw -Path $eventPath | ConvertFrom-Json
        if ($eventName -eq "pull_request" -and $payload.pull_request) {
            $parts.Add([string]$payload.pull_request.title)
            $parts.Add([string]$payload.pull_request.body)
        }
    }

    if ($eventName -eq "pull_request" -and -not [string]::IsNullOrWhiteSpace([string]$env:GITHUB_BASE_REF)) {
        $baseRef = [string]$env:GITHUB_BASE_REF
        $logs = & git log --format=%B "origin/$baseRef...HEAD"
        $parts.Add(($logs -join "`n"))
    }
    else {
        $logs = & git log -n 20 --format=%B
        $parts.Add(($logs -join "`n"))
    }

    return (($parts | Where-Object { -not [string]::IsNullOrWhiteSpace($_) }) -join "`n")
}

$changedFiles = Get-ChangedFiles
$toolingTouched = @($changedFiles | Where-Object { $_ -match "^Tools/jira-open/" })
if ($toolingTouched.Count -eq 0) {
    Write-Output "Guard: no changes in Tools/jira-open detected."
    exit 0
}

$evidenceText = Get-EvidenceText
$hasKey = $evidenceText -match [regex]::Escape($RequiredKey)
$hasMode = $evidenceText.ToLowerInvariant() -match [regex]::Escape($RequiredModeMarker.ToLowerInvariant())

if (-not $hasMode) {
    throw "Guard blocked tooling change: include '$RequiredModeMarker' in PR/commit metadata to confirm OpenSpec explore mode."
}

if (-not $hasKey) {
    throw "Guard blocked tooling change: include authorization key '$RequiredKey' in PR/commit metadata."
}

Write-Output "Guard passed for Tools/jira-open changes."
