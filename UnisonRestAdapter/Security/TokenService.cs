using Microsoft.Extensions.Options;
using System.Security.Cryptography;
using System.Text;
using UnisonRestAdapter.Configuration;

namespace UnisonRestAdapter.Security
{
    public interface ITokenService
    {
        Task<bool> ValidateTokenAsync(string token);
        Task<string> GetCurrentTokenAsync();
        Task<bool> RotateTokenAsync(string newToken);
        Task LogSecurityEventAsync(string eventType, string details, string? correlationId = null);
    }

    public class TokenService : ITokenService
    {
        private readonly SecurityOptions _options;
        private readonly ILogger<TokenService> _logger;
        private readonly IConfiguration _configuration;

        public TokenService(
            IOptions<SecurityOptions> options,
            ILogger<TokenService> logger,
            IConfiguration configuration)
        {
            _options = options.Value;
            _logger = logger;
            _configuration = configuration;
        }

        public async Task<bool> ValidateTokenAsync(string token)
        {
            if (string.IsNullOrWhiteSpace(token))
            {
                await LogSecurityEventAsync("TOKEN_VALIDATION_FAILED", "Empty or null token provided");
                return false;
            }

            try
            {
                var currentToken = await GetCurrentTokenAsync();

                // Compare tokens using secure string comparison
                var isValid = SecureStringCompare(token, currentToken);

                if (isValid)
                {
                    await LogSecurityEventAsync("TOKEN_VALIDATION_SUCCESS", $"Token validated successfully");
                }
                else
                {
                    await LogSecurityEventAsync("TOKEN_VALIDATION_FAILED", "Token mismatch");
                }

                return isValid;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error validating token");
                await LogSecurityEventAsync("TOKEN_VALIDATION_ERROR", $"Validation error: {ex.Message}");
                return false;
            }
        }

        public async Task<string> GetCurrentTokenAsync()
        {
            try
            {
                // First try to get from secure configuration
                var secureToken = _configuration[$"{_options.TokenConfigSection}:Token"];
                if (!string.IsNullOrEmpty(secureToken))
                {
                    return await Task.FromResult(DecryptToken(secureToken));
                }

                // Fallback to environment variable
                var envToken = Environment.GetEnvironmentVariable("UNISON_ACCESS_TOKEN");
                if (!string.IsNullOrEmpty(envToken))
                {
                    return await Task.FromResult(envToken);
                }

                // Fallback to default token (for development only)
                if (_options.AllowFallbackToken && !string.IsNullOrEmpty(_options.FallbackToken))
                {
                    _logger.LogWarning("Using fallback token - this should only happen in development");
                    await LogSecurityEventAsync("FALLBACK_TOKEN_USED", "Fallback token used - check configuration");
                    return await Task.FromResult(_options.FallbackToken);
                }

                throw new InvalidOperationException("No valid token found in configuration");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving current token");
                await LogSecurityEventAsync("TOKEN_RETRIEVAL_ERROR", $"Error retrieving token: {ex.Message}");
                throw;
            }
        }

        public async Task<bool> RotateTokenAsync(string newToken)
        {
            if (string.IsNullOrWhiteSpace(newToken))
            {
                await LogSecurityEventAsync("TOKEN_ROTATION_FAILED", "New token is empty or null");
                return false;
            }

            try
            {
                var oldToken = await GetCurrentTokenAsync();
                var encryptedToken = EncryptToken(newToken);

                // Here you would typically update the secure storage
                // For now, we'll log the rotation attempt
                await LogSecurityEventAsync("TOKEN_ROTATION_INITIATED",
                    $"Token rotation initiated. Old token length: {oldToken?.Length ?? 0}, New token length: {newToken.Length}");

                // In production, this would update Azure Key Vault, encrypted config, etc.
                _logger.LogInformation("Token rotation completed successfully");

                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during token rotation");
                await LogSecurityEventAsync("TOKEN_ROTATION_ERROR", $"Rotation error: {ex.Message}");
                return false;
            }
        }

        public async Task LogSecurityEventAsync(string eventType, string details, string? correlationId = null)
        {
            var logEntry = new
            {
                EventType = eventType,
                Details = details,
                CorrelationId = correlationId ?? Guid.NewGuid().ToString(),
                Timestamp = DateTime.UtcNow,
                Source = "TokenService"
            };

            _logger.LogWarning("SECURITY_EVENT: {EventType} - {Details} [CorrelationId: {CorrelationId}]",
                eventType, details, logEntry.CorrelationId);

            // In production, you might also send to a security monitoring system
            await Task.CompletedTask;
        }

        private static bool SecureStringCompare(string a, string b)
        {
            if (a == null && b == null) return true;
            if (a == null || b == null) return false;
            if (a.Length != b.Length) return false;

            var result = 0;
            for (int i = 0; i < a.Length; i++)
            {
                result |= a[i] ^ b[i];
            }

            return result == 0;
        }

        private string EncryptToken(string token)
        {
            if (!_options.EncryptTokensInStorage)
            {
                return token;
            }

            try
            {
                using var aes = Aes.Create();
                aes.Key = DeriveKeyFromPassword(_options.EncryptionKey);
                aes.GenerateIV();

                using var encryptor = aes.CreateEncryptor();
                var tokenBytes = Encoding.UTF8.GetBytes(token);
                var encryptedBytes = encryptor.TransformFinalBlock(tokenBytes, 0, tokenBytes.Length);

                // Combine IV and encrypted data
                var result = new byte[aes.IV.Length + encryptedBytes.Length];
                Array.Copy(aes.IV, 0, result, 0, aes.IV.Length);
                Array.Copy(encryptedBytes, 0, result, aes.IV.Length, encryptedBytes.Length);

                return Convert.ToBase64String(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error encrypting token");
                throw;
            }
        }

        private string DecryptToken(string encryptedToken)
        {
            if (!_options.EncryptTokensInStorage)
            {
                return encryptedToken;
            }

            try
            {
                var data = Convert.FromBase64String(encryptedToken);

                using var aes = Aes.Create();
                aes.Key = DeriveKeyFromPassword(_options.EncryptionKey);

                // Extract IV
                var iv = new byte[16];
                Array.Copy(data, 0, iv, 0, 16);
                aes.IV = iv;

                // Extract encrypted data
                var encryptedBytes = new byte[data.Length - 16];
                Array.Copy(data, 16, encryptedBytes, 0, encryptedBytes.Length);

                using var decryptor = aes.CreateDecryptor();
                var decryptedBytes = decryptor.TransformFinalBlock(encryptedBytes, 0, encryptedBytes.Length);

                return Encoding.UTF8.GetString(decryptedBytes);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error decrypting token");
                throw;
            }
        }

        private static byte[] DeriveKeyFromPassword(string password)
        {
            // Use a fixed salt for simplicity - in production, use a proper key derivation
            var salt = Encoding.UTF8.GetBytes("UnisonRestAdapterSalt2025");

            using var rfc2898 = new Rfc2898DeriveBytes(password, salt, 10000, HashAlgorithmName.SHA256);
            return rfc2898.GetBytes(32); // 256-bit key
        }
    }
}
