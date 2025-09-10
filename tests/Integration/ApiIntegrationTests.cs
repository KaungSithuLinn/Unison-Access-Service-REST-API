using Microsoft.AspNetCore.Mvc.Testing;
using System.Net;
using System.Text;
using System.Text.Json;
using Xunit;

namespace UnisonRestAdapter.Tests.Integration
{
    public class ApiIntegrationTests : IClassFixture<WebApplicationFactory<Program>>
    {
        private readonly WebApplicationFactory<Program> _factory;
        private readonly HttpClient _client;

        public ApiIntegrationTests(WebApplicationFactory<Program> factory)
        {
            _factory = factory;
            _client = _factory.CreateClient();
        }

        [Fact]
        public async Task HealthEndpoint_ReturnsHealthy()
        {
            // Act
            var response = await _client.GetAsync("/health");

            // Assert
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            var content = await response.Content.ReadAsStringAsync();
            Assert.Equal("Healthy", content);
        }

        [Fact]
        public async Task SwaggerEndpoint_ReturnsSuccess()
        {
            // Act
            var response = await _client.GetAsync("/swagger/v1/swagger.json");

            // Assert
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            var content = await response.Content.ReadAsStringAsync();
            Assert.Contains("Unison REST Adapter API", content);
        }

        [Fact]
        public async Task ApiEndpoint_WithoutToken_ReturnsUnauthorized()
        {
            // Arrange
            var updateCardRequest = new
            {
                cardId = "12345",
                userName = "testuser",
                firstName = "Test",
                lastName = "User",
                email = "test@example.com",
                department = "IT",
                title = "Developer",
                isActive = true
            };

            var json = JsonSerializer.Serialize(updateCardRequest);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            // Act
            var response = await _client.PostAsync("/api/cards/update", content);

            // Assert
            Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);

            var responseContent = await response.Content.ReadAsStringAsync();
            var errorResponse = JsonSerializer.Deserialize<JsonElement>(responseContent);

            Assert.Equal("Unauthorized", errorResponse.GetProperty("error").GetString());
            Assert.Contains("Missing authentication token", errorResponse.GetProperty("message").GetString());
            Assert.True(errorResponse.TryGetProperty("correlationId", out _));
        }

        [Fact]
        public async Task ApiEndpoint_WithValidToken_ProcessesRequest()
        {
            // Arrange
            var updateCardRequest = new
            {
                cardId = "12345",
                userName = "testuser",
                firstName = "Test",
                lastName = "User",
                email = "test@example.com",
                department = "IT",
                title = "Developer",
                isActive = true
            };

            var json = JsonSerializer.Serialize(updateCardRequest);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            // Add valid token header
            _client.DefaultRequestHeaders.Add("Unison-Token", "595d799a-9553-4ddf-8fd9-c27b1f233ce7");

            // Act
            var response = await _client.PostAsync("/api/cards/update", content);

            // Assert
            // Note: This might return various status codes depending on the backend SOAP service availability
            // We're mainly testing that authentication passes and the request is processed
            Assert.NotEqual(HttpStatusCode.Unauthorized, response.StatusCode);

            var responseContent = await response.Content.ReadAsStringAsync();

            // If the service is available, we should get a success or business error
            // If not available, we should get a service error, not authentication error
            if (response.StatusCode != HttpStatusCode.OK)
            {
                // Ensure it's not an authentication error
                Assert.DoesNotContain("Unauthorized", responseContent);
                Assert.DoesNotContain("Missing authentication token", responseContent);
            }
        }

        [Fact]
        public async Task ApiEndpoint_WithInvalidToken_ReturnsUnauthorized()
        {
            // Arrange
            var updateCardRequest = new
            {
                cardId = "12345",
                userName = "testuser",
                firstName = "Test",
                lastName = "User",
                email = "test@example.com",
                department = "IT",
                title = "Developer",
                isActive = true
            };

            var json = JsonSerializer.Serialize(updateCardRequest);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            // Add invalid token header
            _client.DefaultRequestHeaders.Add("Unison-Token", "invalid-token-123");

            // Act
            var response = await _client.PostAsync("/api/cards/update", content);

            // Assert
            Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);

            var responseContent = await response.Content.ReadAsStringAsync();
            var errorResponse = JsonSerializer.Deserialize<JsonElement>(responseContent);

            Assert.Equal("Unauthorized", errorResponse.GetProperty("error").GetString());
            Assert.Contains("Invalid authentication token", errorResponse.GetProperty("message").GetString());
        }

        [Fact]
        public async Task ApiEndpoint_WithAuthorizationBearerToken_ProcessesRequest()
        {
            // Arrange
            var updateCardRequest = new
            {
                cardId = "12345",
                userName = "testuser",
                firstName = "Test",
                lastName = "User",
                email = "test@example.com",
                department = "IT",
                title = "Developer",
                isActive = true
            };

            var json = JsonSerializer.Serialize(updateCardRequest);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            // Add Bearer token in Authorization header
            _client.DefaultRequestHeaders.Add("Authorization", "Bearer 595d799a-9553-4ddf-8fd9-c27b1f233ce7");

            // Act
            var response = await _client.PostAsync("/api/cards/update", content);

            // Assert
            Assert.NotEqual(HttpStatusCode.Unauthorized, response.StatusCode);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                _client?.Dispose();
            }
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }
    }
}
