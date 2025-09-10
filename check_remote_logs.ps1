# PowerShell script to check WCF trace logs on remote server
# Use PSRemoting or alternative method to access logs

$serverIP = "192.168.10.206"
$logPath = "C:\logs"

Write-Host "Step 5: Checking WCF Trace Logs after UpdateCard Error Reproduction" -ForegroundColor Green
Write-Host "=========================================================" -ForegroundColor Green

# Try to use Invoke-Command with different credential approaches
try {
    # Method 1: Try with current credentials
    Write-Host "Attempting to connect to $serverIP..." -ForegroundColor Yellow
    
    # Check if we can at least ping the server
    $pingResult = Test-NetConnection -ComputerName $serverIP -Port 9003
    if ($pingResult.TcpTestSucceeded) {
        Write-Host "✓ Server $serverIP:9003 is accessible" -ForegroundColor Green
        
        # Try to list log files (this will require authentication)
        # You may need to provide credentials or use Windows authentication
        Write-Host "Log files should be located at: \\$serverIP\c$\logs" -ForegroundColor Yellow
        Write-Host "Manual steps to check logs:" -ForegroundColor Cyan
        Write-Host "1. RDP to $serverIP" -ForegroundColor White  
        Write-Host "2. Navigate to C:\logs" -ForegroundColor White
        Write-Host "3. Look for files: WCFTrace.svclog, WCFMessages.svclog, Serialization.svclog" -ForegroundColor White
        Write-Host "4. Check file timestamps to confirm they were updated after our SOAP request" -ForegroundColor White
        
        # Alternative: Check Windows Event Log remotely
        Write-Host "`nAttempting to check Windows Event Log remotely..." -ForegroundColor Yellow
        
        # Get recent Application log entries
        try {
            $events = Get-WinEvent -ComputerName $serverIP -LogName Application -MaxEvents 10 -ErrorAction Stop | 
            Where-Object { $_.TimeCreated -gt (Get-Date).AddMinutes(-10) }
            
            if ($events) {
                Write-Host "✓ Recent Application Log Events:" -ForegroundColor Green
                $events | Format-Table TimeCreated, Id, LevelDisplayName, Message -Wrap
            }
        }
        catch {
            Write-Host "✗ Could not access Windows Event Log: $($_.Exception.Message)" -ForegroundColor Red
        }
        
    }
    else {
        Write-Host "✗ Cannot connect to $serverIP:9003" -ForegroundColor Red
    }
    
}
catch {
    Write-Host "✗ Connection failed: $($_.Exception.Message)" -ForegroundColor Red
    Write-Host "Manual log verification required" -ForegroundColor Yellow
}

Write-Host "`n=== LOG ANALYSIS EXPECTED CONTENT ===" -ForegroundColor Magenta
Write-Host "Based on our SOAP request, the following should be in the logs:" -ForegroundColor White
Write-Host "1. WCFMessages.svclog: Complete SOAP request and response messages" -ForegroundColor White
Write-Host "2. WCFTrace.svclog: Service model tracing with error details" -ForegroundColor White  
Write-Host "3. Serialization.svclog: XML serialization issues if any" -ForegroundColor White
Write-Host "4. Look for 400 Bad Request error details and root cause" -ForegroundColor White

Write-Host "`n=== NEXT STEPS ===" -ForegroundColor Magenta
Write-Host "1. Manually verify log files were created/updated" -ForegroundColor White
Write-Host "2. Analyze log content using WCF Service Trace Viewer" -ForegroundColor White
Write-Host "3. Identify specific error cause in SOAP request format" -ForegroundColor White
Write-Host "4. Apply targeted fix based on log analysis" -ForegroundColor White
