using System.Text.Json.Serialization;

namespace UnisonRestAdapter.Models.Response
{
    /// <summary>
    /// Response model for card activation operation
    /// </summary>
    public class CardActivationResponse
    {
        /// <summary>
        /// Indicates if the operation was successful
        /// </summary>
        [JsonPropertyName("success")]
        public bool Success { get; set; }

        /// <summary>
        /// Operation message
        /// </summary>
        [JsonPropertyName("message")]
        public string Message { get; set; } = string.Empty;

        /// <summary>
        /// Card identifier that was processed
        /// </summary>
        [JsonPropertyName("cardId")]
        public string CardId { get; set; } = string.Empty;

        /// <summary>
        /// User identifier associated with the card
        /// </summary>
        [JsonPropertyName("userId")]
        public string UserId { get; set; } = string.Empty;

        /// <summary>
        /// New card status after activation/deactivation
        /// </summary>
        [JsonPropertyName("newStatus")]
        public string? NewStatus { get; set; }

        /// <summary>
        /// Response timestamp
        /// </summary>
        [JsonPropertyName("timestamp")]
        public DateTime Timestamp { get; set; } = DateTime.UtcNow;
    }
}
