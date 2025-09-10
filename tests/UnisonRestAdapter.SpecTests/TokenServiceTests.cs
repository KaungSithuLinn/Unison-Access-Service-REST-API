#nullable enable
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;
using UnisonRestAdapter.Configuration;
using UnisonRestAdapter.Security;
using Xunit;

namespace UnisonRestAdapter.SpecTests
{
    public class TokenServiceTests
    {
        private readonly Mock<IOptions<SecurityOptions>> _mockOptions;
        private readonly Mock<IConfiguration> _mockConfig;
        private readonly Mock<ILogger<TokenService>> _mockLogger;
        private readonly TokenService _tokenService;

        public TokenServiceTests()
        {
            _mockOptions = new Mock<IOptions<SecurityOptions>>();
            _mockConfig = new Mock<IConfiguration>();
            _mockLogger = new Mock<ILogger<TokenService>>();

            var securityOptions = new SecurityOptions
            {
                ValidTokens = new List<string> { "595d799a-9553-4ddf-8fd9-c27b1f233ce7" },
                AllowFallbackToken = true,
                FallbackToken = "595d799a-9553-4ddf-8fd9-c27b1f233ce7"
            };
            _mockOptions.Setup(x => x.Value).Returns(securityOptions);

            _tokenService = new TokenService(_mockOptions.Object, _mockLogger.Object, _mockConfig.Object);
        }

        [Fact]
        public async Task ValidateTokenAsync_WithValidToken_ReturnsTrue()
        {
            var result = await _tokenService.ValidateTokenAsync("595d799a-9553-4ddf-8fd9-c27b1f233ce7");
            result.Should().BeTrue();
        }

        [Fact]
        public async Task ValidateTokenAsync_WithInvalidToken_ReturnsFalse()
        {
            var result = await _tokenService.ValidateTokenAsync("invalid-token");
            result.Should().BeFalse();
        }

        [Fact]
        public async Task ValidateTokenAsync_WithNullToken_ReturnsFalse()
        {
            var result = await _tokenService.ValidateTokenAsync(null!);
            result.Should().BeFalse();
        }

        [Fact]
        public async Task ValidateTokenAsync_WithEmptyToken_ReturnsFalse()
        {
            var result = await _tokenService.ValidateTokenAsync("");
            result.Should().BeFalse();
        }

        [Fact]
        public async Task ValidateTokenAsync_WithWhitespaceToken_ReturnsFalse()
        {
            var result = await _tokenService.ValidateTokenAsync("   ");
            result.Should().BeFalse();
        }

        [Fact]
        public async Task GetCurrentTokenAsync_WithFallbackEnabled_ReturnsFallbackToken()
        {
            var token = await _tokenService.GetCurrentTokenAsync();
            token.Should().Be("595d799a-9553-4ddf-8fd9-c27b1f233ce7");
        }
    }
}