using Microsoft.Extensions.Options;
using System.Net;
using UnisonRestAdapter.Configuration;

namespace UnisonRestAdapter.Security
{
    /// <summary>
    /// Middleware to enforce IP address whitelist
    /// </summary>
    public class IpWhitelistMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<IpWhitelistMiddleware> _logger;
        private readonly SecurityOptions _securityOptions;

        /// <summary>
        /// Initializes a new instance of the IpWhitelistMiddleware
        /// </summary>
        /// <param name="next">Next middleware in pipeline</param>
        /// <param name="logger">Logger instance</param>
        /// <param name="securityOptions">Security configuration options</param>
        public IpWhitelistMiddleware(
            RequestDelegate next,
            ILogger<IpWhitelistMiddleware> logger,
            IOptions<SecurityOptions> securityOptions)
        {
            _next = next;
            _logger = logger;
            _securityOptions = securityOptions.Value;
        }

        /// <summary>
        /// Invokes the middleware to enforce IP whitelist
        /// </summary>
        /// <param name="context">HTTP context</param>
        /// <returns>Task representing the async operation</returns>
        public async Task InvokeAsync(HttpContext context)
        {
            if (!_securityOptions.EnableIpWhitelist ||
                _securityOptions.AllowedIpAddresses == null ||
                !_securityOptions.AllowedIpAddresses.Any())
            {
                await _next(context);
                return;
            }

            var clientIp = GetClientIpAddress(context);

            // Skip IP checking for health checks and swagger
            var path = context.Request.Path.Value?.ToLowerInvariant();
            if (path != null && (path.Contains("/health") || path.Contains("/swagger") || path.Contains("/api/docs")))
            {
                await _next(context);
                return;
            }

            try
            {
                if (IsIpAllowed(clientIp))
                {
                    _logger.LogDebug("IP address {ClientIp} allowed", clientIp);
                    await _next(context);
                }
                else
                {
                    _logger.LogWarning("IP address {ClientIp} not in whitelist, blocking request", clientIp);

                    context.Response.StatusCode = 403; // Forbidden
                    await context.Response.WriteAsync("Access denied: IP address not allowed");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in IP whitelist middleware for IP: {ClientIp}", clientIp);
                context.Response.StatusCode = 500;
                await context.Response.WriteAsync("Internal server error");
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

        private bool IsIpAllowed(string clientIp)
        {
            if (string.IsNullOrEmpty(clientIp) || clientIp == "unknown")
            {
                return false;
            }

            // Allow localhost in development
            if (clientIp == "::1" || clientIp == "127.0.0.1" || clientIp.StartsWith("192.168.") || clientIp.StartsWith("10."))
            {
                _logger.LogDebug("Allowing local IP address: {ClientIp}", clientIp);
                return true;
            }

            // Parse client IP
            if (!IPAddress.TryParse(clientIp, out var clientIpAddress))
            {
                _logger.LogWarning("Invalid IP address format: {ClientIp}", clientIp);
                return false;
            }

            // Check against whitelist
            foreach (var allowedIp in _securityOptions.AllowedIpAddresses)
            {
                if (string.IsNullOrWhiteSpace(allowedIp))
                    continue;

                // Handle CIDR notation
                if (allowedIp.Contains('/'))
                {
                    if (IsIpInCidrRange(clientIpAddress, allowedIp))
                    {
                        return true;
                    }
                }
                // Handle exact IP match
                else if (IPAddress.TryParse(allowedIp, out var allowedIpAddress))
                {
                    if (clientIpAddress.Equals(allowedIpAddress))
                    {
                        return true;
                    }
                }
                // Handle wildcard patterns (basic implementation)
                else if (allowedIp.Contains('*'))
                {
                    var pattern = allowedIp.Replace("*", ".*");
                    if (System.Text.RegularExpressions.Regex.IsMatch(clientIp, pattern))
                    {
                        return true;
                    }
                }
            }

            return false;
        }

        private bool IsIpInCidrRange(IPAddress ipAddress, string cidrRange)
        {
            try
            {
                var parts = cidrRange.Split('/');
                if (parts.Length != 2)
                    return false;

                if (!IPAddress.TryParse(parts[0], out var networkAddress) ||
                    !int.TryParse(parts[1], out var prefixLength))
                {
                    return false;
                }

                // Convert to bytes for comparison
                var ipBytes = ipAddress.GetAddressBytes();
                var networkBytes = networkAddress.GetAddressBytes();

                if (ipBytes.Length != networkBytes.Length)
                    return false;

                // Calculate number of bytes and bits to check
                var bytesToCheck = prefixLength / 8;
                var bitsToCheck = prefixLength % 8;

                // Check full bytes
                for (int i = 0; i < bytesToCheck; i++)
                {
                    if (ipBytes[i] != networkBytes[i])
                        return false;
                }

                // Check remaining bits
                if (bitsToCheck > 0 && bytesToCheck < ipBytes.Length)
                {
                    var mask = (byte)(0xFF << (8 - bitsToCheck));
                    if ((ipBytes[bytesToCheck] & mask) != (networkBytes[bytesToCheck] & mask))
                        return false;
                }

                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error checking CIDR range {CidrRange} for IP {IpAddress}", cidrRange, ipAddress);
                return false;
            }
        }
    }

    /// <summary>
    /// Extension methods for registering IP whitelist middleware
    /// </summary>
    public static class IpWhitelistMiddlewareExtensions
    {
        /// <summary>
        /// Adds IP whitelist middleware to the pipeline
        /// </summary>
        /// <param name="builder">Application builder</param>
        /// <returns>Application builder</returns>
        public static IApplicationBuilder UseIpWhitelist(this IApplicationBuilder builder)
        {
            return builder.UseMiddleware<IpWhitelistMiddleware>();
        }
    }
}
