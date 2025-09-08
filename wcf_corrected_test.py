import requests
import os
import base64
from datetime import datetime


def test_wcf_corrected_requests():
    """Test API endpoints using the correct WCF format based on help pages."""

    base_url = "http://192.168.10.206:9003/Unison.AccessService"
    token = os.getenv("UNISON_API_TOKEN")

    if not token:
        print("ERROR: UNISON_API_TOKEN environment variable must be set")
        return False

    # Test connectivity first
    print("=== CONNECTIVITY TEST ===")
    headers = {"Unison-Token": token}
    try:
        ping_response = requests.get(
            f"{base_url}/Ping", headers=headers, verify=False, timeout=10
        )
        print(f"Ping Status: {ping_response.status_code}")
        if ping_response.status_code != 200:
            print("❌ Connectivity failed")
            return False
        print("✅ Connectivity successful")
    except Exception as e:
        print(f"❌ Connectivity error: {e}")
        return False

    print("\n=== WCF CORRECTED API TESTING ===")

    # Generate test data
    test_user_id = f"testuser_{datetime.now().strftime('%Y%m%d_%H%M%S')}"
    test_card_number = f"1000{datetime.now().strftime('%H%M%S')}"

    print(f"Test User ID: {test_user_id}")
    print(f"Test Card Number: {test_card_number}")

    # Test 1: UpdateUser - WCF expects wrapped parameters
    print(f"\n--- Test 1: UpdateUser (WCF Format) ---")
    print("Based on help page: URL with userId parameter + wrapped body")

    # Try different approaches for UpdateUser
    approaches = [
        {
            "name": "Query Parameter + JSON Body",
            "url": f"{base_url}/UpdateUser?userId={test_user_id}",
            "headers": {"Content-Type": "application/json", "Unison-Token": token},
            "body": {
                "firstName": "John",
                "lastName": "Doe",
                "pinCode": "1234",
                "validFrom": "2025-09-01T00:00:00",
                "validUntil": "2026-09-01T00:00:00",
                "accessFlags": 0,
                "fields": [],
            },
        },
        {
            "name": "Query Parameter + XML Body",
            "url": f"{base_url}/UpdateUser?userId={test_user_id}",
            "headers": {"Content-Type": "text/xml", "Unison-Token": token},
            "body": """<?xml version="1.0" encoding="utf-8"?>
<UpdateUser>
  <firstName>John</firstName>
  <lastName>Doe</lastName>
  <pinCode>1234</pinCode>
  <validFrom>2025-09-01T00:00:00</validFrom>
  <validUntil>2026-09-01T00:00:00</validUntil>
  <accessFlags>0</accessFlags>
  <fields></fields>
</UpdateUser>""",
        },
        {
            "name": "All Parameters as Query Strings",
            "url": f"{base_url}/UpdateUser?userId={test_user_id}&firstName=John&lastName=Doe&pinCode=1234&validFrom=2025-09-01T00:00:00&validUntil=2026-09-01T00:00:00&accessFlags=0",
            "headers": {"Unison-Token": token},
            "body": "",
        },
    ]

    for approach in approaches:
        print(f"\n  Testing: {approach['name']}")
        try:
            if approach["body"]:
                if isinstance(approach["body"], dict):
                    response = requests.post(
                        approach["url"],
                        headers=approach["headers"],
                        json=approach["body"],
                        verify=False,
                        timeout=10,
                    )
                else:
                    response = requests.post(
                        approach["url"],
                        headers=approach["headers"],
                        data=approach["body"],
                        verify=False,
                        timeout=10,
                    )
            else:
                response = requests.post(
                    approach["url"],
                    headers=approach["headers"],
                    verify=False,
                    timeout=10,
                )

            print(f"    Status: {response.status_code}")
            print(f"    Response: {response.text[:200]}...")

            if response.status_code == 200:
                print("    🎉 SUCCESS! This approach works!")
                break
            elif response.status_code != 400:
                print(f"    🔍 Different status code: {response.status_code}")
            else:
                print("    ❌ Still 400 error")

        except Exception as e:
            print(f"    ❌ Error: {e}")

    # Test 2: UpdateUserPhoto - WCF expects byte array format
    print(f"\n--- Test 2: UpdateUserPhoto (WCF Format) ---")
    print("Based on help page: URL with userId parameter + byte array body")

    # Create a small test image as byte array
    test_image_b64 = "/9j/4AAQSkZJRgABAQEAYABgAAD/2wBDAAEBAQEBAQEBAQEBAQEBAQEBAQEBAQEBAQEBAQEBAQEBAQEBAQEBAQEBAQEBAQEBAQEBAQEBAQEBAQEBAQEBAQH/2wBDAQEBAQEBAQEBAQEBAQEBAQEBAQEBAQEBAQEBAQEBAQEBAQEBAQEBAQEBAQEBAQEBAQEBAQEBAQEBAQEBAQEBAQH/wAARCAABAAEDASIAAhEBAxEB/8QAFQABAQAAAAAAAAAAAAAAAAAAAAv/xAAUEAEAAAAAAAAAAAAAAAAAAAAA/8QAFQEBAQAAAAAAAAAAAAAAAAAAAAX/xAAUEQEAAAAAAAAAAAAAAAAAAAAA/9oADAMBAAIRAxEAPwA/wA8A"

    # Convert base64 to bytes
    photo_bytes = base64.b64decode(test_image_b64)

    # Convert to byte array format as shown in help page
    byte_array = list(photo_bytes)

    photo_approaches = [
        {
            "name": "Query Parameter + JSON Byte Array",
            "url": f"{base_url}/UpdateUserPhoto?userId={test_user_id}",
            "headers": {"Content-Type": "application/json", "Unison-Token": token},
            "body": byte_array,
        },
        {
            "name": "Query Parameter + XML format",
            "url": f"{base_url}/UpdateUserPhoto?userId={test_user_id}",
            "headers": {"Content-Type": "text/xml", "Unison-Token": token},
            "body": f'<base64Binary xmlns="http://schemas.microsoft.com/2003/10/Serialization/">{test_image_b64}</base64Binary>',
        },
        {
            "name": "Query Parameter + Raw Bytes",
            "url": f"{base_url}/UpdateUserPhoto?userId={test_user_id}",
            "headers": {
                "Content-Type": "application/octet-stream",
                "Unison-Token": token,
            },
            "body": photo_bytes,
        },
    ]

    for approach in photo_approaches:
        print(f"\n  Testing: {approach['name']}")
        try:
            if approach["name"] == "Query Parameter + JSON Byte Array":
                response = requests.post(
                    approach["url"],
                    headers=approach["headers"],
                    json=approach["body"],
                    verify=False,
                    timeout=10,
                )
            elif approach["name"] == "Query Parameter + Raw Bytes":
                response = requests.post(
                    approach["url"],
                    headers=approach["headers"],
                    data=approach["body"],
                    verify=False,
                    timeout=10,
                )
            else:
                response = requests.post(
                    approach["url"],
                    headers=approach["headers"],
                    data=approach["body"],
                    verify=False,
                    timeout=10,
                )

            print(f"    Status: {response.status_code}")
            print(f"    Response: {response.text[:200]}...")

            if response.status_code == 200:
                print("    🎉 SUCCESS! This approach works!")
                break
            elif response.status_code != 400:
                print(f"    🔍 Different status code: {response.status_code}")
            else:
                print("    ❌ Still 400 error")

        except Exception as e:
            print(f"    ❌ Error: {e}")

    # Test 3: UpdateCard - Try different formats
    print(f"\n--- Test 3: UpdateCard (WCF Format) ---")
    print("Based on help page: Wrapped body format")

    card_approaches = [
        {
            "name": "JSON Body Only",
            "url": f"{base_url}/UpdateCard",
            "headers": {"Content-Type": "application/json", "Unison-Token": token},
            "body": {
                "userId": test_user_id,
                "profileName": "Default",
                "cardNumber": test_card_number,
                "systemNumber": "",
                "versionNumber": "",
                "miscNumber": "",
                "cardStatus": 1,
            },
        },
        {
            "name": "XML Body",
            "url": f"{base_url}/UpdateCard",
            "headers": {"Content-Type": "text/xml", "Unison-Token": token},
            "body": f"""<?xml version="1.0" encoding="utf-8"?>
<UpdateCard>
  <userId>{test_user_id}</userId>
  <profileName>Default</profileName>
  <cardNumber>{test_card_number}</cardNumber>
  <systemNumber></systemNumber>
  <versionNumber></versionNumber>
  <miscNumber></miscNumber>
  <cardStatus>1</cardStatus>
</UpdateCard>""",
        },
        {
            "name": "Query Parameters",
            "url": f"{base_url}/UpdateCard?userId={test_user_id}&profileName=Default&cardNumber={test_card_number}&systemNumber=&versionNumber=&miscNumber=&cardStatus=1",
            "headers": {"Unison-Token": token},
            "body": "",
        },
    ]

    for approach in card_approaches:
        print(f"\n  Testing: {approach['name']}")
        try:
            if approach["body"]:
                if isinstance(approach["body"], dict):
                    response = requests.post(
                        approach["url"],
                        headers=approach["headers"],
                        json=approach["body"],
                        verify=False,
                        timeout=10,
                    )
                else:
                    response = requests.post(
                        approach["url"],
                        headers=approach["headers"],
                        data=approach["body"],
                        verify=False,
                        timeout=10,
                    )
            else:
                response = requests.post(
                    approach["url"],
                    headers=approach["headers"],
                    verify=False,
                    timeout=10,
                )

            print(f"    Status: {response.status_code}")
            print(f"    Response: {response.text[:200]}...")

            if response.status_code == 200:
                print("    🎉 SUCCESS! This approach works!")
                break
            elif response.status_code != 400:
                print(f"    🔍 Different status code: {response.status_code}")
            else:
                print("    ❌ Still 400 error")

        except Exception as e:
            print(f"    ❌ Error: {e}")

    print("\n=== TEST SUMMARY ===")
    print("🔍 Tested multiple WCF format approaches based on service help pages")
    print("📋 Key findings from WCF help pages:")
    print("   - UpdateUser: Expects userId as URL parameter + wrapped body")
    print("   - UpdateUserPhoto: Expects userId as URL parameter + byte array body")
    print("   - UpdateCard: Expects wrapped body format")

    return True


if __name__ == "__main__":
    test_wcf_corrected_requests()
