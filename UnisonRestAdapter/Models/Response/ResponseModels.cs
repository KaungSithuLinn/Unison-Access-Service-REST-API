namespace UnisonRestAdapter.Models.Response
{
    /// <summary>
    /// Response model for card update operations
    /// </summary>
    public class UpdateCardResponse
    {
        public bool Success { get; set; }

        public string Message { get; set; } = string.Empty;

        public string CardId { get; set; } = string.Empty;

        public DateTime Timestamp { get; set; } = DateTime.UtcNow;

        public string? TransactionId { get; set; }
    }

    /// <summary>
    /// Response model for user information
    /// </summary>
    public class UserResponse
    {
        public string UserId { get; set; } = string.Empty;

        public string? UserName { get; set; }

        public string? FirstName { get; set; }

        public string? LastName { get; set; }

        public string? Email { get; set; }

        public string? Department { get; set; }

        public string? Title { get; set; }

        public bool IsActive { get; set; }

        public bool Success { get; set; }

        public string? Message { get; set; }

        public DateTime Timestamp { get; set; } = DateTime.UtcNow;
    }

    /// <summary>
    /// Response model for health check operations
    /// </summary>
    public class HealthResponse
    {
        public bool IsHealthy { get; set; }

        public string Message { get; set; } = string.Empty;

        public DateTime Timestamp { get; set; } = DateTime.UtcNow;

        public string? ServiceVersion { get; set; }

        public Dictionary<string, object>? AdditionalInfo { get; set; }
    }
}
