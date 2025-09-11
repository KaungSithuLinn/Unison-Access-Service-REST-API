namespace UnisonRestAdapter.Models.Monitoring
{
    /// <summary>
    /// System metrics information
    /// </summary>
    public class SystemMetrics
    {
        /// <summary>
        /// Name of the machine hosting the application
        /// </summary>
        public string MachineName { get; set; } = Environment.MachineName;

        /// <summary>
        /// Number of processors on the machine
        /// </summary>
        public int ProcessorCount { get; set; } = Environment.ProcessorCount;

        /// <summary>
        /// Operating system version
        /// </summary>
        public string OSVersion { get; set; } = Environment.OSVersion.ToString();

        /// <summary>
        /// .NET Framework version
        /// </summary>
        public string FrameworkVersion { get; set; } = Environment.Version.ToString();

        /// <summary>
        /// Working set memory usage in MB
        /// </summary>
        public long WorkingSetMemoryMB { get; set; }

        /// <summary>
        /// Private memory usage in MB
        /// </summary>
        public long PrivateMemoryMB { get; set; }

        /// <summary>
        /// CPU usage percentage
        /// </summary>
        public double CpuUsagePercent { get; set; }

        /// <summary>
        /// Number of threads
        /// </summary>
        public int ThreadCount { get; set; }

        /// <summary>
        /// Application uptime
        /// </summary>
        public TimeSpan Uptime { get; set; }

        /// <summary>
        /// Timestamp when metrics were collected
        /// </summary>
        public DateTime Timestamp { get; set; } = DateTime.UtcNow;
    }

    /// <summary>
    /// Application performance metrics
    /// </summary>
    public class ApplicationMetrics
    {
        /// <summary>
        /// Total number of requests processed
        /// </summary>
        public long TotalRequests { get; set; }

        /// <summary>
        /// Average response time in milliseconds
        /// </summary>
        public double AverageResponseTimeMs { get; set; }

        /// <summary>
        /// 95th percentile response time in milliseconds
        /// </summary>
        public double P95ResponseTimeMs { get; set; }

        /// <summary>
        /// Error rate as percentage
        /// </summary>
        public double ErrorRate { get; set; }

        /// <summary>
        /// Number of active connections
        /// </summary>
        public int ActiveConnections { get; set; }

        /// <summary>
        /// When metrics were last reset
        /// </summary>
        public DateTime LastResetTime { get; set; }

        /// <summary>
        /// Timestamp when metrics were collected
        /// </summary>
        public DateTime Timestamp { get; set; } = DateTime.UtcNow;
    }

    /// <summary>
    /// Request metrics information
    /// </summary>
    public class RequestMetrics
    {
        /// <summary>
        /// Total number of requests
        /// </summary>
        public long TotalRequests { get; set; }

        /// <summary>
        /// Number of successful requests
        /// </summary>
        public long SuccessfulRequests { get; set; }

        /// <summary>
        /// Number of error requests
        /// </summary>
        public long ErrorRequests { get; set; }

        /// <summary>
        /// Error rate as percentage
        /// </summary>
        public double ErrorRate => TotalRequests > 0 ? (ErrorRequests / (double)TotalRequests) * 100 : 0;

        /// <summary>
        /// Metrics per endpoint
        /// </summary>
        public Dictionary<string, EndpointMetrics> EndpointMetrics { get; set; } = new();

        /// <summary>
        /// Count of each status code returned
        /// </summary>
        public Dictionary<int, long> StatusCodeCounts { get; set; } = new();

        /// <summary>
        /// When metrics were last reset
        /// </summary>
        public DateTime LastResetTime { get; set; }
    }

    /// <summary>
    /// Per-endpoint metrics
    /// </summary>
    public class EndpointMetrics
    {
        /// <summary>
        /// Endpoint path
        /// </summary>
        public string Endpoint { get; set; } = string.Empty;

        /// <summary>
        /// Number of requests to this endpoint
        /// </summary>
        public long RequestCount { get; set; }

        /// <summary>
        /// Average response time in milliseconds
        /// </summary>
        public double AverageResponseTimeMs { get; set; }

        /// <summary>
        /// Minimum response time in milliseconds
        /// </summary>
        public double MinResponseTimeMs { get; set; }

        /// <summary>
        /// Maximum response time in milliseconds
        /// </summary>
        public double MaxResponseTimeMs { get; set; }

        /// <summary>
        /// Number of errors for this endpoint
        /// </summary>
        public long ErrorCount { get; set; }

        /// <summary>
        /// Last time this endpoint was accessed
        /// </summary>
        public DateTime LastAccessTime { get; set; }
    }

    /// <summary>
    /// Cache performance metrics
    /// </summary>
    public class CacheMetrics
    {
        /// <summary>
        /// Number of cache hits
        /// </summary>
        public long HitCount { get; set; }

        /// <summary>
        /// Number of cache misses
        /// </summary>
        public long MissCount { get; set; }

        /// <summary>
        /// Cache hit ratio as percentage
        /// </summary>
        public double HitRatio => (HitCount + MissCount) > 0 ? (HitCount / (double)(HitCount + MissCount)) * 100 : 0;

        /// <summary>
        /// Number of evicted cache entries
        /// </summary>
        public long EvictionCount { get; set; }

        /// <summary>
        /// Total number of entries in cache
        /// </summary>
        public long TotalEntries { get; set; }

        /// <summary>
        /// Memory usage of cache in bytes
        /// </summary>
        public long MemoryUsageBytes { get; set; }

        /// <summary>
        /// When metrics were last reset
        /// </summary>
        public DateTime LastResetTime { get; set; }
    }

    /// <summary>
    /// Dependency status information
    /// </summary>
    public class DependencyStatus
    {
        /// <summary>
        /// Dependency name
        /// </summary>
        public string Name { get; set; } = string.Empty;

        /// <summary>
        /// Dependency URL
        /// </summary>
        public string Url { get; set; } = string.Empty;

        /// <summary>
        /// Whether dependency is healthy
        /// </summary>
        public bool IsHealthy { get; set; }

        /// <summary>
        /// Status string (healthy/unhealthy)
        /// </summary>
        public string Status => IsHealthy ? "healthy" : "unhealthy";

        /// <summary>
        /// Response time in milliseconds
        /// </summary>
        public double ResponseTimeMs { get; set; }

        /// <summary>
        /// Error message if unhealthy
        /// </summary>
        public string? ErrorMessage { get; set; }

        /// <summary>
        /// When dependency was last checked
        /// </summary>
        public DateTime LastCheckTime { get; set; } = DateTime.UtcNow;
    }

    /// <summary>
    /// Health check response model according to Issue #6 specification
    /// </summary>
    public class HealthCheckResponse
    {
        /// <summary>
        /// Overall health status (healthy/degraded/unhealthy)
        /// </summary>
        public string Status { get; set; } = string.Empty;

        /// <summary>
        /// Timestamp of health check
        /// </summary>
        public DateTime Timestamp { get; set; } = DateTime.UtcNow;

        /// <summary>
        /// Service version
        /// </summary>
        public string Version { get; set; } = "1.0.0";

        /// <summary>
        /// Service uptime string
        /// </summary>
        public string Uptime { get; set; } = string.Empty;

        /// <summary>
        /// Status of dependencies
        /// </summary>
        public Dictionary<string, DependencyStatus> Dependencies { get; set; } = new();
    }
}
