using UnisonRestAdapter.Models.Request;
using UnisonRestAdapter.Services;
using Microsoft.Extensions.Options;
using UnisonRestAdapter.Configuration;
using System.Text;
using System.Xml.Linq;

namespace UnisonRestAdapter.Services
{
    /// <summary>
    /// Implementation of SOAP client service using HttpClient
    /// Implements direct SOAP calls to Unison Access Service
    /// </summary>
    public class SoapClientService : ISoapClientService
    {
        private readonly HttpClient _httpClient;
        private readonly UnisonSettings _settings;
        private readonly ILogger<SoapClientService> _logger;

        public SoapClientService(
            IHttpClientFactory httpClientFactory,
            IOptions<UnisonSettings> settings,
            ILogger<SoapClientService> logger)
        {
            _httpClient = httpClientFactory.CreateClient("UnisonSoapClient");
            _settings = settings.Value;
            _logger = logger;
        }

        public async Task<SoapUpdateCardResponse> UpdateCardAsync(UpdateCardRequest request, string token)
        {
            try
            {
                _logger.LogInformation("Calling SOAP UpdateCard for CardId: {CardId}", request.CardId);

                // Strictly map REST request to required SOAP fields using PascalCase property names
                var userId = request.UserName ?? "DEFAULT_USER";
                var profileName = "Default";
                var cardNumber = request.CardId ?? string.Empty;
                var systemNumber = "001";
                var versionNumber = "1";
                var miscNumber = "000";
                var cardStatus = request.IsActive.HasValue ? (request.IsActive.Value ? "1" : "0") : "1";

                // Build SOAP envelope exactly as required by backend according to WSDL/XSD
                var soapEnvelope = $@"<?xml version='1.0' encoding='utf-8'?>
<soap:Envelope xmlns:soap='http://schemas.xmlsoap.org/soap/envelope/' xmlns:tem='http://tempuri.org/'>
    <soap:Body>
        <tem:UpdateCard>
            <tem:userId>{userId}</tem:userId>
            <tem:profileName>{profileName}</tem:profileName>
            <tem:cardNumber>{cardNumber}</tem:cardNumber>
            <tem:systemNumber>{systemNumber}</tem:systemNumber>
            <tem:versionNumber>{versionNumber}</tem:versionNumber>
            <tem:miscNumber>{miscNumber}</tem:miscNumber>
            <tem:cardStatus>{cardStatus}</tem:cardStatus>
            <tem:cardName>{cardNumber}</tem:cardName>
        </tem:UpdateCard>
    </soap:Body>
</soap:Envelope>";

                // Log the full SOAP envelope for debugging
                _logger.LogInformation("SOAP Envelope Sent:\n{Envelope}", soapEnvelope);

                var content = new StringContent(soapEnvelope, Encoding.UTF8, "text/xml");
                content.Headers.Clear();
                content.Headers.Add("Content-Type", "text/xml; charset=utf-8");

                var requestMessage = new HttpRequestMessage(HttpMethod.Post, _settings.ServiceUrl)
                {
                    Content = content
                };
                requestMessage.Headers.Add("SOAPAction", "http://tempuri.org/IAccessService/UpdateCard");

                // Send SOAP request
                var response = await _httpClient.SendAsync(requestMessage);
                var responseContent = await response.Content.ReadAsStringAsync();

                _logger.LogInformation("SOAP Response: Status={StatusCode}, Content={Content}",
                    response.StatusCode, responseContent);

                // Parse SOAP response
                return ParseUpdateCardResponse(responseContent, response.IsSuccessStatusCode);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "SOAP UpdateCard failed for CardId: {CardId}", request.CardId);
                return new SoapUpdateCardResponse
                {
                    Success = false,
                    Message = $"SOAP call failed: {ex.Message}"
                };
            }
        }

        private SoapUpdateCardResponse ParseUpdateCardResponse(string responseContent, bool isSuccessStatusCode)
        {
            try
            {
                if (!isSuccessStatusCode)
                {
                    // Check if it's a SOAP fault
                    if (responseContent.Contains("soap:Fault") || responseContent.Contains("Fault"))
                    {
                        var faultMessage = ExtractSoapFaultMessage(responseContent);
                        return new SoapUpdateCardResponse
                        {
                            Success = false,
                            Message = $"SOAP Fault: {faultMessage}"
                        };
                    }

                    return new SoapUpdateCardResponse
                    {
                        Success = false,
                        Message = $"HTTP Error: {responseContent}"
                    };
                }

                // Parse successful SOAP response
                var doc = XDocument.Parse(responseContent);
                var ns = XNamespace.Get("http://tempuri.org/");
                var soapNs = XNamespace.Get("http://schemas.xmlsoap.org/soap/envelope/");

                var updateCardResponse = doc.Descendants(ns + "UpdateCardResponse").FirstOrDefault();

                if (updateCardResponse != null)
                {
                    var result = updateCardResponse.Element(ns + "UpdateCardResult")?.Value;
                    return new SoapUpdateCardResponse
                    {
                        Success = true,
                        Message = result ?? "Card updated successfully"
                    };
                }

                // If no specific response format, assume success
                return new SoapUpdateCardResponse
                {
                    Success = true,
                    Message = "Card updated successfully"
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error parsing SOAP response: {Response}", responseContent);
                return new SoapUpdateCardResponse
                {
                    Success = false,
                    Message = $"Response parsing error: {ex.Message}"
                };
            }
        }

        private string ExtractSoapFaultMessage(string responseContent)
        {
            try
            {
                var doc = XDocument.Parse(responseContent);
                var faultString = doc.Descendants().FirstOrDefault(x => x.Name.LocalName == "faultstring")?.Value;
                var faultDetail = doc.Descendants().FirstOrDefault(x => x.Name.LocalName == "detail")?.Value;

                return faultString ?? faultDetail ?? "Unknown SOAP fault";
            }
            catch
            {
                return "Could not parse SOAP fault";
            }
        }

        public async Task<SoapUserResponse> GetUserAsync(string userId, string token)
        {
            try
            {
                _logger.LogInformation("Calling SOAP GetUser for UserId: {UserId}", userId);

                // Create SOAP envelope for GetUser operation
                var soapEnvelope = CreateGetUserSoapEnvelope(userId, token);
                var content = new StringContent(soapEnvelope, Encoding.UTF8, "text/xml");

                // Set required SOAP headers
                content.Headers.Clear();
                content.Headers.Add("Content-Type", "text/xml; charset=utf-8");

                var requestMessage = new HttpRequestMessage(HttpMethod.Post, _settings.ServiceUrl)
                {
                    Content = content
                };
                requestMessage.Headers.Add("SOAPAction", "http://tempuri.org/IAccessService/GetUser");

                // Send SOAP request
                var response = await _httpClient.SendAsync(requestMessage);
                var responseContent = await response.Content.ReadAsStringAsync();

                _logger.LogDebug("SOAP GetUser Response: Status={StatusCode}, Content={Content}",
                    response.StatusCode, responseContent);

                return ParseGetUserResponse(responseContent, response.IsSuccessStatusCode);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "SOAP GetUser failed for UserId: {UserId}", userId);
                return new SoapUserResponse
                {
                    Success = false,
                    UserName = string.Empty
                };
            }
        }

        private string CreateGetUserSoapEnvelope(string userId, string token)
        {
            var soapEnvelope = $@"<?xml version=""1.0"" encoding=""utf-8""?>
<soap:Envelope xmlns:soap=""http://schemas.xmlsoap.org/soap/envelope/"" 
               xmlns:tem=""http://tempuri.org/"">
  <soap:Header>
    <tem:Unison-Token>{token}</tem:Unison-Token>
  </soap:Header>
  <soap:Body>
    <tem:GetUser>
      <tem:userId>{userId}</tem:userId>
    </tem:GetUser>
  </soap:Body>
</soap:Envelope>";

            return soapEnvelope;
        }

        private SoapUserResponse ParseGetUserResponse(string responseContent, bool isSuccessStatusCode)
        {
            try
            {
                if (!isSuccessStatusCode)
                {
                    return new SoapUserResponse
                    {
                        Success = false,
                        UserName = string.Empty
                    };
                }

                var doc = XDocument.Parse(responseContent);
                var ns = XNamespace.Get("http://tempuri.org/");

                var getUserResponse = doc.Descendants(ns + "GetUserResponse").FirstOrDefault();
                if (getUserResponse != null)
                {
                    var userName = getUserResponse.Element(ns + "GetUserResult")?.Value;
                    return new SoapUserResponse
                    {
                        Success = true,
                        UserName = userName ?? "Unknown User"
                    };
                }

                return new SoapUserResponse
                {
                    Success = true,
                    UserName = "User found"
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error parsing GetUser SOAP response: {Response}", responseContent);
                return new SoapUserResponse
                {
                    Success = false,
                    UserName = string.Empty
                };
            }
        }

        public async Task<bool> CheckHealthAsync(string token)
        {
            try
            {
                _logger.LogInformation("Performing SOAP health check");

                // Use a simple WSDL request to check service health
                var healthCheckUrl = $"{_settings.ServiceUrl}?wsdl";
                var response = await _httpClient.GetAsync(healthCheckUrl);

                if (response.IsSuccessStatusCode)
                {
                    var content = await response.Content.ReadAsStringAsync();
                    // Check if response contains WSDL content
                    bool isValidWsdl = content.Contains("<wsdl:definitions") || content.Contains("definitions");
                    _logger.LogInformation("SOAP health check completed: {IsHealthy}", isValidWsdl);
                    return isValidWsdl;
                }

                _logger.LogWarning("SOAP health check failed: HTTP {StatusCode}", response.StatusCode);
                return false;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "SOAP health check failed");
                return false;
            }
        }
    }
}
