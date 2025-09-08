# PowerShell script to generate Unison AccessService client using svcutil.exe
# Implements the recommended approach from Microsoft Docs research

Write-Host "=== Unison AccessService Client Generation ===" -ForegroundColor Green
Write-Host "Generating WCF client using svcutil.exe with optimized parameters..." -ForegroundColor Yellow

# Service URL
$serviceUrl = "http://192.168.10.206:9003/Unison.AccessService?wsdl"
$outputPath = ".\UnisonHybridClient"

# Ensure output directory exists
if (!(Test-Path $outputPath)) {
    New-Item -ItemType Directory -Path $outputPath -Force
}

Set-Location $outputPath

Write-Host "Service URL: $serviceUrl" -ForegroundColor Cyan
Write-Host "Output Directory: $outputPath" -ForegroundColor Cyan

# Method 1: Standard svcutil with DataContractSerializer (recommended from Microsoft Docs research)
Write-Host "`n--- Method 1: svcutil with DataContractSerializer ---" -ForegroundColor Green
try {
    & svcutil.exe $serviceUrl `
        /dataContractOnly `
        /serializer:DataContractSerializer `
        /out:UnisonServiceClient_DataContract.cs `
        /namespace:*, UnisonAccessService.Generated.DataContract `
        /config:app.config

    if ($LASTEXITCODE -eq 0) {
        Write-Host "SUCCESS: DataContract client generated successfully" -ForegroundColor Green
        
        # Check if files were created
        if (Test-Path "UnisonServiceClient_DataContract.cs") {
            $fileSize = (Get-Item "UnisonServiceClient_DataContract.cs").Length
            Write-Host "Generated file size: $fileSize bytes" -ForegroundColor Cyan
        }
    }
    else {
        Write-Host "WARNING: DataContract generation failed with exit code $LASTEXITCODE" -ForegroundColor Yellow
    }
}
catch {
    Write-Host "ERROR: Failed to execute svcutil for DataContract method: $($_.Exception.Message)" -ForegroundColor Red
}

# Method 2: Standard svcutil with XmlSerializer (fallback)
Write-Host "`n--- Method 2: svcutil with XmlSerializer ---" -ForegroundColor Green
try {
    & svcutil.exe $serviceUrl `
        /serializer:XmlSerializer `
        /out:UnisonServiceClient_Xml.cs `
        /namespace:*, UnisonAccessService.Generated.Xml `
        /config:app_xml.config

    if ($LASTEXITCODE -eq 0) {
        Write-Host "SUCCESS: XmlSerializer client generated successfully" -ForegroundColor Green
        
        if (Test-Path "UnisonServiceClient_Xml.cs") {
            $fileSize = (Get-Item "UnisonServiceClient_Xml.cs").Length
            Write-Host "Generated file size: $fileSize bytes" -ForegroundColor Cyan
        }
    }
    else {
        Write-Host "WARNING: XmlSerializer generation failed with exit code $LASTEXITCODE" -ForegroundColor Yellow
    }
}
catch {
    Write-Host "ERROR: Failed to execute svcutil for XmlSerializer method: $($_.Exception.Message)" -ForegroundColor Red
}

# Method 3: Minimal generation for comparison
Write-Host "`n--- Method 3: Minimal svcutil ---" -ForegroundColor Green
try {
    & svcutil.exe $serviceUrl `
        /out:UnisonServiceClient_Minimal.cs `
        /namespace:*, UnisonAccessService.Generated.Minimal

    if ($LASTEXITCODE -eq 0) {
        Write-Host "SUCCESS: Minimal client generated successfully" -ForegroundColor Green
        
        if (Test-Path "UnisonServiceClient_Minimal.cs") {
            $fileSize = (Get-Item "UnisonServiceClient_Minimal.cs").Length
            Write-Host "Generated file size: $fileSize bytes" -ForegroundColor Cyan
        }
    }
    else {
        Write-Host "WARNING: Minimal generation failed with exit code $LASTEXITCODE" -ForegroundColor Yellow
    }
}
catch {
    Write-Host "ERROR: Failed to execute svcutil for minimal method: $($_.Exception.Message)" -ForegroundColor Red
}

# Summary
Write-Host "`n=== Generation Summary ===" -ForegroundColor Green
$generatedFiles = Get-ChildItem -Filter "UnisonServiceClient_*.cs" -ErrorAction SilentlyContinue

if ($generatedFiles.Count -gt 0) {
    Write-Host "Generated client files:" -ForegroundColor Cyan
    foreach ($file in $generatedFiles) {
        Write-Host "  - $($file.Name) ($($file.Length) bytes)" -ForegroundColor White
    }
    
    Write-Host "`nNext steps:" -ForegroundColor Yellow
    Write-Host "1. Integrate the DataContract client into UnisonAccessServiceClient.cs" -ForegroundColor White
    Write-Host "2. Test enum serialization with UpdateCard operation" -ForegroundColor White
    Write-Host "3. Implement fallback to HttpClient if enum issues persist" -ForegroundColor White
}
else {
    Write-Host "No client files were generated successfully" -ForegroundColor Red
    Write-Host "Will proceed with HttpClient-only implementation" -ForegroundColor Yellow
}

Write-Host "`n=== Client Generation Complete ===" -ForegroundColor Green
