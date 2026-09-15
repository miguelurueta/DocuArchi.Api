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

$tempRoot = Join-Path ([System.IO.Path]::GetTempPath()) ("opsxj-archive-test-" + [System.Guid]::NewGuid().ToString("N"))
New-Item -ItemType Directory -Path $tempRoot -Force | Out-Null

try {
    $repoRoot = Join-Path $tempRoot "repo"
    $toolDir = Join-Path $repoRoot "Tools/jira-open"
    $fakeBin = Join-Path $tempRoot "fake-bin"
    $changeName = "opsxj-321-test-change"

    New-Item -ItemType Directory -Path $toolDir -Force | Out-Null
    New-Item -ItemType Directory -Path (Join-Path $repoRoot "openspec/changes/$changeName") -Force | Out-Null
    New-Item -ItemType Directory -Path $fakeBin -Force | Out-Null
    Copy-Item -Path (Join-Path $PSScriptRoot "opsxj.ps1") -Destination (Join-Path $toolDir "opsxj.ps1") -Force

    Set-Content -Path (Join-Path $repoRoot "README.md") -Value "archive test repo" -NoNewline
    Set-Content -Path (Join-Path $repoRoot "openspec/changes/$changeName/.openspec.yaml") -Value "schema: spec-driven`ncreated: 2026-02-27" -NoNewline

    Run-Git -RepoRoot $repoRoot -CliArgs @("init")
    Run-Git -RepoRoot $repoRoot -CliArgs @("config", "user.email", "opsxj-test@example.com")
    Run-Git -RepoRoot $repoRoot -CliArgs @("config", "user.name", "opsxj-test")
    Run-Git -RepoRoot $repoRoot -CliArgs @("add", ".")
    Run-Git -RepoRoot $repoRoot -CliArgs @("commit", "-m", "init")
    Run-Git -RepoRoot $repoRoot -CliArgs @("branch", "-M", "main")
    Run-Git -RepoRoot $repoRoot -CliArgs @("remote", "add", "origin", "https://github.com/example/repo.git")

    Run-Git -RepoRoot $repoRoot -CliArgs @("checkout", "-b", $changeName)
    Set-Content -Path (Join-Path $repoRoot "feature.txt") -Value "feature" -NoNewline
    Run-Git -RepoRoot $repoRoot -CliArgs @("add", "feature.txt")
    Run-Git -RepoRoot $repoRoot -CliArgs @("commit", "-m", "OPSXJ-321 feature")
    Run-Git -RepoRoot $repoRoot -CliArgs @("checkout", "main")
    Run-Git -RepoRoot $repoRoot -CliArgs @("merge", "--no-ff", $changeName, "-m", "merge $changeName")

    Set-Content -Path (Join-Path $fakeBin "openspec.cmd") -Value "@echo off`r`nexit /b 0" -NoNewline
    $previousPath = $env:PATH
    $env:PATH = "$fakeBin;$previousPath"

    $env:JIRA_BASE_URL = "https://example.atlassian.net"
    $env:JIRA_EMAIL = "bot@example.com"
    $env:JIRA_API_TOKEN = "token"
    $env:GIT_BASE_BRANCH = "main"
    $env:GITHUB_TOKEN = "test-token"
    $env:OPSXJ_TEST_SKIP_REMOTE_FETCH = "1"

    $script:TransitionedToDone = $false
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
                    summary = "Archive test issue"
                    description = @{
                        type = "doc"
                        content = @(
                            @{
                                type = "paragraph"
                                content = @(
                                    @{ type = "text"; text = "Test description" }
                                )
                            }
                        )
                    }
                }
            }
        }

        if ($Method -ieq "Get" -and $Uri -like "*/rest/api/3/issue/*/transitions") {
            return @{
                transitions = @(
                    @{
                        id = "31"
                        to = @{ name = "Done" }
                    }
                )
            }
        }

        if ($Method -ieq "Post" -and $Uri -like "*/rest/api/3/issue/*/transitions") {
            $script:TransitionedToDone = $true
            return @{}
        }

        throw "Unexpected Invoke-RestMethod call: $Method $Uri"
    }

    Push-Location $repoRoot
    try {
        $scriptPath = Join-Path $toolDir "opsxj.ps1"
        $output = (& $scriptPath archive OPSXJ-321 -Yes -SkipSpecs 2>&1 | Out-String)
        Assert-Contains -Value $output -Expected "Git merge validation passed"
        Assert-Contains -Value $output -Expected "Archived change: $changeName"
        Assert-Contains -Value $output -Expected "Jira issue transitioned: OPSXJ-321 -> Done"

    }
    finally {
        Pop-Location
        Remove-Item Function:\Invoke-RestMethod -ErrorAction SilentlyContinue
        $env:PATH = $previousPath
        Remove-Item Env:JIRA_BASE_URL -ErrorAction SilentlyContinue
        Remove-Item Env:JIRA_EMAIL -ErrorAction SilentlyContinue
        Remove-Item Env:JIRA_API_TOKEN -ErrorAction SilentlyContinue
        Remove-Item Env:GIT_BASE_BRANCH -ErrorAction SilentlyContinue
        Remove-Item Env:GITHUB_TOKEN -ErrorAction SilentlyContinue
        Remove-Item Env:OPSXJ_TEST_SKIP_REMOTE_FETCH -ErrorAction SilentlyContinue
    }

    Write-Output "PASS: opsxj:archive validates git merge and transitions Jira to Done."
}
finally {
    Remove-Item -Path $tempRoot -Recurse -Force -ErrorAction SilentlyContinue
}
