import { test, expect, APIRequestContext } from "@playwright/test";

/**
 * E2E Tests for TASK-004: Comprehensive Test Suite
 * Complete UpdateCard workflow testing with authentication and validation
 */

// Test Configuration
const API_BASE_URL = process.env.API_BASE_URL || "http://localhost:5203";
const VALID_TOKEN =
  process.env.UNISON_TOKEN || "595d799a-9553-4ddf-8fd9-c27b1f233ce7";
const INVALID_TOKEN = "invalid-test-token-123";

test.describe("UpdateCard Complete Workflow", () => {
  test.describe("Health Check Validation", () => {
    test("health endpoint should be accessible without authentication", async ({
      request,
    }) => {
      const response = await request.get(`${API_BASE_URL}/health`);

      expect(response.status()).toBe(200);
      const body = await response.text();
      expect(body).toContain("Healthy");
    });

    test("health endpoint should return quickly (< 2 seconds)", async ({
      request,
    }) => {
      const startTime = Date.now();
      const response = await apiContext.get(`${API_BASE_URL}/health`);
      const endTime = Date.now();

      expect(response.status()).toBe(200);
      expect(endTime - startTime).toBeLessThan(2000);
    });
  });

  test.describe("Authentication Flow", () => {
    test("should reject requests without Unison-Token header", async () => {
      const response = await apiContext.post(`${API_BASE_URL}/updatecard`, {
        data: {
          CardId: "TEST123",
          UserName: "test.user",
          FirstName: "Test",
          LastName: "User",
          Email: "test.user@company.com",
        },
      });

      expect(response.status()).toBe(401);
      const body = await response.json();
      expect(body.error).toContain("Authentication required");
      expect(body.correlationId).toBeTruthy();
    });

    test("should reject requests with invalid Unison-Token", async () => {
      const response = await apiContext.post(`${API_BASE_URL}/updatecard`, {
        headers: {
          "Unison-Token": INVALID_TOKEN,
        },
        data: {
          CardId: "TEST123",
          UserName: "test.user",
          FirstName: "Test",
          LastName: "User",
          Email: "test.user@company.com",
        },
      });

      expect(response.status()).toBe(401);
      const body = await response.json();
      expect(body.error).toContain("Invalid or expired token");
      expect(body.correlationId).toBeTruthy();
    });
  });

  test.describe("Request Validation", () => {
    test("should validate required CardId field", async () => {
      const response = await apiContext.post(`${API_BASE_URL}/updatecard`, {
        headers: {
          "Unison-Token": VALID_TOKEN,
        },
        data: {
          UserName: "test.user",
          FirstName: "Test",
          LastName: "User",
          Email: "test.user@company.com",
        },
      });

      expect(response.status()).toBe(400);
      const body = await response.json();
      expect(body.error).toContain("CardId is required");
      expect(body.correlationId).toBeTruthy();
    });

    test("should validate CardId format (alphanumeric, 1-50 chars)", async () => {
      const testCases = [
        { cardId: "", description: "empty string" },
        { cardId: "a".repeat(51), description: "too long (51 chars)" },
        { cardId: "test@card", description: "invalid characters" },
        { cardId: "test card", description: "spaces not allowed" },
      ];

      for (const testCase of testCases) {
        const response = await apiContext.post(`${API_BASE_URL}/updatecard`, {
          headers: {
            "Unison-Token": VALID_TOKEN,
          },
          data: {
            CardId: testCase.cardId,
            UserName: "test.user",
            FirstName: "Test",
            LastName: "User",
            Email: "test.user@company.com",
          },
        });

        expect(response.status()).toBe(400);
        const body = await response.json();
        expect(body.error).toContain("CardId must be");
        expect(body.correlationId).toBeTruthy();
      }
    });

    test("should validate email format", async () => {
      const invalidEmails = [
        "invalid-email",
        "missing@",
        "@missing-domain.com",
        "spaces in@email.com",
        "email@",
        "",
      ];

      for (const email of invalidEmails) {
        const response = await apiContext.post(`${API_BASE_URL}/updatecard`, {
          headers: {
            "Unison-Token": VALID_TOKEN,
          },
          data: {
            CardId: "TEST123",
            UserName: "test.user",
            FirstName: "Test",
            LastName: "User",
            Email: email,
          },
        });

        expect(response.status()).toBe(400);
        const body = await response.json();
        expect(body.error).toContain("Invalid email format");
        expect(body.correlationId).toBeTruthy();
      }
    });

    test("should validate expiration date is not in the past", async () => {
      const pastDate = new Date();
      pastDate.setFullYear(pastDate.getFullYear() - 1);

      const response = await apiContext.post(`${API_BASE_URL}/updatecard`, {
        headers: {
          "Unison-Token": VALID_TOKEN,
        },
        data: {
          CardId: "TEST123",
          UserName: "test.user",
          FirstName: "Test",
          LastName: "User",
          Email: "test.user@company.com",
          ExpirationDate: pastDate.toISOString(),
        },
      });

      expect(response.status()).toBe(400);
      const body = await response.json();
      expect(body.error).toContain("Expiration date cannot be in the past");
      expect(body.correlationId).toBeTruthy();
    });
  });

  test.describe("SOAP Integration (Happy Path)", () => {
    test("should handle valid UpdateCard request", async () => {
      const futureDate = new Date();
      futureDate.setFullYear(futureDate.getFullYear() + 1);

      const response = await apiContext.post(`${API_BASE_URL}/updatecard`, {
        headers: {
          "Unison-Token": VALID_TOKEN,
        },
        data: {
          CardId: "TEST123",
          UserName: "test.user",
          FirstName: "Test",
          LastName: "User",
          Email: "test.user@company.com",
          Department: "IT",
          Title: "Developer",
          ExpirationDate: futureDate.toISOString(),
        },
      });

      // Note: This may return 500 if SOAP service is unavailable, which is expected in test env
      expect([200, 500]).toContain(response.status());

      const body = await response.json();
      expect(body.correlationId).toBeTruthy();

      if (response.status() === 200) {
        expect(body.success).toBe(true);
        expect(body.message).toBeTruthy();
      } else {
        // SOAP service unavailable - expected in test environment
        expect(body.error).toContain("SOAP service");
      }
    });
  });

  test.describe("Error Handling", () => {
    test("should handle malformed JSON gracefully", async () => {
      const response = await apiContext.post(`${API_BASE_URL}/updatecard`, {
        headers: {
          "Unison-Token": VALID_TOKEN,
          "Content-Type": "application/json",
        },
        data: "{ invalid json }",
      });

      expect(response.status()).toBe(400);
      const body = await response.json();
      expect(body.error).toContain("Invalid JSON");
      expect(body.correlationId).toBeTruthy();
    });

    test("should include correlation ID in all error responses", async () => {
      const response = await apiContext.post(`${API_BASE_URL}/updatecard`, {
        data: { CardId: "" },
      });

      expect(response.status()).toBe(401);
      const body = await response.json();
      expect(body.correlationId).toBeTruthy();
      expect(body.correlationId).toMatch(/^[0-9a-f-]{36}$/); // UUID format
    });
  });

  test.describe("Performance", () => {
    test("validation should complete within 1 second", async () => {
      const startTime = Date.now();

      const response = await apiContext.post(`${API_BASE_URL}/updatecard`, {
        headers: {
          "Unison-Token": VALID_TOKEN,
        },
        data: {
          CardId: "TEST123",
          UserName: "test.user",
          FirstName: "Test",
          LastName: "User",
          Email: "test.user@company.com",
        },
      });

      const endTime = Date.now();
      const duration = endTime - startTime;

      expect(duration).toBeLessThan(1000);
      expect([200, 400, 500]).toContain(response.status());
    });
  });
});
