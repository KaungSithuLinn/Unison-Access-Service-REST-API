using System.ComponentModel.DataAnnotations;

namespace UnisonRestAdapter.Configuration;

/// <summary>
/// Configuration options for request tracing and logging
/// </summary>
public class RequestTracingOptions
{
    /// <summary>
    /// Configuration section name
    /// </summary>
    public const string SectionName = "RequestTracing";

    /// <summary>
    /// Whether to log incoming HTTP requests
    /// </summary>
    public bool LogRequests { get; set; } = true;

    /// <summary>
    /// Whether to log outgoing HTTP responses
    /// </summary>
    public bool LogResponses { get; set; } = true;

    /// <summary>
    /// Whether to log HTTP headers (may contain sensitive data)
    /// </summary>
    public bool LogHeaders { get; set; } = false;

    /// <summary>
    /// Whether to log request/response body content
    /// </summary>
    public bool LogBody { get; set; } = true;

    /// <summary>
    /// Maximum size of request/response body to log (bytes)
    /// </summary>
    [Range(1024, 1048576)] // 1KB to 1MB
    public int MaxBodySize { get; set; } = 32768; // 32KB default

    /// <summary>
    /// Headers that should be masked in logs for security
    /// </summary>
    public string[] SensitiveHeaders { get; set; } =
    {
        "Authorization",
        "Unison-Token",
        "Cookie",
        "Set-Cookie",
        "X-API-Key"
    };

    /// <summary>
    /// JSON paths that should be masked in request/response bodies
    /// </summary>
    public string[] SensitiveBodyPaths { get; set; } =
    {
        "$.password",
        "$.token",
        "$.secret",
        "$.apiKey",
        "$.credentials"
    };

    /// <summary>
    /// Whether to include performance metrics in logs
    /// </summary>
    public bool LogPerformanceMetrics { get; set; } = true;

    /// <summary>
    /// Minimum duration (ms) to log slow requests
    /// </summary>
    [Range(100, 30000)] // 100ms to 30s
    public int SlowRequestThresholdMs { get; set; } = 1000;
}
