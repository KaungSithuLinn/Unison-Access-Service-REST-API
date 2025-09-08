using System.ComponentModel.DataAnnotations;

namespace UnisonRestAdapter.Models.Request
{
    /// <summary>
    /// Request model for updating card information
    /// </summary>
    public class UpdateCardRequest
    {
        [Required]
        public string CardId { get; set; } = string.Empty;

        public string? UserName { get; set; }

        public string? FirstName { get; set; }

        public string? LastName { get; set; }

        public string? Email { get; set; }

        public string? Department { get; set; }

        public string? Title { get; set; }

        public bool? IsActive { get; set; }

        public DateTime? ExpirationDate { get; set; }

        /// <summary>
        /// Additional custom fields for card update
        /// </summary>
        public Dictionary<string, string>? CustomFields { get; set; }
    }
}
