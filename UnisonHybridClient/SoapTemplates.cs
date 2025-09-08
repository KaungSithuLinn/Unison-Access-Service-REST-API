using System;

namespace UnisonAccessService
{
    /// <summary>
    /// SOAP request templates for Unison AccessService operations
    /// Based on the working SOAP requests found in the project
    /// </summary>
    public static class SoapTemplates
    {
        public static string CreatePingRequest(string username, string password, string domain)
        {
            return $@"<?xml version=""1.0"" encoding=""utf-8""?>
<soap:Envelope xmlns:soap=""http://schemas.xmlsoap.org/soap/envelope/"" xmlns:tem=""http://tempuri.org/"">
  <soap:Header>
    <tem:AuthenticationHeader>
      <tem:Username>{EscapeXml(username)}</tem:Username>
      <tem:Password>{EscapeXml(password)}</tem:Password>
      <tem:Domain>{EscapeXml(domain)}</tem:Domain>
    </tem:AuthenticationHeader>
  </soap:Header>
  <soap:Body>
    <tem:Ping/>
  </soap:Body>
</soap:Envelope>";
        }

        public static string CreateGetVersionRequest(string username, string password, string domain)
        {
            return $@"<?xml version=""1.0"" encoding=""utf-8""?>
<soap:Envelope xmlns:soap=""http://schemas.xmlsoap.org/soap/envelope/"" xmlns:tem=""http://tempuri.org/"">
  <soap:Header>
    <tem:AuthenticationHeader>
      <tem:Username>{EscapeXml(username)}</tem:Username>
      <tem:Password>{EscapeXml(password)}</tem:Password>
      <tem:Domain>{EscapeXml(domain)}</tem:Domain>
    </tem:AuthenticationHeader>
  </soap:Header>
  <soap:Body>
    <tem:GetVersion/>
  </soap:Body>
</soap:Envelope>";
        }

        public static string CreateUpdateCardRequest(string username, string password, string domain,
            string userId, string profileName, string cardNumber, int systemNumber, int versionNumber,
            int miscNumber, string cardStatus, string cardName)
        {
            return $@"<?xml version=""1.0"" encoding=""utf-8""?>
<soap:Envelope xmlns:soap=""http://schemas.xmlsoap.org/soap/envelope/"" xmlns:tem=""http://tempuri.org/"">
  <soap:Header>
    <tem:AuthenticationHeader>
      <tem:Username>{EscapeXml(username)}</tem:Username>
      <tem:Password>{EscapeXml(password)}</tem:Password>
      <tem:Domain>{EscapeXml(domain)}</tem:Domain>
    </tem:AuthenticationHeader>
  </soap:Header>
  <soap:Body>
    <tem:UpdateCard>
      <tem:userId>{EscapeXml(userId)}</tem:userId>
      <tem:profileName>{EscapeXml(profileName)}</tem:profileName>
      <tem:cardNumber>{EscapeXml(cardNumber)}</tem:cardNumber>
      <tem:systemNumber>{systemNumber}</tem:systemNumber>
      <tem:versionNumber>{versionNumber}</tem:versionNumber>
      <tem:miscNumber>{miscNumber}</tem:miscNumber>
      <tem:cardStatus>{EscapeXml(cardStatus)}</tem:cardStatus>
      <tem:cardName>{EscapeXml(cardName)}</tem:cardName>
    </tem:UpdateCard>
  </soap:Body>
</soap:Envelope>";
        }

        /// <summary>
        /// Escapes XML special characters to prevent injection attacks
        /// </summary>
        private static string EscapeXml(string input)
        {
            if (string.IsNullOrEmpty(input))
                return input;

            return input
                .Replace("&", "&amp;")
                .Replace("<", "&lt;")
                .Replace(">", "&gt;")
                .Replace("\"", "&quot;")
                .Replace("'", "&apos;");
        }
    }
}
