# Postman Collection Setup Guide

This guide explains how to import and configure the Unison REST Adapter API Postman collection.

## Quick Import

### Option 1: Import from File

1. Download the collection file: `Unison-REST-API.postman_collection.json`
2. Open Postman
3. Click "Import" (top left)
4. Drag and drop the JSON file or click "Upload Files"
5. Click "Import" to confirm

### Option 2: Import from URL (if hosted)

1. Open Postman
2. Click "Import" → "Link"
3. Enter the URL to the collection file
4. Click "Continue" → "Import"

## Environment Setup

### Create New Environment

1. In Postman, click "Environments" (left sidebar)
2. Click "Create Environment" or "+"
3. Name it "Unison API - Development" (or similar)

### Configure Variables

Set the following environment variables:

| Variable       | Initial Value           | Current Value           | Description          |
| -------------- | ----------------------- | ----------------------- | -------------------- |
| `base_url`     | `http://localhost:5203` | `http://localhost:5203` | API base URL         |
| `unison_token` | `your-token-here`       | `your-actual-token`     | Authentication token |

### Environment Examples

#### Development Environment

```json
{
  "name": "Unison API - Development",
  "values": [
    {
      "key": "base_url",
      "value": "http://localhost:5203",
      "enabled": true
    },
    {
      "key": "unison_token",
      "value": "dev-token-123",
      "enabled": true
    }
  ]
}
```

#### Staging Environment

```json
{
  "name": "Unison API - Staging",
  "values": [
    {
      "key": "base_url",
      "value": "https://staging-unison-api.company.com",
      "enabled": true
    },
    {
      "key": "unison_token",
      "value": "staging-token-456",
      "enabled": true
    }
  ]
}
```

#### Production Environment

```json
{
  "name": "Unison API - Production",
  "values": [
    {
      "key": "base_url",
      "value": "https://unison-api.company.com",
      "enabled": true
    },
    {
      "key": "unison_token",
      "value": "prod-token-789",
      "enabled": true
    }
  ]
}
```

## Authentication Setup

The collection is pre-configured with API Key authentication:

1. **Collection-Level Auth**:

   - Type: API Key
   - Key: `Unison-Token`
   - Value: `{{unison_token}}` (uses environment variable)
   - Add to: Header

2. **Request-Level Auth**: Most requests inherit from collection auth
3. **No Auth**: Only the basic health check endpoint requires no authentication

## Usage Instructions

### Basic Testing

1. Select your environment (top right dropdown)
2. Verify `base_url` and `unison_token` variables are set
3. Start with "Basic Health Check" to test connectivity
4. Try "Detailed Health Check" to verify authentication
5. Use CRUD operations for cards and users

### Running Collections

1. Click "Collections" → "Unison REST Adapter API"
2. Click "Run" (play button)
3. Select requests to run
4. Choose environment
5. Set iterations and delay if needed
6. Click "Run Unison REST Adapter API"

### Test Scripts

Each request includes test scripts that:

- Validate response codes
- Check response times
- Verify JSON structure
- Store variables for chained requests
- Log debugging information

### Global Variables

The collection automatically sets these global variables:

- `correlation_id`: Unique ID for request tracking
- `timestamp`: Current timestamp
- `created_card_id`: ID of last created card
- `created_user_id`: ID of last created user

## Request Examples

### Health Checks

- **Basic Health**: No auth required, tests API availability
- **Detailed Health**: Requires auth, tests SOAP service connectivity

### Card Management

1. **Create Card**: Creates new card with provided details
2. **Update Card**: Modifies existing card (partial updates supported)
3. **Get Card**: Retrieves card by ID
4. **Delete Card**: Removes card from system

### User Management

1. **Create User**: Creates new user with optional email/department
2. **Update User**: Modifies existing user (partial updates supported)
3. **Get User**: Retrieves user by ID
4. **Delete User**: Removes user from system

### Test Workflows

- **Complete Card Lifecycle**: Full CRUD test sequence
- **Authentication Test**: Tests with invalid token (expect 401)
- **Performance Baseline**: Measures response times

## Troubleshooting

### Common Issues

#### 401 Unauthorized

- **Cause**: Invalid or missing `unison_token`
- **Solution**: Check environment variable value, ensure token is valid

#### Connection Refused

- **Cause**: API not running or wrong `base_url`
- **Solution**: Verify API is running on specified URL/port

#### SSL Certificate Errors

- **Cause**: Self-signed certificates in development
- **Solution**: In Postman settings, disable "SSL certificate verification"

#### Timeout Errors

- **Cause**: Slow API response or network issues
- **Solution**: Increase timeout in Postman settings or check API performance

### Debugging Tips

1. **Check Console**: View logs in Postman console (bottom panel)
2. **Inspect Headers**: Review request/response headers for issues
3. **Validate JSON**: Ensure request bodies are valid JSON
4. **Test Variables**: Use `{{variable_name}}` syntax and check resolution
5. **Run Individual Requests**: Test single requests before running collections

### Postman Console Debugging

Enable the console to see detailed logs:

1. View → Show Postman Console
2. Review request/response details
3. Check test script outputs
4. Monitor variable assignments

## Advanced Features

### Pre-request Scripts

The collection includes pre-request scripts that:

- Generate correlation IDs
- Set timestamps
- Prepare test data

### Test Automation

Test scripts provide:

- Response validation
- Performance monitoring
- Data extraction
- Chain request management

### Environment Management

Best practices:

- Use separate environments for dev/staging/prod
- Store sensitive tokens in "Current Value" only
- Use descriptive environment names
- Document variable purposes

### Collection Sharing

To share the collection:

1. Export → Collection → Save as JSON
2. Share environment separately (without sensitive values)
3. Provide setup instructions
4. Document required permissions/access

## API Documentation Integration

This Postman collection complements:

- **OpenAPI Specification**: Available at `/swagger` endpoint
- **cURL Examples**: In `docs/examples/curl-examples.md`
- **PowerShell Examples**: In `docs/examples/powershell-examples.md`

## Support and Maintenance

### Regular Updates

- Update collection when API changes
- Verify examples with new API versions
- Review and update test scripts
- Maintain environment configurations

### Version Control

- Store collection files in version control
- Document changes in commit messages
- Tag releases for major API changes
- Maintain backwards compatibility notes

---

**Collection Version**: 1.0.0  
**Last Updated**: January 9, 2025  
**Postman Version**: 10.0+ (recommended)

For additional support, refer to the main API documentation or contact the development team.
