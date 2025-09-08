import requests
import base64
import os
import logging
import re
from typing import Optional, Dict, Any
from pathlib import Path

# Load environment variables from .env file if available
try:
    from dotenv import load_dotenv

    load_dotenv()
except ImportError:
    # dotenv not available, use system environment variables only
    pass

# Configure logging
log_level = os.getenv("LOG_LEVEL", "INFO").upper()
logging.basicConfig(
    level=getattr(logging, log_level),
    format="%(asctime)s - %(levelname)s - %(message)s",
)
logger = logging.getLogger(__name__)


class UnisonAPIConfig:
    """Configuration class for Unison API client with security best practices."""

    def __init__(self):
        # Use HTTPS by default - SECURITY CRITICAL
        self.base_url = os.getenv(
            "UNISON_API_BASE_URL", "https://192.168.10.206:9001/Unison.AccessService"
        )

        # Get token from environment - NEVER hardcode tokens
        self.token = os.getenv("UNISON_API_TOKEN")
        if not self.token:
            raise ValueError("UNISON_API_TOKEN environment variable must be set")

        # SSL verification (can be disabled for dev environments with proper warning)
        self.verify_ssl = os.getenv("UNISON_API_VERIFY_SSL", "true").lower() == "true"
        if not self.verify_ssl:
            logger.warning(
                "SSL verification is disabled. This should only be used in development environments."
            )

        # Request timeout configuration
        self.timeout = int(os.getenv("UNISON_API_TIMEOUT", "30"))

        # File upload limits (in bytes)
        self.max_file_size = int(
            os.getenv("UNISON_API_MAX_FILE_SIZE", "5242880")
        )  # 5MB default

        # Validate base URL uses HTTPS
        if not self.base_url.startswith("https://"):
            if self.base_url.startswith("http://"):
                logger.error(
                    "HTTP URLs are not allowed for security reasons. Use HTTPS instead."
                )
                raise ValueError("Base URL must use HTTPS protocol")
            else:
                raise ValueError("Invalid base URL format")

    @property
    def headers(self) -> Dict[str, str]:
        """Get request headers with security considerations."""
        if not self.token:
            raise ValueError("API token is required")

        return {
            "Content-Type": "application/json",
            "Unison-Token": self.token,
            "User-Agent": "UnisonAPIClient/1.0",
            "Accept": "application/json",
        }


# Initialize configuration
config = UnisonAPIConfig()


def validate_user_id(user_id: str) -> bool:
    """Validate user ID format for security."""
    if not user_id or not isinstance(user_id, str):
        return False
    # Allow alphanumeric and underscore, reasonable length
    return bool(re.match(r"^[a-zA-Z0-9_]{1,50}$", user_id))


def validate_file_path(file_path: str) -> bool:
    """Validate file path for security."""
    if not file_path or not isinstance(file_path, str):
        return False

    path = Path(file_path)
    if not path.exists():
        logger.error(f"File does not exist: {file_path}")
        return False

    if path.stat().st_size > config.max_file_size:
        logger.error(
            f"File too large: {path.stat().st_size} bytes (max: {config.max_file_size})"
        )
        return False

    # Check file extension for security
    allowed_extensions = {".jpg", ".jpeg", ".png", ".bmp"}
    if path.suffix.lower() not in allowed_extensions:
        logger.error(f"Invalid file type: {path.suffix}")
        return False

    return True


def make_secure_request(url: str, payload: Dict[str, Any]) -> Optional[Dict[str, Any]]:
    """Make a secure HTTP request with proper error handling."""
    try:
        logger.info(f"Making request to: {url}")
        resp = requests.post(
            url,
            json=payload,
            headers=config.headers,
            timeout=config.timeout,
            verify=config.verify_ssl,
        )

        logger.info(f"Response status: {resp.status_code}")

        if resp.ok:
            return resp.json() if resp.text else {}
        else:
            logger.error(f"Request failed with status {resp.status_code}: {resp.text}")
            return None

    except requests.exceptions.Timeout:
        logger.error("Request timed out")
        return None
    except requests.exceptions.ConnectionError:
        logger.error("Connection error occurred")
        return None
    except requests.exceptions.RequestException as e:
        logger.error(f"Request error: {e}")
        return None
    except Exception as e:
        logger.error(f"Unexpected error: {e}")
        return None


def update_user(
    user_id: str,
    first_name: str,
    last_name: str,
    pin_code: str,
    valid_from: str,
    valid_until: str,
    access_flags: Optional[int] = None,
    fields: Optional[Dict[str, Any]] = None,
) -> Optional[Dict[str, Any]]:
    """Update user information with security validation."""

    # Input validation
    if not validate_user_id(user_id):
        logger.error("Invalid user ID")
        return None

    if not first_name or not last_name:
        logger.error("First name and last name are required")
        return None

    # Validate PIN code format (should be digits only for security)
    if not pin_code or not pin_code.isdigit() or len(pin_code) < 4:
        logger.error("PIN code must be at least 4 digits")
        return None

    url = f"{config.base_url}/UpdateUser"
    payload = {
        "UserID": user_id,
        "FirstName": first_name,
        "LastName": last_name,
        "PINCode": pin_code,
        "ValidFrom": valid_from,
        "ValidUntil": valid_until,
        "AccessFlags": access_flags or 0,
        "Fields": fields or {},
    }

    logger.info(f"Updating user: {user_id}")
    return make_secure_request(url, payload)


def update_user_photo(user_id: str, photo_path: str) -> Optional[Dict[str, Any]]:
    """Update user photo with security validation."""

    if not validate_user_id(user_id):
        logger.error("Invalid user ID")
        return None

    if not validate_file_path(photo_path):
        logger.error("Invalid or unsafe file path")
        return None

    try:
        with open(photo_path, "rb") as f:
            photo_b64 = base64.b64encode(f.read()).decode("utf-8")
    except IOError as e:
        logger.error(f"Error reading file {photo_path}: {e}")
        return None

    url = f"{config.base_url}/UpdateUserPhoto"
    payload = {"UserID": user_id, "Photo": photo_b64}

    logger.info(f"Updating photo for user: {user_id}")
    return make_secure_request(url, payload)


def update_card(
    user_id: str, profile_name: str, card_number: str, card_status: int = 1
) -> Optional[Dict[str, Any]]:
    """Update card information with security validation."""

    if not validate_user_id(user_id):
        logger.error("Invalid user ID")
        return None

    if not profile_name:
        logger.error("Profile name is required")
        return None

    if not card_number or len(card_number) < 5:
        logger.error("Invalid card number")
        return None

    url = f"{config.base_url}/UpdateCard"
    payload = {
        "UserID": user_id,
        "ProfileName": profile_name,
        "CardNumber": card_number,
        "CardStatus": card_status,
    }

    logger.info(f"Updating card for user: {user_id}")
    return make_secure_request(url, payload)


if __name__ == "__main__":
    # Example usage - Use environment variables for production
    # Set these environment variables before running:
    # UNISON_API_TOKEN=your-secure-token-here
    # UNISON_API_BASE_URL=https://your-api-server.com/Unison.AccessService

    logger.info("Starting Unison API Demo with enhanced security")

    # Example values - these should come from secure configuration in production
    user_id = "testuser123"
    first_name = "Minh"
    last_name = "Nguyen"
    pin_code = "1234"
    valid_from = "2025-09-01T00:00:00"
    valid_until = "2026-09-01T00:00:00"
    photo_path = "user_photo.jpg"  # Ensure this file exists and is a valid image
    profile_name = "Default"
    card_number = "CARD123456"

    # 1. Create or update user
    result = update_user(
        user_id, first_name, last_name, pin_code, valid_from, valid_until
    )
    if result:
        logger.info("User updated successfully")
    else:
        logger.error("Failed to update user")

    # 2. Add photo (only if file exists)
    if Path(photo_path).exists():
        result = update_user_photo(user_id, photo_path)
        if result:
            logger.info("Photo updated successfully")
        else:
            logger.error("Failed to update photo")
    else:
        logger.warning(f"Photo file not found: {photo_path}")

    # 3. Add card
    result = update_card(user_id, profile_name, card_number)
    if result:
        logger.info("Card updated successfully")
    else:
        logger.error("Failed to update card")

    logger.info("Unison API Demo completed")
