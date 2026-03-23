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
    $env:JIRA_BASE_URL = "https://example.atlassian.net"
    $env:JIRA_EMAIL = "bot@example.com"
    $env:JIRA_API_TOKEN = "token"
    $env:GITHUB_TOKEN = "test-token"
    $global:IssueStates = @{}
    $global:InProgressTransitionCount = 0
    $global:ReviewTransitionCount = 0
    $global:JiraCommentCount = 0

    function global:Invoke-RestMethod {
        param(
            [string]$Method,
            [string]$Uri,
            [hashtable]$Headers,
            [string]$Body
        )

        if ($Uri -match "/rest/api/3/issue/(?<issue>[A-Z]+-\d+)") {
            $issueKey = $Matches["issue"]
        }

        if ($Method -ieq "Get" -and $Uri -like "*/rest/api/3/issue/*?fields=summary,description,status") {
            if (-not $global:IssueStates.ContainsKey($issueKey)) {
                $global:IssueStates[$issueKey] = "Por hacer"
            }
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
                    status = @{
                        name = $global:IssueStates[$issueKey]
                    }
                }
            }
        }

        if ($Method -ieq "Get" -and $Uri -like "*/rest/api/3/issue/*/transitions") {
            $currentState = [string]$global:IssueStates[$issueKey]
            if ($currentState -eq "En curso") {
                return @{
                    transitions = @(
                        @{
                            id = "21"
                            to = @{ name = "En Revision" }
                        }
                    )
                }
            }

            return @{
                transitions = @(
                    @{
                        id = "11"
                        to = @{ name = "En curso" }
                    }
                    @{
                        id = "21"
                        to = @{ name = "En Revision" }
                    }
                )
            }
        }

        if ($Method -ieq "Post" -and $Uri -like "*/rest/api/3/issue/*/transitions") {
            if ($Body -like '*"11"*') {
                $global:IssueStates[$issueKey] = "En curso"
                $global:InProgressTransitionCount++
                return @{}
            }

            if ($Body -like '*"21"*') {
                $global:IssueStates[$issueKey] = "En Revision"
                $global:ReviewTransitionCount++
                return @{}
            }

            return @{}
        }

        if ($Method -ieq "Post" -and $Uri -like "*/rest/api/3/issue/*/comment") {
            $global:JiraCommentCount++
            Assert-Contains -Value $Body -Expected "Merge requerido: manual."
            Assert-Contains -Value $Body -Expected "https://github.com/example/repo/pull/123"
            return @{ id = "10001" }
        }

        throw "Unexpected Invoke-RestMethod call: $Method $Uri"
    }

    try {
        $scriptPath = Join-Path $toolDir "opsxj.ps1"
        Push-Location $repoRoot
        try {
            $firstRun = (& $scriptPath new OPSXJ-999 2>&1 | Out-String)
            Assert-Contains -Value $firstRun -Expected "Jira issue transitioned: OPSXJ-999 -> En curso"
            Assert-Contains -Value $firstRun -Expected "Pull request created: https://github.com/example/repo/pull/123"
            Assert-Contains -Value $firstRun -Expected "Jira issue transitioned: OPSXJ-999 -> En Revision"
            Assert-Contains -Value $firstRun -Expected "Jira comment added: OPSXJ-999"

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

            $thirdRun = (& $scriptPath new OPSXJ-1000 -NonInteractive 2>&1 | Out-String)
            Assert-Contains -Value $thirdRun -Expected "Jira issue transitioned: OPSXJ-1000 -> En curso"
            Assert-Contains -Value $thirdRun -Expected "Pull request created: https://github.com/example/repo/pull/123"
            Assert-Contains -Value $thirdRun -Expected "Jira issue transitioned: OPSXJ-1000 -> En Revision"
            Assert-Contains -Value $thirdRun -Expected "Jira comment added: OPSXJ-1000"

            $logPath = Join-Path $repoRoot "openspec/logs/OPSXJ-1000.log.jsonl"
            if (-not (Test-Path $logPath)) {
                throw "Expected log file not found: $logPath"
            }

            $logText = Get-Content -Path $logPath -Raw
            Assert-Contains -Value $logText -Expected '"mode":"noninteractive"'

            Remove-Item Env:GITHUB_TOKEN -ErrorAction SilentlyContinue
            try {
                & $scriptPath new OPSXJ-1001 -NonInteractive | Out-Null
                throw "Expected -NonInteractive token validation error was not raised."
            }
            catch {
                Assert-Contains -Value $_.Exception.Message -Expected "Missing GITHUB_TOKEN. Token-based GitHub connection is required in -NonInteractive mode."
            }

            $env:GITHUB_TOKEN = "test-token"
            Remove-Item Env:OPSXJ_TEST_FAKE_PR_URL -ErrorAction SilentlyContinue

            function global:Invoke-RestMethod {
                param(
                    [string]$Method,
                    [string]$Uri,
                    [hashtable]$Headers,
                    [object]$Body,
                    [string]$ContentType
                )

                if ($Uri -match "/rest/api/3/issue/(?<issue>[A-Z]+-\d+)") {
                    $issueKey = $Matches["issue"]
                }

                if ($Method -ieq "Get" -and $Uri -like "*/rest/api/3/issue/*?fields=summary,description,status") {
                    if (-not $global:IssueStates.ContainsKey($issueKey)) {
                        $global:IssueStates[$issueKey] = "Por hacer"
                    }
                    return @{
                        fields = @{
                            summary = "MIGRACIÓN-NET-Formatea_fecha_time_framework"
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
                            status = @{
                                name = $global:IssueStates[$issueKey]
                            }
                        }
                    }
                }

                if ($Method -ieq "Get" -and $Uri -like "*/rest/api/3/issue/*/transitions") {
                    $currentState = [string]$global:IssueStates[$issueKey]
                    if ($currentState -eq "En curso") {
                        return @{
                            transitions = @(
                                @{
                                    id = "21"
                                    to = @{ name = "En Revision" }
                                }
                            )
                        }
                    }

                    return @{
                        transitions = @(
                            @{
                                id = "11"
                                to = @{ name = "En curso" }
                            }
                            @{
                                id = "21"
                                to = @{ name = "En Revision" }
                            }
                        )
                    }
                }

                if ($Method -ieq "Post" -and $Uri -like "*/rest/api/3/issue/*/transitions") {
                    $bodyText = if ($Body -is [byte[]]) {
                        [System.Text.Encoding]::UTF8.GetString($Body)
                    }
                    else {
                        [string]$Body
                    }

                    if ($bodyText -like '*"11"*') {
                        $global:IssueStates[$issueKey] = "En curso"
                        $global:InProgressTransitionCount++
                        return @{}
                    }

                    if ($bodyText -like '*"21"*') {
                        $global:IssueStates[$issueKey] = "En Revision"
                        $global:ReviewTransitionCount++
                        return @{}
                    }

                    return @{}
                }

                if ($Method -ieq "Post" -and $Uri -like "*/rest/api/3/issue/*/comment") {
                    $global:JiraCommentCount++
                    $bodyText = if ($Body -is [byte[]]) {
                        [System.Text.Encoding]::UTF8.GetString($Body)
                    }
                    else {
                        [string]$Body
                    }
                    Assert-Contains -Value $bodyText -Expected "Merge requerido: manual."
                    Assert-Contains -Value $bodyText -Expected "https://github.com/example/repo/pull/1002"
                    return @{ id = "10002" }
                }

                if ($Method -ieq "Get" -and $Uri -like "https://api.github.com/repos/*/pulls?state=open*") {
                    return @(
                        @{
                            number = ""
                            title = ""
                            html_url = ""
                        }
                    )
                }

                if ($Method -ieq "Post" -and $Uri -like "https://api.github.com/repos/*/pulls") {
                    if ($ContentType -ne "application/json; charset=utf-8") {
                        throw "Expected UTF-8 GitHub content type. Actual: $ContentType"
                    }

                    $decodedBody = if ($Body -is [byte[]]) {
                        [System.Text.Encoding]::UTF8.GetString($Body)
                    }
                    else {
                        [string]$Body
                    }

                    Assert-Contains -Value $decodedBody -Expected "MIGRACIÓN-NET-Formatea_fecha_time_framework"

                    return @{
                        html_url = "https://github.com/example/repo/pull/1002"
                    }
                }

                throw "Unexpected Invoke-RestMethod call: $Method $Uri"
            }

            $fourthRun = (& $scriptPath new OPSXJ-1002 -NonInteractive 2>&1 | Out-String)
            Assert-Contains -Value $fourthRun -Expected "Jira issue transitioned: OPSXJ-1002 -> En curso"
            Assert-Contains -Value $fourthRun -Expected "Pull request created: https://github.com/example/repo/pull/1002"
            Assert-Contains -Value $fourthRun -Expected "Jira issue transitioned: OPSXJ-1002 -> En Revision"
            Assert-Contains -Value $fourthRun -Expected "Jira comment added: OPSXJ-1002"

            if ($global:ReviewTransitionCount -lt 3) {
                throw "Expected Jira review transition to run for each newly created PR. Count: $global:ReviewTransitionCount"
            }
            if ($global:InProgressTransitionCount -lt 3) {
                throw "Expected Jira in-progress transition to run for each new validated change before review. Count: $global:InProgressTransitionCount"
            }
            if ($global:JiraCommentCount -lt 3) {
                throw "Expected Jira PR comment to run for each newly created PR. Count: $global:JiraCommentCount"
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
        Remove-Item Env:JIRA_BASE_URL -ErrorAction SilentlyContinue
        Remove-Item Env:JIRA_EMAIL -ErrorAction SilentlyContinue
        Remove-Item Env:JIRA_API_TOKEN -ErrorAction SilentlyContinue
        Remove-Item Env:GITHUB_TOKEN -ErrorAction SilentlyContinue
    }

    Write-Output "PASS: opsxj:new legacy and -NonInteractive flows create PRs and enforce token policy."
}
finally {
    Remove-Item -Path $tempRoot -Recurse -Force -ErrorAction SilentlyContinue
}
