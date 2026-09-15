param()

$ErrorActionPreference = "Stop"
[Net.ServicePointManager]::SecurityProtocol = `
    [Net.ServicePointManager]::SecurityProtocol -bor `
    [Net.SecurityProtocolType]::Tls12

function Get-HttpStatusCodeFromException {
    param([object]$Exception)
    try {
        if ($Exception -and $Exception.Response -and $Exception.Response.StatusCode) {
            return [int]$Exception.Response.StatusCode
        }
    }
    catch {
        return $null
    }
    return $null
}

function Test-IsTransientNetworkFailure {
    param(
        [int]$StatusCode,
        [string]$Message
    )

    if ($StatusCode -eq 408 -or $StatusCode -eq 429) { return $true }
    if ($StatusCode -ge 500 -and $StatusCode -lt 600) { return $true }
    if ($StatusCode -eq 0) { return $true }

    $text = ([string]$Message).ToLowerInvariant()
    if ($text -match "no es posible conectar") { return $true }
    if ($text -match "timed out|timeout") { return $true }
    if ($text -match "name or service not known") { return $true }
    if ($text -match "temporary|temporarily|temporarily unavailable") { return $true }
    if ($text -match "unable to connect|connection reset|connection refused") { return $true }
    if ($text -match "remote server") { return $true }

    return $false
}

function Invoke-JiraGetWithRetry {
    param(
        [string]$Uri,
        [hashtable]$Headers,
        [int]$MaxAttempts = 4
    )

    $attempt = 1
    while ($attempt -le $MaxAttempts) {
        try {
            return Invoke-RestMethod -Method Get -Uri $Uri -Headers $Headers
        }
        catch {
            $statusCode = Get-HttpStatusCodeFromException -Exception $_.Exception
            if ($null -eq $statusCode) { $statusCode = 0 }
            $message = $_.Exception.Message
            $isTransient = Test-IsTransientNetworkFailure -StatusCode $statusCode -Message $message
            $isLastAttempt = $attempt -ge $MaxAttempts
            if (-not $isTransient -or $isLastAttempt) {
                throw
            }

            $delayMs = [int](500 * [math]::Pow(2, $attempt - 1))
            Start-Sleep -Milliseconds $delayMs
            $attempt++
        }
    }
}

function Get-JiraConfigValue {
    param(
        [string]$Key,
        [string]$ConfigPath
    )

    if (-not (Test-Path $ConfigPath)) {
        return $null
    }

    $line = Get-Content -Path $ConfigPath |
        Where-Object { $_ -match "^\s*$Key\s*=" } |
        Select-Object -First 1

    if (-not $line) { return $null }
    return (($line -split "=", 2)[1]).Trim()
}

$configPath = Join-Path $PSScriptRoot ".jira-open.env"
$baseUrl = Get-JiraConfigValue -Key "JIRA_BASE_URL" -ConfigPath $configPath
$email = Get-JiraConfigValue -Key "JIRA_EMAIL" -ConfigPath $configPath
$token = Get-JiraConfigValue -Key "JIRA_API_TOKEN" -ConfigPath $configPath

if (-not $baseUrl) { $baseUrl = $env:JIRA_BASE_URL }
if (-not $email) { $email = $env:JIRA_EMAIL }
if (-not $token) { $token = $env:JIRA_API_TOKEN }

if (-not $baseUrl -or -not $email -or -not $token) {
    throw "Set JIRA_BASE_URL, JIRA_EMAIL and JIRA_API_TOKEN environment variables."
}

$baseUrl = $baseUrl.TrimEnd("/")
$uri = "$baseUrl/rest/api/3/myself"

$rawAuth = "$email`:$token"
$authBytes = [System.Text.Encoding]::UTF8.GetBytes($rawAuth)
$basic = [Convert]::ToBase64String($authBytes)

$headers = @{
    "Authorization" = "Basic $basic"
    "Accept"        = "application/json"
}

try {
    $response = Invoke-JiraGetWithRetry -Uri $uri -Headers $headers
}
catch {
    throw "Jira connection failed. $($_.Exception.Message)"
}

Write-Output "Jira connection successful."
Write-Output "User: $($response.displayName)"
Write-Output "Account ID: $($response.accountId)"
