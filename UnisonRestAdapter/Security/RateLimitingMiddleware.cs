using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Options;
using System.Net;
using UnisonRestAdapter.Configuration;

namespace UnisonRestAdapter.Security
{
    /// <summary>
    /// Middleware to implement rate limiting per IP address
    /// </summary>
    public class RateLimitingMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<RateLimitingMiddleware> _logger;
        private readonly SecurityOptions _securityOptions;
        private readonly IMemoryCache _cache;

        private const string RateLimitPrefix = "rate_limit_";
        private const string BlockedPrefix = "blocked_";

        /// <summary>
        /// Initializes a new instance of the RateLimitingMiddleware
        /// </summary>
        /// <param name="next">Next middleware in pipeline</param>
        /// <param name="logger">Logger instance</param>
        /// <param name="securityOptions">Security configuration options</param>
        /// <param name="cache">Memory cache for storing rate limit data</param>
        public RateLimitingMiddleware(
            RequestDelegate next,
            ILogger<RateLimitingMiddleware> logger,
            IOptions<SecurityOptions> securityOptions,
            IMemoryCache cache)
        {
            _next = next;
            _logger = logger;
            _securityOptions = securityOptions.Value;
            _cache = cache;
        }

        /// <summary>
        /// Invokes the middleware to enforce rate limiting
        /// </summary>
        /// <param name="context">HTTP context</param>
        /// <returns>Task representing the async operation</returns>
        public async Task InvokeAsync(HttpContext context)
        {
            if (!_securityOptions.EnableRateLimiting)
            {
                await _next(context);
                return;
            }

            var clientIp = GetClientIpAddress(context);

            // Skip rate limiting for health checks
            var path = context.Request.Path.Value?.ToLowerInvariant();
            if (path != null && (path.Contains("/health") || path.Contains("/swagger")))
            {
                await _next(context);
                return;
            }

            try
            {
                // Check if IP is temporarily blocked
                var blockedKey = $"{BlockedPrefix}{clientIp}";
                if (_cache.TryGetValue(blockedKey, out _))
                {
                    _logger.LogWarning("Request from blocked IP address: {ClientIp}", clientIp);
                    context.Response.StatusCode = 429; // Too Many Requests
                    context.Response.Headers["Retry-After"] = "3600"; // 1 hour
                    await context.Response.WriteAsync("IP address temporarily blocked due to rate limit violations");
                    return;
                }

                // Check rate limit
                var rateLimitKey = $"{RateLimitPrefix}{clientIp}";
                var requestCount = await GetRequestCount(rateLimitKey);

                if (requestCount >= _securityOptions.MaxRequestsPerHour)
                {
                    // Block IP for 1 hour
                    _cache.Set(blockedKey, DateTime.UtcNow, TimeSpan.FromHours(1));

                    _logger.LogWarning("Rate limit exceeded for IP: {ClientIp}. Requests: {RequestCount}/{MaxRequests}",
                        clientIp, requestCount, _securityOptions.MaxRequestsPerHour);

                    context.Response.StatusCode = 429;
                    context.Response.Headers["Retry-After"] = "3600";
                    await context.Response.WriteAsync("Rate limit exceeded. IP temporarily blocked.");
                    return;
                }

                // Increment request count
                await IncrementRequestCount(rateLimitKey);

                // Add rate limit headers
                context.Response.Headers["X-RateLimit-Limit"] = _securityOptions.MaxRequestsPerHour.ToString();
                context.Response.Headers["X-RateLimit-Remaining"] = Math.Max(0, _securityOptions.MaxRequestsPerHour - requestCount - 1).ToString();
                context.Response.Headers["X-RateLimit-Reset"] = GetResetTime().ToString();

                await _next(context);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in rate limiting middleware for IP: {ClientIp}", clientIp);
                await _next(context);
            }
        }

        private string GetClientIpAddress(HttpContext context)
        {
            // Check for forwarded headers first (for reverse proxy scenarios)
            var forwarded = context.Request.Headers["X-Forwarded-For"].FirstOrDefault();
            if (!string.IsNullOrEmpty(forwarded))
            {
                var ips = forwarded.Split(',', StringSplitOptions.RemoveEmptyEntries);
                if (ips.Length > 0)
                {
                    return ips[0].Trim();
                }
            }

            var realIp = context.Request.Headers["X-Real-IP"].FirstOrDefault();
            if (!string.IsNullOrEmpty(realIp))
            {
                return realIp;
            }

            return context.Connection.RemoteIpAddress?.ToString() ?? "unknown";
        }

        private Task<int> GetRequestCount(string key)
        {
            if (_cache.TryGetValue(key, out var cached) && cached is int count)
            {
                return Task.FromResult(count);
            }
            return Task.FromResult(0);
        }

        private async Task IncrementRequestCount(string key)
        {
            var currentCount = await GetRequestCount(key);
            var newCount = currentCount + 1;

            // Cache for 1 hour with sliding expiration
            var cacheOptions = new MemoryCacheEntryOptions
            {
                AbsoluteExpirationRelativeToNow = TimeSpan.FromHours(1),
                Priority = CacheItemPriority.Normal
            };

            _cache.Set(key, newCount, cacheOptions);
        }

        private long GetResetTime()
        {
            var nextHour = DateTime.UtcNow.AddHours(1);
            var resetTime = new DateTime(nextHour.Year, nextHour.Month, nextHour.Day, nextHour.Hour, 0, 0, DateTimeKind.Utc);
            return ((DateTimeOffset)resetTime).ToUnixTimeSeconds();
        }
    }

    /// <summary>
    /// Extension methods for registering rate limiting middleware
    /// </summary>
    public static class RateLimitingMiddlewareExtensions
    {
        /// <summary>
        /// Adds rate limiting middleware to the pipeline
        /// </summary>
        /// <param name="builder">Application builder</param>
        /// <returns>Application builder</returns>
        public static IApplicationBuilder UseRateLimiting(this IApplicationBuilder builder)
        {
            return builder.UseMiddleware<RateLimitingMiddleware>();
        }
    }
}
