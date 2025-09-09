using Microsoft.Extensions.Options;
using UnisonRestAdapter.Configuration;
using System.Collections.Concurrent;
using System.Net;

namespace UnisonRestAdapter.Middleware
{
    /// <summary>
    /// Enhanced security middleware for token validation, rate limiting, and IP filtering
    /// </summary>
    public class SecurityMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<SecurityMiddleware> _logger;
        private readonly SecurityOptions _securityOptions;

        // Rate limiting storage
        private readonly ConcurrentDictionary<string, TokenUsage> _tokenUsage = new();
        private readonly ConcurrentDictionary<string, List<DateTime>> _requestHistory = new();

        public SecurityMiddleware(RequestDelegate next, ILogger<SecurityMiddleware> logger, IOptions<SecurityOptions> securityOptions)
        {
            _next = next;
            _logger = logger;
            _securityOptions = securityOptions.Value;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            // Skip security checks for health checks and swagger
            if (context.Request.Path.StartsWithSegments("/health") ||
                context.Request.Path.StartsWithSegments("/swagger"))
            {
                await _next(context);
                return;
            }

            var clientIp = GetClientIpAddress(context);
            var token = context.Request.Headers["Unison-Token"].FirstOrDefault();

            // IP Whitelist Check
            if (_securityOptions.EnableIpWhitelist && !IsIpAllowed(clientIp))
            {
                _logger.LogWarning("Blocked request from unauthorized IP: {IpAddress}", clientIp);
                await WriteErrorResponse(context, 403, "Access denied from this IP address");
                return;
            }

            // Token Validation
            if (string.IsNullOrEmpty(token) || !IsValidToken(token))
            {
                _logger.LogWarning("Invalid or missing token from IP: {IpAddress}", clientIp);
                await WriteErrorResponse(context, 401, "Invalid or missing authentication token");
                return;
            }

            // Rate Limiting
            if (_securityOptions.EnableRateLimiting && IsRateLimited(token))
            {
                _logger.LogWarning("Rate limit exceeded for token: {TokenPrefix}... from IP: {IpAddress}",
                    token.Substring(0, Math.Min(8, token.Length)), clientIp);
                await WriteErrorResponse(context, 429, "Rate limit exceeded");
                return;
            }

            // Log security event
            if (_securityOptions.LogSecurityEvents)
            {
                _logger.LogInformation("Authorized request from IP: {IpAddress} with token: {TokenPrefix}...",
                    clientIp, token.Substring(0, Math.Min(8, token.Length)));
            }

            // Update usage tracking
            UpdateTokenUsage(token);

            await _next(context);
        }

        private string GetClientIpAddress(HttpContext context)
        {
            // Check for forwarded headers first
            var forwarded = context.Request.Headers["X-Forwarded-For"].FirstOrDefault();
            if (!string.IsNullOrEmpty(forwarded))
            {
                return forwarded.Split(',')[0].Trim();
            }

            var realIp = context.Request.Headers["X-Real-IP"].FirstOrDefault();
            if (!string.IsNullOrEmpty(realIp))
            {
                return realIp;
            }

            return context.Connection.RemoteIpAddress?.ToString() ?? "unknown";
        }

        private bool IsIpAllowed(string ipAddress)
        {
            if (!_securityOptions.AllowedIpAddresses.Any())
                return true; // No restrictions if list is empty

            return _securityOptions.AllowedIpAddresses.Contains(ipAddress) ||
                   _securityOptions.AllowedIpAddresses.Any(allowed => IsIpInRange(ipAddress, allowed));
        }

        private bool IsIpInRange(string ipAddress, string allowedRange)
        {
            // Simple subnet matching (can be enhanced for CIDR notation)
            if (allowedRange.Contains('*'))
            {
                var pattern = allowedRange.Replace("*", ".*");
                return System.Text.RegularExpressions.Regex.IsMatch(ipAddress, pattern);
            }
            return false;
        }

        private bool IsValidToken(string token)
        {
            // Check against configured valid tokens
            if (_securityOptions.ValidTokens.Any())
            {
                return _securityOptions.ValidTokens.Contains(token);
            }

            // Fallback to default token validation (for backward compatibility)
            return !string.IsNullOrEmpty(token) && token.Length >= 10;
        }

        private bool IsRateLimited(string token)
        {
            var now = DateTime.UtcNow;
            var key = token;

            // Clean old entries periodically
            CleanOldEntries(key, now);

            if (!_requestHistory.TryGetValue(key, out var requests))
            {
                requests = new List<DateTime>();
                _requestHistory[key] = requests;
            }

            // Count requests in the last hour
            var recentRequests = requests.Where(r => r > now.AddHours(-1)).Count();

            if (recentRequests >= _securityOptions.MaxRequestsPerHour)
            {
                return true;
            }

            // Add current request
            requests.Add(now);
            return false;
        }

        private void UpdateTokenUsage(string token)
        {
            var usage = _tokenUsage.GetOrAdd(token, _ => new TokenUsage
            {
                FirstUsed = DateTime.UtcNow,
                LastUsed = DateTime.UtcNow,
                UsageCount = 0
            });

            usage.LastUsed = DateTime.UtcNow;
            usage.UsageCount++;
        }

        private void CleanOldEntries(string key, DateTime now)
        {
            if (_requestHistory.TryGetValue(key, out var requests))
            {
                var cutoff = now.AddHours(-2); // Keep 2 hours of history
                requests.RemoveAll(r => r < cutoff);
            }
        }

        private async Task WriteErrorResponse(HttpContext context, int statusCode, string message)
        {
            context.Response.StatusCode = statusCode;
            context.Response.ContentType = "application/json";

            var response = new
            {
                error = message,
                timestamp = DateTime.UtcNow,
                statusCode = statusCode
            };

            await context.Response.WriteAsync(System.Text.Json.JsonSerializer.Serialize(response));
        }
    }

    /// <summary>
    /// Token usage tracking
    /// </summary>
    public class TokenUsage
    {
        public DateTime FirstUsed { get; set; }
        public DateTime LastUsed { get; set; }
        public long UsageCount { get; set; }
    }
}
