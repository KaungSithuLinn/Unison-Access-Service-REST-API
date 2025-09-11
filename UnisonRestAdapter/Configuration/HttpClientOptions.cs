using System.Net;

namespace UnisonRestAdapter.Configuration;

/// <summary>
/// Configuration options for HTTP client performance optimization
/// </summary>
public class HttpClientOptions
{
    /// <summary>
    /// Configuration section name
    /// </summary>
    public const string SectionName = "HttpClientOptions";

    /// <summary>
    /// Maximum number of connections per endpoint
    /// </summary>
    public int MaxConnectionsPerEndpoint { get; set; } = 100;

    /// <summary>
    /// Connection timeout in seconds
    /// </summary>
    public int ConnectionTimeoutSeconds { get; set; } = 30;

    /// <summary>
    /// Request timeout in seconds
    /// </summary>
    public int RequestTimeoutSeconds { get; set; } = 120;

    /// <summary>
    /// Connection pool timeout in seconds
    /// </summary>
    public int PooledConnectionLifetimeSeconds { get; set; } = 600;

    /// <summary>
    /// Connection pool idle timeout in seconds
    /// </summary>
    public int PooledConnectionIdleTimeoutSeconds { get; set; } = 90;

    /// <summary>
    /// Maximum automatic redirections
    /// </summary>
    public int MaxAutomaticRedirections { get; set; } = 10;

    /// <summary>
    /// Enable connection pooling
    /// </summary>
    public bool EnableConnectionPooling { get; set; } = true;

    /// <summary>
    /// Enable keep-alive connections
    /// </summary>
    public bool EnableKeepAlive { get; set; } = true;

    /// <summary>
    /// TCP keep-alive time in seconds
    /// </summary>
    public int TcpKeepAliveTime { get; set; } = 7200;

    /// <summary>
    /// TCP keep-alive interval in seconds
    /// </summary>
    public int TcpKeepAliveInterval { get; set; } = 75;
}
