using Playwright;
using Microsoft.Playwright;
using Xunit;

namespace UnisonRestAdapter.Tests.E2E
{
    /// <summary>
    /// End-to-end tests using Playwright for browser automation
    /// </summary>
    public class SwaggerE2ETests : IAsyncLifetime
    {
        private IBrowser? _browser;
        private const string BaseUrl = "http://localhost:5203";

        public async Task InitializeAsync()
        {
            var playwright = await Microsoft.Playwright.Playwright.CreateAsync();
            _browser = await playwright.Chromium.LaunchAsync(new BrowserTypeLaunchOptions
            {
                Headless = true
            });
        }

        public async Task DisposeAsync()
        {
            if (_browser != null)
            {
                await _browser.CloseAsync();
            }
        }

        [Fact]
        public async Task SwaggerUI_LoadsCorrectly()
        {
            // Arrange
            var page = await _browser!.NewPageAsync();

            try
            {
                // Act
                await page.GotoAsync($"{BaseUrl}/swagger/index.html");
                await page.WaitForLoadStateAsync(LoadState.NetworkIdle);

                // Assert
                var title = await page.TitleAsync();
                Assert.Contains("Swagger", title);

                var swaggerWrapper = await page.QuerySelectorAsync(".swagger-ui");
                Assert.NotNull(swaggerWrapper);

                // Check if API endpoints are visible
                var endpoints = await page.QuerySelectorAllAsync(".opblock");
                Assert.True(endpoints.Count > 0, "No API endpoints found in Swagger UI");
            }
            finally
            {
                await page.CloseAsync();
            }
        }

        [Fact]
        public async Task SwaggerUI_ShowsUpdateCardEndpoint()
        {
            // Arrange
            var page = await _browser!.NewPageAsync();

            try
            {
                // Act
                await page.GotoAsync($"{BaseUrl}/swagger/index.html");
                await page.WaitForLoadStateAsync(LoadState.NetworkIdle);

                // Assert
                var updateCardEndpoint = await page.GetByText("POST").And(page.GetByText("/api/cards/update")).FirstAsync();
                Assert.NotNull(updateCardEndpoint);

                // Click to expand endpoint details
                await updateCardEndpoint.ClickAsync();
                await page.WaitForTimeoutAsync(1000);

                // Check for authentication requirement
                var authSection = await page.QuerySelectorAsync("text=Unison-Token");
                Assert.NotNull(authSection);
            }
            finally
            {
                await page.CloseAsync();
            }
        }

        [Fact]
        public async Task HealthEndpoint_AccessibleViaSwagger()
        {
            // Arrange
            var page = await _browser!.NewPageAsync();

            try
            {
                // Act
                await page.GotoAsync($"{BaseUrl}/swagger/index.html");
                await page.WaitForLoadStateAsync(LoadState.NetworkIdle);

                // Look for health endpoint
                var healthEndpoint = await page.GetByText("GET").And(page.GetByText("/health")).FirstAsync();
                Assert.NotNull(healthEndpoint);

                // Expand and try to execute
                await healthEndpoint.ClickAsync();
                await page.WaitForTimeoutAsync(1000);

                var tryItOutButton = await page.QuerySelectorAsync("button.try-out__btn");
                if (tryItOutButton != null)
                {
                    await tryItOutButton.ClickAsync();
                    await page.WaitForTimeoutAsync(500);

                    var executeButton = await page.QuerySelectorAsync("button.execute");
                    if (executeButton != null)
                    {
                        await executeButton.ClickAsync();
                        await page.WaitForTimeoutAsync(2000);

                        // Check for successful response
                        var responseCode = await page.QuerySelectorAsync(".response-col_status");
                        Assert.NotNull(responseCode);
                    }
                }
            }
            finally
            {
                await page.CloseAsync();
            }
        }
    }
}
