namespace UnisonRestAdapter.Models.Response
{
    /// <summary>
    /// Response model for card update operations
    /// </summary>
    public class UpdateCardResponse
    {
        /// <summary>
        /// Indicates whether the update operation was successful
        /// </summary>
        /// <example>true</example>
        public bool Success { get; set; }

        /// <summary>
        /// Human-readable message describing the result
        /// </summary>
        /// <example>Card updated successfully</example>
        public string Message { get; set; } = string.Empty;

        /// <summary>
        /// The card ID that was updated
        /// </summary>
        /// <example>CARD123</example>
        public string CardId { get; set; } = string.Empty;

        /// <summary>
        /// Timestamp when the operation was completed
        /// </summary>
        /// <example>2025-01-05T10:30:00Z</example>
        public DateTime Timestamp { get; set; } = DateTime.UtcNow;

        /// <summary>
        /// Unique identifier for this transaction
        /// </summary>
        /// <example>TXN-20250105-103000-CARD123</example>
        public string? TransactionId { get; set; }
    }

    /// <summary>
    /// Response model for user information
    /// </summary>
    public class UserResponse
    {
        /// <summary>
        /// Unique identifier for the user
        /// </summary>
        /// <example>USER123</example>
        public string UserId { get; set; } = string.Empty;

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
        /// Whether the user is active
        /// </summary>
        /// <example>true</example>
        public bool IsActive { get; set; }

        /// <summary>
        /// Indicates whether the operation was successful
        /// </summary>
        /// <example>true</example>
        public bool Success { get; set; }

        /// <summary>
        /// Human-readable message describing the result
        /// </summary>
        /// <example>User information retrieved successfully</example>
        public string? Message { get; set; }

        /// <summary>
        /// Timestamp when the operation was completed
        /// </summary>
        /// <example>2025-01-05T10:30:00Z</example>
        public DateTime Timestamp { get; set; } = DateTime.UtcNow;
    }

    /// <summary>
    /// Response model for health check operations
    /// Enhanced for TASK-005: Setup Continuous Endpoint Monitoring
    /// </summary>
    public class HealthResponse
    {
        /// <summary>
        /// Indicates whether the service is healthy
        /// </summary>
        /// <example>true</example>
        public bool IsHealthy { get; set; }

        /// <summary>
        /// Human-readable message describing the health status
        /// </summary>
        /// <example>Service is healthy and all dependencies are operational</example>
        public string Message { get; set; } = string.Empty;

        /// <summary>
        /// Timestamp when the health check was performed
        /// </summary>
        /// <example>2025-01-05T10:30:00Z</example>
        public DateTime Timestamp { get; set; } = DateTime.UtcNow;

        /// <summary>
        /// Version of the service
        /// </summary>
        /// <example>1.0.0</example>
        public string? ServiceVersion { get; set; }

        /// <summary>
        /// Additional health information including system metrics
        /// </summary>
        /// <example>{"uptime": "2 days", "memoryUsage": "45 MB"}</example>
        public Dictionary<string, object>? AdditionalInfo { get; set; }

        /// <summary>
        /// Response time for the health check operation in milliseconds
        /// </summary>
        /// <example>25</example>
        public long? ResponseTime { get; set; }
    }
}
