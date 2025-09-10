using Microsoft.Extensions.Options;
using System.Security.Cryptography;
using System.Text;
using UnisonRestAdapter.Configuration;

namespace UnisonRestAdapter.Security
{
    public class TokenValidationMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<TokenValidationMiddleware> _logger;
        private readonly SecurityOptions _securityOptions;

        public TokenValidationMiddleware(
            RequestDelegate next,
            ILogger<TokenValidationMiddleware> logger,
            IOptions<SecurityOptions> securityOptions)
        {
            _next = next;
            _logger = logger;
            _securityOptions = securityOptions.Value;
        }

        public async Task InvokeAsync(HttpContext context, ITokenService tokenService)
        {
            // Skip token validation for health checks and swagger
            var path = context.Request.Path.Value?.ToLowerInvariant();
            if (path != null && (path.Contains("/health") ||
                                path.Contains("/swagger") ||
                                path.Contains("/api/docs")))
            {
                await _next(context);
                return;
            }

            // Skip validation for non-API endpoints
            if (path == null || !path.StartsWith("/api/"))
            {
                await _next(context);
                return;
            }

            var correlationId = Guid.NewGuid().ToString();
            context.Items["CorrelationId"] = correlationId;

            try
            {
                var token = ExtractToken(context.Request);

                if (string.IsNullOrEmpty(token))
                {
                    _logger.LogWarning("Missing authentication token. CorrelationId: {CorrelationId}", correlationId);
                    await WriteUnauthorizedResponse(context, "Missing authentication token", correlationId);
                    return;
                }

                var isValid = await tokenService.ValidateTokenAsync(token);
                if (!isValid)
                {
                    _logger.LogWarning("Invalid authentication token provided. CorrelationId: {CorrelationId}", correlationId);
                    await WriteUnauthorizedResponse(context, "Invalid authentication token", correlationId);
                    return;
                }

                // Log successful authentication
                _logger.LogInformation("Token validated successfully. CorrelationId: {CorrelationId}", correlationId);

                // Add token to context for downstream use
                context.Items["UnisonToken"] = token;
                context.Items["IsAuthenticated"] = true;

                await _next(context);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during token validation. CorrelationId: {CorrelationId}", correlationId);
                await WriteErrorResponse(context, "Authentication error", correlationId);
            }
        }

        private static string? ExtractToken(HttpRequest request)
        {
            // Check Unison-Token header first
            if (request.Headers.TryGetValue("Unison-Token", out var headerValue))
            {
                return headerValue.FirstOrDefault();
            }

            // Check Authorization header as fallback
            if (request.Headers.TryGetValue("Authorization", out var authHeader))
            {
                var authValue = authHeader.FirstOrDefault();
                if (!string.IsNullOrEmpty(authValue) && authValue.StartsWith("Bearer "))
                {
                    return authValue.Substring("Bearer ".Length);
                }
            }

            return null;
        }

        private static async Task WriteUnauthorizedResponse(HttpContext context, string message, string correlationId)
        {
            context.Response.StatusCode = 401;
            context.Response.ContentType = "application/json";

            var response = new
            {
                error = "Unauthorized",
                message = message,
                correlationId = correlationId,
                timestamp = DateTime.UtcNow
            };

            await context.Response.WriteAsync(System.Text.Json.JsonSerializer.Serialize(response));
        }

        private static async Task WriteErrorResponse(HttpContext context, string message, string correlationId)
        {
            context.Response.StatusCode = 500;
            context.Response.ContentType = "application/json";

            var response = new
            {
                error = "Internal Server Error",
                message = message,
                correlationId = correlationId,
                timestamp = DateTime.UtcNow
            };

            await context.Response.WriteAsync(System.Text.Json.JsonSerializer.Serialize(response));
        }
    }
}
