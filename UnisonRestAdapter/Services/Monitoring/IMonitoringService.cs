using UnisonRestAdapter.Models.Monitoring;

namespace UnisonRestAdapter.Services.Monitoring
{
    /// <summary>
    /// Service interface for application monitoring and metrics collection
    /// </summary>
    public interface IMonitoringService
    {
        /// <summary>
        /// Gets comprehensive system metrics
        /// </summary>
        Task<SystemMetrics> GetSystemMetricsAsync();

        /// <summary>
        /// Gets application performance metrics
        /// </summary>
        ApplicationMetrics GetApplicationMetrics();

        /// <summary>
        /// Records a request metric
        /// </summary>
        void RecordRequestMetric(string endpoint, int statusCode, double responseTimeMs);

        /// <summary>
        /// Gets request metrics summary
        /// </summary>
        RequestMetrics GetRequestMetrics();

        /// <summary>
        /// Gets cache metrics if caching is enabled
        /// </summary>
        CacheMetrics GetCacheMetrics();

        /// <summary>
        /// Checks if a dependency is healthy
        /// </summary>
        Task<DependencyStatus> CheckDependencyAsync(string dependencyName, string url);
    }
}
