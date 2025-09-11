using Microsoft.Extensions.Configuration;
using Microsoft.Playwright;
using UnisonRestAdapter.IntegrationTests.Configuration;
using System.Diagnostics;

namespace UnisonRestAdapter.IntegrationTests.Infrastructure;

/// <summary>
/// Base class for all Playwright API integration tests
/// Issue #7: Integration Testing with Playwright
/// 
/// Provides common setup, configuration, and utilities for testing
/// the Unison REST Adapter endpoints with comprehensive validation.
/// </summary>
[TestFixture]
public abstract class PlaywrightTestBase
{
    protected IAPIRequestContext ApiContext = null!;
    protected IPlaywright Playwright = null!;
    protected TestConfiguration Config = null!;

    /// <summary>
    /// Base URL for all API requests
    /// </summary>
    protected string BaseUrl => Config.BaseUrl;

    /// <summary>
    /// Valid authentication token for API requests
    /// </summary>
    protected string ValidToken => Config.ValidToken;

    /// <summary>
    /// Invalid authentication token for testing unauthorized scenarios
    /// </summary>
    protected string InvalidToken => Config.InvalidToken;

    [OneTimeSetUp]
    public async Task GlobalSetup()
    {
        // Load configuration from appsettings.json
        var configuration = new ConfigurationBuilder()
            .SetBasePath(Directory.GetCurrentDirectory())
            .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
            .Build();

        Config = new TestConfiguration();
        configuration.GetSection("TestConfiguration").Bind(Config);

        // Initialize Playwright
        Microsoft.Playwright.Program.Main(new[] { "install" });
        Playwright = await Microsoft.Playwright.Playwright.CreateAsync();

        // Create API request context with common configuration
        ApiContext = await Playwright.APIRequest.NewContextAsync(new APIRequestNewContextOptions
        {
            BaseURL = BaseUrl,
            IgnoreHTTPSErrors = true,
            ExtraHTTPHeaders = new Dictionary<string, string>
            {
                ["Content-Type"] = "application/json",
                ["Accept"] = "application/json",
                ["User-Agent"] = "UnisonRestAdapter.IntegrationTests/1.0"
            }
        });

        Console.WriteLine($"Playwright integration tests initialized. Base URL: {BaseUrl}");
    }

    [OneTimeTearDown]
    public async Task GlobalTeardown()
    {
        if (ApiContext != null)
        {
            await ApiContext.DisposeAsync();
        }

        if (Playwright != null)
        {
            Playwright.Dispose();
        }

        Console.WriteLine("Playwright integration tests cleanup completed.");
    }

    /// <summary>
    /// Creates API request headers with authentication token
    /// </summary>
    /// <param name="token">Authentication token (defaults to ValidToken)</param>
    /// <returns>Headers dictionary for API requests</returns>
    protected Dictionary<string, string> CreateAuthHeaders(string? token = null)
    {
        return new Dictionary<string, string>
        {
            ["Unison-Token"] = token ?? ValidToken,
            ["Content-Type"] = "application/json"
        };
    }

    /// <summary>
    /// Measures response time of an API operation
    /// </summary>
    /// <param name="operation">The API operation to measure</param>
    /// <returns>Tuple containing the response and elapsed time in milliseconds</returns>
    protected async Task<(IAPIResponse Response, long ElapsedMs)> MeasureResponseTime(Func<Task<IAPIResponse>> operation)
    {
        var stopwatch = Stopwatch.StartNew();
        var response = await operation();
        stopwatch.Stop();

        Console.WriteLine($"API operation completed in {stopwatch.ElapsedMilliseconds}ms");
        return (response, stopwatch.ElapsedMilliseconds);
    }

    /// <summary>
    /// Asserts that a response meets performance threshold requirements
    /// Issue #7 Requirement: All endpoints under 200ms threshold (health) or 1000ms (other endpoints)
    /// </summary>
    /// <param name="elapsedMs">Actual response time in milliseconds</param>
    /// <param name="thresholdMs">Maximum allowed response time in milliseconds</param>
    /// <param name="endpointName">Name of the endpoint being tested (for logging)</param>
    protected void AssertPerformanceThreshold(long elapsedMs, int thresholdMs, string endpointName)
    {
        if (elapsedMs <= thresholdMs)
        {
            Console.WriteLine($"✅ Performance: {endpointName} responded in {elapsedMs}ms (threshold: {thresholdMs}ms)");
        }
        else
        {
            Console.WriteLine($"⚠️ Performance: {endpointName} responded in {elapsedMs}ms (exceeds threshold: {thresholdMs}ms)");
        }

        Assert.That(elapsedMs, Is.LessThanOrEqualTo(thresholdMs),
            $"Endpoint {endpointName} response time ({elapsedMs}ms) exceeds threshold ({thresholdMs}ms)");
    }

    /// <summary>
    /// Validates response status code and logs result
    /// </summary>
    /// <param name="response">API response to validate</param>
    /// <param name="expectedStatus">Expected HTTP status code</param>
    /// <param name="endpointName">Name of the endpoint being tested</param>
    protected async Task AssertResponseStatus(IAPIResponse response, int expectedStatus, string endpointName)
    {
        var actualStatus = response.Status;
        var responseText = await response.TextAsync();

        if (actualStatus == expectedStatus)
        {
            Console.WriteLine($"✅ Status: {endpointName} returned expected status {expectedStatus}");
        }
        else
        {
            Console.WriteLine($"❌ Status: {endpointName} returned status {actualStatus}, expected {expectedStatus}. Response: {responseText}");
        }

        Assert.That(actualStatus, Is.EqualTo(expectedStatus),
            $"Endpoint {endpointName} returned status {actualStatus}, expected {expectedStatus}. Response: {responseText}");
    }

    /// <summary>
    /// Validates that response contains valid JSON and returns parsed content
    /// </summary>
    /// <param name="response">API response to validate</param>
    /// <param name="endpointName">Name of the endpoint being tested</param>
    /// <returns>Parsed JSON response content</returns>
    protected async Task<string> AssertValidJsonResponse(IAPIResponse response, string endpointName)
    {
        var responseText = await response.TextAsync();
        Assert.That(string.IsNullOrEmpty(responseText), Is.False,
            $"Endpoint {endpointName} returned empty response");

        try
        {
            // Validate JSON by attempting to parse
            var jsonObject = Newtonsoft.Json.JsonConvert.DeserializeObject(responseText);
            Assert.That(jsonObject, Is.Not.Null,
                $"Endpoint {endpointName} returned invalid JSON");

            Console.WriteLine($"✅ JSON: {endpointName} returned valid JSON response");
            return responseText;
        }
        catch (Newtonsoft.Json.JsonException ex)
        {
            Console.WriteLine($"❌ JSON: {endpointName} returned invalid JSON. Response: {responseText}, Error: {ex.Message}");

            Assert.Fail($"Endpoint {endpointName} returned invalid JSON: {ex.Message}. Response: {responseText}");
            return string.Empty; // This will never execute due to Assert.Fail
        }
    }

    /// <summary>
    /// Creates test data for UpdateCard request
    /// </summary>
    /// <param name="cardId">Card ID (defaults to ValidCardId from config)</param>
    /// <returns>UpdateCard request object</returns>
    protected object CreateUpdateCardRequest(string? cardId = null)
    {
        return new
        {
            cardId = cardId ?? Config.TestData.ValidCardId,
            userName = Config.TestData.TestUser.UserName,
            firstName = Config.TestData.TestUser.FirstName,
            lastName = Config.TestData.TestUser.LastName,
            email = Config.TestData.TestUser.Email,
            department = Config.TestData.TestUser.Department,
            title = Config.TestData.TestUser.Title,
            isActive = true,
            expirationDate = DateTime.UtcNow.AddYears(1).ToString("yyyy-MM-ddTHH:mm:ssZ")
        };
    }

    /// <summary>
    /// Retry logic for operations that might fail transiently
    /// Implements Issue #7 requirement for >90% error condition coverage
    /// </summary>
    /// <param name="operation">Operation to retry</param>
    /// <param name="maxRetries">Maximum retry attempts (defaults to config value)</param>
    /// <param name="delay">Delay between retries (defaults to config value)</param>
    /// <returns>Result of the successful operation</returns>
    protected async Task<T> WithRetry<T>(Func<Task<T>> operation, int? maxRetries = null, int? delay = null)
    {
        var retries = maxRetries ?? Config.RetryConfiguration.MaxRetries;
        var retryDelay = delay ?? Config.RetryConfiguration.RetryDelayMs;

        Exception lastException = new InvalidOperationException("Retry operation failed");

        for (int attempt = 1; attempt <= retries; attempt++)
        {
            try
            {
                var result = await operation();
                if (attempt > 1)
                {
                    Console.WriteLine("✅ Retry: Operation succeeded on attempt {Attempt}/{MaxRetries}", attempt, retries);
                }
                return result;
            }
            catch (Exception ex)
            {
                lastException = ex;
                Console.WriteLine("Logging statement");

                if (attempt < retries)
                {
                    await Task.Delay(retryDelay);
                }
            }
        }

        Console.WriteLine("Logging statement");
        throw lastException;
    }
}

