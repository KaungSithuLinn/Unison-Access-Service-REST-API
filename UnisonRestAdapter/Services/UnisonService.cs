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

        public UnisonService(
            ISoapClientService soapClientService,
            ILogger<UnisonService> logger,
            IConfiguration configuration)
        {
            _soapClientService = soapClientService;
            _logger = logger;
            _configuration = configuration;
        }

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

        public async Task<HealthResponse> CheckHealthAsync(string token)
        {
            try
            {
                _logger.LogInformation("Processing health check request");

                // This will be implemented after SOAP client proxy is generated
                var isHealthy = await _soapClientService.CheckHealthAsync(token);

                return new HealthResponse
                {
                    IsHealthy = isHealthy,
                    Message = isHealthy ? "Service is healthy" : "Service is unhealthy",
                    Timestamp = DateTime.UtcNow
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during health check");
                return new HealthResponse
                {
                    IsHealthy = false,
                    Message = $"Health check failed: {ex.Message}",
                    Timestamp = DateTime.UtcNow
                };
            }
        }
    }
}
