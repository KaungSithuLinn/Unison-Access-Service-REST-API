Write-Host "=== Step 5: WCF Trace Log Analysis ===" -ForegroundColor Green

$serverIP = "192.168.10.206"
$currentTime = Get-Date -Format "yyyy-MM-dd HH:mm:ss"

Write-Host "Server: $serverIP"
Write-Host "Current Time: $currentTime"

Write-Host "`n=== EXPECTED LOG FILES ===" -ForegroundColor Yellow
Write-Host "Location: C:\logs\ on server $serverIP"
Write-Host "Files to check:"
Write-Host "- WCFTrace.svclog"
Write-Host "- WCFMessages.svclog" 
Write-Host "- Serialization.svclog"

Write-Host "`n=== ERROR CONTEXT ===" -ForegroundColor Magenta  
Write-Host "SOAP Request: UpdateCard operation"
Write-Host "Response: 400 Bad Request"
Write-Host "SOAPAction: http://tempuri.org/IAccessService/UpdateCard"
