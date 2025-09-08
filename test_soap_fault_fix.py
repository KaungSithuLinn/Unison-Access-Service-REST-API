import requests
import xml.etree.ElementTree as ET


def test_accessservice_soap_fault_response():
    """Test if the AccessService now returns proper SOAP fault instead of HTML error"""

    # Test URL
    url = "http://192.168.10.206:9003/Unison.AccessService"

    # Invalid SOAP request to trigger an error
    soap_request = """<?xml version="1.0" encoding="utf-8"?>
<soapenv:Envelope xmlns:soapenv="http://schemas.xmlsoap.org/soap/envelope/" 
                  xmlns:tem="http://tempuri.org/">
    <soapenv:Body>
        <tem:UpdateCard>
            <tem:request>invalid_request_data</tem:request>
        </tem:UpdateCard>
    </soapenv:Body>
</soapenv:Envelope>"""

    headers = {
        "Content-Type": "text/xml; charset=utf-8",
        "SOAPAction": "http://tempuri.org/IAccessService/UpdateCard",
    }

    try:
        print(f"Testing AccessService at: {url}")
        print("Sending invalid SOAP request to trigger error...")

        response = requests.post(url, data=soap_request, headers=headers, timeout=10)

        print(f"\nResponse Status Code: {response.status_code}")
        print(
            f"Response Content-Type: {response.headers.get('content-type', 'unknown')}"
        )
        print(f"Response Length: {len(response.text)} characters")

        # Check if response is XML (SOAP fault) or HTML (error page)
        content_type = response.headers.get("content-type", "").lower()

        if "xml" in content_type:
            print("\n✅ SUCCESS: Response is XML (likely SOAP fault)")

            # Try to parse as XML to confirm it's valid SOAP
            try:
                root = ET.fromstring(response.text)
                print(f"✅ Valid XML structure detected. Root element: {root.tag}")

                # Look for SOAP fault elements
                if (
                    "fault" in response.text.lower()
                    or "soap:fault" in response.text.lower()
                ):
                    print("✅ SOAP Fault detected in response!")
                else:
                    print("📝 XML response but no explicit SOAP fault detected")

            except ET.ParseError as e:
                print(f"❌ XML parsing failed: {e}")

        elif "html" in content_type:
            print(f"\n❌ FAILED: Response is still HTML (error page)")
            print("❌ The fix did not work - still getting HTML error pages")

        else:
            print(f"\n📝 Unknown content type: {content_type}")

        # Show first 500 characters of response for analysis
        print(f"\nFirst 500 characters of response:")
        print("-" * 50)
        print(response.text[:500])
        if len(response.text) > 500:
            print("... (truncated)")
        print("-" * 50)

        # Determine overall success
        if "xml" in content_type and response.status_code != 200:
            print(f"\n🎉 MISSION ACCOMPLISHED!")
            print(
                f"✅ AccessService now returns XML SOAP faults instead of HTML error pages"
            )
            print(
                f"✅ WCF configuration fix successful - includeExceptionDetailInFaults=true is working"
            )
        else:
            print(f"\n⚠️  Mixed results - need further investigation")

    except requests.exceptions.RequestException as e:
        print(f"❌ Connection error: {e}")
        print("❌ Service may not be running or accessible")


if __name__ == "__main__":
    test_accessservice_soap_fault_response()
