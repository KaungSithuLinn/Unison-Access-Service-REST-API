import requests
import os

base_url = "http://192.168.10.206:9003/Unison.AccessService"
token = os.getenv("UNISON_API_TOKEN")

headers = {"Unison-Token": token, "Accept": "text/html", "User-Agent": "Mozilla/5.0"}

print("=== Checking UpdateUser Operation Details ===")

try:
    response = requests.get(
        f"{base_url}/help/operations/UpdateUser",
        headers=headers,
        verify=False,
        timeout=10,
    )
    print(f"Status: {response.status_code}")
    print(f"Content-Type: {response.headers.get('Content-Type')}")
    print(f"Response:\n{response.text}")
except Exception as e:
    print(f"Error: {e}")

print("\n=== Checking UpdateUserPhoto Operation Details ===")

try:
    response = requests.get(
        f"{base_url}/help/operations/UpdateUserPhoto",
        headers=headers,
        verify=False,
        timeout=10,
    )
    print(f"Status: {response.status_code}")
    print(f"Content-Type: {response.headers.get('Content-Type')}")
    print(f"Response:\n{response.text}")
except Exception as e:
    print(f"Error: {e}")

print("\n=== Checking UpdateCard Operation Details ===")

try:
    response = requests.get(
        f"{base_url}/help/operations/UpdateCard",
        headers=headers,
        verify=False,
        timeout=10,
    )
    print(f"Status: {response.status_code}")
    print(f"Content-Type: {response.headers.get('Content-Type')}")
    print(f"Response:\n{response.text}")
except Exception as e:
    print(f"Error: {e}")
