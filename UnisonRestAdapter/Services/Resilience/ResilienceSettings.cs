namespace UnisonRestAdapter.Services.Resilience
{
    /// <summary>
    /// Configuration settings for resilience patterns (retry, circuit breaker, timeout)
    /// </summary>
    public class ResilienceSettings
    {
        /// <summary>
        /// Configuration section name
        /// </summary>
        public const string SectionName = "Resilience";

        /// <summary>
        /// Retry policy configuration
        /// </summary>
        public RetrySettings Retry { get; set; } = new();

        /// <summary>
        /// Circuit breaker policy configuration
        /// </summary>
        public CircuitBreakerSettings CircuitBreaker { get; set; } = new();

        /// <summary>
        /// Timeout policy configuration
        /// </summary>
        public TimeoutSettings Timeout { get; set; } = new();
    }

    /// <summary>
    /// Retry policy configuration
    /// </summary>
    public class RetrySettings
    {
        /// <summary>
        /// Maximum number of retry attempts
        /// </summary>
        public int MaxAttempts { get; set; } = 3;

        /// <summary>
        /// Base delay for exponential backoff in seconds
        /// </summary>
        public double BaseDelaySeconds { get; set; } = 1.0;

        /// <summary>
        /// Maximum delay between retries in seconds
        /// </summary>
        public double MaxDelaySeconds { get; set; } = 10.0;

        /// <summary>
        /// Enable jitter to avoid thundering herd
        /// </summary>
        public bool UseJitter { get; set; } = true;

        /// <summary>
        /// HTTP status codes that should trigger a retry
        /// </summary>
        public int[] RetriableStatusCodes { get; set; } = new[]
        {
            408, // Request Timeout
            429, // Too Many Requests
            500, // Internal Server Error
            502, // Bad Gateway
            503, // Service Unavailable
            504  // Gateway Timeout
        };
    }

    /// <summary>
    /// Circuit breaker policy configuration
    /// </summary>
    public class CircuitBreakerSettings
    {
        /// <summary>
        /// Duration to keep circuit open in seconds
        /// </summary>
        public double DurationOfBreakSeconds { get; set; } = 30.0;

        /// <summary>
        /// Number of exceptions allowed before opening circuit
        /// </summary>
        public int ExceptionsAllowedBeforeBreaking { get; set; } = 5;

        /// <summary>
        /// Sampling duration for failure threshold in seconds
        /// </summary>
        public double SamplingDurationSeconds { get; set; } = 60.0;

        /// <summary>
        /// Minimum number of actions through circuit before statistical significance
        /// </summary>
        public int MinimumThroughput { get; set; } = 10;
    }

    /// <summary>
    /// Timeout policy configuration
    /// </summary>
    public class TimeoutSettings
    {
        /// <summary>
        /// Request timeout in seconds
        /// </summary>
        public double RequestTimeoutSeconds { get; set; } = 30.0;

        /// <summary>
        /// Overall operation timeout in seconds (including retries)
        /// </summary>
        public double OverallTimeoutSeconds { get; set; } = 90.0;
    }
}
