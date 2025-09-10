# SOAP Request Templates - TASK-002

This directory contains SOAP request templates for the Unison Access Service REST API adapter.

## Templates

### UpdateCard.soap.xml

Template for the UpdateCard operation with the following parameters:

- **Token**: Authentication token (required)
- **CardNumber**: Card number to update (required, numeric, 1-20 digits)
- **Action**: Action to perform (required, one of: create, update, delete, activate, deactivate)
- **PersonId**: Person identifier (optional, positive integer)
- **FirstName**: Person's first name (optional)
- **LastName**: Person's last name (optional)
- **Email**: Person's email address (optional, valid email format)
- **Department**: Person's department (optional)
- **Title**: Person's title (optional)

## Usage

These templates are used by the `ValidationService` to:

1. Validate incoming JSON requests
2. Convert valid requests to SOAP envelopes
3. Ensure proper XML escaping of special characters
4. Maintain consistent SOAP structure

## Validation Rules

- All string fields are XML-escaped to prevent injection
- CardNumber must be numeric and between 1-20 digits
- Action must be one of the allowed enum values
- PersonId must be positive when provided
- Email must be valid format when provided

## Error Handling

Invalid requests result in structured JSON error responses with:

- `success: false`
- `message`: Human-readable error description
- `correlationId`: Unique identifier for request tracking
- `timestamp`: UTC timestamp
- `errors`: Array of specific validation errors
