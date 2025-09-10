import { defineConfig, devices } from "@playwright/test";

/**
 * Playwright Configuration for TASK-004: Comprehensive Test Suite
 * E2E testing configuration for Unison REST Adapter API
 */
export default defineConfig({
  testDir: "./e2e",
  fullyParallel: true,
  forbidOnly: !!process.env.CI,
  retries: process.env.CI ? 2 : 0,
  workers: process.env.CI ? 1 : undefined,
  reporter: [
    ["html"],
    ["junit", { outputFile: "test-results/junit-results.xml" }],
    ["json", { outputFile: "test-results/results.json" }],
  ],
  use: {
    baseURL: process.env.API_BASE_URL || "http://localhost:5203",
    trace: "on-first-retry",
    extraHTTPHeaders: {
      "Content-Type": "application/json",
    },
  },

  projects: [
    {
      name: "api-tests",
      use: {
        ...devices["Desktop Chrome"],
        // API tests don't need browser context
      },
    },
  ],

  webServer: {
    command: "dotnet run --project ../UnisonRestAdapter",
    port: 5203,
    reuseExistingServer: !process.env.CI,
    timeout: 30000,
  },
});
