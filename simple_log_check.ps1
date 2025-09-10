$serverIP = "192.168.10.206"

Write-Host "=== Step 5: WCF Trace Log Analysis ===" -ForegroundColor Green

# Check server connectivity
$pingResult = Test-NetConnection -ComputerName $serverIP -Port 9003
if ($pingResult.TcpTestSucceeded) {
    Write-Host "✓ Server accessible at $serverIP:9003" -ForegroundColor Green
}
else {
    Write-Host "✗ Server not accessible" -ForegroundColor Red
}

Write-Host "`n=== EXPECTED LOG FILES ===" -ForegroundColor Yellow
Write-Host "Location: C:\logs\ on server $serverIP"
Write-Host "Files to check:"
Write-Host "- WCFTrace.svclog (Service model traces)"
Write-Host "- WCFMessages.svclog (Message logging)" 
Write-Host "- Serialization.svclog (XML serialization)"

Write-Host "`n=== MANUAL VERIFICATION NEEDED ===" -ForegroundColor Cyan
Write-Host "1. RDP to $serverIP"
Write-Host "2. Check C:\logs directory"
Write-Host "3. Verify files updated after SOAP request (around $(Get-Date -Format 'yyyy-MM-dd HH:mm:ss'))"
Write-Host "4. Open logs with WCF Service Trace Viewer for detailed analysis"

Write-Host "`n=== ERROR ANALYSIS CONTEXT ===" -ForegroundColor Magenta  
Write-Host "- SOAP Request: UpdateCard operation"
Write-Host "- Response: 400 Bad Request"
Write-Host "- SOAPAction: http://tempuri.org/IAccessService/UpdateCard"
Write-Host "- Expected in logs: Detailed error cause for 400 response"
