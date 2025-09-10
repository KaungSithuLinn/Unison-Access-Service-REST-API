using Microsoft.Extensions.Options;
using System.Text.RegularExpressions;
using UnisonRestAdapter.Configuration;

namespace UnisonRestAdapter.Security
{
    /// <summary>
    /// Middleware to validate requests and block malicious patterns
    /// </summary>
    public class RequestValidationMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<RequestValidationMiddleware> _logger;
        private readonly SecurityOptions _securityOptions;

        private static readonly string[] SuspiciousPatterns = new[]
        {
            // SQL Injection patterns
            @"(\b(SELECT|INSERT|UPDATE|DELETE|DROP|CREATE|ALTER|EXEC|EXECUTE|UNION|SCRIPT)\b)",
            @"(--|;|\/\*|\*\/)",
            @"(\b(OR|AND)\b\s*\d+\s*=\s*\d+)",
            
            // XSS patterns
            @"(<script[\s\S]*?>[\s\S]*?<\/script>)",
            @"(javascript:)",
            @"(on\w+\s*=)",
            @"(<iframe[\s\S]*?>)",
            
            // Path traversal patterns
            @"(\.\.\/)|(\.\.\\)",
            @"(\.\.%2f)|(\.\.%5c)",
            
            // Command injection patterns
            @"(\b(cmd|powershell|bash|sh|exec|system|eval)\b)",
            @"(\||&|;|\$\(|\`)",
            
            // File inclusion patterns
            @"(file:\/\/)",
            @"(php:\/\/)",
            @"(data:\/\/)",
            
            // LDAP injection patterns
            @"(\(|\)|\*|\||&)",
            
            // Header injection patterns
            @"(\r\n|\n\r|\r|\n)(?=\w+:)"
        };

        private static readonly Regex[] CompiledPatterns = SuspiciousPatterns
            .Select(pattern => new Regex(pattern, RegexOptions.IgnoreCase | RegexOptions.Compiled))
            .ToArray();

        /// <summary>
        /// Initializes a new instance of the RequestValidationMiddleware
        /// </summary>
        /// <param name="next">Next middleware in pipeline</param>
        /// <param name="logger">Logger instance</param>
        /// <param name="securityOptions">Security configuration options</param>
        public RequestValidationMiddleware(
            RequestDelegate next,
            ILogger<RequestValidationMiddleware> logger,
            IOptions<SecurityOptions> securityOptions)
        {
            _next = next;
            _logger = logger;
            _securityOptions = securityOptions.Value;
        }

        /// <summary>
        /// Invokes the middleware to validate requests
        /// </summary>
        /// <param name="context">HTTP context</param>
        /// <returns>Task representing the async operation</returns>
        public async Task InvokeAsync(HttpContext context)
        {
            if (!_securityOptions.EnableRequestValidation)
            {
                await _next(context);
                return;
            }

            try
            {
                // Validate request size
                if (_securityOptions.MaxRequestSizeBytes > 0 &&
                    context.Request.ContentLength > _securityOptions.MaxRequestSizeBytes)
                {
                    _logger.LogWarning("Request blocked: Content length {ContentLength} exceeds maximum {MaxSize} bytes",
                        context.Request.ContentLength, _securityOptions.MaxRequestSizeBytes);

                    context.Response.StatusCode = 413; // Payload Too Large
                    await context.Response.WriteAsync("Request payload too large");
                    return;
                }

                // Validate request headers
                if (await ValidateHeaders(context))
                {
                    // Validate request URL and query parameters
                    if (await ValidateUrl(context))
                    {
                        // Validate request body if present
                        if (await ValidateRequestBody(context))
                        {
                            await _next(context);
                            return;
                        }
                    }
                }

                // If we reach here, validation failed
                context.Response.StatusCode = 400; // Bad Request
                await context.Response.WriteAsync("Malicious request detected");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during request validation");
                context.Response.StatusCode = 500;
                await context.Response.WriteAsync("Internal server error");
            }
        }

        private Task<bool> ValidateHeaders(HttpContext context)
        {
            if (!_securityOptions.BlockMaliciousPatterns)
                return Task.FromResult(true);

            foreach (var header in context.Request.Headers)
            {
                var headerValue = string.Join(" ", header.Value.ToArray());

                if (ContainsMaliciousPattern(headerValue))
                {
                    _logger.LogWarning("Malicious pattern detected in header {HeaderName}: {HeaderValue}",
                        header.Key, headerValue);
                    return Task.FromResult(false);
                }
            }

            return Task.FromResult(true);
        }

        private Task<bool> ValidateUrl(HttpContext context)
        {
            if (!_securityOptions.BlockMaliciousPatterns)
                return Task.FromResult(true);

            var url = context.Request.Path + context.Request.QueryString;

            if (ContainsMaliciousPattern(url))
            {
                _logger.LogWarning("Malicious pattern detected in URL: {Url}", url);
                return Task.FromResult(false);
            }

            return Task.FromResult(true);
        }

        private async Task<bool> ValidateRequestBody(HttpContext context)
        {
            if (!_securityOptions.BlockMaliciousPatterns)
                return true;

            if (context.Request.ContentLength == 0 || context.Request.ContentLength == null)
                return true;

            // Only validate certain content types
            var contentType = context.Request.ContentType?.ToLowerInvariant();
            if (contentType == null ||
                (!contentType.Contains("application/json") &&
                 !contentType.Contains("application/xml") &&
                 !contentType.Contains("text/plain") &&
                 !contentType.Contains("application/x-www-form-urlencoded")))
            {
                return true;
            }

            try
            {
                // Enable request body buffering to allow multiple reads
                context.Request.EnableBuffering();

                using var reader = new StreamReader(context.Request.Body, leaveOpen: true);
                var body = await reader.ReadToEndAsync();

                // Reset stream position for subsequent middleware
                context.Request.Body.Position = 0;

                if (!string.IsNullOrEmpty(body) && ContainsMaliciousPattern(body))
                {
                    _logger.LogWarning("Malicious pattern detected in request body");
                    return false;
                }
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Failed to read request body for validation");
                // Allow request to continue if body can't be read
            }

            return true;
        }

        private static bool ContainsMaliciousPattern(string input)
        {
            if (string.IsNullOrEmpty(input))
                return false;

            foreach (var pattern in CompiledPatterns)
            {
                if (pattern.IsMatch(input))
                {
                    return true;
                }
            }

            return false;
        }
    }

    /// <summary>
    /// Extension methods for registering request validation middleware
    /// </summary>
    public static class RequestValidationMiddlewareExtensions
    {
        /// <summary>
        /// Adds request validation middleware to the pipeline
        /// </summary>
        /// <param name="builder">Application builder</param>
        /// <returns>Application builder</returns>
        public static IApplicationBuilder UseRequestValidation(this IApplicationBuilder builder)
        {
            return builder.UseMiddleware<RequestValidationMiddleware>();
        }
    }
}
