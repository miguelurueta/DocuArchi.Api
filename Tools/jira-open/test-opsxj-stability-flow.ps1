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

$tempRoot = Join-Path ([System.IO.Path]::GetTempPath()) ("opsxj-stability-test-" + [System.Guid]::NewGuid().ToString("N"))
New-Item -ItemType Directory -Path $tempRoot -Force | Out-Null

try {
    $repoRoot = Join-Path $tempRoot "repo"
    $toolDir = Join-Path $repoRoot "Tools/jira-open"
    $fakeBin = Join-Path $tempRoot "fake-bin"
    $changeName = "opsxj-777-archive-guard"
    $archiveName = "2026-03-01-$changeName"

    New-Item -ItemType Directory -Path $toolDir -Force | Out-Null
    New-Item -ItemType Directory -Path (Join-Path $repoRoot "openspec/changes/archive/$archiveName") -Force | Out-Null
    New-Item -ItemType Directory -Path (Join-Path $repoRoot "openspec/logs") -Force | Out-Null
    New-Item -ItemType Directory -Path $fakeBin -Force | Out-Null
    Copy-Item -Path (Join-Path $PSScriptRoot "opsxj.ps1") -Destination (Join-Path $toolDir "opsxj.ps1") -Force

    Set-Content -Path (Join-Path $repoRoot "README.md") -Value "stability test repo" -NoNewline
    Run-Git -RepoRoot $repoRoot -CliArgs @("init")
    Run-Git -RepoRoot $repoRoot -CliArgs @("config", "user.email", "opsxj-test@example.com")
    Run-Git -RepoRoot $repoRoot -CliArgs @("config", "user.name", "opsxj-test")
    Run-Git -RepoRoot $repoRoot -CliArgs @("add", ".")
    Run-Git -RepoRoot $repoRoot -CliArgs @("commit", "-m", "init")
    Run-Git -RepoRoot $repoRoot -CliArgs @("branch", "-M", "main")
    Run-Git -RepoRoot $repoRoot -CliArgs @("remote", "add", "origin", "https://github.com/example/repo.git")

    Set-Content -Path (Join-Path $fakeBin "openspec.cmd") -Value "@echo off`r`nexit /b 0" -NoNewline

    $previousPath = $env:PATH
    $env:PATH = "$fakeBin;$previousPath"
    $env:OPSXJ_TEST_SKIP_GIT_PUSH = "1"
    $env:OPSXJ_TEST_FAKE_PR_URL = "https://github.com/example/repo/pull/777"
    $env:GITHUB_TOKEN = "test-token"
    $env:JIRA_BASE_URL = "https://example.atlassian.net"
    $env:JIRA_EMAIL = "bot@example.com"
    $env:JIRA_API_TOKEN = "token"

    function global:Invoke-RestMethod {
        param(
            [string]$Method,
            [string]$Uri,
            [hashtable]$Headers,
            [string]$Body
        )

        if ($Method -ieq "Get" -and $Uri -like "*/rest/api/3/issue/*?fields=summary,description") {
            return @{
                fields = @{
                    summary = "Archive guard"
                    description = @{
                        type = "doc"
                        content = @(
                            @{
                                type = "paragraph"
                                content = @(
                                    @{ type = "text"; text = "Mock description" }
                                )
                            }
                        )
                    }
                }
            }
        }

        throw "Unexpected Invoke-RestMethod call: $Method $Uri"
    }

    try {
        $scriptPath = Join-Path $toolDir "opsxj.ps1"
        Push-Location $repoRoot
        try {
            $firstRun = (& $scriptPath new OPSXJ-777 2>&1 | Out-String)
            Assert-Contains -Value $firstRun -Expected "already archived"

            $secondRun = (& $scriptPath new OPSXJ-777 -Reopen 2>&1 | Out-String)
            Assert-Contains -Value $secondRun -Expected "Reopen flag detected"
            Assert-Contains -Value $secondRun -Expected "Created/updated OpenSpec change: $changeName"

            $lockPath = Join-Path $repoRoot "openspec/logs/OPSXJ-777.lock"
            Set-Content -Path $lockPath -Value "manual lock"
            try {
                & $scriptPath new OPSXJ-777 | Out-Null
                throw "Expected lock error was not raised."
            }
            catch {
                Assert-Contains -Value $_.Exception.Message -Expected "Issue lock is busy for 'OPSXJ-777'"
            }
            finally {
                Remove-Item -Path $lockPath -Force -ErrorAction SilentlyContinue
            }
        }
        finally {
            Pop-Location
        }
    }
    finally {
        Remove-Item Function:\Invoke-RestMethod -ErrorAction SilentlyContinue
        $env:PATH = $previousPath
        Remove-Item Env:OPSXJ_TEST_SKIP_GIT_PUSH -ErrorAction SilentlyContinue
        Remove-Item Env:OPSXJ_TEST_FAKE_PR_URL -ErrorAction SilentlyContinue
        Remove-Item Env:GITHUB_TOKEN -ErrorAction SilentlyContinue
        Remove-Item Env:JIRA_BASE_URL -ErrorAction SilentlyContinue
        Remove-Item Env:JIRA_EMAIL -ErrorAction SilentlyContinue
        Remove-Item Env:JIRA_API_TOKEN -ErrorAction SilentlyContinue
    }

    Write-Output "PASS: opsxj stability guards (archive idempotency, reopen, lock) work."
}
finally {
    Remove-Item -Path $tempRoot -Recurse -Force -ErrorAction SilentlyContinue
}
