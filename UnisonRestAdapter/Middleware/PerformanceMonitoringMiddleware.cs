using UnisonRestAdapter.Services;
using UnisonRestAdapter.Services.Tracing;
using System.Diagnostics;

namespace UnisonRestAdapter.Middleware;

/// <summary>
/// Middleware for automatic performance monitoring of API requests
/// </summary>
public class PerformanceMonitoringMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<PerformanceMonitoringMiddleware> _logger;

    /// <summary>
    /// Initializes a new instance of the PerformanceMonitoringMiddleware
    /// </summary>
    /// <param name="next">Next middleware in pipeline</param>
    /// <param name="logger">Logger instance</param>
    public PerformanceMonitoringMiddleware(RequestDelegate next, ILogger<PerformanceMonitoringMiddleware> logger)
    {
        _next = next ?? throw new ArgumentNullException(nameof(next));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <summary>
    /// Process the HTTP context with performance monitoring
    /// </summary>
    /// <param name="context">HTTP context</param>
    /// <param name="performanceMonitoring">Performance monitoring service</param>
    /// <param name="correlationService">Correlation service</param>
    public async Task InvokeAsync(HttpContext context, IPerformanceMonitoringService performanceMonitoring, ICorrelationService correlationService)
    {
        var correlationId = correlationService.CorrelationId;
        var operationName = $"{context.Request.Method} {context.Request.Path}";
        var stopwatch = performanceMonitoring.StartTiming(operationName, correlationId);
        var success = false;

        try
        {
            // Log memory usage before request processing
            if (_logger.IsEnabled(LogLevel.Debug))
            {
                performanceMonitoring.LogMemoryUsage($"Before-{operationName}", correlationId);
            }

            await _next(context);

            // Consider 2xx and 3xx status codes as successful
            success = context.Response.StatusCode < 400;
        }
        catch (Exception ex)
        {
            success = false;
            _logger.LogError(ex, "Unhandled exception in performance monitoring: {OperationName}, CorrelationId: {CorrelationId}",
                operationName, correlationId);
            throw; // Re-throw to maintain error handling behavior
        }
        finally
        {
            var additionalMetrics = new Dictionary<string, object>
            {
                ["StatusCode"] = context.Response.StatusCode,
                ["Method"] = context.Request.Method,
                ["Path"] = context.Request.Path.Value ?? "",
                ["UserAgent"] = context.Request.Headers.UserAgent.ToString(),
                ["ContentLength"] = context.Response.ContentLength ?? 0
            };

            performanceMonitoring.StopTiming(stopwatch, operationName, correlationId, success, additionalMetrics);

            // Log memory usage after request processing (only in debug mode)
            if (_logger.IsEnabled(LogLevel.Debug))
            {
                performanceMonitoring.LogMemoryUsage($"After-{operationName}", correlationId);
            }
        }
    }
}
