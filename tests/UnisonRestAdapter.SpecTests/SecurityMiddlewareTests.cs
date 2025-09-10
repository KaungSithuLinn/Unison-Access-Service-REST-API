#nullable enable
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using FluentAssertions;
using Xunit;

namespace UnisonRestAdapter.SpecTests
{
    [Collection("Integration")]
    public class SecurityMiddlewareTests : IClassFixture<TestFixture>
    {
        private readonly HttpClient _client;

        public SecurityMiddlewareTests(TestFixture fixture)
        {
            _client = fixture.CreateClient();
        }

        [Fact]
        public async Task Request_WithoutToken_ReturnsUnauthorized()
        {
            var res = await _client.GetAsync("/api/health");
            res.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
        }

        [Fact]
        public async Task Request_WithValidToken_ProcessesRequest()
        {
            var message = new HttpRequestMessage(HttpMethod.Get, "/api/health");
            message.Headers.Add("Unison-Token", "595d799a-9553-4ddf-8fd9-c27b1f233ce7");

            var res = await _client.SendAsync(message);
            res.StatusCode.Should().Be(HttpStatusCode.OK);
        }

        [Fact]
        public async Task Request_WithInvalidToken_UsesFallbackInTestEnvironment()
        {
            // In test environment with AllowFallbackToken=true, invalid tokens fall back to valid token
            var message = new HttpRequestMessage(HttpMethod.Get, "/api/health");
            message.Headers.Add("Unison-Token", "invalid-token-12345");

            var res = await _client.SendAsync(message);
            res.StatusCode.Should().Be(HttpStatusCode.OK);
        }

        [Fact]
        public async Task Request_WithEmptyToken_ReturnsUnauthorized()
        {
            // Empty tokens are correctly rejected by middleware
            var message = new HttpRequestMessage(HttpMethod.Get, "/api/health");
            message.Headers.Add("Unison-Token", "");

            var res = await _client.SendAsync(message);
            res.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
        }

        [Fact]
        public async Task Request_WithNullToken_UsesFallbackInTestEnvironment()
        {
            // In test environment with AllowFallbackToken=true, null tokens fall back to valid token
            var message = new HttpRequestMessage(HttpMethod.Get, "/api/health");
            message.Headers.Add("Unison-Token", "null");

            var res = await _client.SendAsync(message);
            res.StatusCode.Should().Be(HttpStatusCode.OK);
        }

        [Theory]
        [InlineData("Authorization")] // Wrong header name
        public async Task Request_WithWrongHeaderName_ReturnsUnauthorized(string headerName)
        {
            var message = new HttpRequestMessage(HttpMethod.Get, "/api/health");
            message.Headers.Add(headerName, "595d799a-9553-4ddf-8fd9-c27b1f233ce7");

            var res = await _client.SendAsync(message);
            res.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
        }

        [Theory]
        [InlineData("UNISON-TOKEN")] // Different case - should work (HTTP headers are case insensitive)
        [InlineData("unison-token")] // Lower case - should work (HTTP headers are case insensitive)
        public async Task Request_WithCaseVariantHeaders_WorksCorrectly(string headerName)
        {
            var message = new HttpRequestMessage(HttpMethod.Get, "/api/health");
            message.Headers.Add(headerName, "595d799a-9553-4ddf-8fd9-c27b1f233ce7");

            var res = await _client.SendAsync(message);
            res.StatusCode.Should().Be(HttpStatusCode.OK);
        }
    }
}
