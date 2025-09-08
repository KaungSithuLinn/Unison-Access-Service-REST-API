import requests
import xml.etree.ElementTree as ET
from datetime import datetime
import json


# Test UpdateCard endpoint
def test_updatecard_endpoint():
    """Test the Unison Access Service UpdateCard SOAP endpoint"""

    # Endpoint details
    endpoint_url = "http://192.168.10.206:9003/Unison.AccessService"
    wsdl_url = "http://192.168.10.206:9003/Unison.AccessService?wsdl"
    security_token = "595d799a-9553-4ddf-8fd9-c27b1f233ce7"

    # First, try to get the WSDL to understand the service contract
    print("Step 1: Fetching WSDL to understand service contract...")
    try:
        wsdl_response = requests.get(wsdl_url, timeout=10)
        print(f"WSDL Response Status: {wsdl_response.status_code}")

        if wsdl_response.status_code == 200:
            print("WSDL Content (first 1000 chars):")
            print(wsdl_response.text[:1000])

            # Parse WSDL to extract namespace information
            try:
                root = ET.fromstring(wsdl_response.text)
                print("\nWSDL Root element and namespaces:")
                print(f"Root tag: {root.tag}")
                print("Namespaces:")
                for prefix, uri in root.attrib.items():
                    if prefix.startswith("xmlns"):
                        print(f"  {prefix}: {uri}")
            except ET.ParseError as e:
                print(f"Error parsing WSDL XML: {e}")
        else:
            print(f"Failed to fetch WSDL: {wsdl_response.status_code}")

    except requests.RequestException as e:
        print(f"Error fetching WSDL: {e}")

    print("\n" + "=" * 60)
    print("Step 2: Testing UpdateCard SOAP endpoint...")

    # SOAP request with multiple namespace variations to test
    soap_requests = [
        # Version 1: Using http://tempuri.org/ namespace
        {
            "name": "tempuri.org namespace",
            "soap_body": """<?xml version="1.0" encoding="utf-8"?>
<soap:Envelope xmlns:soap="http://schemas.xmlsoap.org/soap/envelope/" 
               xmlns:tem="http://tempuri.org/">
  <soap:Header>
    <Unison-Token xmlns="http://tempuri.org/">595d799a-9553-4ddf-8fd9-c27b1f233ce7</Unison-Token>
  </soap:Header>
  <soap:Body>
    <tem:UpdateCard>
      <tem:userId>TEST_USER_001</tem:userId>
      <tem:profileName></tem:profileName>
      <tem:cardNumber>12345678</tem:cardNumber>
      <tem:systemNumber>001</tem:systemNumber>
      <tem:versionNumber>1</tem:versionNumber>
      <tem:miscNumber>000</tem:miscNumber>
      <tem:cardStatus>Active</tem:cardStatus>
    </tem:UpdateCard>
  </soap:Body>
</soap:Envelope>""",
            "soap_action": "http://tempuri.org/IAccessService/UpdateCard",
        },
        # Version 2: Without explicit namespace prefixes
        {
            "name": "minimal namespace",
            "soap_body": """<?xml version="1.0" encoding="utf-8"?>
<soap:Envelope xmlns:soap="http://schemas.xmlsoap.org/soap/envelope/">
  <soap:Header>
    <Unison-Token>595d799a-9553-4ddf-8fd9-c27b1f233ce7</Unison-Token>
  </soap:Header>
  <soap:Body>
    <UpdateCard xmlns="http://tempuri.org/">
      <userId>TEST_USER_001</userId>
      <profileName></profileName>
      <cardNumber>12345678</cardNumber>
      <systemNumber>001</systemNumber>
      <versionNumber>1</versionNumber>
      <miscNumber>000</miscNumber>
      <cardStatus>Active</cardStatus>
    </UpdateCard>
  </soap:Body>
</soap:Envelope>""",
            "soap_action": "http://tempuri.org/IAccessService/UpdateCard",
        },
    ]

    results = []

    for i, request_data in enumerate(soap_requests, 1):
        print(f"\nTest {i}: {request_data['name']}")
        print("-" * 40)

        # Headers for SOAP request
        headers = {
            "Content-Type": "text/xml; charset=utf-8",
            "SOAPAction": f'"{request_data["soap_action"]}"',
            "Unison-Token": security_token,
            "User-Agent": "UpdateCard-Test-Client/1.0",
        }

        try:
            # Send SOAP request
            response = requests.post(
                endpoint_url,
                data=request_data["soap_body"],
                headers=headers,
                timeout=30,
            )

            result = {
                "test_name": request_data["name"],
                "status_code": response.status_code,
                "success": response.status_code in [200, 202],
                "response_headers": dict(response.headers),
                "response_content": response.text,
                "error": None,
            }

            print(f"Status Code: {response.status_code}")
            print(f"Response Headers: {dict(response.headers)}")
            print(f"Response Content (first 500 chars): {response.text[:500]}")

            if response.status_code == 200:
                print("✅ SUCCESS: Request completed successfully")
            elif response.status_code == 500:
                print("❌ SOAP Fault: Server returned internal error")
                # Try to parse SOAP fault
                try:
                    if (
                        "soap" in response.text.lower()
                        or "fault" in response.text.lower()
                    ):
                        print("SOAP Fault details:")
                        print(response.text)
                except:
                    pass
            else:
                print(f"❌ ERROR: HTTP {response.status_code}")

        except requests.RequestException as e:
            result = {
                "test_name": request_data["name"],
                "status_code": None,
                "success": False,
                "response_headers": {},
                "response_content": "",
                "error": str(e),
            }
            print(f"❌ REQUEST ERROR: {e}")

        results.append(result)
        print()

    # Summary
    print("=" * 60)
    print("SUMMARY")
    print("=" * 60)

    successful_tests = [r for r in results if r["success"]]
    failed_tests = [r for r in results if not r["success"]]

    print(f"Successful tests: {len(successful_tests)}/{len(results)}")
    print(f"Failed tests: {len(failed_tests)}/{len(results)}")

    if successful_tests:
        print("\n✅ Successful tests:")
        for test in successful_tests:
            print(f"  - {test['test_name']}: HTTP {test['status_code']}")

    if failed_tests:
        print("\n❌ Failed tests:")
        for test in failed_tests:
            if test["error"]:
                print(f"  - {test['test_name']}: {test['error']}")
            else:
                print(f"  - {test['test_name']}: HTTP {test['status_code']}")

    # Save detailed results to file
    timestamp = datetime.now().strftime("%Y%m%d_%H%M%S")
    results_file = f"updatecard_test_results_{timestamp}.json"

    with open(results_file, "w") as f:
        json.dump(
            {
                "timestamp": timestamp,
                "endpoint": endpoint_url,
                "security_token_used": "595d799a-9553-4ddf-8fd9-c27b1f233ce7",
                "test_results": results,
                "summary": {
                    "total_tests": len(results),
                    "successful": len(successful_tests),
                    "failed": len(failed_tests),
                },
            },
            f,
            indent=2,
        )

    print(f"\n📄 Detailed results saved to: {results_file}")

    return results


if __name__ == "__main__":
    print("Unison Access Service UpdateCard Endpoint Test")
    print("=" * 60)
    test_updatecard_endpoint()
