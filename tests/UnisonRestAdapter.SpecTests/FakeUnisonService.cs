using System;
using System.Threading.Tasks;
using UnisonRestAdapter.Models.Request;
using UnisonRestAdapter.Models.Response;
using UnisonRestAdapter.Services;

namespace UnisonRestAdapter.SpecTests
{
    public class FakeUnisonService : IUnisonService
    {
        public Task<UpdateCardResponse> UpdateCardAsync(UpdateCardRequest request, string token)
        {
            // Simulate backend HTML error converted to structured response
            return Task.FromResult(new UpdateCardResponse
            {
                Success = false,
                Message = "HTTP Error: [HTML error from SOAP backend]",
                CardId = request.CardId,
                TransactionId = null
            });
        }

        public Task<UserResponse> GetUserAsync(string userId, string token)
        {
            return Task.FromResult(new UserResponse
            {
                UserId = userId,
                Success = true,
                IsActive = true
            });
        }

        public Task<HealthResponse> CheckHealthAsync(string token)
        {
            return Task.FromResult(new HealthResponse
            {
                IsHealthy = true,
                Message = "Service is healthy",
            });
        }
    }
}
