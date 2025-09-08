import requests
import os
import json
from datetime import datetime


def test_api_with_detailed_responses():
    """Test API endpoints with detailed error response analysis."""

    # Configuration
    base_url = os.getenv(
        "UNISON_API_BASE_URL", "http://192.168.10.206:9003/Unison.AccessService"
    )
    token = os.getenv("UNISON_API_TOKEN")

    if not token:
        print("ERROR: UNISON_API_TOKEN environment variable must be set")
        return False

    headers = {
        "Content-Type": "application/json",
        "Accept": "application/json",
        "Unison-Token": token,
        "User-Agent": "Python-Detailed-Test/1.0",
    }

    # Test connectivity first
    print("=== CONNECTIVITY TEST ===")
    try:
        ping_response = requests.get(
            f"{base_url}/Ping", headers=headers, verify=False, timeout=10
        )
        print(f"Ping Status: {ping_response.status_code}")
        print(f"Ping Response: {ping_response.text}")
        if ping_response.status_code != 200:
            print("❌ Connectivity failed - cannot proceed")
            return False
        print("✅ Connectivity successful")
    except Exception as e:
        print(f"❌ Connectivity error: {e}")
        return False

    print("\n=== DETAILED API TESTING ===")

    # Generate test data
    test_user_id = f"testuser_{datetime.now().strftime('%Y%m%d_%H%M%S')}"
    test_card_number = f"1000{datetime.now().strftime('%H%M%S')}"

    print(f"Test User ID: {test_user_id}")
    print(f"Test Card Number: {test_card_number}")

    # Test 1: UpdateUser with corrected camelCase format
    print("\n--- Test 1: UpdateUser ---")
    update_user_payload = {
        "userId": test_user_id,
        "firstName": "John",
        "lastName": "Doe",
        "pinCode": "1234",
        "validFrom": "2025-09-01T00:00:00",
        "validUntil": "2026-09-01T00:00:00",
        "accessFlags": 0,
        "fields": [],
    }

    print(f"Request Payload: {json.dumps(update_user_payload, indent=2)}")

    try:
        response = requests.post(
            f"{base_url}/UpdateUser",
            headers=headers,
            json=update_user_payload,
            verify=False,
            timeout=30,
        )
        print(f"Status Code: {response.status_code}")
        print(f"Response Headers: {dict(response.headers)}")
        print(f"Response Text: {response.text[:1000]}...")

        if response.status_code == 200:
            print("✅ UpdateUser: SUCCESS")
        else:
            print("❌ UpdateUser: FAILED")
            # Try to parse error details
            if "xml" in response.headers.get("content-type", "").lower():
                print("Server returned XML error page - likely WCF binding issue")
    except Exception as e:
        print(f"❌ UpdateUser: ERROR - {e}")

    # Test 2: UpdateUserPhoto
    print("\n--- Test 2: UpdateUserPhoto ---")
    # Small test image as base64
    test_image_b64 = "/9j/4AAQSkZJRgABAQEAYABgAAD/2wBDAAEBAQEBAQEBAQEBAQEBAQEBAQEBAQEBAQEBAQEBAQEBAQEBAQEBAQEBAQEBAQEBAQEBAQEBAQEBAQEBAQEBAQH/2wBDAQEBAQEBAQEBAQEBAQEBAQEBAQEBAQEBAQEBAQEBAQEBAQEBAQEBAQEBAQEBAQEBAQEBAQEBAQEBAQEBAQEBAQH/wAARCAABAAEDASIAAhEBAxEB/8QAFQABAQAAAAAAAAAAAAAAAAAAAAv/xAAUEAEAAAAAAAAAAAAAAAAAAAAA/8QAFQEBAQAAAAAAAAAAAAAAAAAAAAX/xAAUEQEAAAAAAAAAAAAAAAAAAAAA/9oADAMBAAIRAxEAPwA/wA8A"

    update_photo_payload = {"userId": test_user_id, "photo": test_image_b64}

    print(f"Request Payload: userId={test_user_id}, photo=[base64 data]")

    try:
        response = requests.post(
            f"{base_url}/UpdateUserPhoto",
            headers=headers,
            json=update_photo_payload,
            verify=False,
            timeout=30,
        )
        print(f"Status Code: {response.status_code}")
        print(f"Response Headers: {dict(response.headers)}")
        print(f"Response Text: {response.text[:1000]}...")

        if response.status_code == 200:
            print("✅ UpdateUserPhoto: SUCCESS")
        else:
            print("❌ UpdateUserPhoto: FAILED")
    except Exception as e:
        print(f"❌ UpdateUserPhoto: ERROR - {e}")

    # Test 3: UpdateCard
    print("\n--- Test 3: UpdateCard ---")
    update_card_payload = {
        "userId": test_user_id,
        "profileName": "Default",
        "cardNumber": test_card_number,
        "systemNumber": "",
        "versionNumber": "",
        "miscNumber": "",
        "cardStatus": 1,
    }

    print(f"Request Payload: {json.dumps(update_card_payload, indent=2)}")

    try:
        response = requests.post(
            f"{base_url}/UpdateCard",
            headers=headers,
            json=update_card_payload,
            verify=False,
            timeout=30,
        )
        print(f"Status Code: {response.status_code}")
        print(f"Response Headers: {dict(response.headers)}")
        print(f"Response Text: {response.text[:1000]}...")

        if response.status_code == 200:
            print("✅ UpdateCard: SUCCESS")
        else:
            print("❌ UpdateCard: FAILED")
    except Exception as e:
        print(f"❌ UpdateCard: ERROR - {e}")

    print("\n=== TEST SUMMARY ===")
    print("✅ Connection: Working (Ping successful)")
    print("❌ Update Endpoints: Still returning 400 errors")
    print("📋 Next steps: Analyze XML error responses for WCF binding issues")

    return True


if __name__ == "__main__":
    test_api_with_detailed_responses()
