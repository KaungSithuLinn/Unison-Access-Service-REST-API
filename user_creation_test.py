import requests
import datetime

# Configuration
base_url = "http://192.168.10.206:9003/Unison.AccessService"
token = "774e8e5e-2b2c-4a41-8d6d-20a786ec1fea"

# Generate unique user ID for testing
timestamp = datetime.datetime.now().strftime("%Y%m%d_%H%M%S")
test_user_id = f"step2_test_{timestamp}"

print(f"Testing user creation workflow with ID: {test_user_id}")

# Test UpdateUser with query parameters (WORKING FORMAT)
params = {
    "userId": test_user_id,
    "firstName": "Step2",
    "lastName": "Test",
    "pinCode": "9999",
    "validFrom": "2025-09-01T00:00:00",
    "validUntil": "2026-09-01T00:00:00",
    "accessFlags": "0",
}

try:
    response = requests.post(
        f"{base_url}/UpdateUser",
        headers={"Unison-Token": token},
        params=params,
        verify=False,
        timeout=30,
    )

    print(f"Status Code: {response.status_code}")
    print(f"Response: {response.text[:200]}...")

    if response.status_code == 200:
        print(
            "✅ UpdateUser request successful - proceeding with database verification"
        )
        print(f"Test User ID for DB verification: {test_user_id}")
    else:
        print("❌ UpdateUser request failed")

except Exception as e:
    print(f"❌ Request error: {e}")
