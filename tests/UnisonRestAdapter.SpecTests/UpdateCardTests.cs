using System;
using System.Net;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;
using FluentAssertions;
using UnisonRestAdapter.Models.Request;
using UnisonRestAdapter.Models.Response;
using Xunit;

namespace UnisonRestAdapter.SpecTests
{
    public class UpdateCardTests : IClassFixture<TestFixture>
    {
        private readonly HttpClient _client;

        public UpdateCardTests(TestFixture factory)
        {
            _client = factory.CreateClient();
        }

        [Fact]
        public async Task Ping_ReturnsHealthy()
        {
            var res = await _client.GetAsync("/api/health/ping");
            res.StatusCode.Should().Be(HttpStatusCode.OK);
            var body = await res.Content.ReadFromJsonAsync<object>();
            body.Should().NotBeNull();
        }

        [Fact]
        public async Task UpdateCard_Unauthenticated_ReturnsUnauthorized()
        {
            var req = new UpdateCardRequest { CardId = "TEST123" };
            var res = await _client.PutAsJsonAsync("/api/cards/update", req);
            res.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
        }

        [Fact]
        public async Task UpdateCard_WithToken_BackendHtmlError_ReturnsStructuredJson()
        {
            var req = new UpdateCardRequest { CardId = "SERVER_TEST_001" };
            var message = new HttpRequestMessage(HttpMethod.Put, "/api/cards/update")
            {
                Content = JsonContent.Create(req)
            };
            message.Headers.Add("Unison-Token", "test-token");

            var res = await _client.SendAsync(message);
            // Fake service returns BadRequest in controller for success=false
            res.StatusCode.Should().Be(HttpStatusCode.BadRequest);

            var body = await res.Content.ReadFromJsonAsync<UpdateCardResponse>();
            body.Should().NotBeNull();
            body!.Success.Should().BeFalse();
            body.Message.Should().Contain("HTTP Error");
            body.CardId.Should().Be(req.CardId);
        }
    }
}
