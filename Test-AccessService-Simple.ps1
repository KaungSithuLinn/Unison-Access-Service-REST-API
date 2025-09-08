# Test AccessService SOAP Fault Response
Write-Host "Testing AccessService SOAP Fault Response..." -ForegroundColor Yellow

$soapRequest = '<?xml version="1.0" encoding="utf-8"?><soapenv:Envelope xmlns:soapenv="http://schemas.xmlsoap.org/soap/envelope/" xmlns:tem="http://tempuri.org/"><soapenv:Body><tem:UpdateCard><tem:request>invalid_request_data</tem:request></tem:UpdateCard></soapenv:Body></soapenv:Envelope>'

$url = "http://192.168.10.206:9003/Unison.AccessService"
$headers = @{
    'Content-Type' = 'text/xml; charset=utf-8'
    'SOAPAction'   = 'http://tempuri.org/IAccessService/UpdateCard'
}

Write-Host "URL: $url"
Write-Host "Sending invalid SOAP request to trigger error..."

try {
    $response = Invoke-WebRequest -Uri $url -Method Post -Body $soapRequest -Headers $headers
    Write-Host "SUCCESS: Got response without exception" -ForegroundColor Green
    Write-Host "Content Type: $($response.Headers.'Content-Type')"
    Write-Host "Response: $($response.Content.Substring(0, [Math]::Min(500, $response.Content.Length)))"
}
catch {
    $statusCode = $_.Exception.Response.StatusCode
    $contentType = $_.Exception.Response.ContentType
    
    Write-Host "Status Code: $statusCode" -ForegroundColor Cyan
    Write-Host "Content Type: $contentType" -ForegroundColor Cyan
    
    if ($contentType -like "*xml*") {
        Write-Host "SUCCESS: Response is XML (SOAP fault)!" -ForegroundColor Green
        Write-Host "WCF configuration fix SUCCESSFUL!" -ForegroundColor Green
    }
    elseif ($contentType -like "*html*") {
        Write-Host "FAILED: Response is still HTML" -ForegroundColor Red
    }
    else {
        Write-Host "Unknown content type: $contentType" -ForegroundColor Yellow
    }
}

Write-Host "Test completed." -ForegroundColor Yellow
