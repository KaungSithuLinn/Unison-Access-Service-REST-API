import requests
import os

base_url = "http://192.168.10.206:9003/Unison.AccessService"
token = os.getenv("UNISON_API_TOKEN")

headers = {"Unison-Token": token, "Accept": "text/html", "User-Agent": "Mozilla/5.0"}

print("=== Accessing WCF Service Help Page ===")

try:
    response = requests.get(
        f"{base_url}/help", headers=headers, verify=False, timeout=10
    )
    print(f"Status: {response.status_code}")
    print(f"Content-Type: {response.headers.get('Content-Type')}")
    print(f"Response length: {len(response.text)}")
    print(f"Response:\n{response.text}")
except Exception as e:
    print(f"Error: {e}")

# Also try without help path
print("\n=== Accessing Base Service URL ===")
try:
    response = requests.get(base_url, headers=headers, verify=False, timeout=10)
    print(f"Status: {response.status_code}")
    print(f"Content-Type: {response.headers.get('Content-Type')}")
    print(f"Response:\n{response.text[:1000]}")
except Exception as e:
    print(f"Error: {e}")
