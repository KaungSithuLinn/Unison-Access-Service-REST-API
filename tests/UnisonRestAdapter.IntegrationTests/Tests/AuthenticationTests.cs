using UnisonRestAdapter.IntegrationTests.Infrastructure;
using FluentAssertions;
using Microsoft.Playwright;

namespace UnisonRestAdapter.IntegrationTests.Tests;

/// <summary>
/// Integration tests for Authentication and Authorization
/// Issue #7: Integration Testing with Playwright
/// 
/// Tests authentication mechanisms and authorization policies:
/// - Valid token scenarios
/// - Invalid token scenarios  
/// - Missing token scenarios
/// - Token format validation
/// - Authorization for different endpoints
/// 
/// Performance Requirements: Authentication checks under 100ms
/// Security Requirements: >90% error condition coverage for security scenarios
/// </summary>
[TestFixture]
public class AuthenticationTests : PlaywrightTestBase
{
    private const int AuthPerformanceThresholdMs = 100;

    [Test]
    [Description("Tests valid authentication token acceptance across all secured endpoints")]
    public async Task Authentication_ValidToken_ShouldAccessAllSecuredEndpoints()
    {
        // Arrange
        var securedEndpoints = new[]
        {
            new { Method = "PUT", Endpoint = "/api/cards/update", RequiresBody = true },
            new { Method = "GET", Endpoint = $"/api/cards/{Config.TestData.ValidCardId}", RequiresBody = false },
            new { Method = "POST", Endpoint = "/api/cards/validate", RequiresBody = true },
            new { Method = "PUT", Endpoint = $"/api/cards/{Config.TestData.ValidCardId}/activate", RequiresBody = false },
            new { Method = "PUT", Endpoint = $"/api/cards/{Config.TestData.ValidCardId}/deactivate", RequiresBody = false }
        };

        var headers = CreateAuthHeaders();

        // Act & Assert - Test each secured endpoint
        foreach (var endpointInfo in securedEndpoints)
        {
            var (response, elapsedMs) = await MeasureResponseTime(async () =>
            {
                var options = new APIRequestContextOptions { Headers = headers };

                if (endpointInfo.RequiresBody)
                {
                    var requestData = endpointInfo.Endpoint.Contains("validate")
                        ? new { cardId = Config.TestData.ValidCardId, userName = Config.TestData.TestUser.UserName }
                        : (object)CreateUpdateCardRequest();

                    options.Data = Newtonsoft.Json.JsonConvert.SerializeObject(requestData);
                }

                return endpointInfo.Method switch
                {
                    "GET" => await ApiContext.GetAsync(endpointInfo.Endpoint, options),
                    "POST" => await ApiContext.PostAsync(endpointInfo.Endpoint, options),
                    "PUT" => await ApiContext.PutAsync(endpointInfo.Endpoint, options),
                    _ => throw new NotSupportedException($"HTTP method {endpointInfo.Method} not supported")
                };
            });

            // Assert - Should be authorized (not 401)
            Assert.That(response.Status, Is.Not.EqualTo(401),
                $"Endpoint {endpointInfo.Method} {endpointInfo.Endpoint} should accept valid token but returned {response.Status}");

            // Assert - Authentication performance
            AssertPerformanceThreshold(elapsedMs, AuthPerformanceThresholdMs + 900, // Allow more time for full endpoint processing
                $"Auth Check {endpointInfo.Method} {endpointInfo.Endpoint}");
        }

        Console.WriteLine($"✅ Valid Token: Successfully authenticated against {securedEndpoints.Length} secured endpoints");
    }

    [Test]
    [Description("Tests invalid authentication token rejection across all secured endpoints")]
    public async Task Authentication_InvalidToken_ShouldRejectAllSecuredEndpoints()
    {
        // Arrange
        var securedEndpoints = new[]
        {
            new { Method = "PUT", Endpoint = "/api/cards/update", RequiresBody = true },
            new { Method = "GET", Endpoint = $"/api/cards/{Config.TestData.ValidCardId}", RequiresBody = false },
            new { Method = "POST", Endpoint = "/api/cards/validate", RequiresBody = true },
            new { Method = "PUT", Endpoint = $"/api/cards/{Config.TestData.ValidCardId}/activate", RequiresBody = false },
            new { Method = "PUT", Endpoint = $"/api/cards/{Config.TestData.ValidCardId}/deactivate", RequiresBody = false }
        };

        var headers = CreateAuthHeaders(InvalidToken);

        // Act & Assert - Test each secured endpoint with invalid token
        foreach (var endpointInfo in securedEndpoints)
        {
            var (response, elapsedMs) = await MeasureResponseTime(async () =>
            {
                var options = new APIRequestContextOptions { Headers = headers };

                if (endpointInfo.RequiresBody)
                {
                    var requestData = endpointInfo.Endpoint.Contains("validate")
                        ? new { cardId = Config.TestData.ValidCardId, userName = Config.TestData.TestUser.UserName }
                        : (object)CreateUpdateCardRequest();

                    options.Data = Newtonsoft.Json.JsonConvert.SerializeObject(requestData);
                }

                return endpointInfo.Method switch
                {
                    "GET" => await ApiContext.GetAsync(endpointInfo.Endpoint, options),
                    "POST" => await ApiContext.PostAsync(endpointInfo.Endpoint, options),
                    "PUT" => await ApiContext.PutAsync(endpointInfo.Endpoint, options),
                    _ => throw new NotSupportedException($"HTTP method {endpointInfo.Method} not supported")
                };
            });

            // Assert - Should be unauthorized (401)
            await AssertResponseStatus(response, 401, $"Invalid Token {endpointInfo.Method} {endpointInfo.Endpoint}");

            // Assert - Authentication rejection should be fast
            AssertPerformanceThreshold(elapsedMs, AuthPerformanceThresholdMs,
                $"Auth Rejection {endpointInfo.Method} {endpointInfo.Endpoint}");
        }

        Console.WriteLine("Logging statement");
    }

    [Test]
    [Description("Tests missing authentication token rejection across all secured endpoints")]
    public async Task Authentication_MissingToken_ShouldRejectAllSecuredEndpoints()
    {
        // Arrange
        var securedEndpoints = new[]
        {
            new { Method = "PUT", Endpoint = "/api/cards/update", RequiresBody = true },
            new { Method = "GET", Endpoint = $"/api/cards/{Config.TestData.ValidCardId}", RequiresBody = false },
            new { Method = "POST", Endpoint = "/api/cards/validate", RequiresBody = true },
            new { Method = "PUT", Endpoint = $"/api/cards/{Config.TestData.ValidCardId}/activate", RequiresBody = false },
            new { Method = "PUT", Endpoint = $"/api/cards/{Config.TestData.ValidCardId}/deactivate", RequiresBody = false }
        };

        var headers = new Dictionary<string, string>
        {
            ["Content-Type"] = "application/json"
            // Deliberately omitting Unison-Token header
        };

        // Act & Assert - Test each secured endpoint without token
        foreach (var endpointInfo in securedEndpoints)
        {
            var (response, elapsedMs) = await MeasureResponseTime(async () =>
            {
                var options = new APIRequestContextOptions { Headers = headers };

                if (endpointInfo.RequiresBody)
                {
                    var requestData = endpointInfo.Endpoint.Contains("validate")
                        ? new { cardId = Config.TestData.ValidCardId, userName = Config.TestData.TestUser.UserName }
                        : (object)CreateUpdateCardRequest();

                    options.Data = Newtonsoft.Json.JsonConvert.SerializeObject(requestData);
                }

                return endpointInfo.Method switch
                {
                    "GET" => await ApiContext.GetAsync(endpointInfo.Endpoint, options),
                    "POST" => await ApiContext.PostAsync(endpointInfo.Endpoint, options),
                    "PUT" => await ApiContext.PutAsync(endpointInfo.Endpoint, options),
                    _ => throw new NotSupportedException($"HTTP method {endpointInfo.Method} not supported")
                };
            });

            // Assert - Should be unauthorized (401)
            await AssertResponseStatus(response, 401, $"Missing Token {endpointInfo.Method} {endpointInfo.Endpoint}");

            // Assert - Authentication rejection should be fast
            AssertPerformanceThreshold(elapsedMs, AuthPerformanceThresholdMs,
                $"Auth Missing {endpointInfo.Method} {endpointInfo.Endpoint}");
        }

        Console.WriteLine("Logging statement");
    }

    [Test]
    [Description("Tests that health endpoints do not require authentication")]
    public async Task Authentication_HealthEndpoints_ShouldNotRequireAuthentication()
    {
        // Arrange
        var publicEndpoints = new[]
        {
            "/health",
            "/health/liveness",
            "/health/readiness"
        };

        var headersWithoutAuth = new Dictionary<string, string>
        {
            ["Content-Type"] = "application/json"
            // No Unison-Token header
        };

        // Act & Assert - Test each health endpoint without authentication
        foreach (var endpoint in publicEndpoints)
        {
            var (response, elapsedMs) = await MeasureResponseTime(async () =>
                await ApiContext.GetAsync(endpoint, new APIRequestContextOptions
                {
                    Headers = headersWithoutAuth
                })
            );

            // Assert - Should be accessible (200, not 401)
            await AssertResponseStatus(response, 200, $"Public {endpoint}");

            // Assert - Should be fast
            AssertPerformanceThreshold(elapsedMs, 200, $"Public {endpoint}");
        }

        Console.WriteLine("Logging statement");
    }

    [Test]
    [Description("Tests various invalid token formats for comprehensive security coverage")]
    public async Task Authentication_InvalidTokenFormats_ShouldRejectAllFormats()
    {
        // Arrange
        var invalidTokenFormats = new[]
        {
            "",                                    // Empty token
            "   ",                                // Whitespace token  
            "invalid-token",                      // Invalid format
            "123",                               // Numbers only
            "abc-123",                          // Short format
            "not-a-guid-format",               // Invalid GUID format
            "595d799a-9553-4ddf-8fd9",        // Incomplete GUID
            "595d799a-9553-4ddf-8fd9-c27b1f233ce7-extra", // GUID with extra content
            "INVALID-TOKEN-FORMAT",            // Wrong format entirely
            "Bearer 595d799a-9553-4ddf-8fd9-c27b1f233ce7", // Bearer format (not expected)
        };

        const string testEndpoint = "/api/cards/validate";
        var requestData = new { cardId = Config.TestData.ValidCardId, userName = Config.TestData.TestUser.UserName };

        // Act & Assert - Test each invalid token format
        foreach (var invalidToken in invalidTokenFormats)
        {
            var headers = new Dictionary<string, string>
            {
                ["Content-Type"] = "application/json",
                ["Unison-Token"] = invalidToken
            };

            var (response, elapsedMs) = await MeasureResponseTime(async () =>
                await ApiContext.PostAsync(testEndpoint, new APIRequestContextOptions
                {
                    Headers = headers,
                    Data = Newtonsoft.Json.JsonConvert.SerializeObject(requestData)
                })
            );

            // Assert - Should be unauthorized (401)
            await AssertResponseStatus(response, 401, $"Invalid Token Format: '{invalidToken}'");

            // Assert - Rejection should be fast
            AssertPerformanceThreshold(elapsedMs, AuthPerformanceThresholdMs,
                $"Invalid Format Auth Rejection");
        }

        Console.WriteLine("Logging statement");
    }

    [Test]
    [Description("Tests authentication under high load with concurrent requests")]
    public async Task Authentication_ConcurrentLoad_ShouldMaintainSecurity()
    {
        // Arrange
        const int concurrentRequests = 10;
        const string testEndpoint = "/api/cards/validate";
        var requestData = new { cardId = Config.TestData.ValidCardId, userName = Config.TestData.TestUser.UserName };

        // Mix of valid and invalid tokens to test security under load
        var authScenarios = new[]
        {
            new { Token = ValidToken, ExpectedStatus = 200, Scenario = "ValidToken" },
            new { Token = InvalidToken, ExpectedStatus = 401, Scenario = "InvalidToken" },
            new { Token = "", ExpectedStatus = 401, Scenario = "EmptyToken" },
            new { Token = "not-a-token", ExpectedStatus = 401, Scenario = "BadFormatToken" }
        };

        // Act - Execute concurrent requests with mixed authentication scenarios
        var tasks = Enumerable.Range(0, concurrentRequests).Select(async i =>
        {
            var scenario = authScenarios[i % authScenarios.Length];
            var headers = new Dictionary<string, string>
            {
                ["Content-Type"] = "application/json",
                ["Unison-Token"] = scenario.Token
            };

            var (response, elapsedMs) = await MeasureResponseTime(async () =>
                await ApiContext.PostAsync(testEndpoint, new APIRequestContextOptions
                {
                    Headers = headers,
                    Data = Newtonsoft.Json.JsonConvert.SerializeObject(requestData)
                })
            );

            return new { Response = response, ElapsedMs = elapsedMs, Scenario = scenario, RequestIndex = i };
        });

        var results = await Task.WhenAll(tasks);

        // Assert - All requests handled correctly according to their authentication scenario
        foreach (var result in results)
        {
            await AssertResponseStatus(result.Response, result.Scenario.ExpectedStatus,
                $"Concurrent Auth {result.Scenario.Scenario} #{result.RequestIndex}");

            AssertPerformanceThreshold(result.ElapsedMs, AuthPerformanceThresholdMs + 900, // Allow more time under load
                $"Concurrent Auth Performance {result.Scenario.Scenario} #{result.RequestIndex}");
        }

        // Group results by scenario for summary
        var scenarioGroups = results.GroupBy(r => r.Scenario.Scenario).ToList();
        foreach (var group in scenarioGroups)
        {
            var avgResponseTime = group.Average(r => r.ElapsedMs);
            Console.WriteLine("Logging statement");
        }

        Console.WriteLine("Logging statement");
    }

    [Test]
    [Description("Tests token case sensitivity for security validation")]
    public async Task Authentication_TokenCaseSensitivity_ShouldEnforceExactMatch()
    {
        // Arrange
        const string testEndpoint = "/api/cards/validate";
        var requestData = new { cardId = Config.TestData.ValidCardId, userName = Config.TestData.TestUser.UserName };

        var caseVariations = new[]
        {
            ValidToken.ToUpper(),    // All uppercase
            ValidToken.ToLower(),    // All lowercase
            char.ToUpper(ValidToken[0]) + ValidToken.Substring(1).ToLower() // First char upper, rest lower
        };

        // Act & Assert - Test case sensitivity (assuming case-sensitive tokens)
        foreach (var tokenVariation in caseVariations)
        {
            // Skip if the variation is actually the same as the valid token
            if (tokenVariation == ValidToken) continue;

            var headers = new Dictionary<string, string>
            {
                ["Content-Type"] = "application/json",
                ["Unison-Token"] = tokenVariation
            };

            var (response, elapsedMs) = await MeasureResponseTime(async () =>
                await ApiContext.PostAsync(testEndpoint, new APIRequestContextOptions
                {
                    Headers = headers,
                    Data = Newtonsoft.Json.JsonConvert.SerializeObject(requestData)
                })
            );

            // Assert - Case variations should be rejected if tokens are case-sensitive
            // Note: This test verifies the security posture - tokens should be case-sensitive for better security
            var expectedStatus = tokenVariation == ValidToken ? 200 : 401;
            await AssertResponseStatus(response, expectedStatus, $"Token Case Sensitivity: {tokenVariation}");

            AssertPerformanceThreshold(elapsedMs, AuthPerformanceThresholdMs, "Token Case Check");
        }

        Console.WriteLine("✅ Token Case Sensitivity: Validated security requirements for token case handling");
    }
}

