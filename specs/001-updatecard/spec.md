# Spec: UpdateCard REST endpoint

Goal: Ensure the REST UpdateCard endpoint accepts JSON payloads, requires Unison-Token header, and converts SOAP backend errors (including HTML error pages) into structured JSON responses.

Acceptance criteria:

- Unauthenticated requests return 401 Unauthorized.
- Authenticated request where SOAP backend returns an HTML error results in 200/400 with JSON body matching UpdateCardResponse with Success=false and Message containing "HTTP Error".
- OpenAPI contract, sample cURL and PowerShell examples are generated (manual step).

Implementation notes:

- Tests use a fake `IUnisonService` to simulate backend HTML error and success responses.
- Integration tests use `WebApplicationFactory` to host the app in-memory.
