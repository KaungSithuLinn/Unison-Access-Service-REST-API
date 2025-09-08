# Direct SOAP Test Script
$soapEnvelope = @"
<?xml version="1.0" encoding="utf-8"?>
<soap:Envelope xmlns:soap="http://schemas.xmlsoap.org/soap/envelope/" 
               xmlns:tem="http://tempuri.org/">
  <soap:Header>
    <Unison-Token xmlns="http://tempuri.org/">595d799a-9553-4ddf-8fd9-c27b1f233ce7</Unison-Token>
  </soap:Header>
  <soap:Body>
    <tem:UpdateCard>
      <tem:userId>TEST_USER_001</tem:userId>
      <tem:profileName>Default</tem:profileName>
      <tem:cardNumber>12345678</tem:cardNumber>
      <tem:systemNumber>001</tem:systemNumber>
      <tem:versionNumber>1</tem:versionNumber>
      <tem:miscNumber>000</tem:miscNumber>
      <tem:cardStatus>Active</tem:cardStatus>
    </tem:UpdateCard>
  </soap:Body>
</soap:Envelope>
"@

$headers = @{
    "Content-Type" = "text/xml; charset=utf-8"
    "SOAPAction"   = '"http://tempuri.org/IAccessService/UpdateCard"'
    "Unison-Token" = "595d799a-9553-4ddf-8fd9-c27b1f233ce7"
    "User-Agent"   = "UpdateCard-SOAP-Test/1.0"
}

try {
    Write-Host "Testing direct SOAP connection to backend..." -ForegroundColor Green
    Write-Host "SOAP Envelope:" -ForegroundColor Cyan
    Write-Host $soapEnvelope -ForegroundColor Gray
    
    $response = Invoke-WebRequest -Uri "http://192.168.10.206:9003/Unison.AccessService" -Method POST -Body $soapEnvelope -Headers $headers
    Write-Host "SOAP Response Status: $($response.StatusCode)" -ForegroundColor Green
    Write-Host "SOAP Response Content:" -ForegroundColor Yellow
    Write-Host $response.Content
}
catch {
    Write-Host "SOAP Request failed: $($_.Exception.Message)" -ForegroundColor Red
    Write-Host "Error Details: $($_.ErrorDetails)" -ForegroundColor Red
}
