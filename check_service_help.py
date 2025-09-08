import requests


def check_service_help():
    """Check the service help page to understand correct request format"""

    help_url = "http://192.168.10.206:9001/Unison.AccessService/help"

    print("Fetching service help page...")
    print(f"URL: {help_url}")
    print()

    try:
        response = requests.get(help_url, timeout=30)
        print(f"Status: {response.status_code}")
        print("Content:")
        print("=" * 60)
        print(response.text)
        print("=" * 60)

    except Exception as e:
        print(f"Error: {e}")


def check_service_operations():
    """Check what operations are available"""

    operations_to_check = ["UpdateCard", "Ping", "GetVersion", "UpdateUser"]

    base_url = "http://192.168.10.206:9001/Unison.AccessService"

    for op in operations_to_check:
        print(f"\nChecking operation: {op}")
        print("-" * 40)

        url = f"{base_url}/{op}"

        # Try GET first to see what happens
        try:
            response = requests.get(url, timeout=10)
            print(f"GET {op}: Status {response.status_code}")
            if response.status_code != 404:
                print(f"Response snippet: {response.text[:200]}...")
        except Exception as e:
            print(f"GET {op}: Error {e}")

        # Try POST with empty body
        try:
            response = requests.post(
                url, json={}, headers={"Content-Type": "application/json"}, timeout=10
            )
            print(f"POST {op}: Status {response.status_code}")
            if response.status_code != 404:
                print(f"Response snippet: {response.text[:200]}...")
        except Exception as e:
            print(f"POST {op}: Error {e}")


if __name__ == "__main__":
    print("Unison Access Service - Help Page and Operations Check")
    print("=" * 60)

    check_service_help()

    print("\n" + "=" * 60)
    print("OPERATIONS CHECK")
    print("=" * 60)

    check_service_operations()
