using UnisonRestAdapter.Models.Request;
using UnisonRestAdapter.Models.Response;

namespace UnisonRestAdapter.Services
{
    /// <summary>
    /// Implementation of Unison Access Service operations
    /// Wraps SOAP client calls and handles authentication
    /// </summary>
    public class UnisonService : IUnisonService
    {
        private readonly ISoapClientService _soapClientService;
        private readonly ILogger<UnisonService> _logger;
        private readonly IConfiguration _configuration;

        /// <summary>
        /// Initializes a new instance of the UnisonService
        /// </summary>
        /// <param name="soapClientService">SOAP client service for backend communication</param>
        /// <param name="logger">Logger instance</param>
        /// <param name="configuration">Application configuration</param>
        public UnisonService(
            ISoapClientService soapClientService,
            ILogger<UnisonService> logger,
            IConfiguration configuration)
        {
            _soapClientService = soapClientService;
            _logger = logger;
            _configuration = configuration;
        }

        /// <summary>
        /// Updates card information via SOAP service
        /// </summary>
        /// <param name="request">Card update request</param>
        /// <param name="token">Authentication token</param>
        /// <returns>Update response</returns>
        public async Task<UpdateCardResponse> UpdateCardAsync(UpdateCardRequest request, string token)
        {
            try
            {
                _logger.LogInformation("Processing UpdateCard request for CardId: {CardId}", request.CardId);

                // This will be implemented after SOAP client proxy is generated
                var soapResponse = await _soapClientService.UpdateCardAsync(request, token);

                return new UpdateCardResponse
                {
                    Success = soapResponse.Success,
                    Message = soapResponse.Message,
                    CardId = request.CardId
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing UpdateCard request for CardId: {CardId}", request.CardId);
                return new UpdateCardResponse
                {
                    Success = false,
                    Message = $"Error: {ex.Message}",
                    CardId = request.CardId
                };
            }
        }

        /// <summary>
        /// Retrieves user information via SOAP service
        /// </summary>
        /// <param name="userId">User identifier</param>
        /// <param name="token">Authentication token</param>
        /// <returns>User information</returns>
        public async Task<UserResponse> GetUserAsync(string userId, string token)
        {
            try
            {
                _logger.LogInformation("Processing GetUser request for UserId: {UserId}", userId);

                // This will be implemented after SOAP client proxy is generated
                var soapResponse = await _soapClientService.GetUserAsync(userId, token);

                return new UserResponse
                {
                    UserId = userId,
                    UserName = soapResponse.UserName,
                    Success = true
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing GetUser request for UserId: {UserId}", userId);
                return new UserResponse
                {
                    UserId = userId,
                    Success = false,
                    Message = $"Error: {ex.Message}"
                };
            }
        }

        /// <summary>
        /// Health check for SOAP service connectivity
        /// </summary>
        /// <param name="token">Authentication token</param>
        /// <returns>Health status</returns>
        public async Task<HealthResponse> CheckHealthAsync(string token)
        {
            var stopwatch = System.Diagnostics.Stopwatch.StartNew();
            try
            {
                _logger.LogInformation("Processing health check request");

                // This will be implemented after SOAP client proxy is generated
                var isHealthy = await _soapClientService.CheckHealthAsync(token);

                stopwatch.Stop();

                return new HealthResponse
                {
                    IsHealthy = isHealthy,
                    Message = isHealthy ? "Service is healthy" : "Service is unhealthy",
                    Timestamp = DateTime.UtcNow,
                    ResponseTime = stopwatch.ElapsedMilliseconds
                };
            }
            catch (Exception ex)
            {
                stopwatch.Stop();
                _logger.LogError(ex, "Error during health check");
                return new HealthResponse
                {
                    IsHealthy = false,
                    Message = $"Health check failed: {ex.Message}",
                    Timestamp = DateTime.UtcNow,
                    ResponseTime = stopwatch.ElapsedMilliseconds
                };
            }
        }

        /// <summary>
        /// Validates card information via SOAP service
        /// </summary>
        /// <param name="request">Card validation request</param>
        /// <param name="token">Authentication token</param>
        /// <returns>Validation response</returns>
        public async Task<CardValidationResponse> ValidateCardAsync(CardValidationRequest request, string token)
        {
            try
            {
                _logger.LogInformation("Processing ValidateCard request for CardId: {CardId}", request.CardId);

                // For now, we'll simulate validation using the UpdateCard SOAP operation
                // In a real implementation, this would call a specific validation SOAP method
                var updateRequest = new UpdateCardRequest
                {
                    CardId = request.CardId,
                    UserName = request.UserId ?? "VALIDATION",
                    FirstName = "Validation",
                    LastName = "Check"
                };

                var soapResponse = await _soapClientService.UpdateCardAsync(updateRequest, token);

                return new CardValidationResponse
                {
                    Success = soapResponse.Success,
                    Message = soapResponse.Success ? "Card validation completed" : soapResponse.Message,
                    CardId = request.CardId,
                    IsValid = soapResponse.Success,
                    CardStatus = soapResponse.Success ? "Valid" : "Invalid"
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing ValidateCard request for CardId: {CardId}", request.CardId);
                return new CardValidationResponse
                {
                    Success = false,
                    Message = $"Validation error: {ex.Message}",
                    CardId = request.CardId,
                    IsValid = false,
                    CardStatus = "Error"
                };
            }
        }

        /// <summary>
        /// Activates a card via SOAP service
        /// </summary>
        /// <param name="request">Card activation request</param>
        /// <param name="token">Authentication token</param>
        /// <returns>Activation response</returns>
        public async Task<CardActivationResponse> ActivateCardAsync(CardActivationRequest request, string token)
        {
            try
            {
                _logger.LogInformation("Processing ActivateCard request for CardId: {CardId}", request.CardId);

                // Implement card activation using UpdateCard SOAP operation with IsActive = true
                var updateRequest = new UpdateCardRequest
                {
                    CardId = request.CardId,
                    UserName = request.UserId,
                    IsActive = true
                };

                var soapResponse = await _soapClientService.UpdateCardAsync(updateRequest, token);

                return new CardActivationResponse
                {
                    Success = soapResponse.Success,
                    Message = soapResponse.Success ? "Card activated successfully" : soapResponse.Message,
                    CardId = request.CardId,
                    UserId = request.UserId,
                    NewStatus = soapResponse.Success ? "Active" : null
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing ActivateCard request for CardId: {CardId}", request.CardId);
                return new CardActivationResponse
                {
                    Success = false,
                    Message = $"Activation error: {ex.Message}",
                    CardId = request.CardId,
                    UserId = request.UserId
                };
            }
        }

        /// <summary>
        /// Deactivates a card via SOAP service
        /// </summary>
        /// <param name="request">Card activation request (with deactivation intent)</param>
        /// <param name="token">Authentication token</param>
        /// <returns>Activation response</returns>
        public async Task<CardActivationResponse> DeactivateCardAsync(CardActivationRequest request, string token)
        {
            try
            {
                _logger.LogInformation("Processing DeactivateCard request for CardId: {CardId}", request.CardId);

                // Implement card deactivation using UpdateCard SOAP operation with IsActive = false
                var updateRequest = new UpdateCardRequest
                {
                    CardId = request.CardId,
                    UserName = request.UserId,
                    IsActive = false
                };

                var soapResponse = await _soapClientService.UpdateCardAsync(updateRequest, token);

                return new CardActivationResponse
                {
                    Success = soapResponse.Success,
                    Message = soapResponse.Success ? "Card deactivated successfully" : soapResponse.Message,
                    CardId = request.CardId,
                    UserId = request.UserId,
                    NewStatus = soapResponse.Success ? "Inactive" : null
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing DeactivateCard request for CardId: {CardId}", request.CardId);
                return new CardActivationResponse
                {
                    Success = false,
                    Message = $"Deactivation error: {ex.Message}",
                    CardId = request.CardId,
                    UserId = request.UserId
                };
            }
        }

        /// <summary>
        /// Gets system version information via SOAP service
        /// </summary>
        /// <param name="token">Authentication token</param>
        /// <returns>Version information</returns>
        public async Task<VersionResponse> GetVersionAsync(string token)
        {
            try
            {
                _logger.LogInformation("Processing GetVersion request");

                // For now, return adapter version and attempt to get backend version
                var response = new VersionResponse
                {
                    Success = true,
                    Message = "Version information retrieved",
                    ApiVersion = "1.0.0"
                };

                try
                {
                    // Try to get backend version if possible (this would require a specific SOAP operation)
                    var isHealthy = await _soapClientService.CheckHealthAsync(token);
                    if (isHealthy)
                    {
                        response.BackendVersion = "Connected"; // Placeholder
                        response.BuildInfo.Environment = _configuration["Environment"] ?? "Development";
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "Could not retrieve backend version");
                    response.BackendVersion = "Unknown";
                }

                return response;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing GetVersion request");
                return new VersionResponse
                {
                    Success = false,
                    Message = $"Version retrieval error: {ex.Message}",
                    ApiVersion = "1.0.0"
                };
            }
        }

        /// <summary>
        /// Ping operation to test SOAP service connectivity
        /// </summary>
        /// <param name="token">Authentication token</param>
        /// <returns>Ping response status</returns>
        public async Task<bool> PingAsync(string token)
        {
            try
            {
                _logger.LogInformation("Processing Ping request");
                return await _soapClientService.CheckHealthAsync(token);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing Ping request");
                return false;
            }
        }
    }
}
