# Unison-Access-Service-REST-API

Enterprise-grade .NET 9.0 REST-to-SOAP adapter for Unison Access Service, featuring secure multi-environment deployment, automated CI/CD pipelines, Docker containerization, and comprehensive test automation.

## Service Architecture Overview

The Unison Access Service backend is a **SOAP 1.1 WCF service** that provides comprehensive access control functionality. Since modern frontend applications require REST/JSON APIs, this project implements a **REST-to-SOAP adapter** that:

- **Translates** REST/JSON requests to SOAP/XML format
- **Provides** modern REST API interface for frontend applications
- **Preserves** all existing backend functionality and security
- **Maintains** compatibility with the Unison Access Service

### Technical Evidence

The service type has been definitively confirmed through systematic validation:

- ✅ **WSDL Available:** Service exposes complete WSDL at `?wsdl` endpoint
- ✅ **50+ SOAP Operations:** Comprehensive operation set including User, Card, Access Group management
- ✅ **WCF Technology Stack:** Microsoft .NET Framework 4.0, ASP.NET, IIS/WCF
- ✅ **SOAP-only Protocol:** JSON requests rejected with HTML errors, only accepts SOAP/XML

For detailed technical evidence, see: [`docs/soap-vs-rest-evidence.md`](docs/soap-vs-rest-evidence.md)

### Architecture Diagram

```text
┌─────────────────┐    ┌──────────────────┐    ┌─────────────────┐
│   Frontend      │    │   REST Adapter   │    │  SOAP Backend   │
│  Applications   │    │   (This Project) │    │                 │
│                 │    │                  │    │   Unison        │
│ • React         │◄──►│ • REST endpoints │◄──►│ Access Service  │
│ • Vue           │    │ • JSON handling  │    │                 │
│ • Angular       │    │ • SOAP translation│    │ • 50+ operations│
│ • Mobile apps   │    │ • Error mapping  │    │ • WCF service   │
│                 │    │                  │    │ • XML/SOAP      │
└─────────────────┘    └──────────────────┘    └─────────────────┘
```

For complete architectural details, see: [`docs/architecture.md`](docs/architecture.md)
