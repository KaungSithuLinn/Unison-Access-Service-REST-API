import requests
import json
from datetime import datetime


def test_final_updatecard():
    """Final comprehensive test of UpdateCard endpoint with correct methods"""

    base_url = "http://192.168.10.206:9001/Unison.AccessService"
    token = "17748106-ac22-4d5a-a4c3-31c5e531a613"

    print("Final UpdateCard Endpoint Test")
    print("=" * 60)
    print(f"Base URL: {base_url}")
    print(f"Token: {token[:8]}...")
    print()

    # Test 1: Try Ping with GET (as indicated by Allow header)
    print("Test 1: Ping operation with GET method")
    print("-" * 40)

    ping_url = f"{base_url}/Ping"
    headers = {"Unison-Token": token}

    try:
        response = requests.get(ping_url, headers=headers, timeout=30)
        print(f"GET Ping Status: {response.status_code}")
        print(f"Response: {response.text[:200]}...")

        if response.status_code == 200:
            print("✅ Ping successful - service is responding")
            service_working = True
        else:
            print("❌ Ping failed")
            service_working = False

    except Exception as e:
        print(f"❌ Ping error: {e}")
        service_working = False

    print()

    # Test 2: UpdateCard with various approaches
    print("Test 2: UpdateCard operation testing")
    print("-" * 40)

    updatecard_url = f"{base_url}/UpdateCard"

    # Try different approaches for UpdateCard
    test_cases = [
        {
            "name": "POST with JSON (from demo results)",
            "method": "POST",
            "headers": {"Content-Type": "application/json", "Unison-Token": token},
            "payload": {
                "UserID": "testuser123",
                "ProfileName": "Default",
                "CardNumber": "CARD123456",
                "CardStatus": 1,
            },
        },
        {
            "name": "POST with all fields (API spec format)",
            "method": "POST",
            "headers": {"Content-Type": "application/json", "Unison-Token": token},
            "payload": {
                "userId": "testuser123",
                "profileName": "Default",
                "cardNumber": "CARD123456",
                "systemNumber": "",
                "versionNumber": "",
                "miscNumber": "",
                "cardStatus": 1,
            },
        },
        {
            "name": "GET with query parameters",
            "method": "GET",
            "headers": {"Unison-Token": token},
            "params": {
                "userId": "testuser123",
                "profileName": "Default",
                "cardNumber": "CARD123456",
                "cardStatus": "1",
            },
        },
    ]

    successful_test = None

    for i, test_case in enumerate(test_cases, 1):
        print(f"\nSubtest 2.{i}: {test_case['name']}")
        print("." * 30)

        try:
            if test_case["method"] == "POST":
                if "payload" in test_case:
                    print(f"Payload: {json.dumps(test_case['payload'], indent=2)}")
                    response = requests.post(
                        updatecard_url,
                        json=test_case["payload"],
                        headers=test_case["headers"],
                        timeout=30,
                    )
                else:
                    response = requests.post(
                        updatecard_url, headers=test_case["headers"], timeout=30
                    )
            else:  # GET
                print(f"Parameters: {test_case['params']}")
                response = requests.get(
                    updatecard_url,
                    params=test_case["params"],
                    headers=test_case["headers"],
                    timeout=30,
                )

            print(f"Status: {response.status_code}")
            print(f"Response: {response.text[:200]}...")

            if response.status_code == 200:
                print("✅ SUCCESS!")
                successful_test = test_case
                break
            elif response.status_code == 405:
                print("❌ Method not allowed")
            elif response.status_code == 400:
                print("❌ Bad request")
            elif response.status_code == 401:
                print("❌ Unauthorized")
            else:
                print(f"❌ HTTP {response.status_code}")

        except Exception as e:
            print(f"❌ Error: {e}")

    print("\n" + "=" * 60)
    print("FINAL RESULTS")
    print("=" * 60)

    # Summary
    conclusions = []
    results = {
        "timestamp": datetime.now().isoformat(),
        "service_working": service_working,
        "successful_test": successful_test,
        "endpoint_accessible": True,  # We got responses, so endpoint exists
        "auth_token_valid": service_working,  # If ping worked, token is valid
        "conclusions": conclusions,
    }

    if service_working:
        print("✅ Service Status: OPERATIONAL")
        print("✅ Authentication: TOKEN VALID")
        conclusions.append("Service is operational and token is valid")
    else:
        print("❌ Service Status: Issues detected")
        conclusions.append("Service or authentication issues detected")

    if successful_test:
        print(f"✅ UpdateCard: WORKING ({successful_test['name']})")
        conclusions.append(
            f"UpdateCard endpoint working with {successful_test['name']}"
        )
    else:
        print("❌ UpdateCard: FAILED")
        conclusions.append("UpdateCard endpoint not working with tested methods")

        # Provide troubleshooting guidance
        print("\n🔧 Troubleshooting recommendations:")
        print("1. Check if service accepts SOAP instead of REST")
        print("2. Verify correct endpoint URL and method")
        print("3. Check if additional authentication is required")
        print("4. Review service documentation for correct request format")

    # Save results
    timestamp = datetime.now().strftime("%Y%m%d_%H%M%S")
    results_file = f"final_updatecard_test_{timestamp}.json"

    with open(results_file, "w", encoding="utf-8") as f:
        json.dump(results, f, indent=2)

    print(f"\n📄 Results saved to: {results_file}")

    return results


if __name__ == "__main__":
    test_final_updatecard()
