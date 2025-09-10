import { test, expect, APIRequestContext } from "@playwright/test";

/**
 * E2E Tests for TASK-002: SOAP Request Validation Templates
 * Tests validation logic for UpdateCard JSON-to-SOAP conversion
 */

// Test Configuration
const API_BASE_URL = process.env.API_BASE_URL || "http://localhost:5203";
const VALID_TOKEN = process.env.UNISON_TOKEN || "test-token-123";

test.describe("UpdateCard Validation", () => {
  let apiContext: APIRequestContext;

  test.beforeAll(async ({ request }) => {
    apiContext = request;
  });

  test.describe("Authentication Validation", () => {
    test("should return 401 for missing Unison-Token header", async () => {
      const response = await apiContext.post(`${API_BASE_URL}/updatecard`, {
        data: {
          CardNumber: "12345",
          Action: "update",
        },
      });

      expect(response.status()).toBe(401);
      const body = await response.json();
      expect(body).toMatchObject({
        error: "Unauthorized",
        message: expect.stringContaining("Unison-Token"),
      });
    });

    test("should return 401 for invalid Unison-Token header", async () => {
      const response = await apiContext.post(`${API_BASE_URL}/updatecard`, {
        headers: {
          "Unison-Token": "invalid-token",
        },
        data: {
          CardNumber: "12345",
          Action: "update",
        },
      });

      expect(response.status()).toBe(401);
    });
  });

  test.describe("JSON Schema Validation", () => {
    test("should validate required fields in UpdateCard request", async () => {
      const response = await apiContext.post(`${API_BASE_URL}/updatecard`, {
        headers: {
          "Unison-Token": VALID_TOKEN,
        },
        data: {}, // Empty payload
      });

      expect(response.status()).toBe(400);
      const body = await response.json();
      expect(body).toMatchObject({
        success: false,
        message: expect.stringContaining("required"),
      });
    });

    test("should validate CardNumber format", async () => {
      const response = await apiContext.post(`${API_BASE_URL}/updatecard`, {
        headers: {
          "Unison-Token": VALID_TOKEN,
        },
        data: {
          CardNumber: "invalid-format",
          Action: "update",
        },
      });

      expect(response.status()).toBe(400);
      const body = await response.json();
      expect(body).toMatchObject({
        success: false,
        message: expect.stringContaining("CardNumber"),
      });
    });

    test("should validate Action enum values", async () => {
      const response = await apiContext.post(`${API_BASE_URL}/updatecard`, {
        headers: {
          "Unison-Token": VALID_TOKEN,
        },
        data: {
          CardNumber: "12345",
          Action: "invalid-action",
        },
      });

      expect(response.status()).toBe(400);
      const body = await response.json();
      expect(body).toMatchObject({
        success: false,
        message: expect.stringContaining("Action"),
      });
    });
  });

  test.describe("SOAP Template Generation", () => {
    test("should accept valid UpdateCard request and generate proper SOAP envelope", async () => {
      // Mock the SOAP service to return success
      const response = await apiContext.post(`${API_BASE_URL}/updatecard`, {
        headers: {
          "Unison-Token": VALID_TOKEN,
        },
        data: {
          CardNumber: "12345",
          Action: "update",
          PersonId: 123,
          FirstName: "John",
          LastName: "Doe",
        },
      });

      expect(response.status()).toBe(200);
      const body = await response.json();
      expect(body).toMatchObject({
        success: true,
        cardNumber: "12345",
      });
    });

    test("should handle SOAP backend HTML errors gracefully", async () => {
      // This test requires mocking the SOAP service to return HTML error
      const response = await apiContext.post(`${API_BASE_URL}/updatecard`, {
        headers: {
          "Unison-Token": VALID_TOKEN,
        },
        data: {
          CardNumber: "trigger-html-error", // Special test case
          Action: "update",
        },
      });

      expect(response.status()).toBe(200); // or 400
      const body = await response.json();
      expect(body).toMatchObject({
        success: false,
        message: expect.stringContaining("HTTP Error"),
      });
    });
  });

  test.describe("Error Response Structure", () => {
    test("should return structured JSON error responses", async () => {
      const response = await apiContext.post(`${API_BASE_URL}/updatecard`, {
        headers: {
          "Unison-Token": VALID_TOKEN,
        },
        data: {
          CardNumber: "12345",
          // Missing required Action field
        },
      });

      expect(response.status()).toBe(400);
      const body = await response.json();

      // Verify structured error format
      expect(body).toHaveProperty("success", false);
      expect(body).toHaveProperty("message");
      expect(body).toHaveProperty("correlationId");
      expect(body).toHaveProperty("timestamp");
      expect(typeof body.correlationId).toBe("string");
      expect(typeof body.timestamp).toBe("string");
    });
  });
});
