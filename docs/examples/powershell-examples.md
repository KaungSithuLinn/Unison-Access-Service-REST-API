# PowerShell API Examples - Unison REST Adapter

This document provides comprehensive PowerShell examples for interacting with all Unison REST Adapter API endpoints.

## Prerequisites

PowerShell 5.1 or later with `Invoke-RestMethod` cmdlet support.

## Configuration

Set up your environment with the required variables:

```powershell
# Configuration
$ApiBaseUrl = "http://localhost:5203"  # Change to production URL as needed
$UnisonToken = "your-token-here"      # Replace with your actual token

# Create headers for authenticated requests
$AuthHeaders = @{
    "Unison-Token" = $UnisonToken
    "Accept" = "application/json"
}

# Create headers for requests with JSON body
$JsonHeaders = @{
    "Unison-Token" = $UnisonToken
    "Content-Type" = "application/json"
    "Accept" = "application/json"
}
```

## Health Check Endpoints

### Basic Health Check

```powershell
# Basic health check (no authentication required)
function Test-ApiHealth {
    try {
        $response = Invoke-RestMethod -Uri "$ApiBaseUrl/health" -Method GET
        Write-Host "✅ API Health Status: $($response.status)" -ForegroundColor Green
        return $response
    }
    catch {
        Write-Host "❌ API Health Check Failed: $($_.Exception.Message)" -ForegroundColor Red
        throw
    }
}

# Example usage
Test-ApiHealth
```

### Detailed Health Check

```powershell
# Detailed health check with authentication
function Get-DetailedHealth {
    param(
        [string]$Token = $UnisonToken
    )

    try {
        $headers = @{ "Unison-Token" = $Token; "Accept" = "application/json" }
        $response = Invoke-RestMethod -Uri "$ApiBaseUrl/health/detailed" -Method GET -Headers $headers

        Write-Host "✅ Detailed Health Check Results:" -ForegroundColor Green
        Write-Host "  Status: $($response.status)"
        Write-Host "  SOAP Service: $($response.soapService.status)"
        Write-Host "  Response Time: $($response.soapService.responseTime)"
        Write-Host "  Timestamp: $($response.timestamp)"

        return $response
    }
    catch {
        Write-Host "❌ Detailed Health Check Failed: $($_.Exception.Message)" -ForegroundColor Red
        if ($_.Exception.Response.StatusCode -eq 401) {
            Write-Host "  Check your Unison-Token value" -ForegroundColor Yellow
        }
        throw
    }
}

# Example usage
Get-DetailedHealth
```

## Card Management Functions

### Create Card

```powershell
# Create a new card
function New-UnisonCard {
    param(
        [Parameter(Mandatory = $true)]
        [string]$CardId,

        [Parameter(Mandatory = $true)]
        [string]$UserName,

        [Parameter(Mandatory = $true)]
        [string]$FirstName,

        [Parameter(Mandatory = $true)]
        [string]$LastName,

        [bool]$IsActive = $true
    )

    $body = @{
        cardId = $CardId
        userName = $UserName
        firstName = $FirstName
        lastName = $LastName
        isActive = $IsActive
    } | ConvertTo-Json -Depth 10

    try {
        $response = Invoke-RestMethod -Uri "$ApiBaseUrl/api/cards" -Method POST -Headers $JsonHeaders -Body $body

        Write-Host "✅ Card Created Successfully!" -ForegroundColor Green
        Write-Host "  Card ID: $($response.cardId)"
        Write-Host "  Correlation ID: $($response.correlationId)"
        Write-Host "  Timestamp: $($response.timestamp)"

        return $response
    }
    catch {
        Write-Host "❌ Card Creation Failed: $($_.Exception.Message)" -ForegroundColor Red

        # Try to parse error response
        if ($_.Exception.Response) {
            try {
                $errorStream = $_.Exception.Response.GetResponseStream()
                $reader = New-Object System.IO.StreamReader($errorStream)
                $errorBody = $reader.ReadToEnd() | ConvertFrom-Json
                Write-Host "  Error Details: $($errorBody.error)" -ForegroundColor Yellow
                Write-Host "  Correlation ID: $($errorBody.correlationId)" -ForegroundColor Yellow
            }
            catch {
                Write-Host "  HTTP Status: $($_.Exception.Response.StatusCode)" -ForegroundColor Yellow
            }
        }
        throw
    }
}

# Example usage
New-UnisonCard -CardId "PS001" -UserName "john.doe" -FirstName "John" -LastName "Doe"
```

### Update Card

```powershell
# Update an existing card
function Update-UnisonCard {
    param(
        [Parameter(Mandatory = $true)]
        [string]$CardId,

        [string]$UserName,
        [string]$FirstName,
        [string]$LastName,
        [bool]$IsActive
    )

    # Build update object with only provided parameters
    $updateData = @{ cardId = $CardId }

    if ($PSBoundParameters.ContainsKey('UserName')) { $updateData.userName = $UserName }
    if ($PSBoundParameters.ContainsKey('FirstName')) { $updateData.firstName = $FirstName }
    if ($PSBoundParameters.ContainsKey('LastName')) { $updateData.lastName = $LastName }
    if ($PSBoundParameters.ContainsKey('IsActive')) { $updateData.isActive = $IsActive }

    $body = $updateData | ConvertTo-Json -Depth 10

    try {
        $response = Invoke-RestMethod -Uri "$ApiBaseUrl/api/cards/update" -Method PUT -Headers $JsonHeaders -Body $body

        Write-Host "✅ Card Updated Successfully!" -ForegroundColor Green
        Write-Host "  Card ID: $($response.cardId)"
        Write-Host "  Correlation ID: $($response.correlationId)"

        return $response
    }
    catch {
        Write-Host "❌ Card Update Failed: $($_.Exception.Message)" -ForegroundColor Red
        throw
    }
}

# Example usage
Update-UnisonCard -CardId "PS001" -IsActive $false
```

### Get Card Details

```powershell
# Get details for a specific card
function Get-UnisonCard {
    param(
        [Parameter(Mandatory = $true)]
        [string]$CardId
    )

    try {
        $response = Invoke-RestMethod -Uri "$ApiBaseUrl/api/cards/$CardId" -Method GET -Headers $AuthHeaders

        if ($response.success) {
            Write-Host "✅ Card Details Retrieved:" -ForegroundColor Green
            Write-Host "  Card ID: $($response.card.cardId)"
            Write-Host "  User Name: $($response.card.userName)"
            Write-Host "  Full Name: $($response.card.firstName) $($response.card.lastName)"
            Write-Host "  Is Active: $($response.card.isActive)"
            Write-Host "  Last Modified: $($response.card.lastModified)"
        }

        return $response
    }
    catch {
        Write-Host "❌ Get Card Failed: $($_.Exception.Message)" -ForegroundColor Red

        if ($_.Exception.Response.StatusCode -eq 404) {
            Write-Host "  Card not found: $CardId" -ForegroundColor Yellow
        }
        throw
    }
}

# Example usage
Get-UnisonCard -CardId "PS001"
```

### Delete Card

```powershell
# Delete a specific card
function Remove-UnisonCard {
    param(
        [Parameter(Mandatory = $true)]
        [string]$CardId,

        [switch]$Force
    )

    # Confirm deletion unless -Force is used
    if (-not $Force) {
        $confirmation = Read-Host "Are you sure you want to delete card '$CardId'? (y/N)"
        if ($confirmation -ne 'y' -and $confirmation -ne 'Y') {
            Write-Host "Operation cancelled." -ForegroundColor Yellow
            return
        }
    }

    try {
        $response = Invoke-RestMethod -Uri "$ApiBaseUrl/api/cards/$CardId" -Method DELETE -Headers $AuthHeaders

        Write-Host "✅ Card Deleted Successfully!" -ForegroundColor Green
        Write-Host "  Card ID: $($response.cardId)"
        Write-Host "  Correlation ID: $($response.correlationId)"

        return $response
    }
    catch {
        Write-Host "❌ Card Deletion Failed: $($_.Exception.Message)" -ForegroundColor Red
        throw
    }
}

# Example usage
Remove-UnisonCard -CardId "PS001" -Force
```

## User Management Functions

### Create User

```powershell
# Create a new user
function New-UnisonUser {
    param(
        [Parameter(Mandatory = $true)]
        [string]$UserName,

        [Parameter(Mandatory = $true)]
        [string]$FirstName,

        [Parameter(Mandatory = $true)]
        [string]$LastName,

        [string]$Email,
        [string]$Department,
        [bool]$IsActive = $true
    )

    $body = @{
        userName = $UserName
        firstName = $FirstName
        lastName = $LastName
        isActive = $IsActive
    }

    if ($Email) { $body.email = $Email }
    if ($Department) { $body.department = $Department }

    $jsonBody = $body | ConvertTo-Json -Depth 10

    try {
        $response = Invoke-RestMethod -Uri "$ApiBaseUrl/api/users" -Method POST -Headers $JsonHeaders -Body $jsonBody

        Write-Host "✅ User Created Successfully!" -ForegroundColor Green
        Write-Host "  User ID: $($response.userId)"
        Write-Host "  User Name: $($response.userName)"
        Write-Host "  Correlation ID: $($response.correlationId)"

        return $response
    }
    catch {
        Write-Host "❌ User Creation Failed: $($_.Exception.Message)" -ForegroundColor Red
        throw
    }
}

# Example usage
New-UnisonUser -UserName "jane.smith" -FirstName "Jane" -LastName "Smith" -Email "jane.smith@company.com" -Department "Engineering"
```

### Update User

```powershell
# Update an existing user
function Update-UnisonUser {
    param(
        [Parameter(Mandatory = $true)]
        [string]$UserId,

        [string]$FirstName,
        [string]$LastName,
        [string]$Email,
        [string]$Department,
        [bool]$IsActive
    )

    # Build update object
    $updateData = @{}

    if ($PSBoundParameters.ContainsKey('FirstName')) { $updateData.firstName = $FirstName }
    if ($PSBoundParameters.ContainsKey('LastName')) { $updateData.lastName = $LastName }
    if ($PSBoundParameters.ContainsKey('Email')) { $updateData.email = $Email }
    if ($PSBoundParameters.ContainsKey('Department')) { $updateData.department = $Department }
    if ($PSBoundParameters.ContainsKey('IsActive')) { $updateData.isActive = $IsActive }

    if ($updateData.Count -eq 0) {
        Write-Host "⚠️ No update fields provided" -ForegroundColor Yellow
        return
    }

    $body = $updateData | ConvertTo-Json -Depth 10

    try {
        $response = Invoke-RestMethod -Uri "$ApiBaseUrl/api/users/$UserId" -Method PUT -Headers $JsonHeaders -Body $body

        Write-Host "✅ User Updated Successfully!" -ForegroundColor Green
        Write-Host "  User ID: $($response.userId)"
        Write-Host "  Correlation ID: $($response.correlationId)"

        return $response
    }
    catch {
        Write-Host "❌ User Update Failed: $($_.Exception.Message)" -ForegroundColor Red
        throw
    }
}

# Example usage
Update-UnisonUser -UserId "USER123" -Department "Senior Engineering" -IsActive $true
```

## Advanced PowerShell Examples

### Bulk Operations

```powershell
# Bulk create cards from CSV file
function Import-CardsFromCsv {
    param(
        [Parameter(Mandatory = $true)]
        [string]$CsvPath
    )

    if (-not (Test-Path $CsvPath)) {
        Write-Host "❌ CSV file not found: $CsvPath" -ForegroundColor Red
        return
    }

    $cards = Import-Csv $CsvPath
    $results = @()

    foreach ($card in $cards) {
        try {
            Write-Host "Creating card: $($card.CardId)..." -NoNewline

            $result = New-UnisonCard -CardId $card.CardId -UserName $card.UserName -FirstName $card.FirstName -LastName $card.LastName -IsActive ([bool]::Parse($card.IsActive))

            $results += [PSCustomObject]@{
                CardId = $card.CardId
                Status = "Success"
                CorrelationId = $result.correlationId
                Error = $null
            }

            Write-Host " ✅" -ForegroundColor Green
        }
        catch {
            $results += [PSCustomObject]@{
                CardId = $card.CardId
                Status = "Failed"
                CorrelationId = $null
                Error = $_.Exception.Message
            }

            Write-Host " ❌" -ForegroundColor Red
        }

        Start-Sleep -Milliseconds 100  # Rate limiting
    }

    return $results
}

# Example CSV format:
# CardId,UserName,FirstName,LastName,IsActive
# BULK001,user1,User,One,true
# BULK002,user2,User,Two,false
```

### Error Handling and Retry Logic

```powershell
# Robust API call with retry logic
function Invoke-UnisonApiWithRetry {
    param(
        [Parameter(Mandatory = $true)]
        [string]$Uri,

        [Parameter(Mandatory = $true)]
        [string]$Method,

        [hashtable]$Headers = $AuthHeaders,
        [string]$Body,
        [int]$MaxRetries = 3,
        [int]$RetryDelaySeconds = 2
    )

    $attempt = 1

    while ($attempt -le $MaxRetries) {
        try {
            Write-Verbose "Attempt $attempt of $MaxRetries for $Method $Uri"

            $params = @{
                Uri = $Uri
                Method = $Method
                Headers = $Headers
            }

            if ($Body) {
                $params.Body = $Body
            }

            $response = Invoke-RestMethod @params

            Write-Host "✅ API call successful on attempt $attempt" -ForegroundColor Green
            return $response
        }
        catch {
            $statusCode = $_.Exception.Response.StatusCode.value__

            Write-Host "⚠️ Attempt $attempt failed: $($_.Exception.Message)" -ForegroundColor Yellow

            # Don't retry on client errors (4xx)
            if ($statusCode -ge 400 -and $statusCode -lt 500) {
                Write-Host "❌ Client error detected, not retrying" -ForegroundColor Red
                throw
            }

            if ($attempt -eq $MaxRetries) {
                Write-Host "❌ Max retries exceeded" -ForegroundColor Red
                throw
            }

            Write-Host "Waiting $RetryDelaySeconds seconds before retry..." -ForegroundColor Yellow
            Start-Sleep -Seconds $RetryDelaySeconds
            $attempt++
        }
    }
}

# Example usage with retry logic
try {
    $response = Invoke-UnisonApiWithRetry -Uri "$ApiBaseUrl/health/detailed" -Method GET
    Write-Host "Health status: $($response.status)"
}
catch {
    Write-Host "All retry attempts failed: $($_.Exception.Message)" -ForegroundColor Red
}
```

### Monitoring and Health Dashboard

```powershell
# Simple monitoring dashboard
function Show-UnisonDashboard {
    param(
        [int]$RefreshIntervalSeconds = 30
    )

    while ($true) {
        Clear-Host
        Write-Host "=== UNISON REST ADAPTER DASHBOARD ===" -ForegroundColor Cyan
        Write-Host "Last Updated: $(Get-Date -Format 'yyyy-MM-dd HH:mm:ss')" -ForegroundColor Gray
        Write-Host ""

        # Basic Health
        try {
            $basicHealth = Invoke-RestMethod -Uri "$ApiBaseUrl/health" -Method GET -TimeoutSec 5
            Write-Host "📊 Basic Health: " -NoNewline
            Write-Host "$($basicHealth.status)" -ForegroundColor Green
        }
        catch {
            Write-Host "📊 Basic Health: " -NoNewline
            Write-Host "FAILED" -ForegroundColor Red
        }

        # Detailed Health
        try {
            $detailedHealth = Invoke-RestMethod -Uri "$ApiBaseUrl/health/detailed" -Method GET -Headers $AuthHeaders -TimeoutSec 5
            Write-Host "🔍 Detailed Health: " -NoNewline
            Write-Host "$($detailedHealth.status)" -ForegroundColor Green
            Write-Host "🔗 SOAP Service: " -NoNewline
            Write-Host "$($detailedHealth.soapService.status)" -ForegroundColor Green
            Write-Host "⏱️  Response Time: $($detailedHealth.soapService.responseTime)"
        }
        catch {
            Write-Host "🔍 Detailed Health: " -NoNewline
            Write-Host "FAILED" -ForegroundColor Red
        }

        Write-Host ""
        Write-Host "Press Ctrl+C to exit. Refreshing in $RefreshIntervalSeconds seconds..." -ForegroundColor Yellow

        Start-Sleep -Seconds $RefreshIntervalSeconds
    }
}

# Start dashboard
# Show-UnisonDashboard
```

### Performance Testing

```powershell
# Simple load testing function
function Test-ApiPerformance {
    param(
        [int]$RequestCount = 10,
        [int]$ConcurrentRequests = 1
    )

    Write-Host "🚀 Starting performance test..." -ForegroundColor Cyan
    Write-Host "Requests: $RequestCount, Concurrent: $ConcurrentRequests"
    Write-Host ""

    $stopwatch = [System.Diagnostics.Stopwatch]::StartNew()
    $results = @()

    # Split requests into batches for concurrency
    $batches = [Math]::Ceiling($RequestCount / $ConcurrentRequests)

    for ($batch = 0; $batch -lt $batches; $batch++) {
        $jobs = @()

        for ($i = 0; $i -lt $ConcurrentRequests; $i++) {
            $requestIndex = ($batch * $ConcurrentRequests) + $i
            if ($requestIndex -ge $RequestCount) { break }

            $job = Start-Job -ScriptBlock {
                param($ApiUrl, $Headers, $RequestIndex)

                $requestStart = Get-Date
                try {
                    $response = Invoke-RestMethod -Uri "$ApiUrl/health" -Method GET -TimeoutSec 10
                    $requestEnd = Get-Date
                    $duration = ($requestEnd - $requestStart).TotalMilliseconds

                    return [PSCustomObject]@{
                        RequestIndex = $RequestIndex
                        Success = $true
                        Duration = $duration
                        Status = $response.status
                        Error = $null
                    }
                }
                catch {
                    $requestEnd = Get-Date
                    $duration = ($requestEnd - $requestStart).TotalMilliseconds

                    return [PSCustomObject]@{
                        RequestIndex = $RequestIndex
                        Success = $false
                        Duration = $duration
                        Status = $null
                        Error = $_.Exception.Message
                    }
                }
            } -ArgumentList $ApiBaseUrl, $AuthHeaders, $requestIndex

            $jobs += $job
        }

        # Wait for batch to complete
        $batchResults = $jobs | Wait-Job | Receive-Job
        $results += $batchResults
        $jobs | Remove-Job

        Write-Host "Batch $($batch + 1) completed" -ForegroundColor Green
    }

    $stopwatch.Stop()

    # Calculate statistics
    $successCount = ($results | Where-Object Success).Count
    $failureCount = $results.Count - $successCount
    $avgDuration = ($results | Measure-Object Duration -Average).Average
    $minDuration = ($results | Measure-Object Duration -Minimum).Minimum
    $maxDuration = ($results | Measure-Object Duration -Maximum).Maximum
    $totalDuration = $stopwatch.ElapsedMilliseconds
    $requestsPerSecond = [Math]::Round($RequestCount / ($totalDuration / 1000), 2)

    Write-Host ""
    Write-Host "=== PERFORMANCE RESULTS ===" -ForegroundColor Cyan
    Write-Host "Total Requests: $RequestCount"
    Write-Host "Successful: $successCount"
    Write-Host "Failed: $failureCount"
    Write-Host "Total Time: $totalDuration ms"
    Write-Host "Requests/Second: $requestsPerSecond"
    Write-Host "Average Duration: $([Math]::Round($avgDuration, 2)) ms"
    Write-Host "Min Duration: $([Math]::Round($minDuration, 2)) ms"
    Write-Host "Max Duration: $([Math]::Round($maxDuration, 2)) ms"

    return $results
}

# Example usage
# $perfResults = Test-ApiPerformance -RequestCount 50 -ConcurrentRequests 5
```

## Configuration Management

### Environment-Specific Configuration

```powershell
# Configuration manager for different environments
class UnisonApiConfig {
    [string]$Environment
    [string]$BaseUrl
    [string]$Token
    [hashtable]$AuthHeaders
    [hashtable]$JsonHeaders

    UnisonApiConfig([string]$env) {
        $this.Environment = $env
        $this.LoadEnvironment()
    }

    [void]LoadEnvironment() {
        switch ($this.Environment.ToLower()) {
            "development" {
                $this.BaseUrl = "http://localhost:5203"
                $this.Token = "dev-token-123"
            }
            "staging" {
                $this.BaseUrl = "https://staging-unison-api.company.com"
                $this.Token = "staging-token-456"
            }
            "production" {
                $this.BaseUrl = "https://unison-api.company.com"
                $this.Token = $env:UNISON_PRODUCTION_TOKEN  # From environment variable
            }
            default {
                throw "Unknown environment: $($this.Environment)"
            }
        }

        $this.AuthHeaders = @{
            "Unison-Token" = $this.Token
            "Accept" = "application/json"
        }

        $this.JsonHeaders = @{
            "Unison-Token" = $this.Token
            "Content-Type" = "application/json"
            "Accept" = "application/json"
        }
    }

    [void]TestConnection() {
        try {
            $response = Invoke-RestMethod -Uri "$($this.BaseUrl)/health" -Method GET
            Write-Host "✅ Connection to $($this.Environment) successful: $($response.status)" -ForegroundColor Green
        }
        catch {
            Write-Host "❌ Connection to $($this.Environment) failed: $($_.Exception.Message)" -ForegroundColor Red
            throw
        }
    }
}

# Usage example
$config = [UnisonApiConfig]::new("development")
$config.TestConnection()

# Use config in API calls
$response = Invoke-RestMethod -Uri "$($config.BaseUrl)/health/detailed" -Method GET -Headers $config.AuthHeaders
```

## Troubleshooting

### Common Issues and Solutions

```powershell
# Diagnostic function
function Test-UnisonApiDiagnostics {
    param(
        [string]$BaseUrl = $ApiBaseUrl,
        [string]$Token = $UnisonToken
    )

    Write-Host "🔧 Running Unison API Diagnostics..." -ForegroundColor Cyan
    Write-Host ""

    # Test 1: Basic connectivity
    Write-Host "1. Testing basic connectivity..." -NoNewline
    try {
        Test-NetConnection -ComputerName ([System.Uri]$BaseUrl).Host -Port ([System.Uri]$BaseUrl).Port -InformationLevel Quiet
        Write-Host " ✅" -ForegroundColor Green
    }
    catch {
        Write-Host " ❌ Network connectivity failed" -ForegroundColor Red
    }

    # Test 2: Health endpoint
    Write-Host "2. Testing health endpoint..." -NoNewline
    try {
        $health = Invoke-RestMethod -Uri "$BaseUrl/health" -Method GET -TimeoutSec 10
        Write-Host " ✅ ($($health.status))" -ForegroundColor Green
    }
    catch {
        Write-Host " ❌ Health endpoint failed" -ForegroundColor Red
        Write-Host "   Error: $($_.Exception.Message)" -ForegroundColor Yellow
    }

    # Test 3: Authentication
    Write-Host "3. Testing authentication..." -NoNewline
    try {
        $headers = @{ "Unison-Token" = $Token; "Accept" = "application/json" }
        $detailedHealth = Invoke-RestMethod -Uri "$BaseUrl/health/detailed" -Method GET -Headers $headers -TimeoutSec 10
        Write-Host " ✅" -ForegroundColor Green
    }
    catch {
        if ($_.Exception.Response.StatusCode -eq 401) {
            Write-Host " ❌ Invalid token" -ForegroundColor Red
        }
        else {
            Write-Host " ❌ Auth test failed" -ForegroundColor Red
        }
        Write-Host "   Error: $($_.Exception.Message)" -ForegroundColor Yellow
    }

    # Test 4: PowerShell version compatibility
    Write-Host "4. Testing PowerShell compatibility..." -NoNewline
    if ($PSVersionTable.PSVersion.Major -ge 5) {
        Write-Host " ✅ PowerShell $($PSVersionTable.PSVersion)" -ForegroundColor Green
    }
    else {
        Write-Host " ⚠️  PowerShell version may be incompatible" -ForegroundColor Yellow
    }

    Write-Host ""
    Write-Host "Diagnostics completed!" -ForegroundColor Cyan
}

# Run diagnostics
# Test-UnisonApiDiagnostics
```

---

**Documentation Version**: 1.0  
**Last Updated**: September 9, 2025  
**PowerShell Compatibility**: 5.1+ (Windows PowerShell) and 7+ (PowerShell Core)

**Quick Start**:

1. Set configuration variables at the top of your script
2. Test connectivity: `Test-ApiHealth`
3. Try authenticated operations: `Get-DetailedHealth`
4. Import functions into your PowerShell session or save as a module
