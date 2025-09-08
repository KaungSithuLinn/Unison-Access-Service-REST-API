import requests
import json


def test_auth_token():
    """Test if the authentication token works with a simple Ping operation"""

    url = "http://192.168.10.206:9001/Unison.AccessService/Ping"
    token = "17748106-ac22-4d5a-a4c3-31c5e531a613"

    headers = {"Content-Type": "application/json", "Unison-Token": token}

    print("Testing authentication token with Ping operation...")
    print(f"URL: {url}")
    print(f"Token: {token[:8]}...")
    print()

    try:
        # Test with empty JSON body
        response = requests.post(url, json={}, headers=headers, timeout=30)

        print(f"Status Code: {response.status_code}")
        print(f"Response Headers: {dict(response.headers)}")
        print(f"Response Content: {response.text}")

        if response.status_code == 200:
            print("✅ Authentication successful!")
            return True
        else:
            print("❌ Authentication failed")
            return False

    except Exception as e:
        print(f"❌ Error: {e}")
        return False


def test_updatecard_with_different_payloads():
    """Test UpdateCard with different payload formats"""

    base_url = "http://192.168.10.206:9001/Unison.AccessService"
    token = "17748106-ac22-4d5a-a4c3-31c5e531a613"

    headers = {"Content-Type": "application/json", "Unison-Token": token}

    # Different payload variations to test
    payloads = [
        {
            "name": "Format from demo results (camelCase)",
            "payload": {
                "UserID": "testuser123",
                "ProfileName": "Default",
                "CardNumber": "CARD123456",
                "CardStatus": 1,
            },
        },
        {
            "name": "Format from API spec (lowercase)",
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
            "name": "Complete format with all fields",
            "payload": {
                "userId": "testuser123",
                "profileName": "Default",
                "cardNumber": "CARD123456",
                "systemNumber": "001",
                "versionNumber": "1",
                "miscNumber": "000",
                "cardStatus": 1,
            },
        },
    ]

    url = f"{base_url}/UpdateCard"

    for i, test_case in enumerate(payloads, 1):
        print(f"\nTest {i}: {test_case['name']}")
        print("-" * 50)
        print(f"Payload: {json.dumps(test_case['payload'], indent=2)}")

        try:
            response = requests.post(
                url, json=test_case["payload"], headers=headers, timeout=30
            )

            print(f"Status: {response.status_code}")
            print(f"Response: {response.text[:300]}...")

            if response.status_code == 200:
                print("✅ SUCCESS!")
                return test_case
            else:
                print(f"❌ Failed with status {response.status_code}")

        except Exception as e:
            print(f"❌ Error: {e}")

    return None


if __name__ == "__main__":
    print("Unison Access Service - Authentication and UpdateCard Testing")
    print("=" * 70)

    # First test authentication
    auth_works = test_auth_token()

    if auth_works:
        print("\n" + "=" * 70)
        print("TESTING UPDATECARD WITH DIFFERENT PAYLOADS")
        print("=" * 70)

        success = test_updatecard_with_different_payloads()

        if success:
            print(f"\n✅ SUCCESS: UpdateCard working with {success['name']}")
        else:
            print("\n❌ All UpdateCard tests failed")
    else:
        print("\n❌ Authentication failed - cannot proceed with UpdateCard tests")
