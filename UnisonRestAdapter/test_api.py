#!/usr/bin/env python3
"""
Simple test script to validate the Unison REST-to-SOAP adapter
Tests the UpdateCard endpoint with proper authentication
"""

import requests
import json
from datetime import datetime


def test_unison_rest_adapter():
    """Test the Unison REST adapter endpoints"""

    # Configuration
    base_url = "http://localhost:5000/api"  # Will be updated when deployed
    test_token = "595d799a-9553-4ddf-8fd9-c27b1f233ce7"  # Test token from documentation

    print("🚀 Testing Unison REST-to-SOAP Adapter")
    print("=" * 50)

    # Test 1: Health Check
    print("\n1. Testing Health Check...")
    try:
        health_response = requests.get(
            f"{base_url}/health", headers={"Unison-Token": test_token}, timeout=10
        )

        print(f"   Status: {health_response.status_code}")
        if health_response.status_code == 200:
            health_data = health_response.json()
            print(
                f"   Health: {'✅ Healthy' if health_data.get('isHealthy') else '❌ Unhealthy'}"
            )
            print(f"   Message: {health_data.get('message', 'N/A')}")
        else:
            print(f"   Error: {health_response.text}")

    except Exception as e:
        print(f"   ❌ Health check failed: {e}")

    # Test 2: UpdateCard
    print("\n2. Testing UpdateCard...")
    try:
        update_request = {
            "cardId": "TEST_CARD_12345",
            "userName": "TEST_USER_001",
            "firstName": "John",
            "lastName": "Doe",
            "email": "john.doe@test.com",
            "department": "IT",
            "title": "Software Engineer",
            "isActive": True,
            "expirationDate": "2025-12-31T23:59:59Z",
            "customFields": {"location": "Building A", "access_level": "Standard"},
        }

        update_response = requests.put(
            f"{base_url}/cards/update",
            headers={"Unison-Token": test_token, "Content-Type": "application/json"},
            json=update_request,
            timeout=10,
        )

        print(f"   Status: {update_response.status_code}")
        if update_response.status_code == 200:
            update_data = update_response.json()
            print(f"   Success: {'✅' if update_data.get('success') else '❌'}")
            print(f"   Message: {update_data.get('message', 'N/A')}")
            print(f"   Card ID: {update_data.get('cardId', 'N/A')}")
        else:
            print(f"   Error: {update_response.text}")

    except Exception as e:
        print(f"   ❌ UpdateCard failed: {e}")

    # Test 3: GetCard
    print("\n3. Testing GetCard...")
    try:
        get_response = requests.get(
            f"{base_url}/cards/TEST_CARD_12345",
            headers={"Unison-Token": test_token},
            timeout=10,
        )

        print(f"   Status: {get_response.status_code}")
        if get_response.status_code == 200:
            get_data = get_response.json()
            print(f"   Success: {'✅' if get_data.get('success') else '❌'}")
            print(f"   User: {get_data.get('userName', 'N/A')}")
        else:
            print(f"   Error: {get_response.text}")

    except Exception as e:
        print(f"   ❌ GetCard failed: {e}")

    print("\n" + "=" * 50)
    print("🎯 Test completed!")


def test_authentication():
    """Test authentication scenarios"""
    base_url = "http://localhost:5000/api"

    print("\n🔐 Testing Authentication...")

    # Test without token
    try:
        response = requests.get(f"{base_url}/health", timeout=5)
        print(
            f"   No token: {response.status_code} - {'✅ Unauthorized' if response.status_code == 401 else '❌ Should be unauthorized'}"
        )
    except (requests.RequestException, requests.Timeout, ConnectionError) as e:
        print(f"   No token: ❌ Connection failed: {e}")

    # Test with invalid token
    try:
        response = requests.get(
            f"{base_url}/health", headers={"Unison-Token": "invalid-token"}, timeout=5
        )
        print(f"   Invalid token: {response.status_code}")
    except (requests.RequestException, requests.Timeout, ConnectionError) as e:
        print(f"   Invalid token: ❌ Connection failed: {e}")


if __name__ == "__main__":
    test_unison_rest_adapter()
    test_authentication()
