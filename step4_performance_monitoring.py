#!/usr/bin/env python3
"""
Step 4: Monitoring and Performance Validation
WCF REST API Performance Monitoring Tool
"""

import requests
import time
import json
import statistics
import sys
import threading
from datetime import datetime
from typing import Dict, List, Any, Optional
import concurrent.futures
import warnings

try:
    import psutil

    PSUTIL_AVAILABLE = True
except ImportError:
    PSUTIL_AVAILABLE = False
    print("Warning: psutil not available. CPU/Memory monitoring disabled.")

warnings.filterwarnings("ignore")


class WCFPerformanceMonitor:
    def __init__(self, base_url: str = "http://localhost:8081", timeout: int = 30):
        self.base_url = base_url.rstrip("/")
        self.timeout = timeout
        self.results = []
        self.metrics = {
            "response_times": [],
            "error_count": 0,
            "total_requests": 0,
            "successful_requests": 0,
            "uptime_checks": [],
            "cpu_usage": [],
            "memory_usage": [],
        }

    def measure_response_time(
        self, endpoint: str, method: str = "GET", data: dict = None
    ) -> Dict[str, Any]:
        """Measure response time for a specific endpoint"""
        url = f"{self.base_url}/{endpoint.lstrip('/')}"

        start_time = time.time()
        cpu_before = psutil.cpu_percent()
        memory_before = psutil.virtual_memory().percent

        try:
            if method.upper() == "GET":
                response = requests.get(url, timeout=self.timeout)
            elif method.upper() == "POST":
                response = requests.post(url, json=data, timeout=self.timeout)
            else:
                response = requests.request(
                    method, url, json=data, timeout=self.timeout
                )

            end_time = time.time()
            response_time = (end_time - start_time) * 1000  # Convert to milliseconds

            cpu_after = psutil.cpu_percent()
            memory_after = psutil.virtual_memory().percent

            result = {
                "endpoint": endpoint,
                "method": method,
                "status_code": response.status_code,
                "response_time_ms": response_time,
                "response_size_bytes": len(response.content),
                "timestamp": datetime.now().isoformat(),
                "cpu_usage_before": cpu_before,
                "cpu_usage_after": cpu_after,
                "memory_usage_before": memory_before,
                "memory_usage_after": memory_after,
                "success": 200 <= response.status_code < 300,
            }

            self.metrics["total_requests"] += 1
            if result["success"]:
                self.metrics["successful_requests"] += 1
                self.metrics["response_times"].append(response_time)
            else:
                self.metrics["error_count"] += 1

            self.metrics["cpu_usage"].append(cpu_after)
            self.metrics["memory_usage"].append(memory_after)

            return result

        except requests.exceptions.RequestException as e:
            end_time = time.time()
            response_time = (end_time - start_time) * 1000

            result = {
                "endpoint": endpoint,
                "method": method,
                "status_code": 0,
                "response_time_ms": response_time,
                "response_size_bytes": 0,
                "timestamp": datetime.now().isoformat(),
                "error": str(e),
                "success": False,
                "cpu_usage_before": cpu_before,
                "cpu_usage_after": psutil.cpu_percent(),
                "memory_usage_before": memory_before,
                "memory_usage_after": psutil.virtual_memory().percent,
            }

            self.metrics["total_requests"] += 1
            self.metrics["error_count"] += 1

            return result

    def check_uptime(self) -> bool:
        """Check if the API is accessible"""
        try:
            response = requests.get(f"{self.base_url}/help", timeout=5)
            is_up = response.status_code == 200
            self.metrics["uptime_checks"].append(is_up)
            return is_up
        except (requests.RequestException, requests.Timeout, ConnectionError):
            self.metrics["uptime_checks"].append(False)
            return False

    def load_test(
        self, endpoint: str, concurrent_users: int = 5, requests_per_user: int = 10
    ) -> List[Dict[str, Any]]:
        """Perform load testing with concurrent users"""
        print(
            f"Starting load test: {concurrent_users} concurrent users, {requests_per_user} requests each"
        )

        results = []

        def user_session():
            user_results = []
            for _ in range(requests_per_user):
                result = self.measure_response_time(endpoint)
                user_results.append(result)
                time.sleep(0.1)  # Small delay between requests
            return user_results

        with concurrent.futures.ThreadPoolExecutor(
            max_workers=concurrent_users
        ) as executor:
            futures = [executor.submit(user_session) for _ in range(concurrent_users)]

            for future in concurrent.futures.as_completed(futures):
                try:
                    user_results = future.result()
                    results.extend(user_results)
                except Exception as e:
                    print(f"Error in user session: {e}")

        return results

    def test_api_endpoints(self) -> Dict[str, Any]:
        """Test common API endpoints based on WCF REST service patterns"""
        endpoints_to_test = [
            {"endpoint": "help", "method": "GET"},
            {"endpoint": "User", "method": "GET"},
            {"endpoint": "Users", "method": "GET"},
            {"endpoint": "Card", "method": "GET"},
            {"endpoint": "Cards", "method": "GET"},
            {"endpoint": "Status", "method": "GET"},
            {"endpoint": "Health", "method": "GET"},
        ]

        test_results = []

        print("Testing API Endpoints...")
        for test in endpoints_to_test:
            print(f"Testing {test['method']} {test['endpoint']}...")
            result = self.measure_response_time(test["endpoint"], test["method"])
            test_results.append(result)

        return test_results

    def generate_performance_report(self) -> Dict[str, Any]:
        """Generate comprehensive performance report"""
        if not self.metrics["response_times"]:
            return {
                "error": "No successful requests recorded",
                "metrics": self.metrics,
                "timestamp": datetime.now().isoformat(),
            }

        response_times = self.metrics["response_times"]
        uptime_percentage = (
            (
                sum(self.metrics["uptime_checks"])
                / len(self.metrics["uptime_checks"])
                * 100
            )
            if self.metrics["uptime_checks"]
            else 0
        )

        report = {
            "summary": {
                "total_requests": self.metrics["total_requests"],
                "successful_requests": self.metrics["successful_requests"],
                "error_count": self.metrics["error_count"],
                "success_rate_percent": (
                    (
                        self.metrics["successful_requests"]
                        / self.metrics["total_requests"]
                        * 100
                    )
                    if self.metrics["total_requests"] > 0
                    else 0
                ),
                "uptime_percentage": uptime_percentage,
            },
            "response_time_metrics": {
                "average_ms": statistics.mean(response_times),
                "median_ms": statistics.median(response_times),
                "min_ms": min(response_times),
                "max_ms": max(response_times),
                "std_deviation_ms": (
                    statistics.stdev(response_times) if len(response_times) > 1 else 0
                ),
            },
            "resource_usage": {
                "avg_cpu_percent": (
                    statistics.mean(self.metrics["cpu_usage"])
                    if self.metrics["cpu_usage"]
                    else 0
                ),
                "max_cpu_percent": (
                    max(self.metrics["cpu_usage"]) if self.metrics["cpu_usage"] else 0
                ),
                "avg_memory_percent": (
                    statistics.mean(self.metrics["memory_usage"])
                    if self.metrics["memory_usage"]
                    else 0
                ),
                "max_memory_percent": (
                    max(self.metrics["memory_usage"])
                    if self.metrics["memory_usage"]
                    else 0
                ),
            },
            "timestamp": datetime.now().isoformat(),
            "test_configuration": {
                "base_url": self.base_url,
                "timeout_seconds": self.timeout,
            },
        }

        return report


def main():
    print("=== WCF REST API Performance Monitoring ===")
    print("Step 4: Monitoring and Performance Validation")
    print()

    # Initialize monitor
    monitor = WCFPerformanceMonitor("http://localhost:8081")

    # Check if API is accessible
    print("1. Checking API Uptime...")
    is_up = monitor.check_uptime()
    print(f"   API Status: {'UP' if is_up else 'DOWN'}")

    if not is_up:
        print(
            "   WARNING: API is not accessible. Testing will proceed with error recording."
        )

    print()

    # Test individual endpoints
    print("2. Testing API Endpoints...")
    endpoint_results = monitor.test_api_endpoints()

    print("\n   Endpoint Test Results:")
    for result in endpoint_results:
        status = "✅" if result["success"] else "❌"
        print(
            f"   {status} {result['method']} /{result['endpoint']} - {result.get('status_code', 'ERROR')} - {result['response_time_ms']:.2f}ms"
        )

    print()

    # Perform load test if API is accessible
    if is_up:
        print("3. Performing Load Test...")
        load_results = monitor.load_test(
            "help", concurrent_users=3, requests_per_user=5
        )
        print(f"   Load test completed: {len(load_results)} total requests")
    else:
        print("3. Skipping Load Test (API not accessible)")

    print()

    # Generate performance report
    print("4. Generating Performance Report...")
    report = monitor.generate_performance_report()

    # Save report to file
    report_filename = (
        f"step4_performance_report_{datetime.now().strftime('%Y%m%d_%H%M%S')}.json"
    )
    with open(report_filename, "w") as f:
        json.dump(report, f, indent=2)

    print(f"   Report saved to: {report_filename}")

    # Display key metrics
    print("\n=== PERFORMANCE SUMMARY ===")
    if "summary" in report:
        summary = report["summary"]
        print(f"Total Requests: {summary['total_requests']}")
        print(f"Success Rate: {summary['success_rate_percent']:.1f}%")
        print(f"Uptime: {summary['uptime_percentage']:.1f}%")

        if "response_time_metrics" in report:
            rt_metrics = report["response_time_metrics"]
            print(f"Avg Response Time: {rt_metrics['average_ms']:.2f}ms")
            print(f"Max Response Time: {rt_metrics['max_ms']:.2f}ms")

        if "resource_usage" in report:
            resource = report["resource_usage"]
            print(f"Avg CPU Usage: {resource['avg_cpu_percent']:.1f}%")
            print(f"Avg Memory Usage: {resource['avg_memory_percent']:.1f}%")
    else:
        print("ERROR: Unable to generate performance metrics")

    print("\n=== RECOMMENDATIONS ===")

    if not is_up:
        print("🔴 CRITICAL: API is not accessible")
        print("   - Verify API service is running")
        print("   - Check port configuration (expected: 8081)")
        print("   - Review firewall settings")

    if "response_time_metrics" in report:
        avg_time = report["response_time_metrics"]["average_ms"]
        if avg_time > 1000:
            print("⚠️  WARNING: High response times detected")
            print(f"   - Average response time: {avg_time:.2f}ms")
            print("   - Consider performance optimization")
        elif avg_time > 500:
            print("⚠️  CAUTION: Moderate response times")
            print(f"   - Average response time: {avg_time:.2f}ms")
        else:
            print("✅ GOOD: Response times within acceptable range")

    if "summary" in report and report["summary"]["success_rate_percent"] < 95:
        print("⚠️  WARNING: Low success rate")
        print("   - Check API error handling")
        print("   - Review server logs for issues")

    return report


if __name__ == "__main__":
    main()
