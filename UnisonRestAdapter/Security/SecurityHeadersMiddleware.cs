using Microsoft.Extensions.Options;
using UnisonRestAdapter.Configuration;

namespace UnisonRestAdapter.Security
{
    /// <summary>
    /// Middleware to add security headers following OWASP guidelines
    /// </summary>
    public class SecurityHeadersMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<SecurityHeadersMiddleware> _logger;
        private readonly SecurityOptions _securityOptions;

        /// <summary>
        /// Initializes a new instance of the SecurityHeadersMiddleware
        /// </summary>
        /// <param name="next">Next middleware in pipeline</param>
        /// <param name="logger">Logger instance</param>
        /// <param name="securityOptions">Security configuration options</param>
        public SecurityHeadersMiddleware(
            RequestDelegate next,
            ILogger<SecurityHeadersMiddleware> logger,
            IOptions<SecurityOptions> securityOptions)
        {
            _next = next;
            _logger = logger;
            _securityOptions = securityOptions.Value;
        }

        /// <summary>
        /// Invokes the middleware to add security headers
        /// </summary>
        /// <param name="context">HTTP context</param>
        /// <returns>Task representing the async operation</returns>
        public async Task InvokeAsync(HttpContext context)
        {
            // Add security headers before processing the request
            AddSecurityHeaders(context);

            await _next(context);
        }

        private void AddSecurityHeaders(HttpContext context)
        {
            var response = context.Response;
            var headers = response.Headers;

            try
            {
                // X-Content-Type-Options: Prevent MIME sniffing
                if (!headers.ContainsKey("X-Content-Type-Options"))
                {
                    headers["X-Content-Type-Options"] = "nosniff";
                }

                // X-Frame-Options: Prevent clickjacking
                if (!headers.ContainsKey("X-Frame-Options"))
                {
                    headers["X-Frame-Options"] = "DENY";
                }

                // X-XSS-Protection: Enable XSS filtering
                if (!headers.ContainsKey("X-XSS-Protection"))
                {
                    headers["X-XSS-Protection"] = "1; mode=block";
                }

                // Referrer-Policy: Control referrer information
                if (!headers.ContainsKey("Referrer-Policy"))
                {
                    headers["Referrer-Policy"] = "strict-origin-when-cross-origin";
                }

                // Permissions-Policy: Control browser features
                if (!headers.ContainsKey("Permissions-Policy"))
                {
                    headers["Permissions-Policy"] = "camera=(), microphone=(), geolocation=()";
                }

                // Content-Security-Policy: Prevent XSS and injection attacks
                if (_securityOptions.EnableContentSecurityPolicy && !headers.ContainsKey("Content-Security-Policy"))
                {
                    var csp = "default-src 'self'; " +
                             "script-src 'self' 'unsafe-inline' 'unsafe-eval'; " +
                             "style-src 'self' 'unsafe-inline'; " +
                             "img-src 'self' data: https:; " +
                             "font-src 'self'; " +
                             "connect-src 'self'; " +
                             "frame-ancestors 'none'; " +
                             "form-action 'self'; " +
                             "base-uri 'self'";

                    headers["Content-Security-Policy"] = csp;
                }

                // Strict-Transport-Security: Enforce HTTPS
                if (_securityOptions.EnableHsts && context.Request.IsHttps && !headers.ContainsKey("Strict-Transport-Security"))
                {
                    headers["Strict-Transport-Security"] = $"max-age={_securityOptions.HstsMaxAge}; includeSubDomains; preload";
                }

                // Server header removal for security through obscurity
                if (headers.ContainsKey("Server"))
                {
                    headers.Remove("Server");
                }

                // Remove version information
                if (headers.ContainsKey("X-Powered-By"))
                {
                    headers.Remove("X-Powered-By");
                }

                // Add custom security headers
                if (!headers.ContainsKey("X-Security-Version"))
                {
                    headers["X-Security-Version"] = "1.0";
                }

                _logger.LogDebug("Security headers added to response for {RequestPath}", context.Request.Path);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Failed to add security headers for request {RequestPath}", context.Request.Path);
                // Continue processing even if headers fail to add
            }
        }
    }

    /// <summary>
    /// Extension methods for registering security headers middleware
    /// </summary>
    public static class SecurityHeadersMiddlewareExtensions
    {
        /// <summary>
        /// Adds security headers middleware to the pipeline
        /// </summary>
        /// <param name="builder">Application builder</param>
        /// <returns>Application builder</returns>
        public static IApplicationBuilder UseSecurityHeaders(this IApplicationBuilder builder)
        {
            return builder.UseMiddleware<SecurityHeadersMiddleware>();
        }
    }
}
