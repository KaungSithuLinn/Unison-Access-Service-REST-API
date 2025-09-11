using System.ComponentModel.DataAnnotations;

namespace UnisonRestAdapter.Configuration
{
    /// <summary>
    /// Security configuration options for the Unison REST Adapter
    /// </summary>
    public class SecurityOptions
    {
        /// <summary>
        /// Configuration section name
        /// </summary>
        public const string SectionName = "Security";

        /// <summary>
        /// Configuration section for token storage
        /// </summary>
        public string TokenConfigSection { get; set; } = "Security";

        /// <summary>
        /// Encryption key for token storage
        /// </summary>
        public string EncryptionKey { get; set; } = "DefaultEncryptionKey2025";

        /// <summary>
        /// Whether to encrypt tokens in storage
        /// </summary>
        public bool EncryptTokensInStorage { get; set; } = true;

        /// <summary>
        /// Allow fallback token for development
        /// </summary>
        public bool AllowFallbackToken { get; set; } = false;

        /// <summary>
        /// Fallback token for development only
        /// </summary>
        public string FallbackToken { get; set; } = "595d799a-9553-4ddf-8fd9-c27b1f233ce7";

        /// <summary>
        /// Valid Unison authentication tokens
        /// </summary>
        public List<string> ValidTokens { get; set; } = new();

        /// <summary>
        /// Token expiry hours (default: 24 hours)
        /// </summary>
        public int TokenExpiryHours { get; set; } = 24;

        /// <summary>
        /// Enable token rotation
        /// </summary>
        public bool EnableTokenRotation { get; set; } = false;

        /// <summary>
        /// Maximum requests per token per hour
        /// </summary>
        public int MaxRequestsPerHour { get; set; } = 1000;

        /// <summary>
        /// Enable request rate limiting
        /// </summary>
        public bool EnableRateLimiting { get; set; } = true;

        /// <summary>
        /// Rate limiting settings
        /// </summary>
        public int MaxRequestsPerMinute { get; set; } = 100;

        /// <summary>
        /// Rate limiting window duration in minutes
        /// </summary>
        public int RateLimitWindowMinutes { get; set; } = 1;

        /// <summary>
        /// Duration to temporarily block IPs that exceed rate limits (in minutes)
        /// </summary>
        public int TemporaryBlockDurationMinutes { get; set; } = 15;

        /// <summary>
        /// Allowed IP addresses (empty = allow all)
        /// </summary>
        public List<string> AllowedIpAddresses { get; set; } = new();

        /// <summary>
        /// Enable IP whitelist enforcement
        /// </summary>
        public bool EnableIpWhitelist { get; set; } = false;

        /// <summary>
        /// Log security events
        /// </summary>
        public bool LogSecurityEvents { get; set; } = true;

        /// <summary>
        /// Block suspicious requests
        /// </summary>
        public bool BlockSuspiciousRequests { get; set; } = true;

        /// <summary>
        /// Require HTTPS for all requests
        /// </summary>
        public bool RequireHttps { get; set; } = true;

        /// <summary>
        /// Enable HTTP Strict Transport Security (HSTS)
        /// </summary>
        public bool EnableHsts { get; set; } = true;

        /// <summary>
        /// HSTS max age in seconds (1 year default)
        /// </summary>
        public int HstsMaxAge { get; set; } = 31536000;

        /// <summary>
        /// Enable Cross-Origin Resource Sharing (CORS)
        /// </summary>
        public bool EnableCors { get; set; } = false;

        /// <summary>
        /// Allowed origins for CORS
        /// </summary>
        public List<string> AllowedOrigins { get; set; } = new();

        /// <summary>
        /// Allowed CORS methods
        /// </summary>
        public List<string> AllowedMethods { get; set; } = new List<string> { "GET", "POST", "PUT", "DELETE", "OPTIONS" };

        /// <summary>
        /// Allowed CORS headers
        /// </summary>
        public List<string> AllowedHeaders { get; set; } = new List<string> { "Authorization", "Content-Type", "X-Requested-With" };

        /// <summary>
        /// Maximum request size in bytes (1MB default)
        /// </summary>
        public long MaxRequestSizeBytes { get; set; } = 1048576;

        /// <summary>
        /// Enable API key authentication
        /// </summary>
        public bool EnableApiKeyAuthentication { get; set; } = true;

        /// <summary>
        /// API key header name
        /// </summary>
        public string ApiKeyHeaderName { get; set; } = "Unison-Token";

        /// <summary>
        /// Enable request validation against malicious patterns
        /// </summary>
        public bool EnableRequestValidation { get; set; } = true;

        /// <summary>
        /// Block requests with malicious patterns
        /// </summary>
        public bool BlockMaliciousPatterns { get; set; } = true;

        /// <summary>
        /// Enable Content Security Policy headers
        /// </summary>
        public bool EnableContentSecurityPolicy { get; set; } = true;
    }
}
