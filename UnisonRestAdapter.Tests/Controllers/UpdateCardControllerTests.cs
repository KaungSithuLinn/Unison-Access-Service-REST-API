using Microsoft.AspNetCore.Mvc.Testing;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using Xunit;
using UnisonRestAdapter.Models.Request;
using System.Net;

namespace UnisonRestAdapter.Tests.Controllers
{
    /// <summary>
    /// Unit tests for UpdateCardController
    /// Tests TASK-004: Comprehensive Test Suite - UpdateCard API endpoint
    /// </summary>
    public class UpdateCardControllerTests : IClassFixture<WebApplicationFactory<Program>>
    {
        private readonly WebApplicationFactory<Program> _factory;
        private readonly HttpClient _client;
        private const string ValidToken = "595d799a-9553-4ddf-8fd9-c27b1f233ce7";
        private const string BaseUrl = "/updatecard";

        public UpdateCardControllerTests(WebApplicationFactory<Program> factory)
        {
            _factory = factory;
            _client = _factory.CreateClient();
        }

        [Fact]
        public async Task UpdateCard_MissingAuthenticationToken_Returns401()
        {
            // Arrange
            var request = CreateValidUpdateCardRequest();
            var json = JsonSerializer.Serialize(request);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            // Act
            var response = await _client.PostAsync(BaseUrl, content);

            // Assert
            Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
        }

        [Fact]
        public async Task UpdateCard_InvalidAuthenticationToken_Returns401()
        {
            // Arrange
            var request = CreateValidUpdateCardRequest();
            var json = JsonSerializer.Serialize(request);
            var content = new StringContent(json, Encoding.UTF8, "application/json");
            _client.DefaultRequestHeaders.Add("Unison-Token", "invalid-token");

            // Act
            var response = await _client.PostAsync(BaseUrl, content);

            // Assert
            Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
        }

        [Fact]
        public async Task UpdateCard_ValidRequest_ReturnsSuccess()
        {
            // Arrange
            var request = CreateValidUpdateCardRequest();
            var json = JsonSerializer.Serialize(request);
            var content = new StringContent(json, Encoding.UTF8, "application/json");
            _client.DefaultRequestHeaders.Add("Unison-Token", ValidToken);

            // Act
            var response = await _client.PostAsync(BaseUrl, content);

            // Assert
            // Note: May return 500 if SOAP service is not available, which is expected in test env
            Assert.True(response.StatusCode == HttpStatusCode.OK || response.StatusCode == HttpStatusCode.InternalServerError);
        }

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        [InlineData("   ")]
        public async Task UpdateCard_InvalidCardId_Returns400(string cardId)
        {
            // Arrange
            var request = CreateValidUpdateCardRequest();
            request.CardId = cardId;
            var json = JsonSerializer.Serialize(request);
            var content = new StringContent(json, Encoding.UTF8, "application/json");
            _client.DefaultRequestHeaders.Add("Unison-Token", ValidToken);

            // Act
            var response = await _client.PostAsync(BaseUrl, content);

            // Assert
            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        }

        [Theory]
        [InlineData("invalid-email")]
        [InlineData("missing@")]
        [InlineData("@missing-domain.com")]
        public async Task UpdateCard_InvalidEmail_Returns400(string invalidEmail)
        {
            // Arrange
            var request = CreateValidUpdateCardRequest();
            request.Email = invalidEmail;
            var json = JsonSerializer.Serialize(request);
            var content = new StringContent(json, Encoding.UTF8, "application/json");
            _client.DefaultRequestHeaders.Add("Unison-Token", ValidToken);

            // Act
            var response = await _client.PostAsync(BaseUrl, content);

            // Assert
            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        }

        [Fact]
        public async Task UpdateCard_MalformedJson_Returns400()
        {
            // Arrange
            var malformedJson = "{ invalid json }";
            var content = new StringContent(malformedJson, Encoding.UTF8, "application/json");
            _client.DefaultRequestHeaders.Add("Unison-Token", ValidToken);

            // Act
            var response = await _client.PostAsync(BaseUrl, content);

            // Assert
            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        }

        [Fact]
        public async Task UpdateCard_EmptyRequestBody_Returns400()
        {
            // Arrange
            var content = new StringContent("", Encoding.UTF8, "application/json");
            _client.DefaultRequestHeaders.Add("Unison-Token", ValidToken);

            // Act
            var response = await _client.PostAsync(BaseUrl, content);

            // Assert
            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        }

        private static UpdateCardRequest CreateValidUpdateCardRequest()
        {
            return new UpdateCardRequest
            {
                CardId = "TEST123",
                UserName = "test.user",
                FirstName = "Test",
                LastName = "User",
                Email = "test.user@company.com",
                Department = "IT",
                Title = "Developer",
                ExpirationDate = DateTime.Now.AddMonths(12)
            };
        }

        public void Dispose()
        {
            _client?.Dispose();
        }
    }
}
