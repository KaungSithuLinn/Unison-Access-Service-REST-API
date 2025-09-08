using UnisonRestAdapter.Models.Request;
using UnisonRestAdapter.Models.Response;

namespace UnisonRestAdapter.Services
{
    /// <summary>
    /// Interface for SOAP client operations
    /// This will be implemented after SOAP client proxy generation
    /// </summary>
    public interface ISoapClientService
    {
        Task<SoapUpdateCardResponse> UpdateCardAsync(UpdateCardRequest request, string token);
        Task<SoapUserResponse> GetUserAsync(string userId, string token);
        Task<bool> CheckHealthAsync(string token);
    }

    /// <summary>
    /// SOAP response models (temporary until proxy is generated)
    /// </summary>
    public class SoapUpdateCardResponse
    {
        public bool Success { get; set; }
        public string Message { get; set; } = string.Empty;
    }

    public class SoapUserResponse
    {
        public string UserName { get; set; } = string.Empty;
        public bool Success { get; set; }
    }
}
