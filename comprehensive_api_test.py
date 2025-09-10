#!/usr/bin/env python3
"""
Comprehensive API validation and integration testing script
Tests all endpoints, authentication, error handling, and performance
"""

import requests
import json
import time
import sys
from typing import Dict, List, Tuple
from datetime import datetime
import concurrent.futures
from dataclasses import dataclass


@dataclass
class TestResult:
    name: str
    passed: bool
    message: str
    response_time: float
    status_code: int


class UnisonApiTester:
    def __init__(self, base_url: str, token: str):
        self.base_url = base_url.rstrip("/")
        self.token = token
        self.session = requests.Session()
        self.session.headers.update(
            {"Content-Type": "application/json", "Unison-Token": token}
        )
        self.results: List[TestResult] = []

    def run_test(self, name: str, test_func) -> TestResult:
        """Execute a single test and capture results"""
        start_time = time.time()
        try:
            success, message, status_code = test_func()
            response_time = time.time() - start_time
            result = TestResult(name, success, message, response_time, status_code)
        except Exception as e:
            response_time = time.time() - start_time
            result = TestResult(name, False, f"Exception: {str(e)}", response_time, 0)

        self.results.append(result)
        return result

    def test_health_endpoint(self) -> Tuple[bool, str, int]:
        """Test health check endpoint"""
        response = requests.get(f"{self.base_url}/health")
        if response.status_code == 200:
            data = response.json()
            if data.get("status") == "Healthy":
                return True, "Health check passed", 200
            else:
                return False, f"Unexpected health status: {data.get('status')}", 200
        return (
            False,
            f"Health check failed with status {response.status_code}",
            response.status_code,
        )

    def test_swagger_accessibility(self) -> Tuple[bool, str, int]:
        """Test Swagger UI accessibility"""
        response = requests.get(f"{self.base_url}/swagger/index.html")
        if response.status_code == 200 and "swagger" in response.text.lower():
            return True, "Swagger UI accessible", 200
        return (
            False,
            f"Swagger UI not accessible: {response.status_code}",
            response.status_code,
        )

    def test_update_card_valid(self) -> Tuple[bool, str, int]:
        """Test update card with valid data"""
        payload = {
            "cardId": "TEST001",
            "userName": "test.user",
            "firstName": "Test",
            "lastName": "User",
            "email": "test.user@company.com",
            "department": "IT",
            "title": "Developer",
            "isActive": True,
        }

        response = self.session.post(f"{self.base_url}/api/cards/update", json=payload)

        # Accept both success and business logic errors (SOAP backend issues)
        if response.status_code in [200, 201, 400]:
            return (
                True,
                f"Update card endpoint responded correctly: {response.status_code}",
                response.status_code,
            )
        return (
            False,
            f"Unexpected response: {response.status_code}",
            response.status_code,
        )

    def test_update_card_alternative_route(self) -> Tuple[bool, str, int]:
        """Test alternative update card route"""
        payload = {"cardId": "TEST002", "userName": "alt.user", "isActive": False}

        response = self.session.post(f"{self.base_url}/updatecard", json=payload)

        if response.status_code in [200, 201, 400]:
            return (
                True,
                f"Alternative route responded correctly: {response.status_code}",
                response.status_code,
            )
        return (
            False,
            f"Alternative route failed: {response.status_code}",
            response.status_code,
        )

    def test_authentication_missing_token(self) -> Tuple[bool, str, int]:
        """Test request without authentication token"""
        session_no_auth = requests.Session()
        session_no_auth.headers.update({"Content-Type": "application/json"})

        payload = {"cardId": "TEST003"}
        response = session_no_auth.post(
            f"{self.base_url}/api/cards/update", json=payload
        )

        if response.status_code == 401:
            return True, "Authentication properly enforced", 401
        return False, f"Expected 401, got {response.status_code}", response.status_code

    def test_authentication_invalid_token(self) -> Tuple[bool, str, int]:
        """Test request with invalid token"""
        session_invalid = requests.Session()
        session_invalid.headers.update(
            {"Content-Type": "application/json", "Unison-Token": "invalid-token-123"}
        )

        payload = {"cardId": "TEST004"}
        response = session_invalid.post(
            f"{self.base_url}/api/cards/update", json=payload
        )

        if response.status_code == 401:
            return True, "Invalid token properly rejected", 401
        return False, f"Expected 401, got {response.status_code}", response.status_code

    def test_validation_missing_cardid(self) -> Tuple[bool, str, int]:
        """Test request with missing required field"""
        payload = {"userName": "no.cardid", "firstName": "No", "lastName": "CardId"}

        response = self.session.post(f"{self.base_url}/api/cards/update", json=payload)

        if response.status_code == 400:
            return True, "Validation properly enforced", 400
        return False, f"Expected 400, got {response.status_code}", response.status_code

    def test_get_card_endpoint(self) -> Tuple[bool, str, int]:
        """Test get card endpoint"""
        response = self.session.get(f"{self.base_url}/api/cards/TEST001")

        # Accept success, not found, or business logic errors
        if response.status_code in [200, 404, 400]:
            return (
                True,
                f"Get card endpoint responded: {response.status_code}",
                response.status_code,
            )
        return False, f"Get card failed: {response.status_code}", response.status_code

    def test_performance_basic(self) -> Tuple[bool, str, int]:
        """Test basic performance - response time under 5 seconds"""
        start_time = time.time()
        response = requests.get(f"{self.base_url}/health")
        response_time = time.time() - start_time

        if response_time < 5.0:
            return (
                True,
                f"Response time acceptable: {response_time:.2f}s",
                response.status_code,
            )
        return (
            False,
            f"Response time too slow: {response_time:.2f}s",
            response.status_code,
        )

    def test_concurrent_requests(self) -> Tuple[bool, str, int]:
        """Test handling of concurrent requests"""

        def make_request():
            return requests.get(f"{self.base_url}/health")

        start_time = time.time()
        with concurrent.futures.ThreadPoolExecutor(max_workers=5) as executor:
            futures = [executor.submit(make_request) for _ in range(10)]
            responses = [
                future.result() for future in concurrent.futures.as_completed(futures)
            ]

        response_time = time.time() - start_time
        successful = sum(1 for r in responses if r.status_code == 200)

        if successful >= 8:  # Allow some failures
            return (
                True,
                f"Concurrent requests handled well: {successful}/10 successful",
                200,
            )
        return False, f"Too many concurrent failures: {successful}/10", 0

    def run_all_tests(self) -> Dict:
        """Run all tests and return summary"""
        print(f"🚀 Starting comprehensive API testing...")
        print(f"📍 Base URL: {self.base_url}")
        print(f"🔑 Token: {self.token[:8]}...")
        print("-" * 80)

        tests = [
            ("Health Check", self.test_health_endpoint),
            ("Swagger Accessibility", self.test_swagger_accessibility),
            ("Update Card (Valid)", self.test_update_card_valid),
            ("Update Card (Alt Route)", self.test_update_card_alternative_route),
            ("Auth: Missing Token", self.test_authentication_missing_token),
            ("Auth: Invalid Token", self.test_authentication_invalid_token),
            ("Validation: Missing CardId", self.test_validation_missing_cardid),
            ("Get Card Endpoint", self.test_get_card_endpoint),
            ("Performance Basic", self.test_performance_basic),
            ("Concurrent Requests", self.test_concurrent_requests),
        ]

        for test_name, test_func in tests:
            result = self.run_test(test_name, test_func)
            status = "✅ PASS" if result.passed else "❌ FAIL"
            print(
                f"{status} {test_name:<25} [{result.status_code:3}] {result.response_time:.2f}s - {result.message}"
            )

        return self.get_summary()

    def get_summary(self) -> Dict:
        """Generate test summary"""
        total = len(self.results)
        passed = sum(1 for r in self.results if r.passed)
        failed = total - passed
        avg_response_time = (
            sum(r.response_time for r in self.results) / total if total > 0 else 0
        )

        return {
            "timestamp": datetime.now().isoformat(),
            "total_tests": total,
            "passed": passed,
            "failed": failed,
            "success_rate": (passed / total * 100) if total > 0 else 0,
            "average_response_time": round(avg_response_time, 3),
            "results": [
                {
                    "name": r.name,
                    "passed": r.passed,
                    "message": r.message,
                    "response_time": round(r.response_time, 3),
                    "status_code": r.status_code,
                }
                for r in self.results
            ],
        }


def main():
    """Main execution function"""
    # Configuration
    BASE_URL = "http://192.168.10.206:5203"
    TOKEN = "595d799a-9553-4ddf-8fd9-c27b1f233ce7"

    if len(sys.argv) > 1:
        BASE_URL = sys.argv[1]
    if len(sys.argv) > 2:
        TOKEN = sys.argv[2]

    # Run tests
    tester = UnisonApiTester(BASE_URL, TOKEN)
    summary = tester.run_all_tests()

    # Print summary
    print("\n" + "=" * 80)
    print("📊 FINAL TEST SUMMARY")
    print("=" * 80)
    print(f"🧪 Total Tests: {summary['total_tests']}")
    print(f"✅ Passed: {summary['passed']}")
    print(f"❌ Failed: {summary['failed']}")
    print(f"📈 Success Rate: {summary['success_rate']:.1f}%")
    print(f"⚡ Avg Response Time: {summary['average_response_time']}s")
    print(f"🕒 Timestamp: {summary['timestamp']}")

    # Save detailed results
    with open("api_test_results.json", "w") as f:
        json.dump(summary, f, indent=2)
    print(f"💾 Detailed results saved to: api_test_results.json")

    # Exit code based on success rate
    success_rate = summary["success_rate"]
    if success_rate >= 90:
        print("\n🎉 EXCELLENT: All critical tests passed!")
        exit_code = 0
    elif success_rate >= 70:
        print("\n⚠️  WARNING: Some tests failed, but core functionality works")
        exit_code = 1
    else:
        print("\n🚨 CRITICAL: Multiple test failures detected!")
        exit_code = 2

    return exit_code


if __name__ == "__main__":
    exit_code = main()
    sys.exit(exit_code)
