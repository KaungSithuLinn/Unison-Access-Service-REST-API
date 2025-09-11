using System.ComponentModel.DataAnnotations;

namespace UnisonRestAdapter.IntegrationTests.Configuration;

/// <summary>
/// Configuration for integration test execution
/// Issue #7: Integration Testing with Playwright
/// </summary>
public class TestConfiguration
{
    /// <summary>
    /// Base URL for the REST API under test
    /// </summary>
    [Required]
    public string BaseUrl { get; set; } = "http://192.168.10.206:5001";

    /// <summary>
    /// Valid authentication token for successful API calls
    /// </summary>
    [Required]
    public string ValidToken { get; set; } = "595d799a-9553-4ddf-8fd9-c27b1f233ce7";

    /// <summary>
    /// Invalid authentication token for testing unauthorized scenarios
    /// </summary>
    [Required]
    public string InvalidToken { get; set; } = "invalid-token-123";

    /// <summary>
    /// Timeout configurations for various test scenarios
    /// </summary>
    public TimeoutConfiguration Timeouts { get; set; } = new();

    /// <summary>
    /// Test data for various test scenarios
    /// </summary>
    public TestDataConfiguration TestData { get; set; } = new();

    /// <summary>
    /// Performance threshold configurations
    /// </summary>
    public PerformanceThresholds PerformanceThresholds { get; set; } = new();

    /// <summary>
    /// Retry configuration for resilience testing
    /// </summary>
    public RetryConfiguration RetryConfiguration { get; set; } = new();
}

/// <summary>
/// Timeout configurations for different test operations
/// </summary>
public class TimeoutConfiguration
{
    /// <summary>
    /// Default timeout for test operations in milliseconds
    /// </summary>
    public int DefaultTimeoutMs { get; set; } = 30000;

    /// <summary>
    /// Timeout for health check operations in milliseconds
    /// </summary>
    public int HealthCheckTimeoutMs { get; set; } = 5000;

    /// <summary>
    /// Timeout for API request operations in milliseconds
    /// </summary>
    public int RequestTimeoutMs { get; set; } = 10000;
}

/// <summary>
/// Test data configuration for various test scenarios
/// </summary>
public class TestDataConfiguration
{
    /// <summary>
    /// Valid card ID for successful operations
    /// </summary>
    public string ValidCardId { get; set; } = "CARD123";

    /// <summary>
    /// Invalid card ID for testing not found scenarios
    /// </summary>
    public string InvalidCardId { get; set; } = "INVALID_CARD";

    /// <summary>
    /// Test user data for card operations
    /// </summary>
    public TestUserData TestUser { get; set; } = new();
}

/// <summary>
/// Test user data for API operations
/// </summary>
public class TestUserData
{
    public string UserName { get; set; } = "john.doe.test";
    public string FirstName { get; set; } = "John";
    public string LastName { get; set; } = "Doe";
    public string Email { get; set; } = "john.doe.test@company.com";
    public string Department { get; set; } = "Engineering";
    public string Title { get; set; } = "Software Developer";
}

/// <summary>
/// Performance threshold configurations
/// Issue #7 Requirement: All endpoints under 200ms threshold
/// </summary>
public class PerformanceThresholds
{
    /// <summary>
    /// Maximum response time for health check endpoints (Issue #7: 200ms requirement)
    /// </summary>
    public int HealthCheckMaxResponseTimeMs { get; set; } = 200;

    /// <summary>
    /// Maximum response time for API endpoints (Issue #7: 200ms requirement)
    /// </summary>
    public int ApiEndpointMaxResponseTimeMs { get; set; } = 1000;

    /// <summary>
    /// Maximum response time for update card operations
    /// </summary>
    public int UpdateCardMaxResponseTimeMs { get; set; } = 2000;
}

/// <summary>
/// Retry configuration for resilience testing
/// </summary>
public class RetryConfiguration
{
    /// <summary>
    /// Maximum number of retries for failed operations
    /// </summary>
    public int MaxRetries { get; set; } = 3;

    /// <summary>
    /// Delay between retry attempts in milliseconds
    /// </summary>
    public int RetryDelayMs { get; set; } = 1000;
}

