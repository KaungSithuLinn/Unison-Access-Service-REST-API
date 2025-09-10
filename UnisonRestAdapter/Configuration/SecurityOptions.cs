using System.ComponentModel.DataAnnotations;

namespace UnisonRestAdapter.Configuration
{
    /// <summary>
    /// Security configuration options for the Unison REST Adapter
    /// </summary>
    public class SecurityOptions
    {
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
    }
}
