using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace UnisonRestAdapter.Models.Request
{
    /// <summary>
    /// Request model for card activation operation
    /// </summary>
    public class CardActivationRequest
    {
        /// <summary>
        /// Card identifier to activate
        /// </summary>
        [Required(ErrorMessage = "CardId is required")]
        [JsonPropertyName("cardId")]
        public string CardId { get; set; } = string.Empty;

        /// <summary>
        /// User identifier for activation
        /// </summary>
        [Required(ErrorMessage = "UserId is required")]
        [JsonPropertyName("userId")]
        public string UserId { get; set; } = string.Empty;

        /// <summary>
        /// Profile name for activation
        /// </summary>
        [JsonPropertyName("profileName")]
        public string? ProfileName { get; set; }
    }
}
