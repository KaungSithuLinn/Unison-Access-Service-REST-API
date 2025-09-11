using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace UnisonRestAdapter.Models.Request
{
    /// <summary>
    /// Request model for card validation operation
    /// </summary>
    public class CardValidationRequest
    {
        /// <summary>
        /// Card identifier to validate
        /// </summary>
        [Required(ErrorMessage = "CardId is required")]
        [JsonPropertyName("cardId")]
        public string CardId { get; set; } = string.Empty;

        /// <summary>
        /// User identifier for validation context
        /// </summary>
        [JsonPropertyName("userId")]
        public string? UserId { get; set; }

        /// <summary>
        /// Profile name for validation
        /// </summary>
        [JsonPropertyName("profileName")]
        public string? ProfileName { get; set; }
    }
}
