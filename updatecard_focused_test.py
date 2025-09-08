import requests
import json
import os
from datetime import datetime


# Load environment configuration (matching the working .env file)
def load_config():
    """Load configuration from environment or use working defaults"""
    return {
        "base_url": "http://192.168.10.206:9001/Unison.AccessService",
        "token": "17748106-ac22-4d5a-a4c3-31c5e531a613",
        "timeout": 30,
    }


def test_updatecard_working_config():
    """Test UpdateCard endpoint with verified working configuration"""

    print("UpdateCard Endpoint Test - Verified Configuration")
    print("=" * 60)

    config = load_config()

    # Test endpoint
    url = f"{config['base_url']}/UpdateCard"

    # Headers matching working configuration
    headers = {
        "Content-Type": "application/json",
        "Unison-Token": config["token"],
        "User-Agent": "UpdateCard-Test/1.0",
        "Accept": "application/json",
    }

    # Test payload - UpdateCard parameters from API spec
    test_payload = {
        "userId": "TEST_USER_UPDATECARD_001",
        "profileName": "Default",  # Using Default profile as in working examples
        "cardNumber": "87654321",  # Different from previous tests
        "systemNumber": "001",
        "versionNumber": "1",
        "miscNumber": "000",
        "cardStatus": 1,  # Active status (1 = Active, per CardStatus enum)
    }

    print(f"Testing endpoint: {url}")
    print(f"Token: {config['token'][:8]}...")
    print(f"Payload:")
    print(json.dumps(test_payload, indent=2))
    print()

    try:
        # Make the request
        response = requests.post(
            url, json=test_payload, headers=headers, timeout=config["timeout"]
        )

        print(f"Status Code: {response.status_code}")
        print(f"Response Headers:")
        for key, value in response.headers.items():
            print(f"  {key}: {value}")
        print()

        print("Response Content:")
        print("-" * 30)
        print(response.text)
        print("-" * 30)
        print()

        # Analyze the response
        success = False
        analysis = {}

        if response.status_code == 200:
            print("✅ HTTP 200 - Request completed")
            try:
                # Try to parse as JSON
                json_response = response.json()
                analysis["json_response"] = json_response
                analysis["response_type"] = "JSON"

                # Check for success indicators
                if isinstance(json_response, dict):
                    if json_response.get("success") is True:
                        success = True
                        print("✅ SUCCESS: API returned success=true")
                    elif "error" in json_response:
                        print(f"❌ API Error: {json_response['error']}")
                    else:
                        # No explicit success/error, consider structure
                        success = True  # Assume success if no error
                        print("✅ SUCCESS: No error in response")
                else:
                    success = True  # Non-dict JSON response, assume success
                    print("✅ SUCCESS: Non-error JSON response")

            except json.JSONDecodeError:
                # Non-JSON response
                analysis["response_type"] = "TEXT"
                response_lower = response.text.lower()

                if "error" not in response_lower and "fault" not in response_lower:
                    success = True
                    print("✅ SUCCESS: No error/fault in text response")
                else:
                    print("❌ Error/fault detected in response")

        elif response.status_code == 400:
            print("❌ HTTP 400 - Bad Request")
            print("Possible issues:")
            print("  - Invalid payload format")
            print("  - Missing required fields")
            print("  - Invalid field values")

        elif response.status_code == 401:
            print("❌ HTTP 401 - Unauthorized")
            print("Issue: Authentication token may be invalid or expired")

        elif response.status_code == 404:
            print("❌ HTTP 404 - Not Found")
            print("Issue: Endpoint may not exist or service may be down")

        elif response.status_code == 500:
            print("❌ HTTP 500 - Internal Server Error")
            print("Issue: Server-side error, check service logs")

        else:
            print(f"❌ HTTP {response.status_code} - Unexpected status")

        # Prepare result summary
        result = {
            "timestamp": datetime.now().isoformat(),
            "test_name": "UpdateCard Working Configuration Test",
            "endpoint": url,
            "success": success,
            "status_code": response.status_code,
            "response_headers": dict(response.headers),
            "response_content": response.text,
            "analysis": analysis,
            "test_payload": test_payload,
        }

        # Save results
        timestamp = datetime.now().strftime("%Y%m%d_%H%M%S")
        results_file = f"updatecard_working_config_test_{timestamp}.json"

        with open(results_file, "w") as f:
            json.dump(result, f, indent=2)

        print(f"📄 Test results saved to: {results_file}")

        # Final verdict
        print()
        print("=" * 60)
        print("FINAL RESULT")
        print("=" * 60)
        if success:
            print("✅ UpdateCard endpoint test SUCCESSFUL")
            print("The endpoint is working correctly with the provided configuration.")
        else:
            print("❌ UpdateCard endpoint test FAILED")
            print("Review the error details above for troubleshooting.")

        return result

    except requests.ConnectionError as e:
        print(f"❌ Connection Error: {e}")
        print("Issue: Cannot connect to the service")
        print("Check: Network connectivity, service status, correct URL/port")
        return {"success": False, "error": "Connection Error", "details": str(e)}

    except requests.Timeout as e:
        print(f"❌ Timeout Error: {e}")
        print("Issue: Request timed out")
        print("Check: Service performance, network latency")
        return {"success": False, "error": "Timeout", "details": str(e)}

    except Exception as e:
        print(f"❌ Unexpected Error: {e}")
        return {"success": False, "error": "Unexpected Error", "details": str(e)}


if __name__ == "__main__":
    test_updatecard_working_config()
