# Unison REST Adapter API

A REST-to-SOAP adapter service for the Unison Access Service, providing modern REST endpoints for card management operations.

## Overview

The Unison REST Adapter API converts REST requests to SOAP calls for the backend Unison service, handling authentication via the `Unison-Token` header and providing structured JSON responses.

## Quick Start

### Prerequisites

- .NET 9.0 SDK
- Access to Unison SOAP backend service
- Valid Unison authentication token

### Running Locally

1. Clone the repository
2. Build the solution:

   ```bash
   dotnet build "Unison Access Service REST API.sln" -c Release
   ```

3. Run the tests:

   ```bash
   dotnet test "Unison Access Service REST API.sln"
   ```

4. Start the API:

   ```bash
   cd UnisonRestAdapter
   dotnet run
   ```

The API will be available at:

- HTTP: `http://localhost:5000`
- HTTPS: `https://localhost:5001`

## API Endpoints

### Health Check

- **GET** `/api/health/ping` - Returns service health status

### Card Operations

- **PUT** `/api/cards/update` - Update card information
- **POST** `/updatecard` - Legacy endpoint for card updates
- **GET** `/api/cards/{cardId}` - Get card information by ID

## Authentication

All endpoints (except health check) require the `Unison-Token` header:

```http
Unison-Token: your-unison-token-here
```

## Examples

### PowerShell Example

```powershell
# Run the provided example script
.\examples\updatecard.ps1
```

### cURL Example

```bash
# Health check
curl -X GET http://localhost:5001/api/health/ping

# Update card
curl -X PUT http://localhost:5001/api/cards/update \
  -H "Content-Type: application/json" \
  -H "Unison-Token: your-token-here" \
  -d '{
    "cardId": "TEST123",
    "userName": "john.doe",
    "firstName": "John",
    "lastName": "Doe",
    "email": "john.doe@example.com",
    "department": "Engineering",
    "title": "Software Engineer",
    "isActive": true
  }'
```

## API Documentation

- **OpenAPI Specification**: [`openapi/unison-rest-adapter.yaml`](openapi/unison-rest-adapter.yaml)
- **Postman Collection**: [`postman/UnisonRestAdapter.postman_collection.json`](postman/UnisonRestAdapter.postman_collection.json)

## Testing

### Unit Tests

```bash
dotnet test tests/UnisonRestAdapter.SpecTests/
```

### Integration Tests

The test suite includes:

- Health endpoint validation
- Authentication tests (401 responses)
- SOAP error handling (HTML to JSON conversion)

### Postman/Newman Tests

```bash
newman run postman/UnisonRestAdapter.postman_collection.json \
  --env-var "baseUrl=http://localhost:5001" \
  --env-var "unisonToken=your-token"
```

## Configuration

### Environment Variables

- `ASPNETCORE_URLS` - Server URLs (default: `http://localhost:5000;https://localhost:5001`)
- `ASPNETCORE_ENVIRONMENT` - Environment (Development, Staging, Production)

### Application Settings

Configure SOAP backend connection in `appsettings.json`:

```json
{
  "UnisonService": {
    "SoapEndpoint": "http://your-soap-service/endpoint",
    "Timeout": "00:00:30"
  }
}
```

## Development

### Project Structure

```
├── UnisonRestAdapter/           # Main API project
│   ├── Controllers/             # REST controllers
│   ├── Services/                # SOAP service clients
│   ├── Models/                  # Request/response models
│   └── Configuration/           # Service configuration
├── tests/
│   └── UnisonRestAdapter.SpecTests/  # Integration tests
├── openapi/                     # OpenAPI specification
├── postman/                     # Postman collection
├── examples/                    # Usage examples
└── docs/                        # Documentation
```

### Adding New Endpoints

1. Create request/response models in `Models/`
2. Add service interface methods in `Services/IUnisonService.cs`
3. Implement service logic in `Services/UnisonService.cs`
4. Create controller action in appropriate controller
5. Add tests in `tests/UnisonRestAdapter.SpecTests/`
6. Update OpenAPI specification
7. Update Postman collection

### Error Handling

The API converts SOAP errors (including HTML error pages) into structured JSON responses:

```json
{
  "success": false,
  "message": "HTTP Error: [HTML error from SOAP backend]",
  "cardId": "TEST123",
  "timestamp": "2025-09-08T10:30:00Z",
  "transactionId": null
}
```

## Deployment

### Docker

A `Dockerfile` can be added for containerized deployment:

```dockerfile
FROM mcr.microsoft.com/dotnet/aspnet:9.0
WORKDIR /app
COPY bin/Release/net9.0/publish/ .
ENTRYPOINT ["dotnet", "UnisonRestAdapter.dll"]
```

### Environment-Specific Configuration

Use environment-specific `appsettings.{Environment}.json` files for different deployment environments.

## Security

- All API endpoints require authentication via `Unison-Token` header
- HTTPS is enabled by default in production
- Input validation on all request models
- Structured error responses (no sensitive data leakage)

## Monitoring

### Health Checks

The `/api/health/ping` endpoint provides service health status including:

- Service availability
- Backend connectivity (when implemented)
- Service version information

### Logging

The application uses structured logging with:

- Request/response logging
- Error tracking
- Performance metrics

## Contributing

1. Fork the repository
2. Create a feature branch
3. Make changes with tests
4. Run the full test suite
5. Submit a pull request

### Code Quality

- Follow .NET coding standards
- Maintain test coverage
- Update documentation
- Run security scans

## Support

For issues and questions:

- Create GitHub issues for bugs/features
- Check existing documentation
- Review test examples for usage patterns

## License

[MIT License](LICENSE)

---

**Note**: This is a REST adapter service. Ensure you have proper access and authentication for the backend Unison SOAP service before deployment.
