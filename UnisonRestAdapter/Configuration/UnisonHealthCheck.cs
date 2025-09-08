using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.Extensions.Options;
using UnisonRestAdapter.Configuration;
using UnisonRestAdapter.Services;

namespace UnisonRestAdapter.Configuration
{
    /// <summary>
    /// Health check for Unison Access Service
    /// </summary>
    public class UnisonHealthCheck : IHealthCheck
    {
        private readonly IUnisonService _unisonService;
        private readonly UnisonSettings _settings;
        private readonly ILogger<UnisonHealthCheck> _logger;

        public UnisonHealthCheck(
            IUnisonService unisonService,
            IOptions<UnisonSettings> settings,
            ILogger<UnisonHealthCheck> logger)
        {
            _unisonService = unisonService;
            _settings = settings.Value;
            _logger = logger;
        }

        public async Task<HealthCheckResult> CheckHealthAsync(
            HealthCheckContext context,
            CancellationToken cancellationToken = default)
        {
            try
            {
                // Use a dummy token for health check (this should be configurable)
                var healthResponse = await _unisonService.CheckHealthAsync("health-check-token");

                if (healthResponse.IsHealthy)
                {
                    return HealthCheckResult.Healthy("Unison Access Service is healthy");
                }
                else
                {
                    return HealthCheckResult.Unhealthy($"Unison Access Service is unhealthy: {healthResponse.Message}");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Health check failed for Unison Access Service");
                return HealthCheckResult.Unhealthy($"Unison Access Service health check failed: {ex.Message}");
            }
        }
    }
}
