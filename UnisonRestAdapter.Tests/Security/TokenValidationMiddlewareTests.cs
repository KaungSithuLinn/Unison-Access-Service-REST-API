using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;
using UnisonRestAdapter.Configuration;
using UnisonRestAdapter.Security;
using Xunit;

namespace UnisonRestAdapter.Tests.Security
{
    public class TokenValidationMiddlewareTests
    {
        private readonly Mock<RequestDelegate> _nextMock;
        private readonly Mock<ITokenService> _tokenServiceMock;
        private readonly Mock<ILogger<TokenValidationMiddleware>> _loggerMock;
        private readonly SecurityOptions _securityOptions;
        private readonly TokenValidationMiddleware _middleware;

        public TokenValidationMiddlewareTests()
        {
            _nextMock = new Mock<RequestDelegate>();
            _tokenServiceMock = new Mock<ITokenService>();
            _loggerMock = new Mock<ILogger<TokenValidationMiddleware>>();

            _securityOptions = new SecurityOptions();
            var options = Options.Create(_securityOptions);

            _middleware = new TokenValidationMiddleware(
                _nextMock.Object,
                _tokenServiceMock.Object,
                _loggerMock.Object,
                options);
        }

        [Fact]
        public async Task InvokeAsync_HealthCheckEndpoint_SkipsValidation()
        {
            // Arrange
            var context = CreateHttpContext("/health");

            // Act
            await _middleware.InvokeAsync(context);

            // Assert
            _nextMock.Verify(x => x(context), Times.Once);
            _tokenServiceMock.Verify(x => x.ValidateTokenAsync(It.IsAny<string>()), Times.Never);
        }

        [Fact]
        public async Task InvokeAsync_SwaggerEndpoint_SkipsValidation()
        {
            // Arrange
            var context = CreateHttpContext("/swagger/index.html");

            // Act
            await _middleware.InvokeAsync(context);

            // Assert
            _nextMock.Verify(x => x(context), Times.Once);
            _tokenServiceMock.Verify(x => x.ValidateTokenAsync(It.IsAny<string>()), Times.Never);
        }

        [Fact]
        public async Task InvokeAsync_NonApiEndpoint_SkipsValidation()
        {
            // Arrange
            var context = CreateHttpContext("/home");

            // Act
            await _middleware.InvokeAsync(context);

            // Assert
            _nextMock.Verify(x => x(context), Times.Once);
            _tokenServiceMock.Verify(x => x.ValidateTokenAsync(It.IsAny<string>()), Times.Never);
        }

        [Fact]
        public async Task InvokeAsync_ApiEndpointWithoutToken_Returns401()
        {
            // Arrange
            var context = CreateHttpContext("/api/cards/update");

            // Act
            await _middleware.InvokeAsync(context);

            // Assert
            Assert.Equal(401, context.Response.StatusCode);
            _nextMock.Verify(x => x(context), Times.Never);
        }

        [Fact]
        public async Task InvokeAsync_ApiEndpointWithValidToken_CallsNext()
        {
            // Arrange
            var context = CreateHttpContext("/api/cards/update");
            context.Request.Headers["Unison-Token"] = "valid-token";
            _tokenServiceMock.Setup(x => x.ValidateTokenAsync("valid-token")).ReturnsAsync(true);

            // Act
            await _middleware.InvokeAsync(context);

            // Assert
            _nextMock.Verify(x => x(context), Times.Once);
            Assert.Equal("valid-token", context.Items["UnisonToken"]);
            Assert.Equal(true, context.Items["IsAuthenticated"]);
        }

        [Fact]
        public async Task InvokeAsync_ApiEndpointWithInvalidToken_Returns401()
        {
            // Arrange
            var context = CreateHttpContext("/api/cards/update");
            context.Request.Headers["Unison-Token"] = "invalid-token";
            _tokenServiceMock.Setup(x => x.ValidateTokenAsync("invalid-token")).ReturnsAsync(false);

            // Act
            await _middleware.InvokeAsync(context);

            // Assert
            Assert.Equal(401, context.Response.StatusCode);
            _nextMock.Verify(x => x(context), Times.Never);
        }

        [Fact]
        public async Task InvokeAsync_ApiEndpointWithAuthorizationHeader_ExtractsToken()
        {
            // Arrange
            var context = CreateHttpContext("/api/cards/update");
            context.Request.Headers["Authorization"] = "Bearer test-token";
            _tokenServiceMock.Setup(x => x.ValidateTokenAsync("test-token")).ReturnsAsync(true);

            // Act
            await _middleware.InvokeAsync(context);

            // Assert
            _nextMock.Verify(x => x(context), Times.Once);
            Assert.Equal("test-token", context.Items["UnisonToken"]);
        }

        private static DefaultHttpContext CreateHttpContext(string path)
        {
            var context = new DefaultHttpContext();
            context.Request.Path = path;
            context.Response.Body = new MemoryStream();
            return context;
        }
    }
}
