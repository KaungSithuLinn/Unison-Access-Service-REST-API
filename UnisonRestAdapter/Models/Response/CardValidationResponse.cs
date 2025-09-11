using System.Text.Json.Serialization;

namespace UnisonRestAdapter.Models.Response
{
    /// <summary>
    /// Response model for card validation operation
    /// </summary>
    public class CardValidationResponse
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
        /// Card identifier that was validated
        /// </summary>
        [JsonPropertyName("cardId")]
        public string CardId { get; set; } = string.Empty;

        /// <summary>
        /// Card validation status
        /// </summary>
        [JsonPropertyName("isValid")]
        public bool IsValid { get; set; }

        /// <summary>
        /// Card status information
        /// </summary>
        [JsonPropertyName("cardStatus")]
        public string? CardStatus { get; set; }

        /// <summary>
        /// Additional validation details
        /// </summary>
        [JsonPropertyName("validationDetails")]
        public Dictionary<string, object>? ValidationDetails { get; set; }

        /// <summary>
        /// Response timestamp
        /// </summary>
        [JsonPropertyName("timestamp")]
        public DateTime Timestamp { get; set; } = DateTime.UtcNow;
    }
}
