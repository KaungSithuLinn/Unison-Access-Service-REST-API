---
description: AI rules derived by SpecStory from the project AI interaction history
globs: *
---

## PROJECT RULES & CODING STANDARDS

This document outlines the rules, coding standards, workflow guidelines, references, documentation structures, and best practices for this project. All AI coding assistants must adhere to these guidelines.

## TABLE OF CONTENTS

1.  <headers/>
2.  TECH STACK
3.  PROJECT DOCUMENTATION & CONTEXT SYSTEM
4.  CODING STANDARDS
5.  NAMING CONVENTIONS
6.  ERROR HANDLING
7.  LOGGING
8.  PERFORMANCE
9.  SECURITY
10. API ENDPOINTS
11. MONITORING
12. TESTING
13. WORKFLOW & RELEASE RULES
14. DEBUGGING

## TECH STACK

*   .NET 6 or later
*   C#
*   ASP.NET Core
*   Serilog
*   Playwright
*   Terraform
*   NUnit
*   (Potentially) WCF

## PROJECT DOCUMENTATION & CONTEXT SYSTEM

*   **Spec-Kit:** Project uses a "Spec-Kit" system for managing specifications, tasks, and documentation. Key files include:
    *   `specs/latest/spec.md`:  The current project specification document.
    *   `tasks.json`:  A JSON file outlining the tasks and their status.
    *   `chat_summary.md`: Summaries of recent chat interactions.
    *   `memory/current_phase.json`: Tracks the current development phase.

## CODING STANDARDS

*   Follow Microsoft's C# coding conventions.
*   Use dependency injection for managing dependencies.
*   Write unit tests for all components.
*   All code must be analyzed with Codacy before pull requests are submitted.
*   Apply OWASP-recommended security headers.
*   Ensure XML documentation for all public methods and classes.

## NAMING CONVENTIONS

*   Use descriptive and meaningful names for variables, methods, and classes.
*   Follow PascalCase for class and method names.
*   Follow camelCase for variable names.

## ERROR HANDLING

*   Implement robust error handling using try-catch blocks.
*   Translate SOAP faults into meaningful REST error responses.
*   Use Polly for resilience patterns (retry logic, circuit breaker).

## LOGGING

*   Use Serilog for structured logging.
*   Include request tracing and correlation IDs in logs.
*   Enrich log entries with MachineName, ProcessId, ThreadId, EnvironmentName, ApplicationName, Environment, and Version.

## PERFORMANCE

*   Implement HTTP client connection pooling.
*   Use response caching to reduce latency.
*   Monitor performance metrics (response time, memory usage, CPU usage).

## SECURITY

*   Implement OWASP security recommendations.
*   Use HTTPS and enforce HSTS.
*   Sanitize user inputs to prevent XSS and SQL injection attacks.
*   Implement rate limiting and IP whitelisting.

## API ENDPOINTS

*   Adhere to RESTful principles.
*   Provide comprehensive card management endpoints:
    *   `/api/cards/update` (PUT)
    *   `/api/cards/{cardId}` (GET)
    *   `/api/cards/validate` (POST)
    *   `/api/cards/activate` (POST)
    *   `/api/cards/deactivate` (POST)
    *   `/api/cards/status/{cardId}` (GET)
*   Implement system information endpoints:
    *   `/api/system/version` (GET)
    *   `/api/system/ping` (GET)
*   Implement metrics endpoint:
    *   `/api/metrics` (GET)
*   Implement health check endpoints:
    *   `/health` (GET)
    *   `/health/detailed` (GET)
    *   `/health/ready` (GET)
    *   `/health/live` (GET)

## MONITORING

*   Implement comprehensive health checks.
*   Collect application metrics (requests per minute, error rate).
*   Monitor dependency status.

## TESTING

*   Implement integration tests using Playwright.
*   Create tests for all API endpoints and functionalities.

## WORKFLOW & RELEASE RULES

*   Use Git for version control.
*   Create pull requests for all changes.
*   All code must be reviewed before merging.
*   Run Codacy analysis on all pull requests.
*   Create new feature branch for each issue.
*   Ensure all feature branches are pushed to the remote repository before creating pull requests.
*   When merging pull requests, use squash merge and delete the branch: `gh pr merge <number> --squash --delete-branch`

## DEBUGGING

*   Use structured logging to aid in debugging.
*   Use remote debugging tools when necessary.