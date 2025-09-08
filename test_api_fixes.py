import requests
import os
from datetime import datetime


def test_updated_requests():
    """Test the updated request formats for Unison API endpoints."""

    # Configuration (using environment variables for security)
    base_url = os.getenv(
        "UNISON_API_BASE_URL", "https://192.168.10.206:9001/Unison.AccessService"
    )
    token = os.getenv("UNISON_API_TOKEN")

    if not token:
        print("ERROR: UNISON_API_TOKEN environment variable must be set")
        return False

    # Common headers
    headers = {
        "Content-Type": "application/json",
        "Accept": "application/json",
        "Unison-Token": token,
        "User-Agent": "Python-API-Test/1.0",
    }
    # Test 0: Ping endpoint to check connectivity and token
    print("0. Testing /Ping endpoint for connectivity and token...")
    try:
        ping_url = f"{base_url}/Ping"
        ping_response = requests.get(
            ping_url, headers=headers, verify=False, timeout=10
        )
        print(f"   Status Code: {ping_response.status_code}")
        print(f"   Response: {ping_response.text[:200]}...")
        if ping_response.status_code == 200:
            print("   ✅ Ping: SUCCESS")
        else:
            print("   ❌ Ping: FAILED")
    except Exception as e:
        print(f"   ❌ Ping: ERROR - {str(e)}")
    print("-" * 40)

    # Test user ID
    test_user_id = f"testuser_{datetime.now().strftime('%Y%m%d_%H%M%S')}"
    test_card_number = f"1000{datetime.now().strftime('%H%M%S')}"

    print(f"Testing with user ID: {test_user_id}")
    print(f"Testing with card number: {test_card_number}")
    print("=" * 60)
    # Test 1: UpdateUser with corrected format (try both endpoint variants)
    print("1. Testing UpdateUser with corrected request format...")
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
    for suffix in ["/UpdateUser", "/json/UpdateUser"]:
        print(f"   Trying endpoint: {base_url}{suffix}")
        try:
            response = requests.post(
                f"{base_url}{suffix}",
                headers=headers,
                json=update_user_payload,
                verify=False,  # For testing with self-signed certificates
                timeout=30,
            )
            print(f"      Status Code: {response.status_code}")
            print(f"      Response: {response.text[:200]}...")
            if response.status_code == 200:
                print("      ✅ UpdateUser: SUCCESS")
            else:
                print("      ❌ UpdateUser: FAILED")
        except Exception as e:
            print(f"      ❌ UpdateUser: ERROR - {str(e)}")
    print("-" * 40)

    # Test 2: UpdateUserPhoto with corrected format (try both endpoint variants)
    print("2. Testing UpdateUserPhoto with corrected request format...")

    # Small test image (1x1 pixel JPEG in base64)
    test_photo_base64 = "/9j/4AAQSkZJRgABAQEAYABgAAD/2wBDAAEBAQEBAQEBAQEBAQEBAQEBAQEBAQEBAQEBAQEBAQEBAQEBAQEBAQEBAQEBAQEBAQEBAQEBAQEBAQEBAQEBAQH/2wBDAQEBAQEBAQEBAQEBAQEBAQEBAQEBAQEBAQEBAQEBAQEBAQEBAQEBAQEBAQEBAQEBAQEBAQEBAQEBAQEBAQEBAQH/wAARCAABAAEDASIAAhEBAxEB/8QAFQABAQAAAAAAAAAAAAAAAAAAAAv/xAAUEAEAAAAAAAAAAAAAAAAAAAAA/8QAFQEBAQAAAAAAAAAAAAAAAAAAAAX/xAAUEQEAAAAAAAAAAAAAAAAAAAAA/9oADAMBAAIRAxEAPwA/wA8A"
    update_photo_payload = {"userId": test_user_id, "photo": test_photo_base64}
    for suffix in ["/UpdateUserPhoto", "/json/UpdateUserPhoto"]:
        print(f"   Trying endpoint: {base_url}{suffix}")
        try:
            response = requests.post(
                f"{base_url}{suffix}",
                headers=headers,
                json=update_photo_payload,
                verify=False,
                timeout=30,
            )
            print(f"      Status Code: {response.status_code}")
            print(f"      Response: {response.text[:200]}...")
            if response.status_code == 200:
                print("      ✅ UpdateUserPhoto: SUCCESS")
            else:
                print("      ❌ UpdateUserPhoto: FAILED")
        except Exception as e:
            print(f"      ❌ UpdateUserPhoto: ERROR - {str(e)}")
    print("-" * 40)

    # Test 3: UpdateCard with corrected format (try both endpoint variants)
    print("3. Testing UpdateCard with corrected request format...")
    update_card_payload = {
        "userId": test_user_id,
        "profileName": "Default",
        "cardNumber": test_card_number,
        "systemNumber": "",
        "versionNumber": "",
        "miscNumber": "",
        "cardStatus": 1,
    }
    for suffix in ["/UpdateCard", "/json/UpdateCard"]:
        print(f"   Trying endpoint: {base_url}{suffix}")
        try:
            response = requests.post(
                f"{base_url}{suffix}",
                headers=headers,
                json=update_card_payload,
                verify=False,
                timeout=30,
            )
            print(f"      Status Code: {response.status_code}")
            print(f"      Response: {response.text[:200]}...")
            if response.status_code == 200:
                print("      ✅ UpdateCard: SUCCESS")
            else:
                print("      ❌ UpdateCard: FAILED")
        except Exception as e:
            print(f"      ❌ UpdateCard: ERROR - {str(e)}")
    print("=" * 60)
    print("Test Summary:")
    print("- Updated JSON property names to match WCF method parameters")
    print("- Fixed UpdateCard to include all required parameters")
    print("- Changed 'fields' from object to array in UpdateUser")
    print("- All requests now use camelCase parameter names")

    return True


if __name__ == "__main__":
    test_updated_requests()
