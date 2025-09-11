using System.Diagnostics;
using UnisonRestAdapter.Services.Monitoring;

namespace UnisonRestAdapter.Middleware
{
    /// <summary>
    /// Middleware to automatically collect request metrics for monitoring
    /// </summary>
    public class MetricsCollectionMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<MetricsCollectionMiddleware> _logger;

        /// <summary>
        /// Initializes a new instance of the MetricsCollectionMiddleware
        /// </summary>
        /// <param name="next">Next middleware in the pipeline</param>
        /// <param name="logger">Logger for metrics collection</param>
        public MetricsCollectionMiddleware(RequestDelegate next, ILogger<MetricsCollectionMiddleware> logger)
        {
            _next = next;
            _logger = logger;
        }

        /// <summary>
        /// Invokes the middleware to collect metrics for the request
        /// </summary>
        /// <param name="context">HTTP context</param>
        /// <param name="monitoringService">Monitoring service for metrics collection</param>
        public async Task InvokeAsync(HttpContext context, IMonitoringService monitoringService)
        {
            var stopwatch = Stopwatch.StartNew();

            try
            {
                await _next(context);
            }
            finally
            {
                stopwatch.Stop();

                try
                {
                    // Extract endpoint path (normalize for consistent metrics)
                    var endpoint = GetNormalizedEndpoint(context.Request.Path);
                    var statusCode = context.Response.StatusCode;
                    var responseTimeMs = stopwatch.Elapsed.TotalMilliseconds;

                    // Record the metric
                    monitoringService.RecordRequestMetric(endpoint, statusCode, responseTimeMs);

                    _logger.LogDebug("Recorded metrics for {Endpoint}: {StatusCode} in {ResponseTime}ms",
                        endpoint, statusCode, Math.Round(responseTimeMs, 2));
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "Failed to record request metrics");
                }
            }
        }

        /// <summary>
        /// Normalizes endpoint paths for consistent metrics collection
        /// </summary>
        /// <param name="path">Request path</param>
        /// <returns>Normalized endpoint path</returns>
        private static string GetNormalizedEndpoint(PathString path)
        {
            var pathValue = path.Value ?? "/";

            // Normalize common patterns
            pathValue = pathValue.ToLowerInvariant();

            // Replace GUIDs and numeric IDs with placeholders for better grouping
            pathValue = System.Text.RegularExpressions.Regex.Replace(
                pathValue,
                @"/\d+(/|$)",
                "/{id}$1");

            pathValue = System.Text.RegularExpressions.Regex.Replace(
                pathValue,
                @"/[0-9a-f]{8}-[0-9a-f]{4}-[0-9a-f]{4}-[0-9a-f]{4}-[0-9a-f]{12}(/|$)",
                "/{guid}$1");

            return pathValue;
        }
    }
}
