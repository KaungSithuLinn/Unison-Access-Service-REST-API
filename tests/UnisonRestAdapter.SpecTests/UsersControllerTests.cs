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
    public class UsersControllerTests : IClassFixture<TestFixture>
    {
        private readonly HttpClient _client;

        public UsersControllerTests(TestFixture fixture)
        {
            _client = fixture.CreateClient();
        }

        [Fact]
        public async Task GetUser_Unauthenticated_ReturnsUnauthorized()
        {
            var res = await _client.GetAsync("/api/users/testuser");
            res.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
        }

        [Fact]
        public async Task GetUser_WithValidToken_ReturnsUserInfo()
        {
            var message = new HttpRequestMessage(HttpMethod.Get, "/api/users/testuser");
            message.Headers.Add("Unison-Token", "595d799a-9553-4ddf-8fd9-c27b1f233ce7");

            var res = await _client.SendAsync(message);
            res.StatusCode.Should().Be(HttpStatusCode.OK);

            var body = await res.Content.ReadFromJsonAsync<UserResponse>();
            body.Should().NotBeNull();
            body!.Success.Should().BeTrue();
        }

        [Fact]
        public async Task GetUser_WithInvalidToken_ReturnsUnauthorized()
        {
            var message = new HttpRequestMessage(HttpMethod.Get, "/api/users/testuser");
            message.Headers.Add("Unison-Token", "invalid-token");

            var res = await _client.SendAsync(message);
            res.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
        }
    }
}
