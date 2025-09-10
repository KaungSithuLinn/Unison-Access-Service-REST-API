using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using System.Net.Http;
using Xunit;
using System.Threading.Tasks;
using System.Net;

namespace UnisonRestAdapter.Tests.Controllers
{
    /// <summary>
    /// Unit tests for HealthController
    /// Tests TASK-004: Comprehensive Test Suite - Health Check endpoint
    /// </summary>
    public class HealthControllerTests : IClassFixture<WebApplicationFactory<Program>>
    {
        private readonly WebApplicationFactory<Program> _factory;
        private readonly HttpClient _client;

        public HealthControllerTests(WebApplicationFactory<Program> factory)
        {
            _factory = factory;
            _client = _factory.CreateClient();
        }

        [Fact]
        public async Task HealthCheck_Returns_OK_Status()
        {
            // Act
            var response = await _client.GetAsync("/health");

            // Assert
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            var content = await response.Content.ReadAsStringAsync();
            Assert.Contains("Healthy", content);
        }

        [Fact]
        public async Task HealthCheck_Returns_Correct_Content_Type()
        {
            // Act
            var response = await _client.GetAsync("/health");

            // Assert
            Assert.Equal("text/plain", response.Content.Headers.ContentType?.MediaType);
        }

        [Fact]
        public async Task HealthCheck_Multiple_Requests_All_Succeed()
        {
            // Arrange
            var tasks = new Task[5];

            // Act - Fire 5 concurrent health check requests
            for (int i = 0; i < 5; i++)
            {
                tasks[i] = _client.GetAsync("/health");
            }
            var responses = await Task.WhenAll(tasks);

            // Assert
            foreach (var response in responses.Cast<HttpResponseMessage>())
            {
                Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            }
        }

        public void Dispose()
        {
            _client?.Dispose();
        }
    }
}
