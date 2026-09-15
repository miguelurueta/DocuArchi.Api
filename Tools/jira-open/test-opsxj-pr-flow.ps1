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

$tempRoot = Join-Path ([System.IO.Path]::GetTempPath()) ("opsxj-pr-test-" + [System.Guid]::NewGuid().ToString("N"))
New-Item -ItemType Directory -Path $tempRoot -Force | Out-Null

try {
    $repoRoot = Join-Path $tempRoot "repo"
    $toolDir = Join-Path $repoRoot "Tools/jira-open"
    $fakeBin = Join-Path $tempRoot "fake-bin"

    New-Item -ItemType Directory -Path $toolDir -Force | Out-Null
    New-Item -ItemType Directory -Path (Join-Path $repoRoot "openspec/changes") -Force | Out-Null
    New-Item -ItemType Directory -Path $fakeBin -Force | Out-Null

    Copy-Item -Path (Join-Path $PSScriptRoot "opsxj.ps1") -Destination (Join-Path $toolDir "opsxj.ps1") -Force

    Set-Content -Path (Join-Path $repoRoot "README.md") -Value "temp repo" -NoNewline
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
    $env:OPSXJ_TEST_FAKE_PR_URL = "https://github.com/example/repo/pull/123"
    $env:GITHUB_TOKEN = "test-token"

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
                    summary = "OPSXJ-999 mock issue"
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
            $firstRun = (& $scriptPath new OPSXJ-999 2>&1 | Out-String)
            Assert-Contains -Value $firstRun -Expected "Pull request created: https://github.com/example/repo/pull/123"

            $changeName = $null
            if ($firstRun -match "Created/updated OpenSpec change:\s*(?<name>[A-Za-z0-9-]+)") {
                $changeName = $Matches["name"]
            }
            if (-not $changeName) {
                throw "Could not resolve change name from output. Actual: $firstRun"
            }

            $proposalPath = Join-Path $repoRoot "openspec/changes/$changeName/proposal.md"
            if (-not (Test-Path $proposalPath)) {
                throw "Expected proposal file not found: $proposalPath"
            }

            $secondRun = (& $scriptPath new OPSXJ-999 2>&1 | Out-String)
            Assert-Contains -Value $secondRun -Expected "Pull request already exists: https://github.com/example/repo/pull/123"
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
    }

    Write-Output "PASS: opsxj:new creates PR and avoids duplicates."
}
finally {
    Remove-Item -Path $tempRoot -Recurse -Force -ErrorAction SilentlyContinue
}
