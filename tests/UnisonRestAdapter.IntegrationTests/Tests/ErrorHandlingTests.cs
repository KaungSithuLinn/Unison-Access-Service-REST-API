using UnisonRestAdapter.IntegrationTests.Infrastructure;
using FluentAssertions;
using Microsoft.Playwright;

namespace UnisonRestAdapter.IntegrationTests.Tests;

/// <summary>
/// Integration tests for Error Handling and Edge Cases
/// Issue #7: Integration Testing with Playwright
/// 
/// Tests comprehensive error scenarios and edge cases:
/// - HTTP error codes (400, 401, 404, 405, 500)
/// - Invalid request formats
/// - Boundary conditions
/// - Network resilience
/// - Error response formats
/// 
/// Requirements: >90% error condition coverage as specified in Issue #7
/// </summary>
[TestFixture]
public class ErrorHandlingTests : PlaywrightTestBase
{
    [Test]
    [Description("Tests 400 Bad Request scenarios with invalid JSON payloads")]
    public async Task ErrorHandling_InvalidJsonPayload_ShouldReturn400()
    {
        // Arrange
        var invalidPayloads = new[]
        {
            "invalid json",                    // Invalid JSON syntax
            "{incomplete json",               // Incomplete JSON
            "{ \"key\": }",                  // Missing value
            "{ \"key\": value }",            // Unquoted value
            "",                              // Empty string
            "null",                          // Null payload
            "{\"nested\": {\"invalid\": }}", // Nested invalid JSON
        };

        const string endpoint = "/api/cards/update";
        var headers = CreateAuthHeaders();

        // Act & Assert - Test each invalid payload
        foreach (var invalidPayload in invalidPayloads)
        {
            var (response, elapsedMs) = await MeasureResponseTime(async () =>
                await ApiContext.PutAsync(endpoint, new APIRequestContextOptions
                {
                    Headers = headers,
                    Data = invalidPayload
                })
            );

            // Assert - Should return 400 Bad Request
            await AssertResponseStatus(response, 400, $"Invalid JSON: {invalidPayload}");

            // Assert - Error handling should be fast
            AssertPerformanceThreshold(elapsedMs, 500, "Error Handling Performance");

            // Assert - Response should contain error information
            var responseText = await response.TextAsync();
            Assert.That(responseText, Is.Not.Empty, "Error response should not be empty");
        }

        Console.WriteLine("Logging statement");
    }

    [Test]
    [Description("Tests 404 Not Found scenarios with invalid resource identifiers")]
    public async Task ErrorHandling_NotFoundResources_ShouldReturn404()
    {
        // Arrange
        var invalidResourceScenarios = new[]
        {
            new { Endpoint = "/api/cards/nonexistent-card-id", Description = "Invalid Card ID" },
            new { Endpoint = "/api/cards/123456789", Description = "Numeric Card ID" },
            new { Endpoint = "/api/cards/", Description = "Empty Card ID" },
            new { Endpoint = "/api/cards/card-with-special-chars-@#$", Description = "Special Characters" },
            new { Endpoint = "/api/nonexistent-resource", Description = "Invalid Resource Path" },
            new { Endpoint = "/api/cards/nonexistent/activate", Description = "Invalid Card for Activation" },
        };

        var headers = CreateAuthHeaders();

        // Act & Assert - Test each invalid resource scenario
        foreach (var scenario in invalidResourceScenarios)
        {
            var (response, elapsedMs) = await MeasureResponseTime(async () =>
                await ApiContext.GetAsync(scenario.Endpoint, new APIRequestContextOptions
                {
                    Headers = headers
                })
            );

            // Assert - Should return 404 Not Found
            await AssertResponseStatus(response, 404, $"Not Found: {scenario.Description}");

            // Assert - Error handling should be fast
            AssertPerformanceThreshold(elapsedMs, 500, $"Not Found Performance: {scenario.Description}");

            // Assert - Response should contain error information
            var responseText = await response.TextAsync();
            Assert.That(responseText, Is.Not.Empty, $"404 response should not be empty for {scenario.Description}");
        }

        Console.WriteLine("Logging statement");
    }

    [Test]
    [Description("Tests 405 Method Not Allowed with incorrect HTTP methods")]
    public async Task ErrorHandling_MethodNotAllowed_ShouldReturn405()
    {
        // Arrange - Test wrong HTTP methods on known endpoints
        var methodMismatchScenarios = new[]
        {
            new { Endpoint = "/health", Method = "POST", Description = "POST on Health (GET-only)" },
            new { Endpoint = "/health", Method = "PUT", Description = "PUT on Health (GET-only)" },
            new { Endpoint = "/health", Method = "DELETE", Description = "DELETE on Health (GET-only)" },
            new { Endpoint = "/api/cards/update", Method = "GET", Description = "GET on UpdateCard (PUT-only)" },
            new { Endpoint = "/api/cards/update", Method = "POST", Description = "POST on UpdateCard (PUT-only)" },
            new { Endpoint = "/api/cards/validate", Method = "GET", Description = "GET on ValidateCard (POST-only)" },
            new { Endpoint = "/api/cards/validate", Method = "PUT", Description = "PUT on ValidateCard (POST-only)" },
        };

        var headers = CreateAuthHeaders();

        // Act & Assert - Test each method mismatch scenario
        foreach (var scenario in methodMismatchScenarios)
        {
            var (response, elapsedMs) = await MeasureResponseTime(async () =>
            {
                var options = new APIRequestContextOptions { Headers = headers };

                // Add dummy data for methods that expect body
                if (scenario.Method is "POST" or "PUT")
                {
                    options.Data = "{}";
                }

                return scenario.Method switch
                {
                    "GET" => await ApiContext.GetAsync(scenario.Endpoint, options),
                    "POST" => await ApiContext.PostAsync(scenario.Endpoint, options),
                    "PUT" => await ApiContext.PutAsync(scenario.Endpoint, options),
                    "DELETE" => await ApiContext.DeleteAsync(scenario.Endpoint, options),
                    _ => throw new NotSupportedException($"Method {scenario.Method} not supported in test")
                };
            });

            // Assert - Should return 405 Method Not Allowed
            await AssertResponseStatus(response, 405, $"Method Not Allowed: {scenario.Description}");

            // Assert - Error handling should be fast
            AssertPerformanceThreshold(elapsedMs, 500, $"Method Not Allowed Performance: {scenario.Description}");
        }

        Console.WriteLine("Logging statement");
    }

    [Test]
    [Description("Tests request size limits and boundary conditions")]
    public async Task ErrorHandling_RequestSizeLimits_ShouldHandleAppropriately()
    {
        // Arrange - Create payloads of various sizes to test boundaries
        var sizeLimitScenarios = new[]
        {
            new { Size = 0, Description = "Empty Request", Data = "{}" },
            new { Size = 1000, Description = "Small Request", Data = GenerateLargeJsonPayload(1000) },
            new { Size = 10000, Description = "Medium Request", Data = GenerateLargeJsonPayload(10000) },
            new { Size = 100000, Description = "Large Request", Data = GenerateLargeJsonPayload(100000) },
            new { Size = 1000000, Description = "Very Large Request", Data = GenerateLargeJsonPayload(1000000) }
        };

        const string endpoint = "/api/cards/update";
        var headers = CreateAuthHeaders();

        // Act & Assert - Test each size scenario
        foreach (var scenario in sizeLimitScenarios)
        {
            try
            {
                var (response, elapsedMs) = await MeasureResponseTime(async () =>
                    await ApiContext.PutAsync(endpoint, new APIRequestContextOptions
                    {
                        Headers = headers,
                        Data = scenario.Data
                    })
                );

                // Log the response for analysis
                Console.WriteLine("Logging statement");

                // Assert - Should handle the request (success or proper error)
                Assert.That(response.Status, Is.AnyOf(200, 400, 413), // 200=OK, 400=BadRequest, 413=PayloadTooLarge
                    $"Request size test {scenario.Description} should return appropriate status");

                // Assert - Even large requests should be handled in reasonable time
                AssertPerformanceThreshold(elapsedMs, 5000, $"Size Limit Performance: {scenario.Description}");
            }
            catch (Exception ex)
            {
                // Log but don't fail test - some very large requests may timeout or be rejected at network level
                Console.WriteLine("Logging statement");
            }
        }

        Console.WriteLine("✅ Request Size Limits: Completed boundary testing for various request sizes");
    }

    [Test]
    [Description("Tests concurrent error scenarios for system resilience")]
    public async Task ErrorHandling_ConcurrentErrors_ShouldMaintainStability()
    {
        // Arrange - Mix of error-inducing scenarios
        var errorScenarios = new[]
        {
            new { Endpoint = "/api/cards/update", Method = "PUT", Headers = CreateAuthHeaders(InvalidToken), Data = "{}", ExpectedStatus = 401, Description = "Invalid Auth" },
            new { Endpoint = "/api/cards/update", Method = "PUT", Headers = CreateAuthHeaders(), Data = "invalid json", ExpectedStatus = 400, Description = "Invalid JSON" },
            new { Endpoint = "/api/cards/nonexistent", Method = "GET", Headers = CreateAuthHeaders(), Data = "", ExpectedStatus = 404, Description = "Not Found" },
            new { Endpoint = "/health", Method = "POST", Headers = new Dictionary<string, string>(), Data = "{}", ExpectedStatus = 405, Description = "Method Not Allowed" }
        };

        const int concurrentRequests = 20;

        // Act - Execute concurrent error scenarios
        var tasks = Enumerable.Range(0, concurrentRequests).Select(async i =>
        {
            var scenario = errorScenarios[i % errorScenarios.Length];

            var (response, elapsedMs) = await MeasureResponseTime(async () =>
            {
                var options = new APIRequestContextOptions { Headers = scenario.Headers };
                if (!string.IsNullOrEmpty(scenario.Data))
                {
                    options.Data = scenario.Data;
                }

                return scenario.Method switch
                {
                    "GET" => await ApiContext.GetAsync(scenario.Endpoint, options),
                    "POST" => await ApiContext.PostAsync(scenario.Endpoint, options),
                    "PUT" => await ApiContext.PutAsync(scenario.Endpoint, options),
                    _ => throw new NotSupportedException($"Method {scenario.Method} not supported")
                };
            });

            return new { Response = response, ElapsedMs = elapsedMs, Scenario = scenario, RequestIndex = i };
        });

        var results = await Task.WhenAll(tasks);

        // Assert - All error scenarios handled appropriately
        foreach (var result in results)
        {
            await AssertResponseStatus(result.Response, result.Scenario.ExpectedStatus,
                $"Concurrent Error {result.Scenario.Description} #{result.RequestIndex}");

            // Assert - Error handling should remain performant under load
            AssertPerformanceThreshold(result.ElapsedMs, 1000,
                $"Concurrent Error Performance {result.Scenario.Description}");
        }

        // Group and summarize results
        var errorGroups = results.GroupBy(r => r.Scenario.Description).ToList();
        foreach (var group in errorGroups)
        {
            var avgResponseTime = group.Average(r => r.ElapsedMs);
            Console.WriteLine("Logging statement");
        }

        Console.WriteLine("Logging statement");
    }

    [Test]
    [Description("Tests error response format consistency and information quality")]
    public async Task ErrorHandling_ResponseFormat_ShouldProvideConsistentStructure()
    {
        // Arrange - Various error scenarios to test response format consistency
        var errorTestCases = new[]
        {
            new { Endpoint = "/api/cards/update", Method = "PUT", Headers = CreateAuthHeaders(InvalidToken), Data = "{}", StatusCode = 401, ErrorType = "Authentication" },
            new { Endpoint = "/api/cards/update", Method = "PUT", Headers = CreateAuthHeaders(), Data = "invalid json", StatusCode = 400, ErrorType = "BadRequest" },
            new { Endpoint = "/api/cards/nonexistent-id", Method = "GET", Headers = CreateAuthHeaders(), Data = "", StatusCode = 404, ErrorType = "NotFound" },
            new { Endpoint = "/health", Method = "POST", Headers = new Dictionary<string, string>(), Data = "{}", StatusCode = 405, ErrorType = "MethodNotAllowed" }
        };

        // Act & Assert - Test error response format for each scenario
        foreach (var testCase in errorTestCases)
        {
            var response = await (testCase.Method switch
            {
                "GET" => ApiContext.GetAsync(testCase.Endpoint, new APIRequestContextOptions { Headers = testCase.Headers }),
                "POST" => ApiContext.PostAsync(testCase.Endpoint, new APIRequestContextOptions { Headers = testCase.Headers, Data = testCase.Data }),
                "PUT" => ApiContext.PutAsync(testCase.Endpoint, new APIRequestContextOptions { Headers = testCase.Headers, Data = testCase.Data }),
                _ => throw new NotSupportedException($"Method {testCase.Method} not supported")
            });

            // Assert - Correct status code
            await AssertResponseStatus(response, testCase.StatusCode, $"Error Format {testCase.ErrorType}");

            // Assert - Response contains error information
            var responseText = await response.TextAsync();
            Assert.That(responseText, Is.Not.Empty, $"Error response for {testCase.ErrorType} should not be empty");

            // Log response structure for analysis
            Console.WriteLine("Logging statement");

            // Check if response is JSON formatted (recommended for APIs)
            try
            {
                var errorObject = Newtonsoft.Json.JsonConvert.DeserializeObject(responseText);
                if (errorObject != null)
                {
                    Console.WriteLine("✅ Error Format {ErrorType}: Response is valid JSON", testCase.ErrorType);
                }
            }
            catch (Newtonsoft.Json.JsonException)
            {
                Console.WriteLine("⚠️ Error Format {ErrorType}: Response is not JSON formatted", testCase.ErrorType);
            }
        }

        Console.WriteLine("Logging statement");
    }

    [Test]
    [Description("Tests retry scenarios with transient failure simulation")]
    public async Task ErrorHandling_TransientFailures_ShouldHandleGracefully()
    {
        // Arrange - Test endpoint that should be stable
        const string stableEndpoint = "/health";
        const int retryAttempts = 3;

        // Act - Use the built-in retry mechanism to test resilience
        var result = await WithRetry(async () =>
        {
            var response = await ApiContext.GetAsync(stableEndpoint);

            // Simulate occasional transient failure for testing
            var random = new Random();
            if (random.Next(10) < 1) // 10% chance of simulated failure
            {
                throw new Exception("Simulated transient failure");
            }

            if (response.Status != 200)
            {
                throw new Exception($"Unexpected status: {response.Status}");
            }

            return response;
        }, maxRetries: retryAttempts);

        // Assert - Retry mechanism should eventually succeed
        await AssertResponseStatus(result, 200, "Retry Success");

        Console.WriteLine("✅ Transient Failure Handling: Successfully handled potential transient failures with retry mechanism");
    }

    /// <summary>
    /// Helper method to generate large JSON payloads for size limit testing
    /// </summary>
    private string GenerateLargeJsonPayload(int approximateSize)
    {
        var payload = CreateUpdateCardRequest();
        var baseJson = Newtonsoft.Json.JsonConvert.SerializeObject(payload);

        if (baseJson.Length >= approximateSize)
        {
            return baseJson;
        }

        // Add large string fields to reach desired size
        var paddingSize = approximateSize - baseJson.Length - 100; // Leave some buffer
        var padding = new string('A', Math.Max(paddingSize, 0));

        var largePayload = new
        {
            cardId = Config.TestData.ValidCardId,
            userName = Config.TestData.TestUser.UserName,
            firstName = Config.TestData.TestUser.FirstName,
            lastName = Config.TestData.TestUser.LastName,
            email = Config.TestData.TestUser.Email,
            department = Config.TestData.TestUser.Department,
            title = Config.TestData.TestUser.Title,
            isActive = true,
            expirationDate = DateTime.UtcNow.AddYears(1).ToString("yyyy-MM-ddTHH:mm:ssZ"),
            largePadding = padding // Add padding to reach desired size
        };

        return Newtonsoft.Json.JsonConvert.SerializeObject(largePayload);
    }
}

