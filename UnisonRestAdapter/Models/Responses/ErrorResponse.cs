using System.ComponentModel.DataAnnotations;

namespace UnisonRestAdapter.Models.Responses
{
    /// <summary>
    /// Structured error response model for consistent API error handling
    /// </summary>
    public class ErrorResponse
    {
        /// <summary>
        /// Error details
        /// </summary>
        [Required]
        public ErrorDetail Error { get; set; } = null!;

        /// <summary>
        /// Unique correlation ID for tracing this request across logs
        /// </summary>
        [Required]
        public string CorrelationId { get; set; } = null!;

        /// <summary>
        /// Timestamp when the error occurred (UTC)
        /// </summary>
        [Required]
        public DateTime Timestamp { get; set; }

        /// <summary>
        /// Request path that caused the error
        /// </summary>
        [Required]
        public string Path { get; set; } = null!;

        /// <summary>
        /// HTTP method used in the request
        /// </summary>
        [Required]
        public string Method { get; set; } = null!;
    }

    /// <summary>
    /// Detailed error information
    /// </summary>
    public class ErrorDetail
    {
        /// <summary>
        /// Error code for programmatic handling
        /// </summary>
        [Required]
        public string Code { get; set; } = null!;

        /// <summary>
        /// Error type classification
        /// </summary>
        [Required]
        public string Type { get; set; } = null!;

        /// <summary>
        /// Human-readable error message
        /// </summary>
        [Required]
        public string Message { get; set; } = null!;

        /// <summary>
        /// Additional error details (optional)
        /// </summary>
        public Dictionary<string, object>? Details { get; set; }
    }
}
