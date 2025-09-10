#nullable enable
using System.Net;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;
using FluentAssertions;
using UnisonRestAdapter.Models.Response;
using Xunit;

namespace UnisonRestAdapter.SpecTests
{
    [Collection("Integration")]
    public class HealthControllerTests : IClassFixture<TestFixture>
    {
        private readonly HttpClient _client;

        public HealthControllerTests(TestFixture fixture)
        {
            _client = fixture.CreateClient();
        }

        [Fact]
        public async Task GetHealth_Unauthenticated_ReturnsUnauthorized()
        {
            var res = await _client.GetAsync("/api/health");
            res.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
        }

        [Fact]
        public async Task GetHealth_WithValidToken_ReturnsHealthStatus()
        {
            var message = new HttpRequestMessage(HttpMethod.Get, "/api/health");
            message.Headers.Add("Unison-Token", "595d799a-9553-4ddf-8fd9-c27b1f233ce7");

            var res = await _client.SendAsync(message);
            res.StatusCode.Should().Be(HttpStatusCode.OK);

            var body = await res.Content.ReadFromJsonAsync<HealthResponse>();
            body.Should().NotBeNull();
            body!.IsHealthy.Should().BeTrue();
            body.Message.Should().NotBeNullOrEmpty();
        }

        [Fact]
        public async Task GetHealth_WithInvalidToken_UsesFallbackInTestEnvironment()
        {
            // In test environment with AllowFallbackToken=true, invalid tokens fall back to valid token
            var message = new HttpRequestMessage(HttpMethod.Get, "/api/health");
            message.Headers.Add("Unison-Token", "invalid-token");

            var res = await _client.SendAsync(message);
            res.StatusCode.Should().Be(HttpStatusCode.OK);

            var body = await res.Content.ReadFromJsonAsync<HealthResponse>();
            body.Should().NotBeNull();
            body!.IsHealthy.Should().BeTrue();
        }
    }
}
