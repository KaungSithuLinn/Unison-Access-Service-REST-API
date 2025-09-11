using System.Diagnostics;

namespace UnisonRestAdapter.Services.Tracing;

/// <summary>
/// Implementation of correlation service for request tracing
/// </summary>
public class CorrelationService : ICorrelationService
{
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly ILogger<CorrelationService> _logger;

    /// <summary>
    /// Header name for correlation ID
    /// </summary>
    public const string CorrelationIdHeaderName = "X-Correlation-ID";

    /// <summary>
    /// Header name for request ID
    /// </summary>
    public const string RequestIdHeaderName = "X-Request-ID";

    /// <summary>
    /// Context key for storing correlation ID
    /// </summary>
    private const string CorrelationIdKey = "CorrelationId";

    /// <summary>
    /// Initializes a new instance of the CorrelationService
    /// </summary>
    /// <param name="httpContextAccessor">HTTP context accessor</param>
    /// <param name="logger">Logger instance</param>
    public CorrelationService(
        IHttpContextAccessor httpContextAccessor,
        ILogger<CorrelationService> logger)
    {
        _httpContextAccessor = httpContextAccessor;
        _logger = logger;
    }

    /// <inheritdoc />
    public string CorrelationId
    {
        get
        {
            var context = _httpContextAccessor.HttpContext;
            if (context?.Items.TryGetValue(CorrelationIdKey, out var correlationId) == true)
            {
                return correlationId?.ToString() ?? GenerateCorrelationId();
            }

            // Fallback to Activity.Current or generate new
            return Activity.Current?.Id ?? GenerateCorrelationId();
        }
    }

    /// <inheritdoc />
    public void SetCorrelationId(string correlationId)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(correlationId);

        var context = _httpContextAccessor.HttpContext;
        if (context != null)
        {
            context.Items[CorrelationIdKey] = correlationId;

            // Also set in response headers for client tracking
            if (!context.Response.Headers.ContainsKey(CorrelationIdHeaderName))
            {
                context.Response.Headers[CorrelationIdHeaderName] = correlationId;
            }
        }

        _logger.LogDebug("Correlation ID set: {CorrelationId}", correlationId);
    }

    /// <inheritdoc />
    public string GenerateCorrelationId()
    {
        // Use a combination of timestamp and GUID for uniqueness
        var timestamp = DateTimeOffset.UtcNow.Ticks;
        var guid = Guid.NewGuid().ToString("N")[..8]; // Take first 8 chars
        return $"unison-{timestamp:x}-{guid}";
    }

    /// <inheritdoc />
    public Dictionary<string, string> GetTracingHeaders()
    {
        var headers = new Dictionary<string, string>();

        var correlationId = CorrelationId;
        headers[CorrelationIdHeaderName] = correlationId;

        var context = _httpContextAccessor.HttpContext;
        if (context != null)
        {
            // Add request ID if available
            var requestId = context.TraceIdentifier;
            if (!string.IsNullOrEmpty(requestId))
            {
                headers[RequestIdHeaderName] = requestId;
            }

            // Add Activity ID if available
            var activity = Activity.Current;
            if (activity != null)
            {
                headers["X-Trace-ID"] = activity.TraceId.ToString();
                headers["X-Span-ID"] = activity.SpanId.ToString();
            }
        }

        return headers;
    }

    /// <summary>
    /// Extracts correlation ID from request headers or generates new one
    /// </summary>
    /// <param name="context">HTTP context</param>
    /// <returns>Correlation ID</returns>
    public string ExtractOrGenerateCorrelationId(HttpContext context)
    {
        ArgumentNullException.ThrowIfNull(context);

        // Try to get from headers first
        if (context.Request.Headers.TryGetValue(CorrelationIdHeaderName, out var headerValue) &&
            !string.IsNullOrWhiteSpace(headerValue.FirstOrDefault()))
        {
            var extractedId = headerValue.FirstOrDefault()!;
            _logger.LogDebug("Correlation ID extracted from header: {CorrelationId}", extractedId);
            return extractedId;
        }

        // Generate new if not found
        var newId = GenerateCorrelationId();
        _logger.LogDebug("New correlation ID generated: {CorrelationId}", newId);
        return newId;
    }
}
