import requests
import os
from datetime import datetime


def simple_test():
    # Configuration
    base_url = "http://192.168.10.206:9001/Unison.AccessService"
    token = "17748106-ac22-4d5a-a4c3-31c5e531a613"

    # Headers
    headers = {
        "Content-Type": "application/json",
        "Accept": "application/json",
        "Unison-Token": token,
        "User-Agent": "Python-API-Test/1.0",
    }

    print("=" * 60)
    print("UNISON API TEST RESULTS")
    print("=" * 60)

    # Test Ping
    print("Testing /Ping endpoint...")
    try:
        response = requests.get(
            f"{base_url}/Ping", headers=headers, verify=False, timeout=10
        )
        print(f"Status: {response.status_code}")
        print(f"Response: {response.text}")
        print("✅ Ping SUCCESS" if response.status_code == 200 else "❌ Ping FAILED")
    except Exception as e:
        print(f"❌ Ping ERROR: {e}")

    print("-" * 40)

    # Test UpdateUser
    print("Testing /UpdateUser endpoint...")
    user_payload = {
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
            headers=headers,
            json=user_payload,
            verify=False,
            timeout=30,
        )
        print(f"Status: {response.status_code}")
        print(f"Response: {response.text}")
        print(
            "✅ UpdateUser SUCCESS"
            if response.status_code == 200
            else "❌ UpdateUser FAILED"
        )
    except Exception as e:
        print(f"❌ UpdateUser ERROR: {e}")

    print("=" * 60)


if __name__ == "__main__":
    simple_test()
