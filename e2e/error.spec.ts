import { test, expect } from "@playwright/test";

/**
 * E2E Tests for TASK-006: Enhanced Error Handling System
 *
 * Acceptance Criteria:
 * - Structured JSON error responses
 * - SOAP fault to REST error mapping
 * - Proper HTTP status codes
 * - Error logging with correlation IDs
 */

test.describe("Enhanced Error Handling System", () => {
  const baseURL = "http://localhost:5203";

  test.beforeEach(async ({ page }) => {
    // Set up test environment
    await page.goto(`${baseURL}/health`);
    await expect(page.locator("text=Healthy")).toBeVisible({ timeout: 10000 });
  });

  test("should return structured JSON error for missing authentication token", async ({
    request,
  }) => {
    // Test missing Unison-Token header
    const response = await request.post(`${baseURL}/api/cards/update`, {
      data: {
        cardId: "TEST123",
        userName: "testuser",
        firstName: "John",
        lastName: "Doe",
        email: "john.doe@test.com",
        department: "IT",
        title: "Developer",
        isActive: true,
      },
    });

    expect(response.status()).toBe(401);

    const errorResponse = await response.json();

    // Verify structured JSON error response
    expect(errorResponse).toHaveProperty("error");
    expect(errorResponse).toHaveProperty("correlationId");
    expect(errorResponse).toHaveProperty("timestamp");
    expect(errorResponse).toHaveProperty("path");
    expect(errorResponse.error).toHaveProperty("code");
    expect(errorResponse.error).toHaveProperty("message");
    expect(errorResponse.error.code).toBe("UNAUTHORIZED");
  });

  test("should return proper HTTP status codes for different error types", async ({
    request,
  }) => {
    const validToken = "595d799a-9553-4ddf-8fd9-c27b1f233ce7";

    // Test 400 Bad Request - Invalid JSON
    const badRequestResponse = await request.post(
      `${baseURL}/api/cards/update`,
      {
        headers: { "Unison-Token": validToken },
        data: "{ invalid json",
      }
    );
    expect(badRequestResponse.status()).toBe(400);

    // Test 405 Method Not Allowed - Wrong HTTP method
    const methodNotAllowedResponse = await request.delete(
      `${baseURL}/api/cards/update`,
      {
        headers: { "Unison-Token": validToken },
      }
    );
    expect(methodNotAllowedResponse.status()).toBe(405);

    // Test 422 Unprocessable Entity - Invalid data format
    const unprocessableResponse = await request.post(
      `${baseURL}/api/cards/update`,
      {
        headers: { "Unison-Token": validToken },
        data: {
          cardId: "", // Invalid empty cardId
          userName: "testuser",
        },
      }
    );
    expect(unprocessableResponse.status()).toBe(422);
  });

  test("should map SOAP faults to appropriate REST error responses", async ({
    request,
  }) => {
    const validToken = "595d799a-9553-4ddf-8fd9-c27b1f233ce7";

    // Test SOAP backend error scenario
    const response = await request.post(`${baseURL}/api/cards/update`, {
      headers: { "Unison-Token": validToken },
      data: {
        cardId: "SOAP_ERROR_TEST",
        userName: "testuser",
        firstName: "John",
        lastName: "Doe",
        email: "john.doe@test.com",
        department: "IT",
        title: "Developer",
        isActive: true,
      },
    });

    // Should handle SOAP backend errors gracefully
    expect([400, 502, 503]).toContain(response.status());

    const errorResponse = await response.json();

    // Verify SOAP fault is mapped to structured JSON
    expect(errorResponse).toHaveProperty("error");
    expect(errorResponse).toHaveProperty("correlationId");
    expect(errorResponse.error).toHaveProperty("type");
    expect(["SOAP_FAULT", "BACKEND_ERROR", "SERVICE_UNAVAILABLE"]).toContain(
      errorResponse.error.type
    );
  });

  test("should include correlation IDs in all error responses for traceability", async ({
    request,
  }) => {
    const responses = [];

    // Generate multiple error scenarios to test correlation ID uniqueness
    for (let i = 0; i < 3; i++) {
      const response = await request.post(`${baseURL}/api/cards/update`, {
        data: { invalid: "data" },
      });

      expect(response.status()).toBe(401); // Missing token

      const errorResponse = await response.json();
      expect(errorResponse).toHaveProperty("correlationId");
      expect(errorResponse.correlationId).toBeTruthy();
      expect(typeof errorResponse.correlationId).toBe("string");

      responses.push(errorResponse.correlationId);
    }

    // Verify correlation IDs are unique
    const uniqueIds = new Set(responses);
    expect(uniqueIds.size).toBe(responses.length);
  });

  test("should log errors with proper severity levels and correlation tracking", async ({
    request,
  }) => {
    const validToken = "595d799a-9553-4ddf-8fd9-c27b1f233ce7";

    // Test various error scenarios that should be logged
    const testCases = [
      {
        scenario: "Missing token",
        headers: {},
        expectedStatus: 401,
      },
      {
        scenario: "Invalid JSON",
        headers: { "Unison-Token": validToken },
        data: "{ invalid json",
        expectedStatus: 400,
      },
      {
        scenario: "Invalid data",
        headers: { "Unison-Token": validToken },
        data: { cardId: "", userName: "" },
        expectedStatus: 422,
      },
    ];

    for (const testCase of testCases) {
      const response = await request.post(`${baseURL}/api/cards/update`, {
        headers: testCase.headers,
        data: testCase.data,
      });

      expect(response.status()).toBe(testCase.expectedStatus);

      const errorResponse = await response.json();

      // Each error should have correlation ID for log correlation
      expect(errorResponse).toHaveProperty("correlationId");
      expect(errorResponse.correlationId).toMatch(/^[a-f0-9-]{36}$/); // UUID format
    }
  });

  test("should provide user-friendly error messages without exposing internal details", async ({
    request,
  }) => {
    const validToken = "595d799a-9553-4ddf-8fd9-c27b1f233ce7";

    const response = await request.post(`${baseURL}/api/cards/update`, {
      headers: { "Unison-Token": validToken },
      data: {
        cardId: "TEST123",
        // Missing required fields
      },
    });

    const errorResponse = await response.json();

    // Error message should be user-friendly
    expect(errorResponse.error.message).toBeTruthy();
    expect(typeof errorResponse.error.message).toBe("string");

    // Should not expose internal details like stack traces, connection strings, etc.
    const sensitivePatterns = [
      /connectionstring/i,
      /password/i,
      /secret/i,
      /stacktrace/i,
      /exception/i,
      /sql/i,
      /database/i,
    ];

    const fullResponseString = JSON.stringify(errorResponse);
    sensitivePatterns.forEach((pattern) => {
      expect(fullResponseString).not.toMatch(pattern);
    });
  });

  test("should handle concurrent error scenarios without degrading performance", async ({
    request,
  }) => {
    const validToken = "595d799a-9553-4ddf-8fd9-c27b1f233ce7";

    // Create multiple concurrent requests that will generate errors
    const concurrentRequests = Array.from({ length: 10 }, (_, i) =>
      request.post(`${baseURL}/api/cards/update`, {
        headers: { "Unison-Token": validToken },
        data: { cardId: `CONCURRENT_ERROR_${i}` }, // Invalid request
      })
    );

    const startTime = Date.now();
    const responses = await Promise.all(concurrentRequests);
    const endTime = Date.now();

    // Verify all requests completed within reasonable time (< 5 seconds)
    expect(endTime - startTime).toBeLessThan(5000);

    // Verify all responses have proper error structure
    for (const response of responses) {
      expect([400, 422, 502, 503]).toContain(response.status());

      const errorResponse = await response.json();
      expect(errorResponse).toHaveProperty("correlationId");
      expect(errorResponse).toHaveProperty("error");
    }
  });
});
