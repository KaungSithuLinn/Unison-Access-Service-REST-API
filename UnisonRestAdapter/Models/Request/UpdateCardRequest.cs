using System.ComponentModel.DataAnnotations;

namespace UnisonRestAdapter.Models.Request
{
    /// <summary>
    /// Request model for updating card information
    /// </summary>
    public class UpdateCardRequest
    {
        /// <summary>
        /// Unique identifier for the card (required, 1-50 characters, alphanumeric)
        /// </summary>
        /// <example>CARD123</example>
        [Required]
        public string CardId { get; set; } = string.Empty;

        /// <summary>
        /// User's login name
        /// </summary>
        /// <example>john.doe</example>
        public string? UserName { get; set; }

        /// <summary>
        /// User's first name
        /// </summary>
        /// <example>John</example>
        public string? FirstName { get; set; }

        /// <summary>
        /// User's last name
        /// </summary>
        /// <example>Doe</example>
        public string? LastName { get; set; }

        /// <summary>
        /// User's email address
        /// </summary>
        /// <example>john.doe@company.com</example>
        public string? Email { get; set; }

        /// <summary>
        /// User's department
        /// </summary>
        /// <example>Engineering</example>
        public string? Department { get; set; }

        /// <summary>
        /// User's job title
        /// </summary>
        /// <example>Software Developer</example>
        public string? Title { get; set; }

        /// <summary>
        /// Whether the card is active
        /// </summary>
        /// <example>true</example>
        public bool? IsActive { get; set; }

        /// <summary>
        /// Card expiration date
        /// </summary>
        /// <example>2025-12-31T00:00:00Z</example>
        public DateTime? ExpirationDate { get; set; }

        /// <summary>
        /// Additional custom fields for card update
        /// </summary>
        /// <example>{"building": "Building A", "floor": "3"}</example>
        public Dictionary<string, string>? CustomFields { get; set; }
    }
}
