using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;
using UnisonRestAdapter.Configuration;
using UnisonRestAdapter.Security;
using Xunit;

namespace UnisonRestAdapter.Tests.Security
{
    public class TokenServiceTests
    {
        private readonly Mock<ILogger<TokenService>> _loggerMock;
        private readonly Mock<IConfiguration> _configurationMock;
        private readonly SecurityOptions _securityOptions;
        private readonly TokenService _tokenService;

        public TokenServiceTests()
        {
            _loggerMock = new Mock<ILogger<TokenService>>();
            _configurationMock = new Mock<IConfiguration>();

            _securityOptions = new SecurityOptions
            {
                AllowFallbackToken = true,
                FallbackToken = "test-token-123",
                EncryptTokensInStorage = false,
                EncryptionKey = "test-key"
            };

            var options = Options.Create(_securityOptions);
            _tokenService = new TokenService(options, _loggerMock.Object, _configurationMock.Object);
        }

        [Fact]
        public async Task ValidateTokenAsync_WithValidToken_ReturnsTrue()
        {
            // Arrange
            var validToken = "test-token-123";

            // Act
            var result = await _tokenService.ValidateTokenAsync(validToken);

            // Assert
            Assert.True(result);
        }

        [Fact]
        public async Task ValidateTokenAsync_WithInvalidToken_ReturnsFalse()
        {
            // Arrange
            var invalidToken = "invalid-token";

            // Act
            var result = await _tokenService.ValidateTokenAsync(invalidToken);

            // Assert
            Assert.False(result);
        }

        [Fact]
        public async Task ValidateTokenAsync_WithEmptyToken_ReturnsFalse()
        {
            // Arrange
            var emptyToken = "";

            // Act
            var result = await _tokenService.ValidateTokenAsync(emptyToken);

            // Assert
            Assert.False(result);
        }

        [Fact]
        public async Task ValidateTokenAsync_WithNullToken_ReturnsFalse()
        {
            // Arrange
            string? nullToken = null;

            // Act
            var result = await _tokenService.ValidateTokenAsync(nullToken!);

            // Assert
            Assert.False(result);
        }

        [Fact]
        public async Task GetCurrentTokenAsync_WithFallbackEnabled_ReturnsFallbackToken()
        {
            // Act
            var token = await _tokenService.GetCurrentTokenAsync();

            // Assert
            Assert.Equal("test-token-123", token);
        }

        [Fact]
        public async Task RotateTokenAsync_WithValidToken_ReturnsTrue()
        {
            // Arrange
            var newToken = "new-token-456";

            // Act
            var result = await _tokenService.RotateTokenAsync(newToken);

            // Assert
            Assert.True(result);
        }

        [Fact]
        public async Task RotateTokenAsync_WithEmptyToken_ReturnsFalse()
        {
            // Arrange
            var emptyToken = "";

            // Act
            var result = await _tokenService.RotateTokenAsync(emptyToken);

            // Assert
            Assert.False(result);
        }

        [Fact]
        public async Task LogSecurityEventAsync_ShouldCompleteSuccessfully()
        {
            // Arrange
            var eventType = "TEST_EVENT";
            var details = "Test security event";

            // Act & Assert
            await _tokenService.LogSecurityEventAsync(eventType, details);

            // Verify logging was called
            _loggerMock.Verify(
                x => x.Log(
                    LogLevel.Warning,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("SECURITY_EVENT")),
                    It.IsAny<Exception?>(),
                    It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
                Times.Once);
        }
    }
}
