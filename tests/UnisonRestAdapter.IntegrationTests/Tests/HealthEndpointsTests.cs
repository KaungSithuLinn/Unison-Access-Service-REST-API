using UnisonRestAdapter.IntegrationTests.Infrastructure;
using FluentAssertions;
using Microsoft.Playwright;

namespace UnisonRestAdapter.IntegrationTests.Tests;

/// <summary>
/// Integration tests for Health Check endpoints
/// Issue #7: Integration Testing with Playwright
/// 
/// Tests all health check endpoints with comprehensive validation:
/// - /health (basic health check)
/// - /health/liveness (liveness probe)  
/// - /health/readiness (readiness probe)
/// 
/// Performance Requirements: All endpoints under 200ms response time
/// </summary>
[TestFixture]
public class HealthEndpointsTests : PlaywrightTestBase
{
    [Test]
    [Description("Tests basic health endpoint for availability and performance")]
    public async Task Health_BasicEndpoint_ShouldReturnHealthyStatus()
    {
        // Arrange
        const string endpoint = "/health";
        const int performanceThresholdMs = 200;

        // Act
        var (response, elapsedMs) = await MeasureResponseTime(async () =>
            await ApiContext.GetAsync(endpoint)
        );

        // Assert - Status Code
        await AssertResponseStatus(response, 200, "Health Basic");

        // Assert - Performance
        AssertPerformanceThreshold(elapsedMs, performanceThresholdMs, "Health Basic");

        // Assert - Response Content
        var jsonContent = await AssertValidJsonResponse(response, "Health Basic");

        // Validate health status structure
        var healthStatus = Newtonsoft.Json.JsonConvert.DeserializeObject<dynamic>(jsonContent);
        healthStatus.Should().NotBeNull();

        Console.WriteLine("✅ Health Basic: Endpoint validation completed successfully");
    }

    [Test]
    [Description("Tests liveness probe endpoint for container orchestration support")]
    public async Task Health_LivenessEndpoint_ShouldReturnAliveStatus()
    {
        // Arrange
        const string endpoint = "/health/liveness";
        const int performanceThresholdMs = 200;

        // Act
        var (response, elapsedMs) = await MeasureResponseTime(async () =>
            await ApiContext.GetAsync(endpoint)
        );

        // Assert - Status Code
        await AssertResponseStatus(response, 200, "Health Liveness");

        // Assert - Performance
        AssertPerformanceThreshold(elapsedMs, performanceThresholdMs, "Health Liveness");

        // Assert - Response Content
        var jsonContent = await AssertValidJsonResponse(response, "Health Liveness");

        // Validate liveness status structure
        var livenessStatus = Newtonsoft.Json.JsonConvert.DeserializeObject<dynamic>(jsonContent);
        livenessStatus.Should().NotBeNull();

        Console.WriteLine("✅ Health Liveness: Endpoint validation completed successfully");
    }

    [Test]
    [Description("Tests readiness probe endpoint for load balancer integration")]
    public async Task Health_ReadinessEndpoint_ShouldReturnReadyStatus()
    {
        // Arrange
        const string endpoint = "/health/readiness";
        const int performanceThresholdMs = 200;

        // Act
        var (response, elapsedMs) = await MeasureResponseTime(async () =>
            await ApiContext.GetAsync(endpoint)
        );

        // Assert - Status Code
        await AssertResponseStatus(response, 200, "Health Readiness");

        // Assert - Performance
        AssertPerformanceThreshold(elapsedMs, performanceThresholdMs, "Health Readiness");

        // Assert - Response Content
        var jsonContent = await AssertValidJsonResponse(response, "Health Readiness");

        // Validate readiness status structure
        var readinessStatus = Newtonsoft.Json.JsonConvert.DeserializeObject<dynamic>(jsonContent);
        readinessStatus.Should().NotBeNull();

        Console.WriteLine("✅ Health Readiness: Endpoint validation completed successfully");
    }

    [Test]
    [Description("Tests all health endpoints concurrently for load testing")]
    public async Task Health_AllEndpointsConcurrent_ShouldHandleParallelRequests()
    {
        // Arrange
        var endpoints = new[]
        {
            "/health",
            "/health/liveness",
            "/health/readiness"
        };
        const int performanceThresholdMs = 200;

        // Act - Execute all health checks concurrently
        var tasks = endpoints.Select(async endpoint =>
        {
            var (response, elapsedMs) = await MeasureResponseTime(async () =>
                await ApiContext.GetAsync(endpoint)
            );

            return new { Endpoint = endpoint, Response = response, ElapsedMs = elapsedMs };
        });

        var results = await Task.WhenAll(tasks);

        // Assert - All requests completed successfully
        foreach (var result in results)
        {
            await AssertResponseStatus(result.Response, 200, $"Health Concurrent {result.Endpoint}");
            AssertPerformanceThreshold(result.ElapsedMs, performanceThresholdMs, $"Health Concurrent {result.Endpoint}");

            var jsonContent = await AssertValidJsonResponse(result.Response, $"Health Concurrent {result.Endpoint}");
            var status = Newtonsoft.Json.JsonConvert.DeserializeObject<dynamic>(jsonContent);
            status.Should().NotBeNull();
        }

        Console.WriteLine("Logging statement");
    }

    [Test]
    [Description("Tests health endpoint error handling with invalid HTTP methods")]
    public async Task Health_InvalidMethod_ShouldReturnMethodNotAllowed()
    {
        // Arrange
        const string endpoint = "/health";

        // Act - Try POST method on GET-only endpoint
        var response = await ApiContext.PostAsync(endpoint, new APIRequestContextOptions
        {
            Data = "{}"
        });

        // Assert - Should return 405 Method Not Allowed
        await AssertResponseStatus(response, 405, "Health Invalid Method");

        Console.WriteLine("✅ Health Invalid Method: Properly handled unsupported HTTP method");
    }

    [Test]
    [Description("Tests health endpoint resilience with high frequency requests")]
    public async Task Health_HighFrequency_ShouldMaintainPerformance()
    {
        // Arrange
        const string endpoint = "/health";
        const int requestCount = 10;
        const int performanceThresholdMs = 200;

        // Act - Send multiple rapid requests
        var responseTimes = new List<long>();

        for (int i = 0; i < requestCount; i++)
        {
            var (response, elapsedMs) = await MeasureResponseTime(async () =>
                await ApiContext.GetAsync(endpoint)
            );

            responseTimes.Add(elapsedMs);
            await AssertResponseStatus(response, 200, $"Health High Frequency #{i + 1}");
        }

        // Assert - Performance maintained across all requests
        var avgResponseTime = responseTimes.Average();
        var maxResponseTime = responseTimes.Max();

        Console.WriteLine("Logging statement");

        // All individual requests should meet performance threshold
        foreach (var responseTime in responseTimes)
        {
            Assert.That(responseTime, Is.LessThanOrEqualTo(performanceThresholdMs),
                $"Individual health check response time ({responseTime}ms) exceeded threshold ({performanceThresholdMs}ms)");
        }

        // Average should be well within threshold
        Assert.That(avgResponseTime, Is.LessThanOrEqualTo(performanceThresholdMs / 2),
            $"Average health check response time ({avgResponseTime}ms) should be well within threshold");

        Console.WriteLine("Logging statement");
    }
}

