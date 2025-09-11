using UnisonRestAdapter.IntegrationTests.Infrastructure;
using FluentAssertions;
using Microsoft.Playwright;

namespace UnisonRestAdapter.IntegrationTests.Tests;

/// <summary>
/// Integration tests for Card Management endpoints
/// Issue #7: Integration Testing with Playwright
/// 
/// Tests all card-related endpoints with comprehensive validation:
/// - PUT /api/cards/update (UpdateCard)
/// - GET /api/cards/{cardId} (GetCard)
/// - POST /api/cards/validate (ValidateCard)
/// - PUT /api/cards/{cardId}/activate (ActivateCard)
/// - PUT /api/cards/{cardId}/deactivate (DeactivateCard)
/// 
/// Performance Requirements: All endpoints under 1000ms response time
/// Authentication: Requires Unison-Token header for all operations
/// </summary>
[TestFixture]
public class CardEndpointsTests : PlaywrightTestBase
{
    private const int CardEndpointPerformanceThresholdMs = 1000;

    [Test]
    [Description("Tests UpdateCard endpoint with valid authentication and data")]
    public async Task UpdateCard_ValidRequest_ShouldUpdateCardSuccessfully()
    {
        // Arrange
        const string endpoint = "/api/cards/update";
        var requestData = CreateUpdateCardRequest();
        var headers = CreateAuthHeaders();

        // Act
        var (response, elapsedMs) = await MeasureResponseTime(async () =>
            await ApiContext.PutAsync(endpoint, new APIRequestContextOptions
            {
                Headers = headers,
                Data = Newtonsoft.Json.JsonConvert.SerializeObject(requestData)
            })
        );

        // Assert - Status Code
        await AssertResponseStatus(response, 200, "UpdateCard Valid");

        // Assert - Performance
        AssertPerformanceThreshold(elapsedMs, CardEndpointPerformanceThresholdMs, "UpdateCard Valid");

        // Assert - Response Content
        var jsonContent = await AssertValidJsonResponse(response, "UpdateCard Valid");

        var result = Newtonsoft.Json.JsonConvert.DeserializeObject<dynamic>(jsonContent);
        result.Should().NotBeNull();

        Console.WriteLine("✅ UpdateCard Valid: Card updated successfully with valid authentication");
    }

    [Test]
    [Description("Tests UpdateCard endpoint with invalid authentication token")]
    public async Task UpdateCard_InvalidToken_ShouldReturnUnauthorized()
    {
        // Arrange
        const string endpoint = "/api/cards/update";
        var requestData = CreateUpdateCardRequest();
        var headers = CreateAuthHeaders(InvalidToken);

        // Act
        var (response, elapsedMs) = await MeasureResponseTime(async () =>
            await ApiContext.PutAsync(endpoint, new APIRequestContextOptions
            {
                Headers = headers,
                Data = Newtonsoft.Json.JsonConvert.SerializeObject(requestData)
            })
        );

        // Assert - Status Code (should be 401 Unauthorized)
        await AssertResponseStatus(response, 401, "UpdateCard Invalid Token");

        // Assert - Performance (even unauthorized requests should be fast)
        AssertPerformanceThreshold(elapsedMs, CardEndpointPerformanceThresholdMs, "UpdateCard Invalid Token");

        Console.WriteLine("✅ UpdateCard Invalid Token: Properly rejected unauthorized request");
    }

    [Test]
    [Description("Tests UpdateCard endpoint with missing authentication token")]
    public async Task UpdateCard_MissingToken_ShouldReturnUnauthorized()
    {
        // Arrange
        const string endpoint = "/api/cards/update";
        var requestData = CreateUpdateCardRequest();
        var headers = new Dictionary<string, string>
        {
            ["Content-Type"] = "application/json"
            // Deliberately omitting Unison-Token header
        };

        // Act
        var (response, elapsedMs) = await MeasureResponseTime(async () =>
            await ApiContext.PutAsync(endpoint, new APIRequestContextOptions
            {
                Headers = headers,
                Data = Newtonsoft.Json.JsonConvert.SerializeObject(requestData)
            })
        );

        // Assert - Status Code (should be 401 Unauthorized)
        await AssertResponseStatus(response, 401, "UpdateCard Missing Token");

        // Assert - Performance
        AssertPerformanceThreshold(elapsedMs, CardEndpointPerformanceThresholdMs, "UpdateCard Missing Token");

        Console.WriteLine("✅ UpdateCard Missing Token: Properly rejected request without authentication");
    }

    [Test]
    [Description("Tests UpdateCard endpoint with invalid request data")]
    public async Task UpdateCard_InvalidData_ShouldReturnBadRequest()
    {
        // Arrange
        const string endpoint = "/api/cards/update";
        var invalidRequestData = new { }; // Empty object - missing required fields
        var headers = CreateAuthHeaders();

        // Act
        var (response, elapsedMs) = await MeasureResponseTime(async () =>
            await ApiContext.PutAsync(endpoint, new APIRequestContextOptions
            {
                Headers = headers,
                Data = Newtonsoft.Json.JsonConvert.SerializeObject(invalidRequestData)
            })
        );

        // Assert - Status Code (should be 400 Bad Request)
        await AssertResponseStatus(response, 400, "UpdateCard Invalid Data");

        // Assert - Performance
        AssertPerformanceThreshold(elapsedMs, CardEndpointPerformanceThresholdMs, "UpdateCard Invalid Data");

        Console.WriteLine("✅ UpdateCard Invalid Data: Properly validated and rejected invalid request");
    }

    [Test]
    [Description("Tests GetCard endpoint with valid card ID and authentication")]
    public async Task GetCard_ValidCardId_ShouldReturnCardDetails()
    {
        // Arrange
        var cardId = Config.TestData.ValidCardId;
        var endpoint = $"/api/cards/{cardId}";
        var headers = CreateAuthHeaders();

        // Act
        var (response, elapsedMs) = await MeasureResponseTime(async () =>
            await ApiContext.GetAsync(endpoint, new APIRequestContextOptions
            {
                Headers = headers
            })
        );

        // Assert - Status Code
        await AssertResponseStatus(response, 200, "GetCard Valid");

        // Assert - Performance
        AssertPerformanceThreshold(elapsedMs, CardEndpointPerformanceThresholdMs, "GetCard Valid");

        // Assert - Response Content
        var jsonContent = await AssertValidJsonResponse(response, "GetCard Valid");

        var cardDetails = Newtonsoft.Json.JsonConvert.DeserializeObject<dynamic>(jsonContent);
        cardDetails.Should().NotBeNull();

        Console.WriteLine("✅ GetCard Valid: Retrieved card details successfully for ID: {CardId}", cardId);
    }

    [Test]
    [Description("Tests GetCard endpoint with invalid card ID")]
    public async Task GetCard_InvalidCardId_ShouldReturnNotFound()
    {
        // Arrange
        var invalidCardId = Config.TestData.InvalidCardId;
        var endpoint = $"/api/cards/{invalidCardId}";
        var headers = CreateAuthHeaders();

        // Act
        var (response, elapsedMs) = await MeasureResponseTime(async () =>
            await ApiContext.GetAsync(endpoint, new APIRequestContextOptions
            {
                Headers = headers
            })
        );

        // Assert - Status Code (should be 404 Not Found)
        await AssertResponseStatus(response, 404, "GetCard Invalid ID");

        // Assert - Performance
        AssertPerformanceThreshold(elapsedMs, CardEndpointPerformanceThresholdMs, "GetCard Invalid ID");

        Console.WriteLine("Logging statement");
    }

    [Test]
    [Description("Tests ValidateCard endpoint with valid card data")]
    public async Task ValidateCard_ValidData_ShouldReturnValidationResult()
    {
        // Arrange
        const string endpoint = "/api/cards/validate";
        var requestData = new
        {
            cardId = Config.TestData.ValidCardId,
            userName = Config.TestData.TestUser.UserName
        };
        var headers = CreateAuthHeaders();

        // Act
        var (response, elapsedMs) = await MeasureResponseTime(async () =>
            await ApiContext.PostAsync(endpoint, new APIRequestContextOptions
            {
                Headers = headers,
                Data = Newtonsoft.Json.JsonConvert.SerializeObject(requestData)
            })
        );

        // Assert - Status Code
        await AssertResponseStatus(response, 200, "ValidateCard Valid");

        // Assert - Performance
        AssertPerformanceThreshold(elapsedMs, CardEndpointPerformanceThresholdMs, "ValidateCard Valid");

        // Assert - Response Content
        var jsonContent = await AssertValidJsonResponse(response, "ValidateCard Valid");

        var validationResult = Newtonsoft.Json.JsonConvert.DeserializeObject<dynamic>(jsonContent);
        validationResult.Should().NotBeNull();

        Console.WriteLine("✅ ValidateCard Valid: Card validation completed successfully");
    }

    [Test]
    [Description("Tests ActivateCard endpoint with valid card ID")]
    public async Task ActivateCard_ValidCardId_ShouldActivateSuccessfully()
    {
        // Arrange
        var cardId = Config.TestData.ValidCardId;
        var endpoint = $"/api/cards/{cardId}/activate";
        var headers = CreateAuthHeaders();

        // Act
        var (response, elapsedMs) = await MeasureResponseTime(async () =>
            await ApiContext.PutAsync(endpoint, new APIRequestContextOptions
            {
                Headers = headers
            })
        );

        // Assert - Status Code
        await AssertResponseStatus(response, 200, "ActivateCard Valid");

        // Assert - Performance
        AssertPerformanceThreshold(elapsedMs, CardEndpointPerformanceThresholdMs, "ActivateCard Valid");

        // Assert - Response Content
        var jsonContent = await AssertValidJsonResponse(response, "ActivateCard Valid");

        var activationResult = Newtonsoft.Json.JsonConvert.DeserializeObject<dynamic>(jsonContent);
        activationResult.Should().NotBeNull();

        Console.WriteLine("✅ ActivateCard Valid: Card activated successfully for ID: {CardId}", cardId);
    }

    [Test]
    [Description("Tests DeactivateCard endpoint with valid card ID")]
    public async Task DeactivateCard_ValidCardId_ShouldDeactivateSuccessfully()
    {
        // Arrange
        var cardId = Config.TestData.ValidCardId;
        var endpoint = $"/api/cards/{cardId}/deactivate";
        var headers = CreateAuthHeaders();

        // Act
        var (response, elapsedMs) = await MeasureResponseTime(async () =>
            await ApiContext.PutAsync(endpoint, new APIRequestContextOptions
            {
                Headers = headers
            })
        );

        // Assert - Status Code
        await AssertResponseStatus(response, 200, "DeactivateCard Valid");

        // Assert - Performance
        AssertPerformanceThreshold(elapsedMs, CardEndpointPerformanceThresholdMs, "DeactivateCard Valid");

        // Assert - Response Content
        var jsonContent = await AssertValidJsonResponse(response, "DeactivateCard Valid");

        var deactivationResult = Newtonsoft.Json.JsonConvert.DeserializeObject<dynamic>(jsonContent);
        deactivationResult.Should().NotBeNull();

        Console.WriteLine("✅ DeactivateCard Valid: Card deactivated successfully for ID: {CardId}", cardId);
    }

    [Test]
    [Description("Tests card operations workflow: Update -> Get -> Validate -> Activate -> Deactivate")]
    public async Task CardOperations_FullWorkflow_ShouldCompleteSuccessfully()
    {
        // Arrange
        var cardId = Config.TestData.ValidCardId;
        var headers = CreateAuthHeaders();
        var workflowSteps = new List<string>();

        try
        {
            // Step 1: Update Card
            workflowSteps.Add("UpdateCard");
            var updateRequest = CreateUpdateCardRequest(cardId);
            var updateResponse = await ApiContext.PutAsync("/api/cards/update", new APIRequestContextOptions
            {
                Headers = headers,
                Data = Newtonsoft.Json.JsonConvert.SerializeObject(updateRequest)
            });
            await AssertResponseStatus(updateResponse, 200, "Workflow UpdateCard");

            // Step 2: Get Card
            workflowSteps.Add("GetCard");
            var getResponse = await ApiContext.GetAsync($"/api/cards/{cardId}", new APIRequestContextOptions
            {
                Headers = headers
            });
            await AssertResponseStatus(getResponse, 200, "Workflow GetCard");

            // Step 3: Validate Card
            workflowSteps.Add("ValidateCard");
            var validateRequest = new { cardId, userName = Config.TestData.TestUser.UserName };
            var validateResponse = await ApiContext.PostAsync("/api/cards/validate", new APIRequestContextOptions
            {
                Headers = headers,
                Data = Newtonsoft.Json.JsonConvert.SerializeObject(validateRequest)
            });
            await AssertResponseStatus(validateResponse, 200, "Workflow ValidateCard");

            // Step 4: Activate Card
            workflowSteps.Add("ActivateCard");
            var activateResponse = await ApiContext.PutAsync($"/api/cards/{cardId}/activate", new APIRequestContextOptions
            {
                Headers = headers
            });
            await AssertResponseStatus(activateResponse, 200, "Workflow ActivateCard");

            // Step 5: Deactivate Card
            workflowSteps.Add("DeactivateCard");
            var deactivateResponse = await ApiContext.PutAsync($"/api/cards/{cardId}/deactivate", new APIRequestContextOptions
            {
                Headers = headers
            });
            await AssertResponseStatus(deactivateResponse, 200, "Workflow DeactivateCard");

            Console.WriteLine("Logging statement");
        }
        catch (Exception ex)
        {
            var completedSteps = string.Join(" -> ", workflowSteps);
            Console.WriteLine("Logging statement");
            throw;
        }
    }

    [Test]
    [Description("Tests card endpoints under load with multiple concurrent requests")]
    public async Task CardOperations_ConcurrentLoad_ShouldMaintainPerformance()
    {
        // Arrange
        const int concurrentRequests = 5;
        var cardId = Config.TestData.ValidCardId;
        var headers = CreateAuthHeaders();

        // Act - Execute multiple card operations concurrently
        var tasks = Enumerable.Range(0, concurrentRequests).Select(async i =>
        {
            // Mix different operations to simulate realistic load
            switch (i % 3)
            {
                case 0: // GetCard
                    var getResponse = await ApiContext.GetAsync($"/api/cards/{cardId}", new APIRequestContextOptions
                    {
                        Headers = headers
                    });
                    return new { Operation = "GetCard", Response = getResponse, RequestIndex = i };

                case 1: // ValidateCard
                    var validateRequest = new { cardId, userName = Config.TestData.TestUser.UserName };
                    var validateResponse = await ApiContext.PostAsync("/api/cards/validate", new APIRequestContextOptions
                    {
                        Headers = headers,
                        Data = Newtonsoft.Json.JsonConvert.SerializeObject(validateRequest)
                    });
                    return new { Operation = "ValidateCard", Response = validateResponse, RequestIndex = i };

                default: // UpdateCard
                    var updateRequest = CreateUpdateCardRequest(cardId);
                    var updateResponse = await ApiContext.PutAsync("/api/cards/update", new APIRequestContextOptions
                    {
                        Headers = headers,
                        Data = Newtonsoft.Json.JsonConvert.SerializeObject(updateRequest)
                    });
                    return new { Operation = "UpdateCard", Response = updateResponse, RequestIndex = i };
            }
        });

        var results = await Task.WhenAll(tasks);

        // Assert - All concurrent requests completed successfully
        foreach (var result in results)
        {
            await AssertResponseStatus(result.Response, 200, $"Concurrent {result.Operation} #{result.RequestIndex}");
        }

        Console.WriteLine("Logging statement");
    }
}

