using System;
using System.Linq;
using Microsoft.Extensions.Logging;
using Moq;
using UnisonRestAdapter.Models.Request;
using UnisonRestAdapter.Services;
using Xunit;

namespace UnisonRestAdapter.Tests.Services
{
    /// <summary>
    /// Unit tests for ValidationService
    /// Tests TASK-002: SOAP Request Validation Templates
    /// </summary>
    public class ValidationServiceTests
    {
        private readonly ValidationService _validationService;
        private readonly Mock<ILogger<ValidationService>> _mockLogger;

        public ValidationServiceTests()
        {
            _mockLogger = new Mock<ILogger<ValidationService>>();
            _validationService = new ValidationService(_mockLogger.Object);
        }

        [Fact]
        public void Constructor_WithNullLogger_ThrowsArgumentNullException()
        {
            // Act & Assert
            Assert.Throws<ArgumentNullException>(() => new ValidationService(null));
        }

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        [InlineData("   ")]
        public void ValidateUpdateCardRequest_MissingCardId_ReturnsInvalidResult(string cardId)
        {
            // Arrange
            var request = new UpdateCardRequest
            {
                CardId = cardId
            };

            // Act
            var result = _validationService.ValidateUpdateCardRequest(request);

            // Assert
            Assert.False(result.IsValid);
            Assert.Contains("CardId is required", result.Errors.First());
            Assert.NotEmpty(result.CorrelationId);
            Assert.True(result.Timestamp > DateTime.UtcNow.AddMinutes(-1));
        }

        [Theory]
        [InlineData("card@123#")]
        [InlineData("card with spaces")]
        [InlineData("very-long-card-id-that-exceeds-fifty-characters-limit-test")]
        public void ValidateUpdateCardRequest_InvalidCardId_ReturnsInvalidResult(string cardId)
        {
            // Arrange
            var request = new UpdateCardRequest
            {
                CardId = cardId
            };

            // Act
            var result = _validationService.ValidateUpdateCardRequest(request);

            // Assert
            Assert.False(result.IsValid);
            Assert.Contains("CardId must be alphanumeric", result.Errors.First());
        }

        [Theory]
        [InlineData("invalid-email")]
        [InlineData("@invalid.com")]
        [InlineData("user@")]
        public void ValidateUpdateCardRequest_InvalidEmail_ReturnsInvalidResult(string email)
        {
            // Arrange
            var request = new UpdateCardRequest
            {
                CardId = "12345",
                Email = email
            };

            // Act
            var result = _validationService.ValidateUpdateCardRequest(request);

            // Assert
            Assert.False(result.IsValid);
            Assert.Contains("Email format is invalid", result.Errors.First());
        }

        [Fact]
        public void ValidateUpdateCardRequest_PastExpirationDate_ReturnsInvalidResult()
        {
            // Arrange
            var request = new UpdateCardRequest
            {
                CardId = "12345",
                ExpirationDate = DateTime.Now.AddDays(-1) // Past date
            };

            // Act
            var result = _validationService.ValidateUpdateCardRequest(request);

            // Assert
            Assert.False(result.IsValid);
            Assert.Contains("ExpirationDate must be in the future", result.Errors.First());
        }

        [Fact]
        public void ValidateUpdateCardRequest_ValidRequest_ReturnsValidResult()
        {
            // Arrange
            var request = new UpdateCardRequest
            {
                CardId = "12345",
                UserName = "jdoe",
                FirstName = "John",
                LastName = "Doe",
                Email = "john.doe@example.com",
                IsActive = true,
                ExpirationDate = DateTime.Now.AddYears(1)
            };

            // Act
            var result = _validationService.ValidateUpdateCardRequest(request);

            // Assert
            Assert.True(result.IsValid);
            Assert.Empty(result.Errors);
            Assert.NotEmpty(result.CorrelationId);
        }

        [Fact]
        public void GenerateUpdateCardSoapEnvelope_WithNullRequest_ThrowsArgumentNullException()
        {
            // Act & Assert
            Assert.Throws<ArgumentNullException>(() =>
                _validationService.GenerateUpdateCardSoapEnvelope(null, "token"));
        }

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        [InlineData("   ")]
        public void GenerateUpdateCardSoapEnvelope_WithInvalidToken_ThrowsArgumentException(string token)
        {
            // Arrange
            var request = new UpdateCardRequest
            {
                CardId = "12345"
            };

            // Act & Assert
            Assert.Throws<ArgumentException>(() =>
                _validationService.GenerateUpdateCardSoapEnvelope(request, token));
        }

        [Fact]
        public void GenerateUpdateCardSoapEnvelope_WithValidInputs_GeneratesCorrectSoap()
        {
            // Arrange
            var request = new UpdateCardRequest
            {
                CardId = "12345",
                UserName = "jdoe",
                FirstName = "John",
                LastName = "Doe",
                Email = "john.doe@example.com",
                IsActive = true
            };
            var token = "test-token-123";

            // Act
            var result = _validationService.GenerateUpdateCardSoapEnvelope(request, token);

            // Assert
            Assert.Contains("soap:Envelope", result);
            Assert.Contains("UpdateCard", result);
            Assert.Contains("12345", result); // CardId
            Assert.Contains("jdoe", result); // UserName
            Assert.Contains("test-token-123", result); // Token
            Assert.Contains("John", result); // FirstName
            Assert.Contains("Doe", result); // LastName
            Assert.Contains("true", result); // IsActive
        }

        [Fact]
        public void GenerateUpdateCardSoapEnvelope_WithSpecialCharacters_EscapesCorrectly()
        {
            // Arrange
            var request = new UpdateCardRequest
            {
                CardId = "12345",
                FirstName = "John & Jane",
                LastName = "O'Connor"
            };
            var token = "token<>&";

            // Act
            var result = _validationService.GenerateUpdateCardSoapEnvelope(request, token);

            // Assert
            Assert.Contains("John &amp; Jane", result);
            Assert.Contains("O&apos;Connor", result);
            Assert.Contains("token&lt;&gt;&amp;", result);
        }

        [Fact]
        public void CreateValidationErrorResponse_WithNullResult_ThrowsArgumentNullException()
        {
            // Act & Assert
            Assert.Throws<ArgumentNullException>(() =>
                _validationService.CreateValidationErrorResponse(null));
        }

        [Fact]
        public void CreateValidationErrorResponse_WithValidationResult_CreatesStructuredResponse()
        {
            // Arrange
            var correlationId = Guid.NewGuid().ToString();
            var timestamp = DateTime.UtcNow;
            var validationResult = new ValidationResult
            {
                IsValid = false,
                Errors = new[] { "Error 1", "Error 2" }.ToList(),
                CorrelationId = correlationId,
                Timestamp = timestamp
            };

            // Act
            var result = _validationService.CreateValidationErrorResponse(validationResult);

            // Assert
            var json = System.Text.Json.JsonSerializer.Serialize(result);
            Assert.Contains("\"success\":false", json);
            Assert.Contains("\"message\":\"Error 1; Error 2\"", json);
            Assert.Contains($"\"correlationId\":\"{correlationId}\"", json);
            Assert.Contains("\"timestamp\":", json);
            Assert.Contains("\"errors\":", json);
        }
    }
}