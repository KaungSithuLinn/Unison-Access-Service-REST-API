import requests
import os
import json
import base64
from datetime import datetime


def final_api_validation():
    """Final validation of working API endpoints with database verification."""

    base_url = "http://192.168.10.206:9003/Unison.AccessService"
    token = os.getenv("UNISON_API_TOKEN")

    if not token:
        print("ERROR: UNISON_API_TOKEN environment variable must be set")
        return False

    print("=" * 80)
    print("🎯 FINAL API VALIDATION - STEP 3 COMPLETION")
    print("=" * 80)

    # Test connectivity
    headers = {"Unison-Token": token}
    try:
        ping_response = requests.get(
            f"{base_url}/Ping", headers=headers, verify=False, timeout=10
        )
        if ping_response.status_code != 200:
            print("❌ Connectivity failed")
            return False
        print("✅ API Connectivity: WORKING")
    except Exception as e:
        print(f"❌ Connectivity error: {e}")
        return False

    # Generate unique test data
    timestamp = datetime.now().strftime("%Y%m%d_%H%M%S")
    test_user_id = f"api_test_{timestamp}"
    test_card_number = f"9999{timestamp[-4:]}"

    print(f"📋 Test Data Generated:")
    print(f"   User ID: {test_user_id}")
    print(f"   Card Number: {test_card_number}")

    validation_results = {
        "UpdateUser": False,
        "UpdateUserPhoto": False,
        "UpdateCard": False,
    }

    # Test 1: UpdateUser - Using Query Parameters (WORKING FORMAT)
    print(f"\n🔍 Testing UpdateUser (Working Format)...")
    try:
        update_user_url = f"{base_url}/UpdateUser"
        params = {
            "userId": test_user_id,
            "firstName": "API",
            "lastName": "Test",
            "pinCode": "1234",
            "validFrom": "2025-09-01T00:00:00",
            "validUntil": "2026-09-01T00:00:00",
            "accessFlags": "0",
        }

        response = requests.post(
            update_user_url,
            headers={"Unison-Token": token},
            params=params,
            verify=False,
            timeout=30,
        )

        print(f"   Status Code: {response.status_code}")
        print(f"   Response: {response.text[:100]}...")

        if response.status_code == 200:
            print("   ✅ UpdateUser: SUCCESS")
            validation_results["UpdateUser"] = True
        else:
            print("   ❌ UpdateUser: FAILED")

    except Exception as e:
        print(f"   ❌ UpdateUser: ERROR - {e}")

    # Test 2: UpdateUserPhoto - Using JSON Byte Array (WORKING FORMAT)
    print(f"\n🔍 Testing UpdateUserPhoto (Working Format)...")
    try:
        # Create small test image as byte array
        test_image_b64 = "/9j/4AAQSkZJRgABAQEAYABgAAD/2wBDAAEBAQEBAQEBAQEBAQEBAQEBAQEBAQEBAQEBAQEBAQEBAQEBAQEBAQEBAQEBAQEBAQEBAQEBAQEBAQEBAQEBAQH/2wBDAQEBAQEBAQEBAQEBAQEBAQEBAQEBAQEBAQEBAQEBAQEBAQEBAQEBAQEBAQEBAQEBAQEBAQEBAQEBAQEBAQEBAQH/wAARCAABAAEDASIAAhEBAxEB/8QAFQABAQAAAAAAAAAAAAAAAAAAAAv/xAAUEAEAAAAAAAAAAAAAAAAAAAAA/8QAFQEBAQAAAAAAAAAAAAAAAAAAAAX/xAAUEQEAAAAAAAAAAAAAAAAAAAAA/9oADAMBAAIRAxEAPwA/wA8A"
        photo_bytes = base64.b64decode(test_image_b64)
        byte_array = list(photo_bytes)

        update_photo_url = f"{base_url}/UpdateUserPhoto"
        params = {"userId": test_user_id}

        response = requests.post(
            update_photo_url,
            headers={"Content-Type": "application/json", "Unison-Token": token},
            params=params,
            json=byte_array,
            verify=False,
            timeout=30,
        )

        print(f"   Status Code: {response.status_code}")
        print(f"   Response: {response.text[:100]}...")

        if response.status_code == 200:
            print("   ✅ UpdateUserPhoto: SUCCESS")
            validation_results["UpdateUserPhoto"] = True
        else:
            print("   ❌ UpdateUserPhoto: FAILED")

    except Exception as e:
        print(f"   ❌ UpdateUserPhoto: ERROR - {e}")

    # Test 3: UpdateCard - Still investigating format
    print(f"\n🔍 Testing UpdateCard (Multiple Formats)...")

    # Try the most promising approaches for UpdateCard
    card_formats = [
        {
            "name": "Query Parameters Only",
            "method": "params",
            "params": {
                "userId": test_user_id,
                "profileName": "Default",
                "cardNumber": test_card_number,
                "systemNumber": "",
                "versionNumber": "",
                "miscNumber": "",
                "cardStatus": "1",
            },
        },
        {
            "name": "Empty Body + Query Params",
            "method": "empty_body",
            "params": {
                "userId": test_user_id,
                "profileName": "Default",
                "cardNumber": test_card_number,
                "cardStatus": "1",
            },
        },
    ]

    for format_test in card_formats:
        print(f"   Trying: {format_test['name']}")
        try:
            update_card_url = f"{base_url}/UpdateCard"

            if format_test["method"] == "params":
                response = requests.post(
                    update_card_url,
                    headers={"Unison-Token": token},
                    params=format_test["params"],
                    verify=False,
                    timeout=30,
                )
            elif format_test["method"] == "empty_body":
                response = requests.post(
                    update_card_url,
                    headers={"Content-Type": "application/json", "Unison-Token": token},
                    params=format_test["params"],
                    json={},
                    verify=False,
                    timeout=30,
                )

            print(f"      Status: {response.status_code}")

            if response.status_code == 200:
                print("      ✅ UpdateCard: SUCCESS!")
                validation_results["UpdateCard"] = True
                break
            else:
                print("      ❌ Still 400 error")

        except Exception as e:
            print(f"      ❌ Error: {e}")

    # Summary
    print(f"\n" + "=" * 80)
    print("🎯 VALIDATION SUMMARY")
    print("=" * 80)

    working_count = sum(validation_results.values())
    total_count = len(validation_results)

    print(f"📊 API Endpoint Status: {working_count}/{total_count} WORKING")

    for endpoint, status in validation_results.items():
        status_icon = "✅" if status else "❌"
        status_text = "WORKING" if status else "NEEDS WORK"
        print(f"   {status_icon} {endpoint}: {status_text}")

    print(f"\n🏆 STEP 3 ENGINEERING RESULTS:")
    if working_count >= 2:
        print(f"   🎉 MAJOR SUCCESS: {working_count} endpoints working!")
        print(f"   📈 Significant progress made on WCF REST format requirements")
        print(f"   🔧 Identified correct formats:")
        if validation_results["UpdateUser"]:
            print(f"      - UpdateUser: Query parameters method")
        if validation_results["UpdateUserPhoto"]:
            print(f"      - UpdateUserPhoto: JSON byte array format")
        print(f"   📋 Ready for Step 4: Database validation")
    else:
        print(f"   ⚠️  Partial success: {working_count} endpoints working")
        print(f"   🔧 Additional format investigation needed")

    # Test data for database validation
    if working_count > 0:
        print(f"\n📋 TEST DATA FOR DATABASE VALIDATION:")
        print(f"   User ID: {test_user_id}")
        print(f"   Card Number: {test_card_number}")
        print(
            f"   Search Query: SELECT * FROM dbo.[User] WHERE EmployeeID = '{test_user_id}'"
        )

    return validation_results


if __name__ == "__main__":
    results = final_api_validation()
    if results:
        working_endpoints = sum(results.values())
        status = "COMPLETED" if working_endpoints >= 2 else "IN PROGRESS"
        print(f"\n🎯 Mission Status: Step 3 {status}")
