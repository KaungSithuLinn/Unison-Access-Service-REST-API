import requests
import json


def test_different_formats():
    base_url = "http://192.168.10.206:9001/Unison.AccessService"
    token = "17748106-ac22-4d5a-a4c3-31c5e531a613"

    headers_json = {
        "Content-Type": "application/json",
        "Accept": "application/json",
        "Unison-Token": token,
    }

    headers_xml = {
        "Content-Type": "application/xml",
        "Accept": "application/xml",
        "Unison-Token": token,
    }

    print("=" * 60)
    print("TESTING DIFFERENT REQUEST FORMATS")
    print("=" * 60)

    # Test 1: JSON object format (current approach)
    print("1. Testing JSON object format...")
    json_object_payload = {
        "userId": "test123",
        "firstName": "John",
        "lastName": "Doe",
        "pinCode": "1234",
        "validFrom": "2025-09-01T00:00:00",
        "validUntil": "2026-09-01T00:00:00",
        "accessFlags": 0,
        "fields": [],
    }

    try:
        response = requests.post(
            f"{base_url}/UpdateUser",
            headers=headers_json,
            json=json_object_payload,
            verify=False,
            timeout=30,
        )
        print(f"   Status: {response.status_code}")
        if response.status_code != 200:
            print(f"   Error: {response.text[:200]}...")
    except Exception as e:
        print(f"   Error: {e}")

    print("-" * 40)

    # Test 2: JSON array format
    print("2. Testing JSON array format...")
    json_array_payload = [
        "test123",
        "John",
        "Doe",
        "1234",
        "2025-09-01T00:00:00",
        "2026-09-01T00:00:00",
        0,
        [],
    ]

    try:
        response = requests.post(
            f"{base_url}/UpdateUser",
            headers=headers_json,
            json=json_array_payload,
            verify=False,
            timeout=30,
        )
        print(f"   Status: {response.status_code}")
        if response.status_code != 200:
            print(f"   Error: {response.text[:200]}...")
    except Exception as e:
        print(f"   Error: {e}")

    print("-" * 40)

    # Test 3: XML format
    print("3. Testing XML format...")
    xml_payload = """<?xml version="1.0" encoding="utf-8"?>
<UpdateUser>
    <userId>test123</userId>
    <firstName>John</firstName>
    <lastName>Doe</lastName>
    <pinCode>1234</pinCode>
    <validFrom>2025-09-01T00:00:00</validFrom>
    <validUntil>2026-09-01T00:00:00</validUntil>
    <accessFlags>0</accessFlags>
    <fields></fields>
</UpdateUser>"""

    try:
        response = requests.post(
            f"{base_url}/UpdateUser",
            headers=headers_xml,
            data=xml_payload,
            verify=False,
            timeout=30,
        )
        print(f"   Status: {response.status_code}")
        if response.status_code != 200:
            print(f"   Error: {response.text[:200]}...")
    except Exception as e:
        print(f"   Error: {e}")

    print("-" * 40)

    # Test 4: Try GET method (maybe it's expecting GET with query params)
    print("4. Testing GET with query parameters...")
    try:
        params = {
            "userId": "test123",
            "firstName": "John",
            "lastName": "Doe",
            "pinCode": "1234",
            "validFrom": "2025-09-01T00:00:00",
            "validUntil": "2026-09-01T00:00:00",
            "accessFlags": 0,
            "fields": "[]",
        }
        response = requests.get(
            f"{base_url}/UpdateUser",
            headers=headers_json,
            params=params,
            verify=False,
            timeout=30,
        )
        print(f"   Status: {response.status_code}")
        if response.status_code != 200:
            print(f"   Error: {response.text[:200]}...")
    except Exception as e:
        print(f"   Error: {e}")

    print("=" * 60)


if __name__ == "__main__":
    test_different_formats()
