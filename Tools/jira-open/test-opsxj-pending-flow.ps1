$ErrorActionPreference = "Stop"

function Assert-Contains {
    param(
        [string]$Value,
        [string]$Expected
    )
    if (-not $Value.Contains($Expected)) {
        throw "Expected output to contain '$Expected'. Actual: $Value"
    }
}

function Run-Git {
    param(
        [string]$RepoRoot,
        [string[]]$CliArgs
    )
    Push-Location $RepoRoot
    try {
        $previousEap = $ErrorActionPreference
        $ErrorActionPreference = "Continue"
        $output = & git @CliArgs 2>&1
        $ErrorActionPreference = $previousEap
        if ($LASTEXITCODE -ne 0) {
            $text = ($output | ForEach-Object { "$_" }) -join "`n"
            throw "git $($CliArgs -join ' ') failed. $text"
        }
    }
    finally {
        Pop-Location
    }
}

$tempRoot = Join-Path ([System.IO.Path]::GetTempPath()) ("opsxj-pending-test-" + [System.Guid]::NewGuid().ToString("N"))
New-Item -ItemType Directory -Path $tempRoot -Force | Out-Null

try {
    $repoRoot = Join-Path $tempRoot "repo"
    $toolDir = Join-Path $repoRoot "Tools/jira-open"
    New-Item -ItemType Directory -Path $toolDir -Force | Out-Null

    Copy-Item -Path (Join-Path $PSScriptRoot "opsxj.ps1") -Destination (Join-Path $toolDir "opsxj.ps1") -Force

    Set-Content -Path (Join-Path $repoRoot "README.md") -Value "pending test repo" -NoNewline
    Run-Git -RepoRoot $repoRoot -CliArgs @("init")
    Run-Git -RepoRoot $repoRoot -CliArgs @("config", "user.email", "opsxj-test@example.com")
    Run-Git -RepoRoot $repoRoot -CliArgs @("config", "user.name", "opsxj-test")
    Run-Git -RepoRoot $repoRoot -CliArgs @("add", ".")
    Run-Git -RepoRoot $repoRoot -CliArgs @("commit", "-m", "init")
    Run-Git -RepoRoot $repoRoot -CliArgs @("branch", "-M", "main")

    $env:JIRA_BASE_URL = "https://example.atlassian.net"
    $env:JIRA_EMAIL = "bot@example.com"
    $env:JIRA_API_TOKEN = "token"
    $env:JIRA_PROJECT_KEY = "OPSXJ"

    function global:Invoke-RestMethod {
        param(
            [string]$Method,
            [string]$Uri,
            [hashtable]$Headers,
            [string]$Body
        )

        if ($Method -ieq "Get" -and $Uri -like "*/rest/api/3/search*") {
            return @{
                issues = @(
                    @{
                        key = "OPSXJ-10"
                        fields = @{
                            summary = "Ticket pendiente 10"
                            status = @{ name = "In Progress" }
                            assignee = @{ displayName = "A User" }
                            updated = "2026-03-02T10:00:00.000+0000"
                        }
                    },
                    @{
                        key = "OPSXJ-11"
                        fields = @{
                            summary = "Ticket pendiente 11"
                            status = @{ name = "To Do" }
                            assignee = $null
                            updated = "2026-03-02T09:00:00.000+0000"
                        }
                    }
                )
            }
        }

        throw "Unexpected Invoke-RestMethod call: $Method $Uri"
    }

    try {
        $scriptPath = Join-Path $toolDir "opsxj.ps1"
        Push-Location $repoRoot
        try {
            $output = (& $scriptPath jira-pending OPSXJ 2>&1 | Out-String)
            Assert-Contains -Value $output -Expected "Pending Jira issues: 2"
            Assert-Contains -Value $output -Expected "OPSXJ-10"
            Assert-Contains -Value $output -Expected "OPSXJ-11"
        }
        finally {
            Pop-Location
        }
    }
    finally {
        Remove-Item Function:\Invoke-RestMethod -ErrorAction SilentlyContinue
        Remove-Item Env:JIRA_BASE_URL -ErrorAction SilentlyContinue
        Remove-Item Env:JIRA_EMAIL -ErrorAction SilentlyContinue
        Remove-Item Env:JIRA_API_TOKEN -ErrorAction SilentlyContinue
        Remove-Item Env:JIRA_PROJECT_KEY -ErrorAction SilentlyContinue
    }

    Write-Output "PASS: opsxj:jira-pending lists pending issues without side effects."
}
finally {
    Remove-Item -Path $tempRoot -Recurse -Force -ErrorAction SilentlyContinue
}
