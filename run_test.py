#!/usr/bin/env python3
"""
Simple runner for the API tests
"""
import os
import sys
from dotenv import load_dotenv

# Load environment variables
load_dotenv()

# Now run the actual test
if __name__ == "__main__":
    try:
        from test_api_fixes import test_updated_requests

        print("Starting Unison API tests...")
        test_updated_requests()
        print("Test execution completed.")
    except ImportError as e:
        print(f"Error importing test module: {e}")
    except Exception as e:
        print(f"Error running tests: {e}")
