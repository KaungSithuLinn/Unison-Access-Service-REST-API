using System.Diagnostics;
using System.Text;
using System.Text.Json;
using Microsoft.Extensions.Options;
using UnisonRestAdapter.Configuration;
using UnisonRestAdapter.Services.Tracing;

namespace UnisonRestAdapter.Middleware;

/// <summary>
/// Middleware for logging HTTP requests and responses with structured logging
/// </summary>
public class RequestLoggingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<RequestLoggingMiddleware> _logger;
    private readonly RequestTracingOptions _options;

    private static readonly HashSet<string> SensitiveHeadersLowercase = new()
    {
        "authorization",
        "unison-token",
        "cookie",
        "set-cookie",
        "x-api-key"
    };

    /// <summary>
    /// Initializes a new instance of the RequestLoggingMiddleware
    /// </summary>
    /// <param name="next">The next middleware in the pipeline</param>
    /// <param name="logger">Logger instance</param>
    /// <param name="options">Request tracing options</param>
    public RequestLoggingMiddleware(
        RequestDelegate next,
        ILogger<RequestLoggingMiddleware> logger,
        IOptions<RequestTracingOptions> options)
    {
        _next = next;
        _logger = logger;
        _options = options.Value;
    }

    /// <summary>
    /// Process the HTTP request and response logging
    /// </summary>
    public async Task InvokeAsync(HttpContext context, ICorrelationService correlationService)
    {
        ArgumentNullException.ThrowIfNull(context);

        // Set correlation ID early in the pipeline
        var correlationId = context.Request.Headers.TryGetValue("X-Correlation-ID", out var existingId) && !string.IsNullOrEmpty(existingId)
            ? existingId.ToString()
            : correlationService.GenerateCorrelationId();
        correlationService.SetCorrelationId(correlationId);
        context.Items["CorrelationId"] = correlationId;

        // Start timing the request
        var stopwatch = Stopwatch.StartNew();
        var requestStartTime = DateTimeOffset.UtcNow;

        try
        {
            // Log the incoming request
            if (_options.LogRequests)
            {
                await LogRequestAsync(context, correlationId);
            }

            // Enable response buffering to capture response body
            var originalResponseBodyStream = context.Response.Body;
            using var responseBody = new MemoryStream();
            context.Response.Body = responseBody;

            try
            {
                // Continue to next middleware
                await _next(context);
            }
            finally
            {
                // Copy response back to original stream
                responseBody.Position = 0;
                await responseBody.CopyToAsync(originalResponseBodyStream);
                context.Response.Body = originalResponseBodyStream;
            }

            stopwatch.Stop();

            // Log the response
            if (_options.LogResponses)
            {
                await LogResponseAsync(context, correlationId, responseBody, stopwatch.ElapsedMilliseconds);
            }

            // Log performance metrics
            if (_options.LogPerformanceMetrics)
            {
                LogPerformanceMetrics(context, correlationId, stopwatch.ElapsedMilliseconds, requestStartTime);
            }
        }
        catch (Exception ex)
        {
            stopwatch.Stop();
            LogRequestException(context, correlationId, ex, stopwatch.ElapsedMilliseconds);
            throw;
        }
    }

    /// <summary>
    /// Log incoming HTTP request details
    /// </summary>
    private async Task LogRequestAsync(HttpContext context, string correlationId)
    {
        var request = context.Request;

        var requestDetails = new
        {
            CorrelationId = correlationId,
            TraceId = Activity.Current?.TraceId.ToString(),
            RequestId = context.TraceIdentifier,
            Method = request.Method,
            Path = request.Path.Value,
            QueryString = request.QueryString.Value,
            ContentType = request.ContentType,
            ContentLength = request.ContentLength,
            UserAgent = request.Headers.UserAgent.ToString(),
            RemoteIpAddress = context.Connection.RemoteIpAddress?.ToString(),
            Headers = _options.LogHeaders ? SanitizeHeaders(request.Headers) : null,
            Body = _options.LogBody ? await ReadRequestBodyAsync(request) : null
        };

        _logger.LogInformation("HTTP Request: {Method} {Path} {QueryString}",
            request.Method, request.Path, request.QueryString);

        _logger.LogDebug("HTTP Request Details: {@RequestDetails}", requestDetails);
    }

    /// <summary>
    /// Log outgoing HTTP response details
    /// </summary>
    private async Task LogResponseAsync(HttpContext context, string correlationId, MemoryStream responseBody, long elapsedMs)
    {
        var response = context.Response;

        var responseDetails = new
        {
            CorrelationId = correlationId,
            TraceId = Activity.Current?.TraceId.ToString(),
            RequestId = context.TraceIdentifier,
            StatusCode = response.StatusCode,
            ContentType = response.ContentType,
            ContentLength = response.ContentLength,
            ElapsedMs = elapsedMs,
            Headers = _options.LogHeaders ? SanitizeHeaders(response.Headers) : null,
            Body = _options.LogBody ? await ReadResponseBodyAsync(responseBody) : null
        };

        var logLevel = GetLogLevelForStatusCode(response.StatusCode);

        _logger.Log(logLevel, "HTTP Response: {StatusCode} in {ElapsedMs}ms",
            response.StatusCode, elapsedMs);

        _logger.LogDebug("HTTP Response Details: {@ResponseDetails}", responseDetails);
    }

    /// <summary>
    /// Log performance metrics for the request
    /// </summary>
    private void LogPerformanceMetrics(HttpContext context, string correlationId, long elapsedMs, DateTimeOffset requestStart)
    {
        var isSlowRequest = elapsedMs >= _options.SlowRequestThresholdMs;

        var metrics = new
        {
            CorrelationId = correlationId,
            TraceId = Activity.Current?.TraceId.ToString(),
            RequestId = context.TraceIdentifier,
            Method = context.Request.Method,
            Path = context.Request.Path.Value,
            StatusCode = context.Response.StatusCode,
            ElapsedMs = elapsedMs,
            RequestStartTime = requestStart,
            RequestEndTime = DateTimeOffset.UtcNow,
            IsSlowRequest = isSlowRequest,
            ContentLength = context.Response.ContentLength ?? 0
        };

        if (isSlowRequest)
        {
            _logger.LogWarning("Slow Request Detected: {@PerformanceMetrics}", metrics);
        }
        else
        {
            _logger.LogDebug("Request Performance: {@PerformanceMetrics}", metrics);
        }
    }

    /// <summary>
    /// Log request processing exceptions
    /// </summary>
    private void LogRequestException(HttpContext context, string correlationId, Exception exception, long elapsedMs)
    {
        var errorDetails = new
        {
            CorrelationId = correlationId,
            TraceId = Activity.Current?.TraceId.ToString(),
            RequestId = context.TraceIdentifier,
            Method = context.Request.Method,
            Path = context.Request.Path.Value,
            ElapsedMs = elapsedMs,
            ExceptionType = exception.GetType().Name,
            ExceptionMessage = exception.Message
        };

        _logger.LogError(exception, "Request Processing Exception: {@ErrorDetails}", errorDetails);
    }

    /// <summary>
    /// Read and sanitize request body content
    /// </summary>
    private async Task<string?> ReadRequestBodyAsync(HttpRequest request)
    {
        try
        {
            if (request.ContentLength == 0 || request.Body == null)
                return null;

            var contentLength = (int)(request.ContentLength ?? 0);
            if (contentLength > _options.MaxBodySize)
            {
                return $"[Body too large: {contentLength} bytes, max: {_options.MaxBodySize}]";
            }

            request.EnableBuffering();
            request.Body.Position = 0;

            using var reader = new StreamReader(request.Body, Encoding.UTF8, leaveOpen: true);
            var body = await reader.ReadToEndAsync();
            request.Body.Position = 0;

            return SanitizeBodyContent(body);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to read request body");
            return "[Error reading body]";
        }
    }

    /// <summary>
    /// Read and sanitize response body content
    /// </summary>
    private async Task<string?> ReadResponseBodyAsync(MemoryStream responseBody)
    {
        try
        {
            if (responseBody.Length == 0)
                return null;

            if (responseBody.Length > _options.MaxBodySize)
            {
                return $"[Body too large: {responseBody.Length} bytes, max: {_options.MaxBodySize}]";
            }

            responseBody.Position = 0;
            using var reader = new StreamReader(responseBody, Encoding.UTF8, leaveOpen: true);
            var body = await reader.ReadToEndAsync();
            responseBody.Position = 0;

            return SanitizeBodyContent(body);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to read response body");
            return "[Error reading body]";
        }
    }

    /// <summary>
    /// Sanitize headers by masking sensitive values
    /// </summary>
    private Dictionary<string, string> SanitizeHeaders(IHeaderDictionary headers)
    {
        var sanitized = new Dictionary<string, string>();

        foreach (var header in headers)
        {
            var key = header.Key;
            var value = header.Value.ToString();

            if (SensitiveHeadersLowercase.Contains(key.ToLowerInvariant()) ||
                _options.SensitiveHeaders.Any(sh => string.Equals(sh, key, StringComparison.OrdinalIgnoreCase)))
            {
                sanitized[key] = "[MASKED]";
            }
            else
            {
                sanitized[key] = value;
            }
        }

        return sanitized;
    }

    /// <summary>
    /// Sanitize body content by masking sensitive data
    /// </summary>
    private string SanitizeBodyContent(string body)
    {
        if (string.IsNullOrWhiteSpace(body))
            return body;

        try
        {
            // For JSON content, try to parse and mask sensitive fields
            if (IsJsonContent(body))
            {
                return SanitizeJsonBody(body);
            }

            // For non-JSON, do basic sanitization
            return SanitizeTextBody(body);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to sanitize body content");
            return "[Error sanitizing body]";
        }
    }

    /// <summary>
    /// Check if content appears to be JSON
    /// </summary>
    private static bool IsJsonContent(string content)
    {
        var trimmed = content.Trim();
        return (trimmed.StartsWith('{') && trimmed.EndsWith('}')) ||
               (trimmed.StartsWith('[') && trimmed.EndsWith(']'));
    }

    /// <summary>
    /// Sanitize JSON body by masking sensitive paths
    /// </summary>
    private string SanitizeJsonBody(string jsonBody)
    {
        try
        {
            using var document = JsonDocument.Parse(jsonBody);
            // For now, return formatted JSON - could implement JSONPath masking later
            return JsonSerializer.Serialize(document, new JsonSerializerOptions { WriteIndented = false });
        }
        catch
        {
            return SanitizeTextBody(jsonBody);
        }
    }

    /// <summary>
    /// Basic text sanitization for non-JSON content
    /// </summary>
    private string SanitizeTextBody(string body)
    {
        // Basic pattern matching for common sensitive data
        var patterns = new[]
        {
            (@"password\s*[:=]\s*[^\s&]+", "password=[MASKED]"),
            (@"token\s*[:=]\s*[^\s&]+", "token=[MASKED]"),
            (@"secret\s*[:=]\s*[^\s&]+", "secret=[MASKED]"),
            (@"apikey\s*[:=]\s*[^\s&]+", "apikey=[MASKED]")
        };

        var sanitized = body;
        foreach (var (pattern, replacement) in patterns)
        {
            sanitized = System.Text.RegularExpressions.Regex.Replace(
                sanitized, pattern, replacement,
                System.Text.RegularExpressions.RegexOptions.IgnoreCase);
        }

        return sanitized;
    }

    /// <summary>
    /// Get appropriate log level based on HTTP status code
    /// </summary>
    private static LogLevel GetLogLevelForStatusCode(int statusCode)
    {
        return statusCode switch
        {
            >= 200 and < 300 => LogLevel.Information,
            >= 300 and < 400 => LogLevel.Information,
            >= 400 and < 500 => LogLevel.Warning,
            >= 500 => LogLevel.Error,
            _ => LogLevel.Information
        };
    }
}
