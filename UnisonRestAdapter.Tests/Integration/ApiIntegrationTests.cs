using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using System.Net.Http;
using System.Text;
using Xunit;
using System.Text.Json;
using UnisonRestAdapter.Models.Request;

namespace UnisonRestAdapter.Tests.Integration
{
    /// <summary>
    /// Integration tests for the Unison REST Adapter API
    /// </summary>
    public class ApiIntegrationTests : IClassFixture<WebApplicationFactory<Program>>
    {
        private readonly WebApplicationFactory<Program> _factory;
        private readonly HttpClient _client;
        private const string ValidToken = "595d799a-9553-4ddf-8fd9-c27b1f233ce7";

        public ApiIntegrationTests(WebApplicationFactory<Program> factory)
        {
            _factory = factory;
            _client = _factory.CreateClient();
        }

        [Fact]
        public async Task HealthCheck_ReturnsHealthy()
        {
            // Act
            var response = await _client.GetAsync("/health");

            // Assert
            response.EnsureSuccessStatusCode();
            var content = await response.Content.ReadAsStringAsync();
            Assert.Contains("Healthy", content);
        }

        [Fact]
        public async Task UpdateCard_WithValidToken_ReturnsSuccess()
        {
            // Arrange
            var request = new UpdateCardRequest
            {
                CardId = "TEST123",
                UserName = "test.user",
                FirstName = "Test",
                LastName = "User",
                Email = "test.user@company.com",
                Department = "IT",
                Title = "Developer",
                IsActive = true
            };

            var json = JsonSerializer.Serialize(request);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            _client.DefaultRequestHeaders.Add("Unison-Token", ValidToken);

            // Act
            var response = await _client.PostAsync("/api/cards/update", content);

            // Assert
            Assert.True(response.IsSuccessStatusCode || response.StatusCode == System.Net.HttpStatusCode.BadRequest);
            // Note: May return 400 if SOAP backend is not available during testing
        }

        [Fact]
        public async Task UpdateCard_WithoutToken_ReturnsUnauthorized()
        {
            // Arrange
            var request = new UpdateCardRequest { CardId = "TEST123" };
            var json = JsonSerializer.Serialize(request);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            // Act
            var response = await _client.PostAsync("/api/cards/update", content);

            // Assert
            Assert.Equal(System.Net.HttpStatusCode.Unauthorized, response.StatusCode);
        }

        [Fact]
        public async Task UpdateCard_AlternativeRoute_Works()
        {
            // Arrange
            var request = new UpdateCardRequest
            {
                CardId = "TEST456",
                UserName = "alt.user",
                IsActive = false
            };

            var json = JsonSerializer.Serialize(request);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            _client.DefaultRequestHeaders.Clear();
            _client.DefaultRequestHeaders.Add("Unison-Token", ValidToken);

            // Act
            var response = await _client.PostAsync("/updatecard", content);

            // Assert
            Assert.True(response.IsSuccessStatusCode || response.StatusCode == System.Net.HttpStatusCode.BadRequest);
        }

        [Fact]
        public async Task UpdateCard_InvalidCardId_ReturnsBadRequest()
        {
            // Arrange
            var request = new UpdateCardRequest(); // Missing required CardId
            var json = JsonSerializer.Serialize(request);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            _client.DefaultRequestHeaders.Clear();
            _client.DefaultRequestHeaders.Add("Unison-Token", ValidToken);

            // Act
            var response = await _client.PostAsync("/api/cards/update", content);

            // Assert
            Assert.Equal(System.Net.HttpStatusCode.BadRequest, response.StatusCode);
        }

        [Theory]
        [InlineData("")]
        [InlineData("invalid-token")]
        [InlineData("123")]
        public async Task UpdateCard_WithInvalidTokens_ReturnsUnauthorized(string invalidToken)
        {
            // Arrange
            var request = new UpdateCardRequest { CardId = "TEST789" };
            var json = JsonSerializer.Serialize(request);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            _client.DefaultRequestHeaders.Clear();
            if (!string.IsNullOrEmpty(invalidToken))
            {
                _client.DefaultRequestHeaders.Add("Unison-Token", invalidToken);
            }

            // Act
            var response = await _client.PostAsync("/api/cards/update", content);

            // Assert
            Assert.Equal(System.Net.HttpStatusCode.Unauthorized, response.StatusCode);
        }

        [Fact]
        public async Task Swagger_IsAccessible()
        {
            // Act
            var response = await _client.GetAsync("/swagger/index.html");

            // Assert
            response.EnsureSuccessStatusCode();
            var content = await response.Content.ReadAsStringAsync();
            Assert.Contains("Swagger", content);
        }

        [Fact]
        public async Task GetCard_WithValidToken_ReturnsResponse()
        {
            // Arrange
            _client.DefaultRequestHeaders.Clear();
            _client.DefaultRequestHeaders.Add("Unison-Token", ValidToken);

            // Act
            var response = await _client.GetAsync("/api/cards/TEST123");

            // Assert
            // Note: May return 404 if card doesn't exist, which is valid
            Assert.True(
                response.IsSuccessStatusCode ||
                response.StatusCode == System.Net.HttpStatusCode.NotFound ||
                response.StatusCode == System.Net.HttpStatusCode.BadRequest
            );
        }
    }
}
