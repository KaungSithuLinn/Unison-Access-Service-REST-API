# Unison REST Adapter - Deployment Script
# Deploys the application to the target server

param(
    [string]$TargetServer = "192.168.10.206",
    [string]$TargetPath = "C:\Services\UnisonRestAdapter",
    [string]$SourcePath = ".\publish",
    [string]$ServiceName = "UnisonRestAdapter",
    [PSCredential]$Credential = $null
)

Write-Host "🚀 Deploying Unison REST Adapter" -ForegroundColor Green
Write-Host "=" * 50
Write-Host "Target Server: $TargetServer"
Write-Host "Target Path: $TargetPath"
Write-Host "Source Path: $SourcePath"

# Validate source path
if (!(Test-Path $SourcePath)) {
    Write-Error "Source path '$SourcePath' does not exist. Please run 'dotnet publish' first."
    exit 1
}

# Function to execute commands on remote server
function Invoke-RemoteCommand {
    param(
        [string]$Command,
        [string]$Description
    )
    
    Write-Host "   $Description..." -ForegroundColor Yellow
    
    try {
        if ($Credential) {
            $result = Invoke-Command -ComputerName $TargetServer -Credential $Credential -ScriptBlock {
                param($cmd)
                Invoke-Expression $cmd
            } -ArgumentList $Command
        }
        else {
            $result = Invoke-Command -ComputerName $TargetServer -ScriptBlock {
                param($cmd)
                Invoke-Expression $cmd
            } -ArgumentList $Command
        }
        
        Write-Host "   ✅ $Description completed" -ForegroundColor Green
        return $result
    }
    catch {
        Write-Host "   ❌ $Description failed: $($_.Exception.Message)" -ForegroundColor Red
        return $null
    }
}

# Get credentials if not provided
if (!$Credential) {
    Write-Host "Please enter credentials for $TargetServer" -ForegroundColor Yellow
    $Credential = Get-Credential -Message "Enter credentials for $TargetServer"
}

try {
    # Test connection to target server
    Write-Host "`n1. Testing connection to target server..." -ForegroundColor Cyan
    $connectionTest = Test-NetConnection -ComputerName $TargetServer -Port 445 -WarningAction SilentlyContinue
    
    if (!$connectionTest.TcpTestSucceeded) {
        Write-Error "Cannot connect to $TargetServer on port 445 (SMB). Please check network connectivity."
        exit 1
    }
    
    Write-Host "   ✅ Connection successful" -ForegroundColor Green
    
    # Stop the service if it's running
    Write-Host "`n2. Managing service..." -ForegroundColor Cyan
    Invoke-RemoteCommand -Command "Get-Service -Name '$ServiceName' -ErrorAction SilentlyContinue | Stop-Service -Force -ErrorAction SilentlyContinue" -Description "Stopping service"
    
    # Create target directory
    Write-Host "`n3. Preparing target directory..." -ForegroundColor Cyan
    Invoke-RemoteCommand -Command "if (!(Test-Path '$TargetPath')) { New-Item -ItemType Directory -Path '$TargetPath' -Force }" -Description "Creating target directory"
    
    # Copy files to target server
    Write-Host "`n4. Copying application files..." -ForegroundColor Cyan
    $targetUNC = "\\$TargetServer\$($TargetPath.Replace(':', '$'))"
    
    # Map network drive temporarily
    $driveLetter = "Z:"
    try {
        if ($Credential) {
            New-PSDrive -Name "Z" -PSProvider FileSystem -Root $targetUNC -Credential $Credential -ErrorAction Stop | Out-Null
        }
        else {
            New-PSDrive -Name "Z" -PSProvider FileSystem -Root $targetUNC -ErrorAction Stop | Out-Null
        }
        
        # Copy all files
        Copy-Item -Path "$SourcePath\*" -Destination "$driveLetter\" -Recurse -Force
        Write-Host "   ✅ Files copied successfully" -ForegroundColor Green
        
    }
    catch {
        Write-Host "   ❌ File copy failed: $($_.Exception.Message)" -ForegroundColor Red
        Write-Host "   Trying alternative method..." -ForegroundColor Yellow
        
        # Alternative: Use Invoke-Command to copy files
        $files = Get-ChildItem -Path $SourcePath -Recurse
        foreach ($file in $files) {
            $relativePath = $file.FullName.Substring($SourcePath.Length)
            $targetFile = Join-Path $TargetPath $relativePath
            
            if ($file.PSIsContainer) {
                Invoke-RemoteCommand -Command "New-Item -ItemType Directory -Path '$targetFile' -Force" -Description "Creating directory $relativePath"
            }
            else {
                $content = [System.IO.File]::ReadAllBytes($file.FullName)
                $base64 = [System.Convert]::ToBase64String($content)
                Invoke-RemoteCommand -Command "[System.IO.File]::WriteAllBytes('$targetFile', [System.Convert]::FromBase64String('$base64'))" -Description "Copying $($file.Name)"
            }
        }
    }
    finally {
        # Remove mapped drive
        try { Remove-PSDrive -Name "Z" -ErrorAction SilentlyContinue } catch { }
    }
    
    # Update configuration for production
    Write-Host "`n5. Configuring for production..." -ForegroundColor Cyan
    $configUpdate = @"
`$configPath = '$TargetPath\appsettings.json'
if (Test-Path `$configPath) {
    `$config = Get-Content `$configPath | ConvertFrom-Json
    `$config.Unison.ServiceUrl = 'http://192.168.10.206:9003/Unison.AccessService'
    `$config | ConvertTo-Json -Depth 10 | Set-Content `$configPath
}
"@
    Invoke-RemoteCommand -Command $configUpdate -Description "Updating configuration"
    
    # Install/update Windows service
    Write-Host "`n6. Installing Windows service..." -ForegroundColor Cyan
    $serviceInstall = @"
`$serviceName = '$ServiceName'
`$binaryPath = '$TargetPath\UnisonRestAdapter.exe'
`$displayName = 'Unison Access Service REST Adapter'

# Remove existing service
`$existingService = Get-Service -Name `$serviceName -ErrorAction SilentlyContinue
if (`$existingService) {
    Stop-Service -Name `$serviceName -Force -ErrorAction SilentlyContinue
    sc.exe delete `$serviceName
    Start-Sleep -Seconds 2
}

# Create new service
sc.exe create `$serviceName binpath= `$binaryPath displayname= `$displayName start= auto
sc.exe description `$serviceName 'REST-to-SOAP adapter for Unison Access Service'
sc.exe failure `$serviceName reset= 86400 actions= restart/5000/restart/5000/restart/5000
"@
    Invoke-RemoteCommand -Command $serviceInstall -Description "Installing service"
    
    # Start the service
    Write-Host "`n7. Starting service..." -ForegroundColor Cyan
    Invoke-RemoteCommand -Command "Start-Service -Name '$ServiceName'" -Description "Starting service"
    
    # Verify service status
    Write-Host "`n8. Verifying deployment..." -ForegroundColor Cyan
    $serviceStatus = Invoke-RemoteCommand -Command "Get-Service -Name '$ServiceName' | Select-Object Name, Status, StartType" -Description "Checking service status"
    
    if ($serviceStatus) {
        Write-Host "   Service Status:" -ForegroundColor Gray
        $serviceStatus | Format-Table -AutoSize
    }
    
    # Test API endpoint
    Write-Host "`n9. Testing API endpoint..." -ForegroundColor Cyan
    Start-Sleep -Seconds 5  # Wait for service to fully start
    
    try {
        $testUrl = "http://$TargetServer:5000/api/health"
        $testHeaders = @{ "Unison-Token" = "595d799a-9553-4ddf-8fd9-c27b1f233ce7" }
        $testResponse = Invoke-RestMethod -Uri $testUrl -Headers $testHeaders -TimeoutSec 10
        
        if ($testResponse.isHealthy) {
            Write-Host "   ✅ API endpoint is responding correctly" -ForegroundColor Green
        }
        else {
            Write-Host "   ⚠️  API endpoint is responding but reports unhealthy" -ForegroundColor Yellow
        }
    }
    catch {
        Write-Host "   ❌ API endpoint test failed: $($_.Exception.Message)" -ForegroundColor Red
        Write-Host "   The service may need a few more moments to start" -ForegroundColor Yellow
    }
    
    Write-Host "`n🎯 Deployment completed successfully!" -ForegroundColor Green
    Write-Host "`n📊 Deployment Summary:" -ForegroundColor Cyan
    Write-Host "   Target Server: $TargetServer"
    Write-Host "   Installation Path: $TargetPath"
    Write-Host "   Service Name: $ServiceName"
    Write-Host "   API Endpoint: http://$TargetServer:5000/api"
    Write-Host "   Health Check: http://$TargetServer:5000/api/health"
    
}
catch {
    Write-Host "`n❌ Deployment failed: $($_.Exception.Message)" -ForegroundColor Red
    exit 1
}
