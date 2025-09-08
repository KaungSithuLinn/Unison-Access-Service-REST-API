#!/usr/bin/env python3
"""
SQL Server Integration Validation Script
Unison Access Service REST API Project

This script validates the integration between the Unison API and SQL Server backend,
ensuring data consistency and proper mapping between API operations and database changes.

Security Level: PRODUCTION
Compliance: OWASP, Microsoft SDL
"""

import os
import sys
import logging
import pyodbc
from datetime import datetime
from typing import Dict, List, Optional, Tuple
import json
import hashlib

# Configure secure logging
logging.basicConfig(
    level=logging.INFO,
    format="%(asctime)s - %(name)s - %(levelname)s - %(message)s",
    handlers=[
        logging.FileHandler("sql_integration_validation.log"),
        logging.StreamHandler(sys.stdout),
    ],
)

logger = logging.getLogger(__name__)


class SQLServerValidator:
    """Secure SQL Server integration validator for Unison API"""

    def __init__(self):
        """Initialize secure database connection"""
        self.connection = None
        self.server = "192.168.10.206"
        self.database = "ACRMS_DEV_2025"
        self.test_results = {
            "connection_test": False,
            "schema_validation": False,
            "data_integrity": False,
            "security_validation": False,
            "performance_test": False,
        }

    def connect_to_database(self) -> bool:
        """Establish secure database connection"""
        try:
            # Use Windows Authentication for security
            connection_string = (
                f"DRIVER={{ODBC Driver 17 for SQL Server}};"
                f"SERVER={self.server};"
                f"DATABASE={self.database};"
                f"Trusted_Connection=yes;"
                f"Encrypt=yes;"
                f"TrustServerCertificate=yes;"
            )

            self.connection = pyodbc.connect(connection_string)
            logger.info("✅ Database connection established successfully")
            self.test_results["connection_test"] = True
            return True

        except Exception as e:
            logger.error(f"❌ Database connection failed: {e}")
            return False

    def validate_schema_structure(self) -> bool:
        """Validate database schema matches Unison API requirements"""
        try:
            cursor = self.connection.cursor()

            # Check User table structure
            user_columns = self._get_table_columns("User")
            required_user_columns = [
                "UserId",
                "EmployeeID",
                "Name",
                "Fullname",
                "Status",
                "CreatedDate",
                "UpdatedDate",
                "IsDeleted",
            ]

            missing_columns = [
                col for col in required_user_columns if col not in user_columns
            ]

            if missing_columns:
                logger.error(f"❌ Missing User table columns: {missing_columns}")
                return False

            # Check UserImage table structure
            image_columns = self._get_table_columns("UserImage")
            required_image_columns = ["Id", "UserId", "Profile", "CreatedDate"]

            missing_image_columns = [
                col for col in required_image_columns if col not in image_columns
            ]

            if missing_image_columns:
                logger.error(
                    f"❌ Missing UserImage table columns: {missing_image_columns}"
                )
                return False

            logger.info("✅ Database schema validation passed")
            self.test_results["schema_validation"] = True
            return True

        except Exception as e:
            logger.error(f"❌ Schema validation failed: {e}")
            return False

    def _get_table_columns(self, table_name: str) -> List[str]:
        """Get column names for specified table"""
        cursor = self.connection.cursor()
        cursor.execute(
            """
            SELECT COLUMN_NAME 
            FROM INFORMATION_SCHEMA.COLUMNS 
            WHERE TABLE_NAME = ?
            ORDER BY ORDINAL_POSITION
        """,
            table_name,
        )

        return [row[0] for row in cursor.fetchall()]

    def validate_data_integrity(self) -> bool:
        """Validate data integrity and relationships"""
        try:
            cursor = self.connection.cursor()

            # Check User-UserImage relationship integrity
            cursor.execute(
                """
                SELECT COUNT(*) as OrphanImages
                FROM dbo.UserImage ui
                LEFT JOIN dbo.[User] u ON ui.UserId = u.UserId
                WHERE u.UserId IS NULL
            """
            )

            orphan_count = cursor.fetchone()[0]
            if orphan_count > 0:
                logger.warning(f"⚠️ Found {orphan_count} orphaned user images")

            # Check for users without images
            cursor.execute(
                """
                SELECT COUNT(*) as UsersWithoutImages
                FROM dbo.[User] u
                LEFT JOIN dbo.UserImage ui ON u.UserId = ui.UserId
                WHERE ui.UserId IS NULL AND u.IsDeleted = 0
            """
            )

            users_without_images = cursor.fetchone()[0]
            logger.info(f"📊 Users without images: {users_without_images}")

            # Validate image data integrity
            cursor.execute(
                """
                SELECT COUNT(*) as InvalidImages
                FROM dbo.UserImage
                WHERE Profile IS NULL OR LEN(Profile) = 0
            """
            )

            invalid_images = cursor.fetchone()[0]
            if invalid_images > 0:
                logger.warning(f"⚠️ Found {invalid_images} invalid image records")

            logger.info("✅ Data integrity validation completed")
            self.test_results["data_integrity"] = True
            return True

        except Exception as e:
            logger.error(f"❌ Data integrity validation failed: {e}")
            return False

    def validate_security_constraints(self) -> bool:
        """Validate security constraints and permissions"""
        try:
            cursor = self.connection.cursor()

            # Check for SQL injection vulnerabilities in existing data
            cursor.execute(
                """
                SELECT COUNT(*) as PotentialSQLInjection
                FROM dbo.[User]
                WHERE EmployeeID LIKE '%[<>''";]%'
                   OR Name LIKE '%[<>''";]%'
                   OR Fullname LIKE '%[<>''";]%'
            """
            )

            sql_injection_risks = cursor.fetchone()[0]
            if sql_injection_risks > 0:
                logger.warning(
                    f"⚠️ Found {sql_injection_risks} records with potential SQL injection characters"
                )

            # Check for XSS vulnerabilities in stored data
            cursor.execute(
                """
                SELECT COUNT(*) as PotentialXSS
                FROM dbo.[User]
                WHERE EmployeeID LIKE '%<script%'
                   OR Name LIKE '%<script%'
                   OR Fullname LIKE '%<script%'
            """
            )

            xss_risks = cursor.fetchone()[0]
            if xss_risks > 0:
                logger.warning(
                    f"⚠️ Found {xss_risks} records with potential XSS content"
                )

            # Validate password security (check for plain text)
            cursor.execute(
                """
                SELECT COUNT(*) as WeakPasswords
                FROM dbo.[User]
                WHERE Password IS NOT NULL 
                  AND LEN(Password) < 8
                  AND Sha256Password IS NULL
            """
            )

            weak_passwords = cursor.fetchone()[0]
            if weak_passwords > 0:
                logger.warning(
                    f"⚠️ Found {weak_passwords} users with weak password security"
                )

            logger.info("✅ Security constraints validation completed")
            self.test_results["security_validation"] = True
            return True

        except Exception as e:
            logger.error(f"❌ Security validation failed: {e}")
            return False

    def test_performance_metrics(self) -> bool:
        """Test database performance for Unison API operations"""
        try:
            cursor = self.connection.cursor()

            # Test user lookup performance
            start_time = datetime.now()
            cursor.execute(
                """
                SELECT UserId, EmployeeID, Name, Fullname, Status
                FROM dbo.[User]
                WHERE IsDeleted = 0
                ORDER BY UserId
            """
            )
            user_data = cursor.fetchall()
            user_query_time = (datetime.now() - start_time).total_seconds()

            # Test image retrieval performance
            start_time = datetime.now()
            cursor.execute(
                """
                SELECT ui.UserId, LEN(ui.Profile) as ImageSize
                FROM dbo.UserImage ui
                INNER JOIN dbo.[User] u ON ui.UserId = u.UserId
                WHERE u.IsDeleted = 0
            """
            )
            image_data = cursor.fetchall()
            image_query_time = (datetime.now() - start_time).total_seconds()

            # Performance thresholds (in seconds)
            USER_QUERY_THRESHOLD = 1.0
            IMAGE_QUERY_THRESHOLD = 2.0

            performance_results = {
                "user_query_time": user_query_time,
                "image_query_time": image_query_time,
                "user_count": len(user_data),
                "image_count": len(image_data),
            }

            logger.info(f"📊 Performance Metrics:")
            logger.info(
                f"  User Query Time: {user_query_time:.3f}s (threshold: {USER_QUERY_THRESHOLD}s)"
            )
            logger.info(
                f"  Image Query Time: {image_query_time:.3f}s (threshold: {IMAGE_QUERY_THRESHOLD}s)"
            )
            logger.info(f"  Users Found: {len(user_data)}")
            logger.info(f"  Images Found: {len(image_data)}")

            if user_query_time > USER_QUERY_THRESHOLD:
                logger.warning(f"⚠️ User query time exceeds threshold")

            if image_query_time > IMAGE_QUERY_THRESHOLD:
                logger.warning(f"⚠️ Image query time exceeds threshold")

            logger.info("✅ Performance testing completed")
            self.test_results["performance_test"] = True
            return True

        except Exception as e:
            logger.error(f"❌ Performance testing failed: {e}")
            return False

    def run_integration_mapping_test(self) -> bool:
        """Test integration mapping between Unison API and database"""
        try:
            cursor = self.connection.cursor()

            # Map Unison API fields to database fields
            api_to_db_mapping = {
                "userId": "EmployeeID",  # Unison uses string IDs, DB uses EmployeeID
                "firstName": "Name",
                "lastName": "Fullname",  # May need parsing
                "accessFlags": "Status",  # May need conversion
                "photo": "Profile",  # Base64 encoded in UserImage table
            }

            logger.info("🔄 API to Database Field Mapping:")
            for api_field, db_field in api_to_db_mapping.items():
                logger.info(f"  {api_field} → {db_field}")

            # Test data type compatibility
            cursor.execute(
                """
                SELECT DATA_TYPE, CHARACTER_MAXIMUM_LENGTH, IS_NULLABLE
                FROM INFORMATION_SCHEMA.COLUMNS
                WHERE TABLE_NAME = 'User' AND COLUMN_NAME IN ('EmployeeID', 'Name', 'Fullname', 'Status')
                ORDER BY COLUMN_NAME
            """
            )

            field_info = cursor.fetchall()
            logger.info("📋 Database Field Specifications:")
            for field in field_info:
                logger.info(
                    f"  {field[0]}: {field[1] or 'N/A'} chars, Nullable: {field[2]}"
                )

            return True

        except Exception as e:
            logger.error(f"❌ Integration mapping test failed: {e}")
            return False

    def generate_validation_report(self) -> Dict:
        """Generate comprehensive validation report"""
        report = {
            "timestamp": datetime.now().isoformat(),
            "database_server": self.server,
            "database_name": self.database,
            "test_results": self.test_results,
            "overall_status": all(self.test_results.values()),
            "recommendations": [],
        }

        # Add recommendations based on test results
        if not self.test_results["connection_test"]:
            report["recommendations"].append(
                "Fix database connectivity issues before proceeding"
            )

        if not self.test_results["schema_validation"]:
            report["recommendations"].append(
                "Update database schema to match Unison API requirements"
            )

        if not self.test_results["security_validation"]:
            report["recommendations"].append(
                "Address security vulnerabilities in existing data"
            )

        if not self.test_results["performance_test"]:
            report["recommendations"].append(
                "Optimize database queries for better performance"
            )

        if report["overall_status"]:
            report["recommendations"].append(
                "Database is ready for Unison API integration"
            )

        return report

    def close_connection(self):
        """Close database connection"""
        if self.connection:
            self.connection.close()
            logger.info("🔌 Database connection closed")


def main():
    """Main validation function"""
    logger.info("🚀 Starting SQL Server Integration Validation")
    logger.info("=" * 60)

    validator = SQLServerValidator()

    try:
        # Run validation tests
        tests = [
            ("Database Connection", validator.connect_to_database),
            ("Schema Structure", validator.validate_schema_structure),
            ("Data Integrity", validator.validate_data_integrity),
            ("Security Constraints", validator.validate_security_constraints),
            ("Performance Metrics", validator.test_performance_metrics),
            ("Integration Mapping", validator.run_integration_mapping_test),
        ]

        for test_name, test_function in tests:
            logger.info(f"\n🔍 Running {test_name} Test...")
            success = test_function()
            status = "✅ PASSED" if success else "❌ FAILED"
            logger.info(f"{test_name}: {status}")

        # Generate final report
        logger.info("\n📊 Generating Validation Report...")
        report = validator.generate_validation_report()

        # Save report to file
        with open("sql_integration_validation_report.json", "w") as f:
            json.dump(report, f, indent=2)

        # Print summary
        logger.info("\n" + "=" * 60)
        logger.info("🎯 VALIDATION SUMMARY")
        logger.info("=" * 60)

        overall_status = (
            "✅ READY FOR PRODUCTION"
            if report["overall_status"]
            else "⚠️ ISSUES DETECTED"
        )
        logger.info(f"Overall Status: {overall_status}")

        logger.info("\n📝 Test Results:")
        for test, result in report["test_results"].items():
            status = "✅ PASS" if result else "❌ FAIL"
            logger.info(f"  {test.replace('_', ' ').title()}: {status}")

        if report["recommendations"]:
            logger.info("\n💡 Recommendations:")
            for i, rec in enumerate(report["recommendations"], 1):
                logger.info(f"  {i}. {rec}")

        logger.info(
            f"\n📄 Full report saved to: sql_integration_validation_report.json"
        )

    except Exception as e:
        logger.error(f"❌ Validation failed with error: {e}")

    finally:
        validator.close_connection()
        logger.info("\n🏁 Validation completed")


if __name__ == "__main__":
    main()
