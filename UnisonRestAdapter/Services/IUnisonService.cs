using UnisonRestAdapter.Models.Request;
using UnisonRestAdapter.Models.Response;

namespace UnisonRestAdapter.Services
{
    /// <summary>
    /// Interface for Unison Access Service operations
    /// </summary>
    public interface IUnisonService
    {
        /// <summary>
        /// Updates card information via SOAP service
        /// </summary>
        /// <param name="request">Card update request</param>
        /// <param name="token">Authentication token</param>
        /// <returns>Update response</returns>
        Task<UpdateCardResponse> UpdateCardAsync(UpdateCardRequest request, string token);

        /// <summary>
        /// Retrieves user information via SOAP service
        /// </summary>
        /// <param name="userId">User identifier</param>
        /// <param name="token">Authentication token</param>
        /// <returns>User information</returns>
        Task<UserResponse> GetUserAsync(string userId, string token);

        /// <summary>
        /// Health check for SOAP service connectivity
        /// </summary>
        /// <param name="token">Authentication token</param>
        /// <returns>Health status</returns>
        Task<HealthResponse> CheckHealthAsync(string token);

        /// <summary>
        /// Validates card information via SOAP service
        /// </summary>
        /// <param name="request">Card validation request</param>
        /// <param name="token">Authentication token</param>
        /// <returns>Validation response</returns>
        Task<CardValidationResponse> ValidateCardAsync(CardValidationRequest request, string token);

        /// <summary>
        /// Activates a card via SOAP service
        /// </summary>
        /// <param name="request">Card activation request</param>
        /// <param name="token">Authentication token</param>
        /// <returns>Activation response</returns>
        Task<CardActivationResponse> ActivateCardAsync(CardActivationRequest request, string token);

        /// <summary>
        /// Deactivates a card via SOAP service
        /// </summary>
        /// <param name="request">Card activation request (with deactivation intent)</param>
        /// <param name="token">Authentication token</param>
        /// <returns>Activation response</returns>
        Task<CardActivationResponse> DeactivateCardAsync(CardActivationRequest request, string token);

        /// <summary>
        /// Gets system version information via SOAP service
        /// </summary>
        /// <param name="token">Authentication token</param>
        /// <returns>Version information</returns>
        Task<VersionResponse> GetVersionAsync(string token);

        /// <summary>
        /// Ping operation to test SOAP service connectivity
        /// </summary>
        /// <param name="token">Authentication token</param>
        /// <returns>Ping response status</returns>
        Task<bool> PingAsync(string token);
    }
}
