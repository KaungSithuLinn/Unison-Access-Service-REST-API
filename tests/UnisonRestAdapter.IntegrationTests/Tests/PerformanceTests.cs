using UnisonRestAdapter.IntegrationTests.Infrastructure;
using FluentAssertions;
using Microsoft.Playwright;
using System.Diagnostics;

namespace UnisonRestAdapter.IntegrationTests.Tests;

/// <summary>
/// Integration tests for Performance and Load Testing
/// Issue #7: Integration Testing with Playwright
/// 
/// Tests system performance under various load conditions:
/// - Response time thresholds (Health: 200ms, Other: 1000ms)
/// - Concurrent request handling
/// - Load sustainability
/// - Resource utilization patterns
/// - Throughput measurements
/// 
/// Performance Requirements from Issue #7:
/// - Health endpoints: under 200ms response time
/// - Other endpoints: under 1000ms response time
/// - System should maintain performance under concurrent load
/// </summary>
[TestFixture]
public class PerformanceTests : PlaywrightTestBase
{
    private const int HealthPerformanceThresholdMs = 200;
    private const int ApiPerformanceThresholdMs = 1000;
    private const int LoadTestConcurrency = 20;
    private const int SustainabilityTestDurationMs = 30000; // 30 seconds

    [Test]
    [Description("Tests individual endpoint response times under no-load conditions")]
    public async Task Performance_BaselineResponseTimes_ShouldMeetThresholds()
    {
        // Arrange - All endpoints with their expected thresholds
        var performanceTestCases = new[]
        {
            new { Endpoint = "/health", Method = "GET", Headers = new Dictionary<string, string>(), Data = "", ThresholdMs = HealthPerformanceThresholdMs, Description = "Health Basic" },
            new { Endpoint = "/health/liveness", Method = "GET", Headers = new Dictionary<string, string>(), Data = "", ThresholdMs = HealthPerformanceThresholdMs, Description = "Health Liveness" },
            new { Endpoint = "/health/readiness", Method = "GET", Headers = new Dictionary<string, string>(), Data = "", ThresholdMs = HealthPerformanceThresholdMs, Description = "Health Readiness" },
            new { Endpoint = "/api/cards/update", Method = "PUT", Headers = CreateAuthHeaders(), Data = Newtonsoft.Json.JsonConvert.SerializeObject(CreateUpdateCardRequest()), ThresholdMs = ApiPerformanceThresholdMs, Description = "UpdateCard" },
            new { Endpoint = $"/api/cards/{Config.TestData.ValidCardId}", Method = "GET", Headers = CreateAuthHeaders(), Data = "", ThresholdMs = ApiPerformanceThresholdMs, Description = "GetCard" },
            new { Endpoint = "/api/cards/validate", Method = "POST", Headers = CreateAuthHeaders(), Data = Newtonsoft.Json.JsonConvert.SerializeObject(new { cardId = Config.TestData.ValidCardId, userName = Config.TestData.TestUser.UserName }), ThresholdMs = ApiPerformanceThresholdMs, Description = "ValidateCard" }
        };

        var performanceResults = new List<(string Description, long ElapsedMs, bool MetThreshold)>();

        // Act - Test each endpoint's baseline performance
        foreach (var testCase in performanceTestCases)
        {
            // Warm up with a single request first
            await ExecuteRequest(testCase.Method, testCase.Endpoint, testCase.Headers, testCase.Data);

            // Measure actual performance
            var (response, elapsedMs) = await MeasureResponseTime(async () =>
                await ExecuteRequest(testCase.Method, testCase.Endpoint, testCase.Headers, testCase.Data)
            );

            // Assert - Response should be successful
            Assert.That(response.Status, Is.LessThan(300),
                $"Baseline performance test for {testCase.Description} should return successful status");

            // Assert - Performance threshold
            var metThreshold = elapsedMs <= testCase.ThresholdMs;
            performanceResults.Add((testCase.Description, elapsedMs, metThreshold));

            if (metThreshold)
            {
                Console.WriteLine("Logging statement");
            }
            else
            {
                Console.WriteLine("Logging statement");
            }

            AssertPerformanceThreshold(elapsedMs, testCase.ThresholdMs, testCase.Description);
        }

        // Summary
        var totalEndpoints = performanceResults.Count;
        var endpointsMeetingThreshold = performanceResults.Count(r => r.MetThreshold);
        var averageResponseTime = performanceResults.Average(r => r.ElapsedMs);

        Console.WriteLine("Logging statement");

        Assert.That(endpointsMeetingThreshold, Is.EqualTo(totalEndpoints),
            "All endpoints should meet their baseline performance thresholds");
    }

    [Test]
    [Description("Tests system performance under concurrent load")]
    public async Task Performance_ConcurrentLoad_ShouldMaintainResponseTimes()
    {
        // Arrange - Mixed workload of different endpoints
        var concurrentTestCases = new[]
        {
            new { Endpoint = "/health", Method = "GET", Headers = new Dictionary<string, string>(), Data = "", ThresholdMs = HealthPerformanceThresholdMs, Weight = 40 }, // 40% health checks
            new { Endpoint = "/api/cards/update", Method = "PUT", Headers = CreateAuthHeaders(), Data = Newtonsoft.Json.JsonConvert.SerializeObject(CreateUpdateCardRequest()), ThresholdMs = ApiPerformanceThresholdMs, Weight = 20 }, // 20% updates
            new { Endpoint = $"/api/cards/{Config.TestData.ValidCardId}", Method = "GET", Headers = CreateAuthHeaders(), Data = "", ThresholdMs = ApiPerformanceThresholdMs, Weight = 30 }, // 30% gets
            new { Endpoint = "/api/cards/validate", Method = "POST", Headers = CreateAuthHeaders(), Data = Newtonsoft.Json.JsonConvert.SerializeObject(new { cardId = Config.TestData.ValidCardId, userName = Config.TestData.TestUser.UserName }), ThresholdMs = ApiPerformanceThresholdMs, Weight = 10 } // 10% validations
        };

        // Act - Execute concurrent requests
        var tasks = Enumerable.Range(0, LoadTestConcurrency).Select(async i =>
        {
            // Select test case based on weight distribution
            var weightedSelection = (i * 100 / LoadTestConcurrency);
            var testCase = weightedSelection switch
            {
                < 40 => concurrentTestCases[0], // Health
                < 60 => concurrentTestCases[1], // Update
                < 90 => concurrentTestCases[2], // Get
                _ => concurrentTestCases[3]     // Validate
            };

            var (response, elapsedMs) = await MeasureResponseTime(async () =>
                await ExecuteRequest(testCase.Method, testCase.Endpoint, testCase.Headers, testCase.Data)
            );

            return new
            {
                Response = response,
                ElapsedMs = elapsedMs,
                TestCase = testCase,
                RequestIndex = i,
                MetThreshold = elapsedMs <= testCase.ThresholdMs
            };
        });

        var results = await Task.WhenAll(tasks);

        // Assert - All requests should complete successfully
        foreach (var result in results)
        {
            Assert.That(result.Response.Status, Is.LessThan(300),
                $"Concurrent request #{result.RequestIndex} should complete successfully");
        }

        // Assert - Performance thresholds under load
        var performanceViolations = results.Where(r => !r.MetThreshold).ToList();

        foreach (var violation in performanceViolations)
        {
            Console.WriteLine("Logging statement");
        }

        // Allow some tolerance under concurrent load (e.g., 90% should meet thresholds)
        var tolerancePercentage = 90;
        var acceptableViolations = LoadTestConcurrency * (100 - tolerancePercentage) / 100;

        Assert.That(performanceViolations.Count, Is.LessThanOrEqualTo(acceptableViolations),
            $"Under concurrent load, at least {tolerancePercentage}% of requests should meet performance thresholds");

        // Performance statistics
        var avgResponseTime = results.Average(r => r.ElapsedMs);
        var maxResponseTime = results.Max(r => r.ElapsedMs);
        var minResponseTime = results.Min(r => r.ElapsedMs);
        var successfulRequests = results.Count(r => r.Response.Status < 300);

        Console.WriteLine("📊 Concurrent Load Performance Summary:");
        Console.WriteLine("   Total Requests: {TotalRequests}", LoadTestConcurrency);
        Console.WriteLine("   Successful Requests: {SuccessfulRequests}", successfulRequests);
        Console.WriteLine("Logging statement");
        Console.WriteLine("Logging statement");

        Console.WriteLine("Logging statement");
    }

    [Test]
    [Description("Tests sustained load performance over extended duration")]
    public async Task Performance_SustainedLoad_ShouldMaintainStability()
    {
        // Arrange
        var testEndpoint = "/health"; // Use lightweight endpoint for sustained testing
        var requestInterval = 1000; // 1 request per second
        var testDurationMs = SustainabilityTestDurationMs;
        var expectedRequests = testDurationMs / requestInterval;

        var results = new List<(DateTime Timestamp, long ElapsedMs, int StatusCode)>();
        var stopwatch = Stopwatch.StartNew();

        Console.WriteLine("Logging statement");

        // Act - Execute sustained load
        while (stopwatch.ElapsedMilliseconds < testDurationMs)
        {
            var requestStart = stopwatch.ElapsedMilliseconds;

            try
            {
                var (response, elapsedMs) = await MeasureResponseTime(async () =>
                    await ApiContext.GetAsync(testEndpoint)
                );

                results.Add((DateTime.UtcNow, elapsedMs, response.Status));

                // Wait for next interval
                var nextRequestTime = requestStart + requestInterval;
                var delayMs = (int)(nextRequestTime - stopwatch.ElapsedMilliseconds);
                if (delayMs > 0)
                {
                    await Task.Delay(delayMs);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Sustained load request failed: {Error}", ex.Message);
                results.Add((DateTime.UtcNow, -1, 500)); // Mark as error
            }
        }

        stopwatch.Stop();

        // Assert - Sustained performance analysis
        var successfulRequests = results.Where(r => r.StatusCode == 200).ToList();
        var failedRequests = results.Where(r => r.StatusCode != 200).ToList();

        var successRate = successfulRequests.Count * 100.0 / results.Count;
        var avgResponseTime = successfulRequests.Average(r => r.ElapsedMs);
        var maxResponseTime = successfulRequests.Max(r => r.ElapsedMs);

        // Performance should remain stable (success rate > 95%, avg response time within threshold)
        Assert.That(successRate, Is.GreaterThanOrEqualTo(95.0),
            $"Sustained load should maintain >95% success rate, actual: {successRate:F1}%");

        Assert.That(avgResponseTime, Is.LessThanOrEqualTo(HealthPerformanceThresholdMs * 1.5), // Allow 50% tolerance for sustained load
            $"Sustained load average response time should remain reasonable, actual: {avgResponseTime:F1}ms");

        // Performance trend analysis (check for degradation over time)
        var firstQuarter = successfulRequests.Take(successfulRequests.Count / 4).Average(r => r.ElapsedMs);
        var lastQuarter = successfulRequests.Skip(3 * successfulRequests.Count / 4).Average(r => r.ElapsedMs);
        var performanceDegradation = ((lastQuarter - firstQuarter) / firstQuarter) * 100;

        Console.WriteLine("📊 Sustained Load Performance Summary:");
        Console.WriteLine("   Duration: {ActualDurationMs}ms", stopwatch.ElapsedMilliseconds);
        Console.WriteLine("   Total Requests: {TotalRequests}", results.Count);
        Console.WriteLine("Logging statement");
        Console.WriteLine("   Failed Requests: {FailedRequests}", failedRequests.Count);
        Console.WriteLine("   Response Times - Avg: {AvgMs:F1}ms, Max: {MaxMs}ms", avgResponseTime, maxResponseTime);
        Console.WriteLine("Logging statement");

        if (Math.Abs(performanceDegradation) < 20) // Less than 20% change is acceptable
        {
            Console.WriteLine("✅ Performance Stability: System maintained stable performance during sustained load");
        }
        else
        {
            Console.WriteLine("Logging statement");
        }

        Console.WriteLine("Logging statement");
    }

    [Test]
    [Description("Tests throughput capacity with burst load patterns")]
    public async Task Performance_BurstLoad_ShouldHandleTrafficSpikes()
    {
        // Arrange - Simulate burst traffic patterns
        var burstPatterns = new[]
        {
            new { BurstSize = 5, Description = "Small Burst" },
            new { BurstSize = 10, Description = "Medium Burst" },
            new { BurstSize = 25, Description = "Large Burst" }
        };

        var testEndpoint = "/health";
        var burstResults = new List<(string Pattern, int RequestCount, double AvgResponseTime, double MaxResponseTime, double SuccessRate)>();

        // Act & Assert - Test each burst pattern
        foreach (var pattern in burstPatterns)
        {
            Console.WriteLine("Logging statement");

            // Execute burst
            var burstTasks = Enumerable.Range(0, pattern.BurstSize).Select(async i =>
            {
                var (response, elapsedMs) = await MeasureResponseTime(async () =>
                    await ApiContext.GetAsync(testEndpoint)
                );

                return new { Response = response, ElapsedMs = elapsedMs, RequestIndex = i };
            });

            var results = await Task.WhenAll(burstTasks);

            // Analyze burst results
            var successfulResults = results.Where(r => r.Response.Status == 200).ToList();
            var avgResponseTime = successfulResults.Average(r => r.ElapsedMs);
            var maxResponseTime = results.Max(r => r.ElapsedMs);
            var successRate = successfulResults.Count * 100.0 / results.Length;

            burstResults.Add((pattern.Description, pattern.BurstSize, avgResponseTime, maxResponseTime, successRate));

            // Assert - Burst should be handled successfully
            Assert.That(successRate, Is.GreaterThanOrEqualTo(90.0),
                $"Burst pattern {pattern.Description} should maintain >90% success rate, actual: {successRate:F1}%");

            Console.WriteLine("Logging statement");

            // Small delay between burst tests
            await Task.Delay(2000);
        }

        // Summary analysis
        Console.WriteLine("📊 Burst Load Summary:");
        foreach (var result in burstResults)
        {
            Console.WriteLine("Logging statement");
        }

        Console.WriteLine("✅ Burst Load: System successfully handled all burst traffic patterns");
    }

    /// <summary>
    /// Helper method to execute HTTP requests based on method type
    /// </summary>
    private async Task<IAPIResponse> ExecuteRequest(string method, string endpoint, Dictionary<string, string> headers, string data)
    {
        var options = new APIRequestContextOptions { Headers = headers };

        if (!string.IsNullOrEmpty(data))
        {
            options.Data = data;
        }

        return method switch
        {
            "GET" => await ApiContext.GetAsync(endpoint, options),
            "POST" => await ApiContext.PostAsync(endpoint, options),
            "PUT" => await ApiContext.PutAsync(endpoint, options),
            "DELETE" => await ApiContext.DeleteAsync(endpoint, options),
            _ => throw new NotSupportedException($"HTTP method {method} not supported")
        };
    }
}

