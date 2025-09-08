import requests
import os
import json
from datetime import datetime
from bs4 import BeautifulSoup


def get_error_details():
    """Extract detailed error information from WCF service responses."""

    base_url = os.getenv(
        "UNISON_API_BASE_URL", "http://192.168.10.206:9003/Unison.AccessService"
    )
    token = os.getenv("UNISON_API_TOKEN")

    if not token:
        print("ERROR: UNISON_API_TOKEN environment variable must be set")
        return False

    headers = {
        "Content-Type": "application/json",
        "Accept": "application/json",
        "Unison-Token": token,
        "User-Agent": "Python-Error-Analysis/1.0",
    }

    # Test UpdateUser to get detailed error
    print("=== EXTRACTING ERROR DETAILS FROM UPDATEUSER ===")
    test_user_id = f"testuser_error_analysis_{datetime.now().strftime('%H%M%S')}"

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

    try:
        response = requests.post(
            f"{base_url}/UpdateUser",
            headers=headers,
            json=update_user_payload,
            verify=False,
            timeout=30,
        )

        print(f"Status Code: {response.status_code}")
        print(f"Content-Type: {response.headers.get('Content-Type')}")
        print(f"Server: {response.headers.get('Server')}")

        # Save full response for analysis
        full_response = response.text
        print(f"\nFull Response Length: {len(full_response)} characters")

        # Try to extract error message from HTML
        try:
            soup = BeautifulSoup(full_response, "html.parser")

            # Look for common error message patterns
            error_elements = soup.find_all(["pre", "p", "div", "span"])

            print("\n=== EXTRACTED ERROR INFORMATION ===")
            for i, element in enumerate(error_elements):
                text = element.get_text().strip()
                if text and len(text) > 10:  # Only show meaningful text
                    print(f"Element {i+1}: {text}")

            # Also check title
            title = soup.find("title")
            if title:
                print(f"\nPage Title: {title.get_text()}")

            # Look for any text that might contain the actual error
            all_text = soup.get_text()
            print(f"\n=== FULL EXTRACTED TEXT ===")
            print(all_text)

        except Exception as e:
            print(f"Could not parse HTML: {e}")
            print(f"\nRaw response (first 2000 chars):")
            print(full_response[:2000])

    except Exception as e:
        print(f"Request failed: {e}")

    # Also test with different Content-Type headers
    print("\n\n=== TESTING DIFFERENT CONTENT-TYPE HEADERS ===")

    content_types_to_try = [
        "application/json",
        "application/xml",
        "text/xml",
        "application/json; charset=utf-8",
        "text/plain",
    ]

    for content_type in content_types_to_try:
        print(f"\n--- Testing with Content-Type: {content_type} ---")

        test_headers = headers.copy()
        test_headers["Content-Type"] = content_type

        try:
            response = requests.post(
                f"{base_url}/UpdateUser",
                headers=test_headers,
                json=update_user_payload,
                verify=False,
                timeout=10,
            )

            print(f"Status: {response.status_code}")
            if response.status_code != 400:
                print(f"Response: {response.text[:200]}...")
                if response.status_code == 200:
                    print("🎉 SUCCESS! This Content-Type works!")
                    break
            else:
                print("Still 400 error")

        except Exception as e:
            print(f"Error: {e}")


if __name__ == "__main__":
    get_error_details()
