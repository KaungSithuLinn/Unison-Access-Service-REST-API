using System;
using System.IO;
using System.Net.Http;
using System.ServiceModel;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Linq;

namespace UnisonAccessService
{
    /// <summary>
    /// Hybrid client for Unison AccessService that uses svcutil-generated client with fallback to HttpClient
    /// Implements the recommended approach from Step 2 analysis
    /// </summary>
    public class UnisonAccessServiceClient : IDisposable
    {
        private readonly string _serviceUrl;
        private readonly string _username;
        private readonly string _password;
        private readonly string _domain;
        private readonly HttpClient _httpClient;
        private object? _svcutilClient;

        public UnisonAccessServiceClient(string serviceUrl, string username, string password, string domain)
        {
            _serviceUrl = serviceUrl;
            _username = username;
            _password = password;
            _domain = domain;
            _httpClient = new HttpClient();

            // Try to initialize svcutil-generated client
            InitializeSvcutilClient();
        }

        private void InitializeSvcutilClient()
        {
            try
            {
                // This would use the svcutil-generated client
                // For now, we'll implement the HttpClient approach as fallback
                Console.WriteLine("INFO: svcutil-generated client not available, using HttpClient fallback");
                _svcutilClient = null;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"WARNING: Failed to initialize svcutil client: {ex.Message}");
                _svcutilClient = null;
            }
        }

        public async Task<string> PingAsync()
        {
            if (_svcutilClient != null)
            {
                try
                {
                    return await PingUsingSvcutil();
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"WARNING: svcutil Ping failed: {ex.Message}. Falling back to HttpClient.");
                }
            }

            return await PingUsingHttpClient();
        }

        public async Task<string> GetVersionAsync()
        {
            if (_svcutilClient != null)
            {
                try
                {
                    return await GetVersionUsingSvcutil();
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"WARNING: svcutil GetVersion failed: {ex.Message}. Falling back to HttpClient.");
                }
            }

            return await GetVersionUsingHttpClient();
        }

        public async Task<string> UpdateCardAsync(string userId, string profileName, string cardNumber,
            int systemNumber, int versionNumber, int miscNumber, string cardStatus, string cardName)
        {
            if (_svcutilClient != null)
            {
                try
                {
                    return await UpdateCardUsingSvcutil(userId, profileName, cardNumber, systemNumber,
                        versionNumber, miscNumber, cardStatus, cardName);
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"WARNING: svcutil UpdateCard failed: {ex.Message}. Falling back to HttpClient.");
                }
            }

            return await UpdateCardUsingHttpClient(userId, profileName, cardNumber, systemNumber,
                versionNumber, miscNumber, cardStatus, cardName);
        }

        private async Task<string> PingUsingSvcutil()
        {
            // TODO: Implement when svcutil-generated client is available
            throw new NotImplementedException("svcutil client not implemented yet");
        }

        private async Task<string> GetVersionUsingSvcutil()
        {
            // TODO: Implement when svcutil-generated client is available
            throw new NotImplementedException("svcutil client not implemented yet");
        }

        private async Task<string> UpdateCardUsingSvcutil(string userId, string profileName, string cardNumber,
            int systemNumber, int versionNumber, int miscNumber, string cardStatus, string cardName)
        {
            // TODO: Implement when svcutil-generated client is available
            throw new NotImplementedException("svcutil client not implemented yet");
        }

        private async Task<string> PingUsingHttpClient()
        {
            var soapRequest = SoapTemplates.CreatePingRequest(_username, _password, _domain);
            return await SendSoapRequest(soapRequest, "http://tempuri.org/IAccessService/Ping");
        }

        private async Task<string> GetVersionUsingHttpClient()
        {
            var soapRequest = SoapTemplates.CreateGetVersionRequest(_username, _password, _domain);
            return await SendSoapRequest(soapRequest, "http://tempuri.org/IAccessService/GetVersion");
        }

        private async Task<string> UpdateCardUsingHttpClient(string userId, string profileName, string cardNumber,
            int systemNumber, int versionNumber, int miscNumber, string cardStatus, string cardName)
        {
            var soapRequest = SoapTemplates.CreateUpdateCardRequest(_username, _password, _domain,
                userId, profileName, cardNumber, systemNumber, versionNumber, miscNumber, cardStatus, cardName);
            return await SendSoapRequest(soapRequest, "http://tempuri.org/IAccessService/UpdateCard");
        }

        private async Task<string> SendSoapRequest(string soapXml, string soapAction)
        {
            try
            {
                var content = new StringContent(soapXml, Encoding.UTF8, "text/xml");

                // Add proper SOAP headers based on research findings
                content.Headers.Add("SOAPAction", soapAction);

                // Set explicit content type with charset
                content.Headers.Remove("Content-Type");
                content.Headers.Add("Content-Type", "text/xml; charset=utf-8");

                // Add Host header (required for some WCF configurations)
                var uri = new Uri(_serviceUrl);
                _httpClient.DefaultRequestHeaders.Remove("Host");
                _httpClient.DefaultRequestHeaders.Add("Host", $"{uri.Host}:{uri.Port}");

                // Add Content-Length explicitly
                var contentBytes = Encoding.UTF8.GetBytes(soapXml);
                content.Headers.Remove("Content-Length");
                content.Headers.Add("Content-Length", contentBytes.Length.ToString());

                var response = await _httpClient.PostAsync(_serviceUrl, content);
                var responseContent = await response.Content.ReadAsStringAsync();

                // Check if we got HTML instead of SOAP (common error condition)
                if (responseContent.TrimStart().StartsWith("<!DOCTYPE") ||
                    responseContent.TrimStart().StartsWith("<html", StringComparison.OrdinalIgnoreCase) ||
                    responseContent.Contains("<title>Request Error</title>") ||
                    responseContent.Contains("Request Error") ||
                    responseContent.Contains("service help page"))
                {
                    var errorMessage = $"Received HTML error page instead of SOAP response. Status: {response.StatusCode}. " +
                                     $"This indicates a WCF service configuration issue or incorrect request format. " +
                                     $"Check service help page at: {_serviceUrl}/help\n\n" +
                                     $"Response content (first 500 chars): {responseContent.Substring(0, Math.Min(500, responseContent.Length))}";

                    Console.WriteLine($"ERROR: {errorMessage}");
                    throw new InvalidOperationException(errorMessage);
                }                // Validate it's valid XML
                try
                {
                    XDocument.Parse(responseContent);
                }
                catch (XmlException xmlEx)
                {
                    throw new InvalidOperationException($"Invalid XML response: {xmlEx.Message}. Content: {responseContent.Substring(0, Math.Min(500, responseContent.Length))}...");
                }

                return responseContent;
            }
            catch (HttpRequestException httpEx)
            {
                throw new InvalidOperationException($"HTTP request failed: {httpEx.Message}");
            }
            catch (TaskCanceledException timeoutEx)
            {
                throw new InvalidOperationException($"Request timeout: {timeoutEx.Message}");
            }
        }

        public void Dispose()
        {
            _httpClient?.Dispose();
        }
    }
}
