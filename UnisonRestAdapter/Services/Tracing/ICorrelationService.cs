namespace UnisonRestAdapter.Services.Tracing;

/// <summary>
/// Service for managing request correlation IDs and distributed tracing
/// </summary>
public interface ICorrelationService
{
    /// <summary>
    /// Gets the current correlation ID for the request
    /// </summary>
    string CorrelationId { get; }

    /// <summary>
    /// Sets a new correlation ID for the current request
    /// </summary>
    /// <param name="correlationId">The correlation ID to set</param>
    void SetCorrelationId(string correlationId);

    /// <summary>
    /// Generates a new correlation ID
    /// </summary>
    /// <returns>New unique correlation ID</returns>
    string GenerateCorrelationId();

    /// <summary>
    /// Gets all tracing headers for the current request
    /// </summary>
    /// <returns>Dictionary of tracing headers</returns>
    Dictionary<string, string> GetTracingHeaders();
}
