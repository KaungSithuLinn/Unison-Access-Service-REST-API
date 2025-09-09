using System;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using UnisonRestAdapter.Models.Requests;

namespace UnisonRestAdapter.Tests.Manual
{
    class ErrorHandlingManualTest
    {
        private static readonly HttpClient client = new HttpClient();

        static async Task Main(string[] args)
        {
            Console.WriteLine("Testing ErrorHandlingMiddleware...");

            // Test 1: Invalid endpoint (should return 404)
            await TestInvalidEndpoint();

            // Test 2: Missing required token (should return 401)
            await TestMissingToken();

            // Test 3: Invalid JSON (should return 400)
            await TestInvalidJson();

            // Test 4: Invalid HTTP method (should return 405)
            await TestInvalidHttpMethod();

            Console.WriteLine("\nAll tests completed!");
        }

        static async Task TestInvalidEndpoint()
        {
            Console.WriteLine("\n--- Test 1: Invalid Endpoint ---");
            try
            {
                var response = await client.GetAsync("http://localhost:5203/api/invalid");
                var content = await response.Content.ReadAsStringAsync();
                Console.WriteLine($"Status: {(int)response.StatusCode}");
                Console.WriteLine($"Response: {content}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Connection error: {ex.Message}");
            }
        }

        static async Task TestMissingToken()
        {
            Console.WriteLine("\n--- Test 2: Missing Token ---");
            try
            {
                var response = await client.GetAsync("http://localhost:5203/api/users/test");
                var content = await response.Content.ReadAsStringAsync();
                Console.WriteLine($"Status: {(int)response.StatusCode}");
                Console.WriteLine($"Response: {content}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Connection error: {ex.Message}");
            }
        }

        static async Task TestInvalidJson()
        {
            Console.WriteLine("\n--- Test 3: Invalid JSON ---");
            try
            {
                var jsonContent = new StringContent("{ invalid json", Encoding.UTF8, "application/json");
                jsonContent.Headers.Add("Unison-Token", "test-token");
                var response = await client.PostAsync("http://localhost:5203/api/users/updatecard", jsonContent);
                var content = await response.Content.ReadAsStringAsync();
                Console.WriteLine($"Status: {(int)response.StatusCode}");
                Console.WriteLine($"Response: {content}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Connection error: {ex.Message}");
            }
        }

        static async Task TestInvalidHttpMethod()
        {
            Console.WriteLine("\n--- Test 4: Invalid HTTP Method ---");
            try
            {
                var request = new HttpRequestMessage(HttpMethod.Patch, "http://localhost:5203/api/users/test");
                request.Headers.Add("Unison-Token", "test-token");
                var response = await client.SendAsync(request);
                var content = await response.Content.ReadAsStringAsync();
                Console.WriteLine($"Status: {(int)response.StatusCode}");
                Console.WriteLine($"Response: {content}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Connection error: {ex.Message}");
            }
        }
    }
}
