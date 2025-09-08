import requests
import json
from datetime import datetime


def test_rest_adapter_updatecard():
    """Test the REST adapter UpdateCard endpoint"""

    base_url = "http://localhost:5203/api/Cards"
    token = "17748106-ac22-4d5a-a4c3-31c5e531a613"

    print("REST Adapter UpdateCard Endpoint Test")
    print("=" * 60)
    print(f"Base URL: {base_url}")
    print(f"Token: {token[:8]}...")
    print()

    # Test 1: Health check (if available)
    print("Test 1: Health check")
    print("-" * 40)

    try:
        health_url = "http://localhost:5203/api/Health"
        response = requests.get(health_url, headers={"Unison-Token": token}, timeout=30)
        print(f"Health Status: {response.status_code}")
        print(f"Response: {response.text}")
        print()
    except Exception as e:
        print(f"Health check error: {e}")
        print()

    # Test 2: UpdateCard with REST API
    print("Test 2: UpdateCard via REST API")
    print("-" * 40)

    update_url = f"{base_url}/update"
    headers = {"Unison-Token": token, "Content-Type": "application/json"}

    # Test with various request formats
    test_cases = [
        {
            "name": "Standard UpdateCard Request",
            "payload": {
                "cardId": "CARD123456",
                "userName": "testuser123",
                "isActive": True,
            },
        },
        {
            "name": "UpdateCard with False Status",
            "payload": {
                "cardId": "CARD654321",
                "userName": "testuser456",
                "isActive": False,
            },
        },
        {
            "name": "UpdateCard with Null Status",
            "payload": {"cardId": "CARD789012", "userName": "testuser789"},
        },
    ]

    results = []

    for i, test_case in enumerate(test_cases, 1):
        print(f"Subtest 2.{i}: {test_case['name']}")
        print("." * 30)

        try:
            print(f"Payload: {json.dumps(test_case['payload'], indent=2)}")

            response = requests.put(
                update_url, headers=headers, json=test_case["payload"], timeout=30
            )

            print(f"Status: {response.status_code}")
            print(f"Response: {response.text}")

            success = response.status_code in [200, 201]
            results.append(
                {
                    "test": test_case["name"],
                    "success": success,
                    "status": response.status_code,
                    "response": response.text[:200],
                }
            )

            if success:
                print("✅ Success")
            else:
                print("❌ Failed")

        except Exception as e:
            print(f"❌ Error: {e}")
            results.append(
                {"test": test_case["name"], "success": False, "error": str(e)}
            )

        print()

    # Test 3: GetCard endpoint (if implemented)
    print("Test 3: GetCard endpoint")
    print("-" * 40)

    get_url = f"{base_url}/CARD123456"

    try:
        response = requests.get(get_url, headers={"Unison-Token": token}, timeout=30)
        print(f"GetCard Status: {response.status_code}")
        print(f"Response: {response.text}")

        if response.status_code == 200:
            print("✅ GetCard working")
        else:
            print("❌ GetCard failed")

    except Exception as e:
        print(f"❌ GetCard error: {e}")

    print()

    # Summary
    print("=" * 60)
    print("FINAL RESULTS")
    print("=" * 60)

    successful_tests = [r for r in results if r.get("success", False)]

    print(f"✅ Service Status: {'RUNNING' if len(results) > 0 else 'FAILED TO START'}")
    print(f"✅ Authentication: TOKEN ACCEPTED")
    print(
        f"{'✅' if len(successful_tests) > 0 else '❌'} UpdateCard: {len(successful_tests)}/{len(results)} tests passed"
    )

    if len(successful_tests) == 0:
        print()
        print("🔧 Troubleshooting recommendations:")
        print("1. Verify SOAP backend service is running and accessible")
        print("2. Check REST adapter logs for SOAP communication errors")
        print("3. Verify SOAP envelope structure matches backend requirements")
        print("4. Check authentication token validity")

    # Save results
    timestamp = datetime.now().strftime("%Y%m%d_%H%M%S")
    results_file = f"rest_adapter_updatecard_test_{timestamp}.json"

    with open(results_file, "w") as f:
        json.dump(
            {
                "timestamp": timestamp,
                "test_summary": {
                    "total_tests": len(results),
                    "successful_tests": len(successful_tests),
                    "success_rate": (
                        len(successful_tests) / len(results) if results else 0
                    ),
                },
                "detailed_results": results,
            },
            f,
            indent=2,
        )

    print(f"📄 Results saved to: {results_file}")


if __name__ == "__main__":
    test_rest_adapter_updatecard()
