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
    }
}
