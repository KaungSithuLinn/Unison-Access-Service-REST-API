# Test AccessService SOAP Fault Response
# This script tests if the WCF configuration fix worked

$soapRequest = @'
<?xml version="1.0" encoding="utf-8"?>
<soapenv:Envelope xmlns:soapenv="http://schemas.xmlsoap.org/soap/envelope/" xmlns:tem="http://tempuri.org/">
    <soapenv:Body>
        <tem:UpdateCard>
            <tem:request>invalid_request_data</tem:request>
        </tem:UpdateCard>
    </soapenv:Body>
</soapenv:Envelope>
'@

$url = "http://192.168.10.206:9003/Unison.AccessService"
$headers = @{
    'Content-Type' = 'text/xml; charset=utf-8'
    'SOAPAction'   = 'http://tempuri.org/IAccessService/UpdateCard'
}

Write-Host "Testing AccessService SOAP Fault Response..." -ForegroundColor Yellow
Write-Host "URL: $url"
Write-Host "Sending invalid SOAP request to trigger error..."

try {
    $response = Invoke-WebRequest -Uri $url -Method Post -Body $soapRequest -Headers $headers -ErrorAction Stop
    Write-Host "✅ SUCCESS: Got 200 OK response" -ForegroundColor Green
    Write-Host "Content Type: $($response.Headers.'Content-Type')"
    Write-Host "Response (first 500 chars):" 
    Write-Host $response.Content.Substring(0, [Math]::Min(500, $response.Content.Length))
}
catch {
    $statusCode = $_.Exception.Response.StatusCode
    $contentType = $_.Exception.Response.ContentType
    
    Write-Host "Status Code: $statusCode" -ForegroundColor Cyan
    Write-Host "Content Type: $contentType" -ForegroundColor Cyan
    
    # Try to read the response body
    try {
        $result = $_.Exception.Response.GetResponseStream()
        $reader = New-Object System.IO.StreamReader($result)
        $responseBody = $reader.ReadToEnd()
        
        if ($contentType -like "*xml*") {
            Write-Host "✅ SUCCESS: Response is XML (SOAP fault)!" -ForegroundColor Green
            Write-Host "🎉 WCF configuration fix SUCCESSFUL!" -ForegroundColor Green
            Write-Host "✅ AccessService now returns SOAP faults instead of HTML error pages" -ForegroundColor Green
        }
        elseif ($contentType -like "*html*") {
            Write-Host "❌ FAILED: Response is still HTML (error page)" -ForegroundColor Red
            Write-Host "❌ WCF configuration fix did not work" -ForegroundColor Red
        }
        else {
            Write-Host "📝 Unknown content type: $contentType" -ForegroundColor Yellow
        }
        
        Write-Host "Response Body (first 500 chars):" -ForegroundColor Cyan
        Write-Host "-" * 50
        Write-Host $responseBody.Substring(0, [Math]::Min(500, $responseBody.Length))
        if ($responseBody.Length -gt 500) {
            Write-Host "... (truncated)"
        }
        Write-Host "-" * 50
        
    }
    catch {
        Write-Host "Could not read response body: $($_.Exception.Message)" -ForegroundColor Red
    }
}

Write-Host "Test completed." -ForegroundColor Yellow
