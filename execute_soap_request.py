#!/usr/bin/env python3
"""
Step 1: Execute SOAP UpdateCard Request
Fresh validation execution for Unison Access Service UpdateCard endpoint
using the exact SOAP XML from soap_test_direct.xml
"""

import requests
import json
import xml.etree.ElementTree as ET
from datetime import datetime
import os

# Request configuration
SOAP_ENDPOINT = "http://192.168.10.206:9003/Unison.AccessService"
HEADERS = {
    "Content-Type": "text/xml; charset=utf-8",
    "SOAPAction": "http://tempuri.org/IUnisonAccessService/UpdateCard",
    "Unison-Token": "595d799a-9553-4ddf-8fd9-c27b1f233ce7",
}


def load_soap_body():
    """Load SOAP request body from soap_test_direct.xml"""
    try:
        with open("soap_test_direct.xml", "r", encoding="utf-8") as f:
            return f.read()
    except FileNotFoundError:
        print("❌ Error: soap_test_direct.xml not found")
        return None


def execute_soap_request():
    """Execute the SOAP UpdateCard request and capture detailed response"""
    print("🚀 STEP 1: Executing SOAP UpdateCard Request")
    print("=" * 60)

    # Load SOAP body
    soap_body = load_soap_body()
    if not soap_body:
        return None

    print(f"📋 Endpoint: {SOAP_ENDPOINT}")
    print(f"🔐 Headers: {json.dumps(HEADERS, indent=2)}")
    print(f"📝 SOAP Body Preview:")
    print(soap_body[:200] + "..." if len(soap_body) > 200 else soap_body)
    print()

    # Execute request
    try:
        print("⏳ Sending SOAP request...")
        response = requests.post(
            SOAP_ENDPOINT, headers=HEADERS, data=soap_body, timeout=30
        )

        # Capture response details
        response_data = {
            "timestamp": datetime.now().isoformat(),
            "request": {
                "method": "POST",
                "url": SOAP_ENDPOINT,
                "headers": HEADERS,
                "body": soap_body,
            },
            "response": {
                "status_code": response.status_code,
                "status_text": response.reason,
                "headers": dict(response.headers),
                "body": response.text,
                "encoding": response.encoding,
                "elapsed_ms": response.elapsed.total_seconds() * 1000,
            },
        }

        # Display results
        print(f"📊 RESPONSE RECEIVED:")
        print(f"   Status: {response.status_code} {response.reason}")
        print(f"   Content-Type: {response.headers.get('Content-Type', 'N/A')}")
        print(f"   Response Time: {response_data['response']['elapsed_ms']:.2f}ms")
        print(f"   Body Length: {len(response.text)} characters")
        print()

        if response.status_code == 200:
            print("✅ SUCCESS: SOAP request completed successfully")
            try:
                # Try to parse XML response
                root = ET.fromstring(response.text)
                print("📄 XML Response parsed successfully")
                print(f"   Root element: {root.tag}")

                # Look for SOAP fault or UpdateCard response
                if (
                    "soap" in response.text.lower()
                    or "envelope" in response.text.lower()
                ):
                    print("🧼 SOAP envelope detected in response")

            except ET.ParseError as e:
                print(f"⚠️  XML parsing failed: {e}")

        else:
            print(f"❌ ERROR: SOAP request failed with status {response.status_code}")

        print(f"📋 Response Body Preview:")
        print(
            response.text[:500] + "..." if len(response.text) > 500 else response.text
        )

        # Save detailed response
        output_file = f"soap_response_{datetime.now().strftime('%Y%m%d_%H%M%S')}.json"
        with open(output_file, "w", encoding="utf-8") as f:
            json.dump(response_data, f, indent=2, ensure_ascii=False)

        print(f"\n💾 Full response saved to: {output_file}")
        return response_data

    except requests.RequestException as e:
        print(f"❌ REQUEST ERROR: {e}")
        return {"error": str(e), "timestamp": datetime.now().isoformat()}

    except Exception as e:
        print(f"❌ UNEXPECTED ERROR: {e}")
        return {"error": str(e), "timestamp": datetime.now().isoformat()}


if __name__ == "__main__":
    print("🎯 Unison Access Service UpdateCard SOAP Validation")
    print("📅 September 3, 2025 - Fresh Execution")
    print("🔄 MCP Sequential Pipeline - Step 1")
    print()

    result = execute_soap_request()

    if result:
        print("\n" + "=" * 60)
        print("✅ STEP 1 COMPLETED - SOAP Request Executed")
        print("🔗 Ready for Step 2: MarkItDown Documentation")
        print("=" * 60)
    else:
        print("\n" + "=" * 60)
        print("❌ STEP 1 FAILED - Unable to execute SOAP request")
        print("=" * 60)
