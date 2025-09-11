import requests
import json
from datetime import datetime


def test_updatecard_comprehensive():
    """Comprehensive test of UpdateCard endpoint - both SOAP and REST"""

    print("Unison Access Service UpdateCard Comprehensive Test")
    print("=" * 60)

    # Test configurations based on existing Postman collections
    test_configs = [
        {
            "name": "REST JSON Interface (Port 9001)",
            "type": "REST",
            "base_url": "http://192.168.10.206:9001/Unison.AccessService",
            "token": "17748106-ac22-4d5a-a4c3-31c5e531a613",
            "endpoint": "/UpdateCard",
        },
        {
            "name": "SOAP Interface (Port 9003)",
            "type": "SOAP",
            "base_url": "http://192.168.10.206:9003/Unison.AccessService",
            "token": "595d799a-9553-4ddf-8fd9-c27b1f233ce7",
            "endpoint": "",
        },
    ]

    all_results = []

    # Test each configuration
    for config in test_configs:
        print(f"\nTesting: {config['name']}")
        print("-" * 50)

        if config["type"] == "REST":
            result = test_rest_updatecard(config)
        else:
            result = test_soap_updatecard(config)

        all_results.append(result)
        print(f"Result: {'✅ SUCCESS' if result['success'] else '❌ FAILED'}")
        if result.get("error"):
            print(f"Error: {result['error']}")
        print()

    # Summary and documentation
    print("=" * 60)
    print("COMPREHENSIVE TEST SUMMARY")
    print("=" * 60)

    successful_tests = [r for r in all_results if r["success"]]
    failed_tests = [r for r in all_results if not r["success"]]

    print(f"Total tests: {len(all_results)}")
    print(f"Successful: {len(successful_tests)}")
    print(f"Failed: {len(failed_tests)}")

    if successful_tests:
        print("\n✅ Working interfaces:")
        for test in successful_tests:
            print(f"  - {test['interface_type']}: {test['config']['name']}")

    if failed_tests:
        print("\n❌ Failed interfaces:")
        for test in failed_tests:
            print(f"  - {test['interface_type']}: {test['config']['name']}")
            if test.get("error"):
                print(f"    Error: {test['error']}")

    # Save results
    timestamp = datetime.now().strftime("%Y%m%d_%H%M%S")
    results_file = f"updatecard_comprehensive_test_{timestamp}.json"

    with open(results_file, "w") as f:
        json.dump(
            {
                "timestamp": timestamp,
                "test_results": all_results,
                "summary": {
                    "total": len(all_results),
                    "successful": len(successful_tests),
                    "failed": len(failed_tests),
                },
            },
            f,
            indent=2,
        )

    print(f"\n📄 Detailed results saved to: {results_file}")
    return all_results


def test_rest_updatecard(config):
    """Test UpdateCard using REST/JSON interface"""

    url = config["base_url"] + config["endpoint"]

    # JSON payload for UpdateCard
    payload = {
        "userId": "TEST_USER_001",
        "profileName": "Default",
        "cardNumber": "12345678",
        "systemNumber": "001",
        "versionNumber": "1",
        "miscNumber": "000",
        "cardStatus": 1,  # Active = 1
    }

    headers = {
        "Content-Type": "application/json",
        "Unison-Token": config["token"],
        "User-Agent": "UpdateCard-REST-Test/1.0",
    }

    try:
        print(f"REST Request to: {url}")
        print(f"Payload: {json.dumps(payload, indent=2)}")

        response = requests.post(url, json=payload, headers=headers, timeout=30)

        print(f"Status: {response.status_code}")
        print(f"Response: {response.text[:200]}...")

        result = {
            "interface_type": "REST/JSON",
            "config": config,
            "success": response.status_code in [200, 201, 202],
            "status_code": response.status_code,
            "response_content": response.text,
            "error": (
                None
                if response.status_code in [200, 201, 202]
                else f"HTTP {response.status_code}"
            ),
        }

        # Check for specific success indicators
        if response.status_code == 200:
            try:
                json_response = response.json()
                if (
                    "error" not in json_response.lower()
                    if isinstance(json_response, str)
                    else True
                ):
                    result["success"] = True
            except (ValueError, json.JSONDecodeError, KeyError):
                # Non-JSON response might still be success
                if (
                    "error" not in response.text.lower()
                    and "fault" not in response.text.lower()
                ):
                    result["success"] = True

    except requests.RequestException as e:
        result = {
            "interface_type": "REST/JSON",
            "config": config,
            "success": False,
            "status_code": None,
            "response_content": "",
            "error": str(e),
        }
        print(f"Request error: {e}")

    return result


def test_soap_updatecard(config):
    """Test UpdateCard using SOAP interface"""

    url = config["base_url"]

    # SOAP envelope for UpdateCard
    soap_body = f"""<?xml version="1.0" encoding="utf-8"?>
<soap:Envelope xmlns:soap="http://schemas.xmlsoap.org/soap/envelope/" 
               xmlns:tem="http://tempuri.org/">
  <soap:Header>
    <Unison-Token xmlns="http://tempuri.org/">{config['token']}</Unison-Token>
  </soap:Header>
  <soap:Body>
    <tem:UpdateCard>
      <tem:userId>TEST_USER_001</tem:userId>
      <tem:profileName>Default</tem:profileName>
      <tem:cardNumber>12345678</tem:cardNumber>
      <tem:systemNumber>001</tem:systemNumber>
      <tem:versionNumber>1</tem:versionNumber>
      <tem:miscNumber>000</tem:miscNumber>
      <tem:cardStatus>Active</tem:cardStatus>
    </tem:UpdateCard>
  </soap:Body>
</soap:Envelope>"""

    headers = {
        "Content-Type": "text/xml; charset=utf-8",
        "SOAPAction": '"http://tempuri.org/IAccessService/UpdateCard"',
        "Unison-Token": config["token"],
        "User-Agent": "UpdateCard-SOAP-Test/1.0",
    }

    try:
        print(f"SOAP Request to: {url}")
        print("SOAP Envelope (abbreviated):")
        print(soap_body[:300] + "...")

        response = requests.post(url, data=soap_body, headers=headers, timeout=30)

        print(f"Status: {response.status_code}")
        print(f"Response: {response.text[:200]}...")

        result = {
            "interface_type": "SOAP/XML",
            "config": config,
            "success": response.status_code == 200
            and "fault" not in response.text.lower(),
            "status_code": response.status_code,
            "response_content": response.text,
            "error": (
                None if response.status_code == 200 else f"HTTP {response.status_code}"
            ),
        }

        # Additional SOAP-specific checks
        if response.status_code == 200:
            if (
                "soap:fault" in response.text.lower()
                or "faultstring" in response.text.lower()
            ):
                result["success"] = False
                result["error"] = "SOAP Fault detected"

    except requests.RequestException as e:
        result = {
            "interface_type": "SOAP/XML",
            "config": config,
            "success": False,
            "status_code": None,
            "response_content": "",
            "error": str(e),
        }
        print(f"Request error: {e}")

    return result


if __name__ == "__main__":
    test_updatecard_comprehensive()
